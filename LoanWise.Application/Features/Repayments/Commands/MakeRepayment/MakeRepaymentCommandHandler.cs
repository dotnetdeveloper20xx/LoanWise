using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using LoanWise.Domain.Enums;
using LoanWise.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Repayments.Commands.MakeRepayment
{
    public class MakeRepaymentCommandHandler : IRequestHandler<MakeRepaymentCommand, ApiResponse<Guid>>
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IApplicationDbContext _db;
        private readonly ILogger<MakeRepaymentCommandHandler> _logger;
        private readonly IMediator _mediator;

        public MakeRepaymentCommandHandler(
            ILoanRepository loanRepository,
            IApplicationDbContext db,
            ILogger<MakeRepaymentCommandHandler> logger,
            IMediator mediator)
        {
            _loanRepository = loanRepository;
            _db = db;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<ApiResponse<Guid>> Handle(MakeRepaymentCommand request, CancellationToken ct)
        {
            // Fetch loan that owns this repayment, including Repayments + Fundings
            var loan = await _loanRepository.GetLoanByRepaymentIdWithFundingsAsync(request.RepaymentId, ct);
            if (loan is null)
            {
                _logger.LogWarning("Repayment {RepaymentId} not found on any loan.", request.RepaymentId);
                return ApiResponse<Guid>.FailureResult("Repayment not found.");
            }

            if (loan.Status != LoanStatus.Disbursed)
                return ApiResponse<Guid>.FailureResult("Loan must be disbursed before repayments can be made.");

            var repayment = loan.Repayments.First(r => r.Id == request.RepaymentId);
            if (repayment.IsPaid)
                return ApiResponse<Guid>.FailureResult("This repayment is already marked as paid.");

            var paidOn = DateTime.UtcNow;

            // 1) Mark as paid (domain event raised inside)
            repayment.MarkAsPaid(paidOn, loan.BorrowerId);

            // 2) Allocate inflow to lenders proportional to their funding
            var repaymentAmount = repayment.RepaymentAmount;

            // Group total funded per lender
            var byLender = loan.Fundings
                .GroupBy(f => f.LenderId)
                .Select(g => new { LenderId = g.Key, Funded = g.Sum(x => x.Amount) })
                .OrderBy(x => x.LenderId) // deterministic
                .ToList();

            var totalFunded = byLender.Sum(x => x.Funded);
            if (totalFunded <= 0m)
            {
                _logger.LogWarning("Loan {LoanId} has no fundings; skipping lender allocations for repayment {RepaymentId}.",
                    loan.Id, repayment.Id);
            }
            else
            {
                // Cap denominator at loan.Amount to reduce rounding drift if over-funded
                var denom = Math.Min(totalFunded, loan.Amount);
                var allocations = new List<LenderRepayment>(byLender.Count);

                decimal remainder = repaymentAmount;
                for (int i = 0; i < byLender.Count; i++)
                {
                    var share = (i == byLender.Count - 1)
                        ? remainder
                        : Math.Round(repaymentAmount * (byLender[i].Funded / denom), 2, MidpointRounding.AwayFromZero);

                    remainder -= share;

                    // Use your actual entity ctor/shape
                    allocations.Add(new LenderRepayment
                    {
                        Id = Guid.NewGuid(),
                        LenderId = byLender[i].LenderId,
                        LoanId = loan.Id,
                        RepaymentId = repayment.Id,
                        Amount = share,
                        CreatedAtUtc = paidOn
                    });
                }

                await _db.LenderRepayments.AddRangeAsync(allocations, ct);
            }

            // 3) Persist once
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Repayment {RepaymentId} for loan {LoanId} marked as paid on {PaidOn}.",
                repayment.Id, loan.Id, paidOn);

            // Optional: raise an explicit event (MarkAsPaid already added a domain event)
            await _mediator.Publish(new RepaymentPaidEvent(loan.Id, repayment.Id, paidOn), ct);

            return ApiResponse<Guid>.SuccessResult(repayment.Id, "Repayment successful and lender allocations recorded.");
        }
    }
}

using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using LoanWise.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Repayments.Commands.MakeRepayment
{
    /// <summary>
    /// Handles marking a repayment as paid and creating lender repayment records.
    /// </summary>
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

        public async Task<ApiResponse<Guid>> Handle(MakeRepaymentCommand request, CancellationToken cancellationToken)
        {
            // Load all loans with repayments to find this one
            var allLoans = await _loanRepository.GetLoansWithRepaymentsAsync(cancellationToken);
            var loan = allLoans.FirstOrDefault(l => l.Repayments.Any(r => r.Id == request.RepaymentId));

            if (loan == null)
            {
                _logger.LogWarning("Repayment {RepaymentId} not found in any loan", request.RepaymentId);
                return ApiResponse<Guid>.FailureResult("Repayment not found.");
            }

            var repayment = loan.Repayments.First(r => r.Id == request.RepaymentId);

            if (repayment.IsPaid)
            {
                return ApiResponse<Guid>.FailureResult("This repayment is already marked as paid.");
            }

            // Mark repayment as paid
            var paidOn = DateTime.UtcNow;
            repayment.MarkAsPaid(paidOn, loan.BorrowerId);

            // ---- Create lender repayment allocations ----
            var totalFunded = loan.Fundings.Sum(f => f.Amount.Value);
            if (totalFunded > 0)
            {
                var lenderGroups = loan.Fundings
                    .GroupBy(f => f.LenderId)
                    .Select(g => new
                    {
                        LenderId = g.Key,
                        Funded = g.Sum(f => f.Amount.Value)
                    })
                    .ToList();

                var lenderRepayments = lenderGroups.Select(g => new LenderRepayment
                {
                    Id = Guid.NewGuid(),
                    LoanId = loan.Id,
                    RepaymentId = repayment.Id,
                    LenderId = g.LenderId,
                    Amount = Math.Round(repayment.RepaymentAmount * (g.Funded / totalFunded), 2),
                    CreatedAtUtc = DateTime.UtcNow
                }).ToList();

                await _db.LenderRepayments.AddRangeAsync(lenderRepayments, cancellationToken);
            }

            await _loanRepository.UpdateAsync(loan, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Repayment {RepaymentId} marked as paid", repayment.Id);

            // Publish domain event
            await _mediator.Publish(new RepaymentPaidEvent(
                loan.Id,
                repayment.Id,
                paidOn
            ), cancellationToken);

            return ApiResponse<Guid>.SuccessResult(repayment.Id, "Repayment successful.");
        }
    }
}

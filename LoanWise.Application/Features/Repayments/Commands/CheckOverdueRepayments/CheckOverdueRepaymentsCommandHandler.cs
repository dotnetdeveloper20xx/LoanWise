// CheckOverdueRepaymentsCommandHandler.cs
using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.Features.Repayments.Commands.CheckOverdueRepayments;
using LoanWise.Domain.Events;
using MediatR;
using StoreBoost.Application.Common.Models; // ApiResponse<T>

namespace LoanWise.Application.Features.Repayments.Commands.CheckOverdue
{
    public sealed class CheckOverdueRepaymentsCommandHandler
        : IRequestHandler<CheckOverdueRepaymentsCommand, ApiResponse<int>>
    {
        private readonly ILoanRepository _loans;
        private readonly IMediator _mediator;
        private readonly IApplicationDbContext _db;

        public CheckOverdueRepaymentsCommandHandler(
            ILoanRepository loans,
            IMediator mediator,
            IApplicationDbContext db)
        {
            _loans = loans;
            _mediator = mediator;
            _db = db;
        }

        public async Task<ApiResponse<int>> Handle(CheckOverdueRepaymentsCommand request, CancellationToken ct)
        {
            var nowDate = DateTime.UtcNow.Date;
            var loansWithRepayments = await _loans.GetLoansWithRepaymentsAsync(ct);

            var overdueItems = loansWithRepayments
                .SelectMany(l => l.Repayments.Select(r => (loan: l, r)))
                .Where(x =>
                    !x.r.IsPaid &&
                    x.r.DueDate.Date < nowDate &&
                    x.r.OverdueNotifiedAtUtc == null) // remove this check if you didn't add the flag
                .ToList();

            foreach (var item in overdueItems)
            {
                // raise event
                await _mediator.Publish(new RepaymentOverdueEvent(
                    item.loan.Id,
                    item.r.Id,
                    item.r.DueDate
                ), ct);

                // mark to avoid duplicate notifications next run
                item.r.MarkOverdueNotified(DateTime.UtcNow);
                await _loans.UpdateAsync(item.loan, ct); // persist change per loan
            }

            await _db.SaveChangesAsync(ct);
            return ApiResponse<int>.SuccessResult(overdueItems.Count, $"Overdue checked. Raised {overdueItems.Count} event(s).");
        }
    }
}

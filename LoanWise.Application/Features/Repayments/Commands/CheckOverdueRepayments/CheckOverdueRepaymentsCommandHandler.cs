// CheckOverdueRepaymentsCommandHandler.cs
using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.Features.Repayments.Commands.CheckOverdueRepayments;
using LoanWise.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

        // using Microsoft.EntityFrameworkCore;

        public async Task<ApiResponse<int>> Handle(CheckOverdueRepaymentsCommand request, CancellationToken ct)
        {
            var today = DateTime.UtcNow.Date;

            // Unpaid, past-due, not yet notified
            var reps = await _db.Repayments
                .Where(r => !r.IsPaid && r.DueDate < today && r.OverdueNotifiedAtUtc == null)
                .OrderBy(r => r.DueDate)
                .ToListAsync(ct);

            var now = DateTime.UtcNow;
            foreach (var r in reps)
            {
                // raise domain event (keeps your existing behavior)
                await _mediator.Publish(new RepaymentOverdueEvent(r.LoanId, r.Id, r.DueDate), ct);

                r.MarkOverdue();                // raises domain event inside (OK if you prefer one or the other)
                r.MarkOverdueNotified(now);     // latch to avoid duplicate notifications next runs
            }

            await _db.SaveChangesAsync(ct);
            return ApiResponse<int>.SuccessResult(reps.Count, $"Overdue checked. Raised {reps.Count} event(s).");
        }

    }
}

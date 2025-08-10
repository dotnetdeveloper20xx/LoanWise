using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using LoanWise.Domain.Events;
using MediatR;

public sealed class LoanFundedEventHandler : INotificationHandler<LoanFundedEvent>
{
    private readonly INotificationRepository _repo;
    private readonly ILoanRepository _loans;
    private readonly INotificationService _notify; // <— inject

    public LoanFundedEventHandler(INotificationRepository repo, ILoanRepository loans, INotificationService notify)
    {
        _repo = repo; _loans = loans; _notify = notify;
    }

    public async Task Handle(LoanFundedEvent e, CancellationToken ct)
    {
        var loan = await _loans.GetByIdAsync(e.LoanId, ct);
        if (loan is null) return;

        var borrowerTitle = e.IsFullyFunded ? "Loan fully funded 🎉" : "New funding received";
        var borrowerMsg = e.IsFullyFunded
            ? $"Your loan {e.LoanId} received £{e.Amount:N2} and is now fully funded."
            : $"Your loan {e.LoanId} received £{e.Amount:N2}.";

        // 1) Persist (in-app inbox)
        await _repo.AddAsync(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = loan.BorrowerId,
            Title = borrowerTitle,
            Message = borrowerMsg,
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow
        }, ct);

        // 2) Push (SignalR + Email through the composite)
        await _notify.NotifyBorrowerAsync(loan.BorrowerId, borrowerTitle, borrowerMsg, ct);

        // Optional: notify the lender who funded
        var lenderTitle = "Funding confirmed";
        var lenderMsg = $"You funded £{e.Amount:N2} to loan {e.LoanId}.";
        await _notify.NotifyLenderAsync(e.LenderId, lenderTitle, lenderMsg, ct);
    }
}

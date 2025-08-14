namespace LoanWise.Application.DTOs.Repayments
{
    public sealed record CheckOverdueResultDto(
        int Scanned,           // unpaid + past-due
        int NewlyOverdue,      // just marked + notified now
        int AlreadyNotified,   // previously notified
        int PaidIgnored        // past-due but already paid
    );
}

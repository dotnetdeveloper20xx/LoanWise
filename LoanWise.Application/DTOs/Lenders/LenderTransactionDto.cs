namespace LoanWise.Application.DTOs.Lenders;

public sealed class LenderTransactionDto
{
    public Guid Id { get; set; }                 // synthetic transaction id
    public Guid LenderId { get; set; }
    public Guid LoanId { get; set; }
    public string LoanRef { get; set; } = default!;    // e.g., "Loan #123" or title
    public string BorrowerName { get; set; } = default!;
    public DateTime OccurredAtUtc { get; set; }
    public string Type { get; set; } = default!;       // "Funding" | "Repayment"
    public decimal Amount { get; set; }                // signed: negative for funding, positive for repayment
    public string Description { get; set; } = default!;
}

namespace LoanWise.Application.DTOs.Exports
{
    public sealed class RepaymentPlanDoc
    {
        public Guid LoanId { get; init; }
        public string BorrowerName { get; init; } = default!;
        public decimal Amount { get; init; }
        public decimal AnnualInterestRate { get; init; }
        public int DurationInMonths { get; init; }
        public DateTime GeneratedAtUtc { get; init; }
        public IReadOnlyList<RepaymentLine> Lines { get; init; } = Array.Empty<RepaymentLine>();
    }

    public sealed record RepaymentLine(
    int Number,
    DateTime DueDate,
    decimal Amount,
    bool IsPaid,
    DateTime? PaidOnUtc);

}
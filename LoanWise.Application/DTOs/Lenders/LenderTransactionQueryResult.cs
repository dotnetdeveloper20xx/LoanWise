namespace LoanWise.Application.DTOs.Lenders;

public sealed class LenderTransactionQueryResult
{
    public int Total { get; set; }                 // total matching (pre-paging)
    public IReadOnlyList<LenderTransactionDto> Items { get; set; } = Array.Empty<LenderTransactionDto>();
}

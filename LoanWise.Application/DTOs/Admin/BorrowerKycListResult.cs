namespace LoanWise.Application.DTOs.Admin;

public sealed class BorrowerKycListResult
{
    public int Total { get; set; }
    public IReadOnlyList<BorrowerKycListItemDto> Items { get; set; } = Array.Empty<BorrowerKycListItemDto>();
}

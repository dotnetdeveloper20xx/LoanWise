using LoanWise.Application.DTOs.Lenders;

namespace LoanWise.Application.Common.Interfaces;

public interface ILenderReportingRepository
{
    Task<(int total, IEnumerable<LenderTransactionDto> items)> GetLenderTransactionsAsync(
        Guid lenderId,
        DateTime? fromUtc,
        DateTime? toUtc,
        Guid? loanId,
        Guid? borrowerId,
        int page,
        int pageSize,
        CancellationToken ct);
}

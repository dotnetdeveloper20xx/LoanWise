using MediatR;
using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.DTOs.Lenders;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Lenders.Queries.GetTransactions;

public sealed class GetLenderTransactionsQueryHandler
    : IRequestHandler<GetLenderTransactionsQuery, ApiResponse<LenderTransactionQueryResult>>
{
    private readonly ILenderReportingRepository _reporting;
    private readonly IUserContext _user;

    public GetLenderTransactionsQueryHandler(ILenderReportingRepository reporting, IUserContext user)
    {
        _reporting = reporting;
        _user = user;
    }

    public async Task<ApiResponse<LenderTransactionQueryResult>> Handle(GetLenderTransactionsQuery q, CancellationToken ct)
    {
        var lenderId = q.LenderId ?? _user.UserId ?? Guid.Empty;
        if (lenderId == Guid.Empty)
            return ApiResponse<LenderTransactionQueryResult>.FailureResult("Unauthorized: missing user id.");

        if (q.Page <= 0 || q.PageSize <= 0)
            return ApiResponse<LenderTransactionQueryResult>.FailureResult("Invalid paging parameters.");

        var (total, items) = await _reporting.GetLenderTransactionsAsync(
            lenderId, q.FromUtc, q.ToUtc, q.LoanId, q.BorrowerId, q.Page, q.PageSize, ct);

        return ApiResponse<LenderTransactionQueryResult>.SuccessResult(
            new LenderTransactionQueryResult { Total = total, Items = items.ToList() }
        );
    }
}

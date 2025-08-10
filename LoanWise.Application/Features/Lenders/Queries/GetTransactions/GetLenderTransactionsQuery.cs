using MediatR;
using LoanWise.Application.DTOs.Lenders;
using StoreBoost.Application.Common.Models; // ApiResponse<T>

namespace LoanWise.Application.Features.Lenders.Queries.GetTransactions;

public sealed record GetLenderTransactionsQuery(
    Guid? LenderId,                 // null => use current user
    DateTime? FromUtc,
    DateTime? ToUtc,
    Guid? LoanId,
    Guid? BorrowerId,
    int Page = 1,
    int PageSize = 25
) : IRequest<ApiResponse<LenderTransactionQueryResult>>;

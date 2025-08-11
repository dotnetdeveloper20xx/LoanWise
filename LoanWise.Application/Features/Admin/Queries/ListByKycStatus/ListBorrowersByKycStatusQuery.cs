using MediatR;
using LoanWise.Application.DTOs.Admin;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Admin.Borrowers.Queries.ListByKycStatus;

public sealed record ListBorrowersByKycStatusQuery(
    string Status,
    int Page = 1,
    int PageSize = 25,
     string? Search = null,      //  search by borrower name/email
    string? SortBy = null,      //  score|verifiedAt|name|risk|status
    string SortDir = "desc" 
) : IRequest<ApiResponse<BorrowerKycListResult>>;

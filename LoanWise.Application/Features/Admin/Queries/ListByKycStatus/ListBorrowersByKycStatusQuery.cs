using MediatR;
using LoanWise.Application.DTOs.Admin;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Admin.Borrowers.Queries.ListByKycStatus;

public sealed record ListBorrowersByKycStatusQuery(
    string StatusesCsv,              // e.g. "Verified,Pending"
    int Page = 1,
    int PageSize = 25,
    string? Search = null,           // search by name/email
    string? SortBy = null,           // score|verifiedAt|name|risk|status|scoreAt|lastScoreAt
    string SortDir = "desc"          // asc|desc
) : IRequest<ApiResponse<BorrowerKycListResult>>;

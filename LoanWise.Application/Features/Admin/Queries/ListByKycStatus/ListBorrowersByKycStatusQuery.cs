using MediatR;
using LoanWise.Application.DTOs.Admin;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Admin.Borrowers.Queries.ListByKycStatus;

public sealed record ListBorrowersByKycStatusQuery(
    string Status,
    int Page = 1,
    int PageSize = 25
) : IRequest<ApiResponse<BorrowerKycListResult>>;

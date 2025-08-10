// LoanWise.Application/Features/Borrowers/Queries/GetRiskSummary/GetBorrowerRiskSummaryQuery.cs
using MediatR;
using LoanWise.Application.DTOs.Borrowers;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Borrowers.Queries.GetRiskSummary;

public sealed record GetBorrowerRiskSummaryQuery(Guid BorrowerId)
    : IRequest<ApiResponse<BorrowerRiskSummaryDto>>;

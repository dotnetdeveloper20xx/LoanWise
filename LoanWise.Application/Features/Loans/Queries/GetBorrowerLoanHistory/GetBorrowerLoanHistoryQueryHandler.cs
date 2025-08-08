using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.DTOs.Loans;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StoreBoost.Application.Common.Models;
using System.Linq;

namespace LoanWise.Application.Features.Loans.Queries.GetBorrowerLoanHistory
{
    public sealed class GetBorrowerLoanHistoryQueryHandler
         : IRequestHandler<GetBorrowerLoanHistoryQuery, ApiResponse<List<BorrowerLoanHistoryDto>>>
    {
        private readonly IApplicationDbContext _db;
        private readonly IUserContext _userContext;

        public GetBorrowerLoanHistoryQueryHandler(IApplicationDbContext db, IUserContext userContext)
        {
            _db = db;
            _userContext = userContext;
        }

        public async Task<ApiResponse<List<BorrowerLoanHistoryDto>>> Handle(
            GetBorrowerLoanHistoryQuery request,
            CancellationToken ct)
        {
            if (!_userContext.UserId.HasValue)
                return ApiResponse<List<BorrowerLoanHistoryDto>>.FailureResult("Unauthorized: missing user ID");

            Guid borrowerId = _userContext.UserId.Value;

            var loans = await _db.Loans
                .Where(l => l.BorrowerId == borrowerId)
                .OrderByDescending(l => l.CreatedAtUtc)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(l => new BorrowerLoanHistoryDto(
                    l.Id,
                    l.Amount.Value,
                    l.Fundings.Sum(f => f.Amount.Value),
                    l.Purpose.ToString(),
                    l.Status.ToString(),
                    l.RiskLevel.ToString(),
                    l.CreatedAtUtc,
                    l.ApprovedAtUtc,
                    l.DisbursedAtUtc
                ))
                .ToListAsync(ct);

            return ApiResponse<List<BorrowerLoanHistoryDto>>.SuccessResult(loans);
        }
    }
}

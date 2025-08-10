using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.DTOs.Dashboard;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Dashboard.Queries.GetLenderPortfolio
{
    public class GetLenderPortfolioSummaryQueryHandler
        : IRequestHandler<GetLenderPortfolioSummaryQuery, ApiResponse<LenderPortfolioDto>>
    {
        private readonly IApplicationDbContext _db;
        private readonly IUserContext _user;

        public GetLenderPortfolioSummaryQueryHandler(IApplicationDbContext db, IUserContext user)
        {
            _db = db;
            _user = user;
        }

        public async Task<ApiResponse<LenderPortfolioDto>> Handle(
            GetLenderPortfolioSummaryQuery request,
            CancellationToken cancellationToken)
        {
            if (!_user.UserId.HasValue)
                return ApiResponse<LenderPortfolioDto>.FailureResult("Unauthorized.");

            var lenderId = _user.UserId.Value;

            // If you expose role in IUserContext, you can enforce it:
            // if (_user.Role != UserRole.Lender) return ApiResponse<LenderPortfolioDto>.FailureResult("Forbidden.");

            // Total funded by this lender
            var totalFunded = await _db.Fundings
                .Where(f => f.LenderId == lenderId)
                .SumAsync(f => (decimal?)f.Amount.Value, cancellationToken) ?? 0m;

            // Number of distinct loans this lender has funded
            var numberOfLoans = await _db.Fundings
                .Where(f => f.LenderId == lenderId)
                .Select(f => f.LoanId)
                .Distinct()
                .CountAsync(cancellationToken);

            // Total returned to this lender (from proportional repayment slices)
            var totalReturned = await _db.LenderRepayments
                .Where(lr => lr.LenderId == lenderId)
                .SumAsync(lr => (decimal?)lr.Amount, cancellationToken) ?? 0m;

            // Outstanding balance = funded - returned (never negative)
            var outstanding = Math.Max(0m, totalFunded - totalReturned);

            var dto = new LenderPortfolioDto
            {
                TotalFunded = totalFunded,
                NumberOfLoansFunded = numberOfLoans,
                TotalReturned = totalReturned,
                OutstandingBalance = outstanding
            };

            return ApiResponse<LenderPortfolioDto>.SuccessResult(dto);
        }
    }
}

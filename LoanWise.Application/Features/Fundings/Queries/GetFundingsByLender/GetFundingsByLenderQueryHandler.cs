using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.Features.Fundings.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StoreBoost.Application.Common.Models;
using System.Linq;

namespace LoanWise.Application.Features.Fundings.Queries.GetFundingsByLender
{
    public sealed class GetFundingsByLenderQueryHandler
        : IRequestHandler<GetFundingsByLenderQuery, ApiResponse<List<LenderFundingDto>>>
    {
        private readonly IApplicationDbContext _db;
        private readonly IUserContext _userContext;

        public GetFundingsByLenderQueryHandler(IApplicationDbContext db, IUserContext userContext)
        {
            _db = db;
            _userContext = userContext;
        }

        public async Task<ApiResponse<List<LenderFundingDto>>> Handle(GetFundingsByLenderQuery request, CancellationToken ct)
        {
            if (!_userContext.UserId.HasValue)
                return ApiResponse<List<LenderFundingDto>>.FailureResult("Unauthorized: missing user ID");

            var lenderId = _userContext.UserId.Value;

            var fundings = await _db.Fundings
                .Where(f => f.LenderId == lenderId)
                .Include(f => f.Loan)
                    .ThenInclude(l => l.Fundings) // Needed to calculate TotalFunded
                .OrderByDescending(f => f.FundedOn)
                .Select(f => new LenderFundingDto(
                    f.LoanId,                                        // LoanId (Guid)
                    f.Loan.Amount.Value,                             // LoanAmount (decimal)
                    f.Loan.Fundings.Sum(x => x.Amount.Value),        // TotalFunded (decimal)
                    f.Loan.Purpose.ToString(),                       // Purpose (string)
                    f.Loan.Status.ToString(),                        // Status (string)
                    f.Amount.Value                                   // AmountFundedByYou (decimal)
                ))
                .ToListAsync(ct);


            return ApiResponse<List<LenderFundingDto>>.SuccessResult(fundings);
        }
    }
}

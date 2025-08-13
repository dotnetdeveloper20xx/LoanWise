// Application/Features/Fundings/Commands/FundLoan/FundLoanCommandHandler.cs
using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.DTOs.Fundings;
using LoanWise.Domain.Entities;
using LoanWise.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Fundings.Commands.FundLoan
{
    public sealed class FundLoanCommandHandler
        : IRequestHandler<FundLoanCommand, ApiResponse<FundingResultDto>>
    {
        private readonly IApplicationDbContext _db;
        private readonly IUserContext _currentUser;

        public FundLoanCommandHandler(IApplicationDbContext db, IUserContext currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse<FundingResultDto>> Handle(FundLoanCommand cmd, CancellationToken ct)
        {
            // 🔧 FIX: handle nullable Guid from the current user
            var lenderIdOpt = _currentUser.UserId; // Guid?
            if (!lenderIdOpt.HasValue || lenderIdOpt.Value == Guid.Empty)
                return ApiResponse<FundingResultDto>.FailureResult("Not authenticated as a lender.");

            var lenderId = lenderIdOpt.Value; // non-null Guid from here on

            // Load loan + fundings
            var loan = await _db.Loans
                .Include(l => l.Fundings)
                .FirstOrDefaultAsync(l => l.Id == cmd.LoanId, ct);

            if (loan is null)
                return ApiResponse<FundingResultDto>.FailureResult("Loan not found.");

            if (loan.BorrowerId == lenderId)
                return ApiResponse<FundingResultDto>.FailureResult("Lenders cannot fund their own loans.");

            if (loan.Status == LoanStatus.Rejected)
                return ApiResponse<FundingResultDto>.FailureResult("Cannot fund a rejected loan.");

            var contributed = loan.Fundings.Sum(f => f.Amount);
            var remainingBefore = Math.Max(loan.Amount - contributed, 0m);
            if (remainingBefore <= 0m)
                return ApiResponse<FundingResultDto>.FailureResult("Loan is already fully funded.");

            var applied = Math.Min(cmd.Amount, remainingBefore);

            var funding = new Funding(
                id: Guid.NewGuid(),
                lenderId: lenderId,    
                loanId: loan.Id,
                amount: applied,
                fundedOn: DateTime.UtcNow
            );

            loan.AddFunding(funding);
            loan.UpdateFundingStatus(funding);

            await _db.Fundings.AddAsync(funding, ct);
            await _db.SaveChangesAsync(ct);

            var remainingAfter = Math.Max(loan.Amount - (contributed + applied), 0m);
            var fullyFunded = loan.IsFullyFunded();

            var dto = new FundingResultDto(
                LoanId: loan.Id,
                FundingId: funding.Id,
                LenderId: lenderId,             
                requestedAmount: cmd.Amount,
                appliedAmount: applied,
                remainingBefore: remainingBefore,
                remainingAfter: remainingAfter,
                fullyFunded: fullyFunded
            );

            var msg = applied == cmd.Amount
                ? "Funding recorded."
                : $"Funding recorded (auto-adjusted to remaining: {applied:0.00}).";

            return ApiResponse<FundingResultDto>.SuccessResult(dto, msg);
        }


    }
}

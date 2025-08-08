using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using LoanWise.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Fundings.Commands.FundLoan
{
    /// <summary>
    /// Handles funding logic for a loan by a lender.
    /// </summary>
    public class FundLoanCommandHandler : IRequestHandler<FundLoanCommand, ApiResponse<Guid>>
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IFundingRepository _fundingRepository;
        private readonly ILogger<FundLoanCommandHandler> _logger;
        private readonly IUserContext _userContext;

        public FundLoanCommandHandler(
            ILoanRepository loanRepository,
            IFundingRepository fundingRepository,
            ILogger<FundLoanCommandHandler> logger,
            IUserContext userContext)
        {
            _loanRepository = loanRepository;
            _fundingRepository = fundingRepository;
            _logger = logger;
            _userContext = userContext;
        }

        public async Task<ApiResponse<Guid>> Handle(FundLoanCommand request, CancellationToken cancellationToken)
        {
            if (!_userContext.UserId.HasValue)
                return ApiResponse<Guid>.FailureResult("Unauthorized: missing user ID");

            var lenderId = _userContext.UserId.Value;


            var loan = await _loanRepository.GetByIdAsync(request.LoanId, cancellationToken);

            if (loan == null)
            {
                _logger.LogWarning("Loan {LoanId} not found", request.LoanId);
                return ApiResponse<Guid>.FailureResult("Loan not found.");
            }

            // Calculate total already funded
            var currentFunded = loan.Fundings.Sum(f => f.Amount.Value);
            var remaining = loan.Amount.Value - currentFunded;

            if (request.Amount <= 0)
                return ApiResponse<Guid>.FailureResult("Funding amount must be greater than zero.");

            if (request.Amount > remaining)
                return ApiResponse<Guid>.FailureResult("Funding exceeds remaining loan amount.");

            var funding = new Funding(
                id: Guid.NewGuid(),
                loanId: request.LoanId,
                lenderId: lenderId,
                amount: new Money(request.Amount),
                fundedOn: DateTime.UtcNow
            );

            loan.AddFunding(funding);
            loan.UpdateFundingStatus();

            await _fundingRepository.AddAsync(funding, cancellationToken);
            await _loanRepository.UpdateAsync(loan, cancellationToken);

            _logger.LogInformation(
                "Loan {LoanId} funded by lender {LenderId} with {Amount}. Loan status: {Status}",
                request.LoanId, lenderId, request.Amount, loan.Status
            );

            return ApiResponse<Guid>.SuccessResult(funding.Id, "Funding recorded successfully.");
        }
    }
}

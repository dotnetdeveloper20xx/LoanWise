using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using LoanWise.Domain.ValueObjects;
using LoanWise.Domain.Events;
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
        private readonly IMediator _mediator;

        public FundLoanCommandHandler(
            ILoanRepository loanRepository,
            IFundingRepository fundingRepository,
            ILogger<FundLoanCommandHandler> logger,
            IUserContext userContext,
            IMediator mediator)
        {
            _loanRepository = loanRepository;
            _fundingRepository = fundingRepository;
            _logger = logger;
            _userContext = userContext;
            _mediator = mediator;
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

            var currentFunded = loan.Fundings.Sum(f => f.Amount);
            var remaining = loan.Amount - currentFunded;

            if (request.Amount <= 0)
                return ApiResponse<Guid>.FailureResult("Funding amount must be greater than zero.");

            if (request.Amount > remaining)
                return ApiResponse<Guid>.FailureResult("Funding exceeds remaining loan amount.");

            var funding = new Funding(
                id: Guid.NewGuid(),
                loanId: request.LoanId,
                lenderId: lenderId,
                amount: request.Amount,
                fundedOn: DateTime.UtcNow
            );

            loan.AddFunding(funding);
            loan.UpdateFundingStatus(funding); // may flip to Funded

            await _fundingRepository.AddAsync(funding, cancellationToken);
            await _loanRepository.UpdateAsync(loan, cancellationToken);

            _logger.LogInformation(
                "Loan {LoanId} funded by lender {LenderId} with {Amount}. Loan status: {Status}",
                request.LoanId, lenderId, request.Amount, loan.Status
            );

            // Publish domain event (after persistence)
            var isFullyFunded = loan.IsFullyFunded(); // or loan.Status == LoanStatus.Funded
            await _mediator.Publish(new LoanFundedEvent(
                loan.Id,
                funding.Id,
                lenderId,
                request.Amount,
                isFullyFunded
            ), cancellationToken);

            return ApiResponse<Guid>.SuccessResult(funding.Id, "Funding recorded successfully.");
        }
    }
}

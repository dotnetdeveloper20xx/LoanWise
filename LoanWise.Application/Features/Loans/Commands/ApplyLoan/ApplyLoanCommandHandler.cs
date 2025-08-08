using LoanWise.Application.Common.Interfaces;

using LoanWise.Domain.Entities;
using LoanWise.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Loans.Commands.ApplyLoan
{
    /// <summary>
    /// Handles ApplyLoanCommand and persists the new loan.
    /// </summary>
    public class ApplyLoanCommandHandler : IRequestHandler<ApplyLoanCommand, ApiResponse<Guid>>
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IUserContext _userContext;
        private readonly ILogger<ApplyLoanCommandHandler> _logger;

        public ApplyLoanCommandHandler(
            ILoanRepository loanRepository,
            ILogger<ApplyLoanCommandHandler> logger,
            IUserContext userContext)
        {
            _loanRepository = loanRepository;
            _logger = logger;
            _userContext = userContext;
        }

        public async Task<ApiResponse<Guid>> Handle(ApplyLoanCommand request, CancellationToken cancellationToken)
        {

            if (!_userContext.UserId.HasValue)
                return ApiResponse<Guid>.FailureResult("Unauthorized: missing user ID");

            var borrowerId = _userContext.UserId.Value;

            try
            {
                var loan = new Loan(
                    id: Guid.NewGuid(),
                    borrowerId: borrowerId,
                    amount: new Money(request.Amount),
                    durationInMonths: request.DurationInMonths,
                    purpose: request.Purpose
                );

                await _loanRepository.AddAsync(loan, cancellationToken);

                _logger.LogInformation("New loan application created: {LoanId}", loan.Id);

                return ApiResponse<Guid>.SuccessResult(loan.Id, "Loan application submitted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying for loan");
                return ApiResponse<Guid>.FailureResult("An error occurred while processing your loan application.");
            }
        }
    }
}

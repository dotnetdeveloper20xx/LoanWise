using LoanWise.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Loans.Commands.DisburseLoan
{
    /// <summary>
    /// Handles disbursement of a fully funded loan.
    /// </summary>
    public class DisburseLoanCommandHandler : IRequestHandler<DisburseLoanCommand, ApiResponse<Guid>>
    {
        private readonly ILoanRepository _loanRepository;
        private readonly ILogger<DisburseLoanCommandHandler> _logger;

        public DisburseLoanCommandHandler(
            ILoanRepository loanRepository,
            ILogger<DisburseLoanCommandHandler> logger)
        {
            _loanRepository = loanRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<Guid>> Handle(DisburseLoanCommand request, CancellationToken cancellationToken)
        {
            var loan = await _loanRepository.GetByIdAsync(request.LoanId, cancellationToken);

            if (loan == null)
            {
                _logger.LogWarning("Disbursement failed: Loan {LoanId} not found", request.LoanId);
                return ApiResponse<Guid>.FailureResult("Loan not found.");
            }

            try
            {
                loan.Disburse();

                await _loanRepository.UpdateAsync(loan, cancellationToken);

                _logger.LogInformation("Loan {LoanId} marked as Disbursed", loan.Id);
                return ApiResponse<Guid>.SuccessResult(loan.Id, "Loan disbursed successfully.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Disbursement rejected for loan {LoanId}: {Message}", loan.Id, ex.Message);
                return ApiResponse<Guid>.FailureResult(ex.Message);
            }
        }
    }
}

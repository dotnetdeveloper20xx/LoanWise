using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Loans.Commands.DisburseLoan
{
    /// <summary>
    /// Handles disbursement of a fully funded loan and generates repayment schedule.
    /// </summary>
    public class DisburseLoanCommandHandler : IRequestHandler<DisburseLoanCommand, ApiResponse<Guid>>
    {
        private readonly ILoanRepository _loanRepository;
        private readonly ILogger<DisburseLoanCommandHandler> _logger;
        private readonly IMediator _mediator;

        public DisburseLoanCommandHandler(
            ILoanRepository loanRepository,
            ILogger<DisburseLoanCommandHandler> logger,
            IMediator mediator)
        {
            _loanRepository = loanRepository;
            _logger = logger;
            _mediator = mediator;
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
                loan.GenerateRepaymentSchedule(); // Generate monthly repayment schedule

                await _loanRepository.UpdateAsync(loan, cancellationToken);

                _logger.LogInformation("Loan {LoanId} disbursed and repayment schedule generated.", loan.Id);

                // Publish domain event
                await _mediator.Publish(new LoanDisbursedEvent(
                    loan.Id,
                    DateTime.UtcNow
                ), cancellationToken);

                return ApiResponse<Guid>.SuccessResult(loan.Id, "Loan disbursed successfully with repayment schedule.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Disbursement rejected for loan {LoanId}: {Message}", loan.Id, ex.Message);
                return ApiResponse<Guid>.FailureResult(ex.Message);
            }
        }
    }
}

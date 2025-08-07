using LoanWise.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Repayments.Commands.MakeRepayment
{
    /// <summary>
    /// Handles marking a repayment as paid.
    /// </summary>
    public class MakeRepaymentCommandHandler : IRequestHandler<MakeRepaymentCommand, ApiResponse<Guid>>
    {
        private readonly ILoanRepository _loanRepository;
        private readonly ILogger<MakeRepaymentCommandHandler> _logger;

        public MakeRepaymentCommandHandler(ILoanRepository loanRepository, ILogger<MakeRepaymentCommandHandler> logger)
        {
            _loanRepository = loanRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<Guid>> Handle(MakeRepaymentCommand request, CancellationToken cancellationToken)
        {
            // Load all loans with repayments to find this one
            var allLoans = await _loanRepository.GetLoansWithRepaymentsAsync(cancellationToken);
            var loan = allLoans.FirstOrDefault(l => l.Repayments.Any(r => r.Id == request.RepaymentId));

            if (loan == null)
            {
                _logger.LogWarning("Repayment {RepaymentId} not found in any loan", request.RepaymentId);
                return ApiResponse<Guid>.FailureResult("Repayment not found.");
            }

            var repayment = loan.Repayments.First(r => r.Id == request.RepaymentId);

            if (repayment.IsPaid)
            {
                return ApiResponse<Guid>.FailureResult("This repayment is already marked as paid.");
            }

            repayment.MarkAsPaid(DateTime.UtcNow);

            await _loanRepository.UpdateAsync(loan, cancellationToken);

            _logger.LogInformation("Repayment {RepaymentId} marked as paid", repayment.Id);

            return ApiResponse<Guid>.SuccessResult(repayment.Id, "Repayment successful.");
        }
    }
}

using LoanWise.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Repayments.Commands.CheckOverdueRepayments
{
    public class CheckOverdueRepaymentsCommandHandler : IRequestHandler<CheckOverdueRepaymentsCommand, ApiResponse<string>>
    {
        private readonly ILoanRepository _loanRepository;
        private readonly ILogger<CheckOverdueRepaymentsCommandHandler> _logger;

        public CheckOverdueRepaymentsCommandHandler(ILoanRepository loanRepository, ILogger<CheckOverdueRepaymentsCommandHandler> logger)
        {
            _loanRepository = loanRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(CheckOverdueRepaymentsCommand request, CancellationToken cancellationToken)
        {
            var loans = await _loanRepository.GetLoansWithRepaymentsAsync(cancellationToken);

            foreach (var loan in loans)
            {
                loan.MarkOverdueRepayments(DateTime.UtcNow);
                await _loanRepository.UpdateAsync(loan, cancellationToken);
            }

            _logger.LogInformation("Overdue repayments check complete for {Count} loans", loans.Count());

            return ApiResponse<string>.SuccessResult("Overdue repayments checked and updated.");
        }
    }
}

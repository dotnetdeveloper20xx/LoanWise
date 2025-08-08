using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Enums;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Admin.Commands.ApproveLoan
{
    public class ApproveLoanCommandHandler : IRequestHandler<ApproveLoanCommand, ApiResponse<Guid>>
    {
        private readonly ILoanRepository _loanRepository;

        public ApproveLoanCommandHandler(ILoanRepository loanRepository)
        {
            _loanRepository = loanRepository;
        }

        public async Task<ApiResponse<Guid>> Handle(ApproveLoanCommand request, CancellationToken cancellationToken)
        {
            var loan = await _loanRepository.GetByIdAsync(request.LoanId, cancellationToken);
            if (loan is null)
                return ApiResponse<Guid>.FailureResult("Loan not found.");

            // ensure domain rule: only Pending can move to Approved
            if (loan.Status != LoanStatus.Pending)
                return ApiResponse<Guid>.FailureResult("Only pending loans can be approved.");

            // Domain change: prefer a domain method if available (loan.Approve())
            // Otherwise set status directly if allowed
            loan.SetApproved(); // <-- add this domain method; see note below

            await _loanRepository.UpdateAsync(loan, cancellationToken);
            await _loanRepository.SaveChangesAsync(cancellationToken);

            return ApiResponse<Guid>.SuccessResult(loan.Id, "Loan approved.");
        }
    }
}


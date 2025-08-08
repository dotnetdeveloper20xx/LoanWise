using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Enums;
using MediatR;
using StoreBoost.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanWise.Application.Features.Admin.Commands.RejectLoan
{
    public class RejectLoanCommandHandler : IRequestHandler<RejectLoanCommand, ApiResponse<Guid>>
    {
        private readonly ILoanRepository _loanRepository;

        public RejectLoanCommandHandler(ILoanRepository loanRepository)
        {
            _loanRepository = loanRepository;
        }

        public async Task<ApiResponse<Guid>> Handle(RejectLoanCommand request, CancellationToken cancellationToken)
        {
            var loan = await _loanRepository.GetByIdAsync(request.LoanId, cancellationToken);
            if (loan is null)
                return ApiResponse<Guid>.FailureResult("Loan not found.");

            if (loan.Status != LoanStatus.Pending)
                return ApiResponse<Guid>.FailureResult("Only pending loans can be rejected.");

            loan.SetRejected(request.Reason); // <-- add this domain method; see note below

            await _loanRepository.UpdateAsync(loan, cancellationToken);
            await _loanRepository.SaveChangesAsync(cancellationToken);

            return ApiResponse<Guid>.SuccessResult(loan.Id, "Loan rejected.");
        }
    }
}

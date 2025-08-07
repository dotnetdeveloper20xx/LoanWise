
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Loans.Commands.DisburseLoan
{
    /// <summary>
    /// Command issued by an admin to disburse a fully funded loan.
    /// </summary>
    public class DisburseLoanCommand : IRequest<ApiResponse<Guid>>
    {
        public Guid LoanId { get; }

        public DisburseLoanCommand(Guid loanId)
        {
            LoanId = loanId;
        }
    }
}

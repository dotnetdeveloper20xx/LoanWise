
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Fundings.Commands.FundLoan
{
    /// <summary>
    /// Command issued by a lender to fund a loan.
    /// </summary>
    public class FundLoanCommand : IRequest<ApiResponse<Guid>>
    {
        public Guid LoanId { get; set; }       
        public decimal Amount { get; set; }

        public FundLoanCommand(Guid loanId, decimal amount)
        {
            LoanId = loanId;           
            Amount = amount;
        }
    }
}

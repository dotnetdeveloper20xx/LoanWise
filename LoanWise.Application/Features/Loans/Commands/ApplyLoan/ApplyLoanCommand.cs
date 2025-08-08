
using LoanWise.Domain.Enums;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Loans.Commands.ApplyLoan
{
    /// <summary>
    /// Command issued by a borrower to apply for a new loan.
    /// </summary>
    public class ApplyLoanCommand : IRequest<ApiResponse<Guid>>
    {       
        public decimal Amount { get; set; }
        public int DurationInMonths { get; set; }
        public LoanPurpose Purpose { get; set; }
        public string? Description { get; set; }
        public decimal? MonthlyIncome { get; set; }
    }
}

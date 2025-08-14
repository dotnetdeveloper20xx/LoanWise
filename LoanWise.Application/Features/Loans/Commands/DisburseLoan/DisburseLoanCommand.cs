
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Loans.Commands.DisburseLoan
{
    /// <summary>
    /// Command issued by an admin to disburse a fully funded loan.
    /// </summary>
    public sealed record DisburseLoanCommand(Guid LoanId) : IRequest<ApiResponse<Guid>>;
}

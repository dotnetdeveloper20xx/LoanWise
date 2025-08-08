using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Admin.Commands.RejectLoan
{
    public record RejectLoanCommand(Guid LoanId, string? Reason) : IRequest<ApiResponse<Guid>>;

}

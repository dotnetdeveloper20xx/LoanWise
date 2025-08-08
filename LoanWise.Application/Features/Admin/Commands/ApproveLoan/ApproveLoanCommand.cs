using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Admin.Commands.ApproveLoan
{
    public record ApproveLoanCommand(Guid LoanId) : IRequest<ApiResponse<Guid>>;

}

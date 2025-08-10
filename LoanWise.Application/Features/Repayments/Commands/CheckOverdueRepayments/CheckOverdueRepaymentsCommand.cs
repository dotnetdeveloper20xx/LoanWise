
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Repayments.Commands.CheckOverdueRepayments
{
    public sealed record CheckOverdueRepaymentsCommand : IRequest<ApiResponse<int>>;

}

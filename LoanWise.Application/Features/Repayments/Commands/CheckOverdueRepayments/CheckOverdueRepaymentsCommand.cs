
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Repayments.Commands.CheckOverdueRepayments
{
    public class CheckOverdueRepaymentsCommand : IRequest<ApiResponse<string>> { }
}

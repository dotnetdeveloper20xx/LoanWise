
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Repayments.Commands.MakeRepayment
{
    /// <summary>
    /// Command to mark a repayment as paid.
    /// </summary>
    public class MakeRepaymentCommand : IRequest<ApiResponse<Guid>>
    {
        public Guid RepaymentId { get; }

        public MakeRepaymentCommand(Guid repaymentId)
        {
            RepaymentId = repaymentId;
        }
    }
}

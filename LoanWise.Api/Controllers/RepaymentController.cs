
using LoanWise.Application.Features.Repayments.Commands.MakeRepayment;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LoanWise.Api.Controllers
{
    [ApiController]
    [Route("api/repayments")]
    public class RepaymentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RepaymentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Marks a repayment as paid by repayment ID.
        /// </summary>
        /// <param name="repaymentId">The ID of the repayment to mark as paid.</param>
        /// <returns>The repayment ID if successful.</returns>
        /// <response code="200">Repayment marked as paid.</response>
        /// <response code="400">Repayment not found or already paid.</response>
        [HttpPost("{repaymentId}/pay")]
        public async Task<IActionResult> MakeRepayment(Guid repaymentId)
        {
            var result = await _mediator.Send(new MakeRepaymentCommand(repaymentId));
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}

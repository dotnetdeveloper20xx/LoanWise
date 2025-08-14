// --------------------------------------------------------------------------------------
// LoanWise.Api/Controllers/RepaymentController.cs
// Author: Faz Ahmed
// Purpose: Borrower endpoints for making/acknowledging repayments.
// Notes:
//  - Thin controller; business rules & ownership checks live in the MakeRepaymentCommand handler.
//  - Unified ApiResponse<T> contract; explicit response types for Swagger.
//  - Cancellation tokens propagated to handlers.
// --------------------------------------------------------------------------------------

using LoanWise.Application.Features.Repayments.Commands.MakeRepayment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Api.Controllers
{
    [ApiController]
    [Route("api/repayments")]
    [Authorize(Roles = "Borrower")] // Borrowers only; handler should verify ownership of repayment
    [Produces("application/json")]
    [Tags("Repayments")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public sealed class RepaymentController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RepaymentController> _logger;

        public RepaymentController(IMediator mediator, ILogger<RepaymentController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("{repaymentId:guid}/pay")]
        [Authorize(Roles = "Borrower,Admin")]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> MakeRepayment([FromRoute] Guid repaymentId, CancellationToken ct = default)
        {
            if (repaymentId == Guid.Empty)
                return BadRequest(ApiResponse<Guid>.FailureResult("Repayment ID cannot be empty."));

            var result = await _mediator.Send(new MakeRepaymentCommand(repaymentId), ct);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}

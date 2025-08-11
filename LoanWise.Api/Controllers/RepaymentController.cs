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
    [ApiExplorerSettings(GroupName = "Repayments")]
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

        /// <summary>
        /// (Authored by Faz Ahmed) Marks a repayment as paid by repayment ID.
        /// </summary>
        /// <param name="repaymentId">The repayment ID to mark as paid.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>ApiResponse with the repayment ID on success.</returns>
        /// <response code="200">Repayment marked as paid.</response>
        /// <response code="400">Validation/business rule failure (e.g., not found, already paid).</response>
        [HttpPost("{repaymentId:guid}/pay")]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> MakeRepayment([FromRoute] Guid repaymentId, CancellationToken ct = default)
        {
            if (repaymentId == Guid.Empty)
            {
                var bad = ApiResponse<Guid>.FailureResult("Repayment ID cannot be empty.");
                return BadRequest(bad);
            }

            _logger.LogInformation("Borrower requested to mark repayment {RepaymentId} as paid.", repaymentId);

            var result = await _mediator.Send(new MakeRepaymentCommand(repaymentId), ct);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}

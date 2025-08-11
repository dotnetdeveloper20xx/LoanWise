// ------------------------------------------------------------------------------------------------------
// LoanWise.Api/Controllers/FundingController.cs
// Author: Faz Ahmed
// Description: Lender-facing endpoints for funding loans and viewing a lender's funding history.
// Notes:
//  - Uses MediatR (CQRS) to delegate to application commands/queries.
//  - All responses are wrapped in ApiResponse<T> for consistency.
//  - Secured via role-based authorization (Lender).
// ------------------------------------------------------------------------------------------------------

using LoanWise.Application.Features.Fundings.Commands.FundLoan;
using LoanWise.Application.Features.Fundings.DTOs;
using LoanWise.Application.Features.Fundings.Queries.GetFundingsByLender;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Api.Controllers;

[ApiController]
[Route("api/fundings")]
[Authorize(Roles = "Lender")] // Only authenticated lenders can access this controller
public sealed class FundingController : ControllerBase
{
    private readonly IMediator _mediator;

    public FundingController(IMediator mediator)
        => _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    /// <summary>
    /// (Authored by Faz Ahmed) Create or add to a funding contribution for the specified loan.
    /// </summary>
    /// <param name="loanId">The ID of the loan to fund.</param>
    /// <param name="request">Funding payload (e.g., amount).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>
    /// Business rules (e.g., preventing overfunding, ensuring loan status) are enforced in the <see cref="FundLoanCommand"/> handler.
    /// </remarks>
    /// <response code="200">Funding recorded successfully.</response>
    /// <response code="400">Validation or business rule failure.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden (role mismatch).</response>
    [HttpPost("{loanId:guid}")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<Guid>>> FundLoan(
        Guid loanId,
        [FromBody] FundLoanDto request,
        CancellationToken ct)
    {
        // Optional light guard; deep validation happens in FluentValidation + handler.
        if (loanId == Guid.Empty)
        {
            var bad = ApiResponse<Guid>.FailureResult("LoanId cannot be empty.");
            return BadRequest(bad);
        }

        var command = new FundLoanCommand(loanId, request.Amount);
        var result = await _mediator.Send(command, ct);

        // Keep envelope shape consistent and status semantics predictable.
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// (Authored by Faz Ahmed) Retrieve the authenticated lender's funding contributions grouped by loan.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Returns the lender's funded loans.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden (role mismatch).</response>
    [HttpGet("my")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiResponse<List<LenderFundingDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<LenderFundingDto>>>> GetMyFundings(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetFundingsByLenderQuery(), ct);
        return Ok(result); // Handlers return ApiResponse<T> with consistent envelope.
    }
}

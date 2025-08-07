
using LoanWise.Application.Features.Fundings.Commands.FundLoan;
using LoanWise.Application.Features.Fundings.DTOs;
using LoanWise.Application.Features.Fundings.Queries.GetFundingsByLender;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Api.Controllers
{
    [ApiController]
    [Route("api/fundings")]
    public class FundingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FundingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Allows a lender to fund a loan.
        /// </summary>
        [HttpPost("{loanId}")]
        public async Task<IActionResult> FundLoan(Guid loanId, [FromBody] FundLoanDto request)
        {
            var command = new FundLoanCommand(loanId, request.LenderId, request.Amount);
            ApiResponse<Guid> result = await _mediator.Send(command);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Retrieves all loans funded by the specified lender.
        /// </summary>
        /// <param name="lenderId">The lender's user ID.</param>
        /// <returns>A list of fundings grouped by loan.</returns>
        /// <response code="200">Returns lender's funded loans</response>
        /// <response code="400">If lenderId is invalid</response>
        [HttpGet("my")]
        public async Task<IActionResult> GetMyFundings([FromQuery] Guid lenderId)
        {
            var result = await _mediator.Send(new GetFundingsByLenderQuery(lenderId));
            return result.Success ? Ok(result) : BadRequest(result);
        }

    }
}

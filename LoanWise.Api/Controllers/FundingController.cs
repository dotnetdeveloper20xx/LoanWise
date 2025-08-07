
using LoanWise.Application.Features.Fundings.Commands.FundLoan;
using LoanWise.Application.Features.Fundings.DTOs;
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
    }
}

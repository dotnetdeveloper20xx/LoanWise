
using LoanWise.Application.Features.Loans.Commands.ApplyLoan;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Api.Controllers
{
    [ApiController]
    [Route("api/loans")]
    public class LoanController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LoanController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Submits a loan application by a borrower.
        /// </summary>
        [HttpPost("apply")]
        public async Task<IActionResult> ApplyLoan([FromBody] ApplyLoanCommand command)
        {
            ApiResponse<Guid> result = await _mediator.Send(command);

            return result.Success
                ? Ok(result)
                : BadRequest(result);
        }
    }
}


using LoanWise.Application.Features.Dashboard.Queries.GetLenderPortfolio;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LoanWise.Api.Controllers
{
    [ApiController]
    [Route("api/lenders")]
    public class LenderController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LenderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Returns a summary of the lender’s investment portfolio.
        /// </summary>
        /// <param name="lenderId">The lender's user ID.</param>
        /// <returns>A summary including total funded and number of loans funded.</returns>
        /// <response code="200">Portfolio data found</response>
        /// <response code="400">Invalid lender ID</response>
        [HttpGet("{lenderId}/portfolio")]
        public async Task<IActionResult> GetLenderPortfolio(Guid lenderId)
        {
            var response = await _mediator.Send(new GetLenderPortfolioSummaryQuery(lenderId));
            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}

using LoanWise.Application.Features.Dashboard.Queries.GetLenderPortfolio;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoanWise.Api.Controllers
{
    [Authorize(Roles = "Lender")] // Only authenticated lenders can access
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
        /// <returns>A summary including total funded and number of loans funded.</returns>
        /// <response code="200">Portfolio data found</response>
        /// <response code="400">Invalid or missing user ID</response>
        [HttpGet("portfolio")]
        public async Task<IActionResult> GetLenderPortfolio()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var lenderId))
                return BadRequest("Invalid or missing user ID in token.");

            var response = await _mediator.Send(new GetLenderPortfolioSummaryQuery(lenderId));
            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}

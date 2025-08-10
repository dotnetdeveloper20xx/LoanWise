using LoanWise.Application.DTOs.Lenders;
using LoanWise.Application.Features.Dashboard.Queries.GetLenderPortfolio;
using LoanWise.Application.Features.Lenders.Queries.GetTransactions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreBoost.Application.Common.Models;
using System.Security.Claims;

namespace LoanWise.Api.Controllers
{
    [Authorize(Roles = "Lender,Admin")] // Only authenticated lenders can access
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

            var response = await _mediator.Send(new GetLenderPortfolioSummaryQuery());
            return response.Success ? Ok(response) : BadRequest(response);
        }


        // GET /api/lenders/transactions?from=&to=&loanId=&borrowerId=&page=&pageSize=
        [HttpGet("transactions")]
        public async Task<ActionResult<ApiResponse<LenderTransactionQueryResult>>> GetTransactions(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] Guid? loanId,
            [FromQuery] Guid? borrowerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken ct = default)
        {
            var query = new GetLenderTransactionsQuery(
                LenderId: null,  // null => use current user from IUserContext
                FromUtc: from,
                ToUtc: to,
                LoanId: loanId,
                BorrowerId: borrowerId,
                Page: page,
                PageSize: pageSize);

            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
    }
}

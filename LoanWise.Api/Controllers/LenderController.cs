// LoanWise.Api/Controllers/LenderController.cs
// Author: Faz Ahmed — Refactor & documentation by Faz Ahmed to improve clarity, safety, and consistency.

using LoanWise.Application.DTOs.Lenders;
using LoanWise.Application.Features.Dashboard.Queries.GetLenderPortfolio;
using LoanWise.Application.Features.Lenders.Queries.GetTransactions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Api.Controllers
{
    /// <summary>
    /// Lender endpoints for portfolio insights and transaction history.
    /// </summary>
    [Authorize(Roles = "Lender,Admin")] // Enforced per platform guidance.
    [ApiController]
    [Route("api/lenders")]
    public sealed class LenderController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LenderController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Returns a summary of the current lender’s investment portfolio.
        /// </summary>
        /// <remarks>
        /// The underlying query infers the current user from the request context (no explicit ID needed).
        /// </remarks>
        /// <response code="200">Portfolio data returned in ApiResponse.</response>
        /// <response code="400">Request failed; see ApiResponse for details.</response>
        [HttpGet("portfolio")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetLenderPortfolio(CancellationToken ct)
        {
            var response = await _mediator.Send(new GetLenderPortfolioSummaryQuery(), ct);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Gets the current lender’s transactions with optional filters.
        /// </summary>
        /// <param name="from">Start date (UTC). If null, no lower bound.</param>
        /// <param name="to">End date (UTC). If null, no upper bound.</param>
        /// <param name="loanId">Filter by loan.</param>
        /// <param name="borrowerId">Filter by borrower.</param>
        /// <param name="page">Page number (1+).</param>
        /// <param name="pageSize">Page size (1–100).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Paginated transaction results in a standard ApiResponse wrapper.</returns>
        /// <response code="200">Transactions returned.</response>
        [HttpGet("transactions")]
        [ProducesResponseType(typeof(ApiResponse<LenderTransactionQueryResult>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<LenderTransactionQueryResult>>> GetTransactions(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] Guid? loanId,
            [FromQuery] Guid? borrowerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken ct = default)
        {
            // Basic guardrails — avoid extreme/invalid paging values.
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            // Convention: LenderId = null → handler uses current user from IUserContext.
            var query = new GetLenderTransactionsQuery(
                LenderId: null,
                FromUtc: from,
                ToUtc: to,
                LoanId: loanId,
                BorrowerId: borrowerId,
                Page: page,
                PageSize: pageSize
            );

            var result = await _mediator.Send(query, ct);
            return Ok(result); // Handlers already return ApiResponse<T>; preserve shape.
        }
    }
}

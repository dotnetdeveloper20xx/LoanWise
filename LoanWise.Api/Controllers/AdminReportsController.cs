// --------------------------------------------------------------------------------------
// LoanWise.Api - AdminReportsController
// Author: Faz Ahmed
// Purpose: Admin-only reporting endpoints (loans, repayments, fundings).
// Notes:
//  - Clean Architecture + CQRS (MediatR) aligned.
//  - Returns unified ApiResponse<T> envelopes.
//  - Designed for easy Swagger discovery and observability.
// --------------------------------------------------------------------------------------

using LoanWise.Application.DTOs.Reports;
using LoanWise.Application.Features.Admin.Queries.GetFundingReport;
using LoanWise.Application.Features.Admin.Queries.GetLoanReport;
using LoanWise.Application.Features.Admin.Queries.GetRepaymentReport;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Api.Controllers
{
    [ApiController]
    [Route("api/admin/reports")]
    [Authorize(Roles = "Admin")]
    [Produces("application/json")]
    [ApiExplorerSettings(GroupName = "Admin")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)] // Reports are sensitive; don't cache by default
    public sealed class AdminReportsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AdminReportsController> _logger;

        public AdminReportsController(IMediator mediator, ILogger<AdminReportsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Returns an aggregated admin loan report.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns><see cref="ApiResponse{T}"/> containing a list of <see cref="LoanReportDto"/>.</returns>
        [HttpGet("loans")]
        [ProducesResponseType(typeof(ApiResponse<List<LoanReportDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<LoanReportDto>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetLoansReport(CancellationToken ct = default)
        {
            _logger.LogInformation("Admin {Admin} requested Loans report.", User?.Identity?.Name);

            var result = await _mediator.Send(new GetLoanReportQuery(), ct);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Returns an aggregated admin repayment report.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns><see cref="ApiResponse{T}"/> containing a list of <see cref="RepaymentReportDto"/>.</returns>
        [HttpGet("repayments")]
        [ProducesResponseType(typeof(ApiResponse<List<RepaymentReportDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<RepaymentReportDto>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetRepaymentsReport(CancellationToken ct = default)
        {
            _logger.LogInformation("Admin {Admin} requested Repayments report.", User?.Identity?.Name);

            var result = await _mediator.Send(new GetRepaymentReportQuery(), ct);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Returns an aggregated admin funding report.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns><see cref="ApiResponse{T}"/> containing a list of <see cref="FundingReportDto"/>.</returns>
        [HttpGet("fundings")]
        [ProducesResponseType(typeof(ApiResponse<List<FundingReportDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<FundingReportDto>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetFundingsReport(CancellationToken ct = default)
        {
            _logger.LogInformation("Admin {Admin} requested Fundings report.", User?.Identity?.Name);

            var result = await _mediator.Send(new GetFundingReportQuery(), ct);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}

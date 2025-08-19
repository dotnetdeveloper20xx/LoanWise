// LoanWise.Api/Controllers/LoanController.cs
// Refactored by Faz Ahmed — August 2025
//
// This controller is intentionally thin: all business logic is handled via
// MediatR commands/queries in the Application layer, with cross-cutting concerns
// (validation, logging, performance) enforced by pipeline behaviors.
// See: Clean Architecture + CQRS + MediatR design for LoanWise. 

using LoanWise.Application.DTOs.Loans;
using LoanWise.Application.DTOs.Repayments;
using LoanWise.Application.Features.Dashboard.Queries.GetAdminLoanStats;
using LoanWise.Application.Features.Dashboard.Queries.GetBorrowerDashboard;
using LoanWise.Application.Features.Loans.Commands.ApplyLoan;
using LoanWise.Application.Features.Loans.Commands.DisburseLoan;
using LoanWise.Application.Features.Loans.Queries.GetBorrowerLoanHistory;
using LoanWise.Application.Features.Loans.Queries.GetLoansByBorrower;
using LoanWise.Application.Features.Loans.Queries.GetOpenLoans;
using LoanWise.Application.Features.Repayments.Queries.GetRepaymentsByLoanId;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Api.Controllers
{
    /// <summary>
    /// Loan endpoints (Borrower, Lender, Admin).
    /// Thin controller by design — all logic via MediatR handlers.
    /// Author: Faz Ahmed
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/loans")]
    [Produces("application/json")]
    public sealed class LoanController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LoanController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Borrower applies for a new loan.
        /// </summary>
        [HttpPost("apply")]
        [Authorize(Roles = "Borrower")]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ApplyLoan([FromBody] ApplyLoanCommand command, CancellationToken ct)
        {
            // Validation is handled by FluentValidation via MediatR behaviors.
            ApiResponse<Guid> result = await _mediator.Send(command, ct);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lenders can browse open (approved & not fully funded) loans.
        /// </summary>
        [HttpGet("open")]
        [Authorize(Roles = "Lender, Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOpenLoans(CancellationToken ct)
        {
            var response = await _mediator.Send(new GetOpenLoansQuery(), ct);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Borrower gets their own loans.
        /// </summary>
        [HttpGet("my")]
        [Authorize(Roles = "Borrower")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLoansByBorrower(CancellationToken ct)
        {
            var response = await _mediator.Send(new GetLoansByBorrowerQuery(), ct);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Admin disburses a funded loan (generates repayment schedule).
        /// </summary>
        [HttpPost("{loanId:guid}/disburse")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DisburseLoan([FromRoute] Guid loanId, CancellationToken ct)
        {
            var response = await _mediator.Send(new DisburseLoanCommand(loanId), ct);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Returns the repayment schedule for a specific loan.
        /// Borrowers can view their own loans; Admins can view any (handler enforces ownership).
        /// </summary>
        [HttpGet("{loanId:guid}/repayments")]
        [Authorize(Roles = "Borrower,Admin")]
        [ProducesResponseType(typeof(ApiResponse<List<RepaymentDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<RepaymentDto>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetRepaymentsByLoanId([FromRoute] Guid loanId, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetRepaymentsByLoanIdQuery(loanId), ct);
            return result.Success ? Ok(result) : BadRequest(result);
        }




        /// <summary>
        /// Borrower dashboard summary (totals, upcoming repayment, outstanding).
        /// </summary>
        [HttpGet("borrowers/dashboard")]
        [Authorize(Roles = "Borrower")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBorrowerDashboard(CancellationToken ct)
        {
            var response = await _mediator.Send(new GetBorrowerDashboardQuery(), ct);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Admin loan statistics (by status, overdue counts, etc.).
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLoanStats(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAdminLoanStatsQuery(), ct);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Borrower loan history (paginated/filterable via query object).
        /// </summary>
        [HttpGet("borrowers/history")]
        [Authorize(Roles = "Borrower")]
        [ProducesResponseType(typeof(ApiResponse<List<BorrowerLoanHistoryDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBorrowerLoanHistory([FromQuery] GetBorrowerLoanHistoryQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result); // always wrapped in ApiResponse<T>
        }

        // --------------------------------------------------------------------
        // Back-compat route (optional): keep until Postman collections migrate.
        // Produces /api/loans/loans/stats like earlier snapshots.
        // Remove after clients switch to /api/loans/stats.
        // --------------------------------------------------------------------
        [HttpGet("loans/stats")]
        [Obsolete("Use GET /api/loans/stats")]
        [Authorize(Roles = "Admin")]
        public Task<IActionResult> GetLoanStatsBackCompat(CancellationToken ct) => GetLoanStats(ct);
    }
}

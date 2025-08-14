// --------------------------------------------------------------------------------------
// LoanWise.Api - AdminController
// Author: Faz Ahmed
// Purpose: Administrative endpoints for loan approvals, user management, and repayments.
// Notes:
//  - Follows Clean Architecture + CQRS with MediatR.
//  - Returns a unified ApiResponse<T> envelope to keep clients consistent.
//  - Uses explicit request contracts for clarity and Swagger documentation.
//  - Consider moving the nested request DTOs into an Api.Contracts/Admin folder.
// --------------------------------------------------------------------------------------

using LoanWise.Application.DTOs.Users;
using LoanWise.Application.Features.Admin.Commands.ApproveLoan;
using LoanWise.Application.Features.Admin.Commands.RejectLoan;
using LoanWise.Application.Features.Admin.Commands.UpdateUserStatus;
using LoanWise.Application.Features.Admin.Queries.GetUsers;
using LoanWise.Application.Features.Repayments.Commands.CheckOverdueRepayments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace LoanWise.Api.Controllers
{
    [Authorize(Roles = "Admin")] // Admin-only surface area
    [ApiController]
    [Route("api/admin")]
    [Produces("application/json")]   
    [Tags("Admin")]
    public sealed class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IMediator mediator, ILogger<AdminController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        // -------------------------------
        // Requests (local DTO contracts)
        // -------------------------------

        /// <summary>
        /// Request body for rejecting a loan.
        /// </summary>
        public sealed record RejectLoanRequest(
            [property: StringLength(512, ErrorMessage = "Reason must be 512 characters or fewer.")]
            string? Reason
        );

        /// <summary>
        /// Request body for updating the active status of a user account.
        /// </summary>
        public sealed record UpdateUserStatusRequest(
            [property: Required] bool IsActive
        );

        // -------------------------------
        // Loans: Approve / Reject
        // -------------------------------

        /// <summary>
        /// Approves a pending loan.
        /// </summary>
        /// <param name="loanId">Loan identifier.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>ApiResponse with approved loan ID.</returns>
        [HttpPost("loans/{loanId:guid}/approve")]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ApproveLoan(Guid loanId, CancellationToken ct = default)
        {
            _logger.LogInformation("Admin (by {Admin}) approving loan {LoanId}", User?.Identity?.Name, loanId);

            var res = await _mediator.Send(new ApproveLoanCommand(loanId), ct);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        /// <summary>
        /// Rejects a pending loan with an optional reason.
        /// </summary>
        /// <param name="loanId">Loan identifier.</param>
        /// <param name="body">Optional reason for rejection.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>ApiResponse with rejected loan ID.</returns>
        [HttpPost("loans/{loanId:guid}/reject")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RejectLoan(Guid loanId, [FromBody] RejectLoanRequest body, CancellationToken ct = default)
        {
            var reason = body?.Reason?.Trim();
            _logger.LogInformation("Admin (by {Admin}) rejecting loan {LoanId}. Reason provided: {HasReason}", User?.Identity?.Name, loanId, !string.IsNullOrWhiteSpace(reason));

            var res = await _mediator.Send(new RejectLoanCommand(loanId, reason), ct);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        // -------------------------------
        // Repayments: Overdue Check
        // -------------------------------

        /// <summary>
        /// Triggers overdue checks for all repayments across all loans.
        /// </summary>
        [HttpPost("repayments/check-overdue")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CheckOverdueRepayments(CancellationToken ct = default)
        {
            var res = await _mediator.Send(new CheckOverdueRepaymentsCommand(), ct);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        // -------------------------------
        // Users: Query / Status Update
        // -------------------------------

        /// <summary>
        /// Returns a paginated list of users.
        /// </summary>
        [HttpGet("users")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResult<UserListDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query, CancellationToken ct = default)
        {
            _logger.LogDebug("Admin fetching users with query: {@Query}", query);
            var result = await _mediator.Send(query, ct);
            return Ok(result); // Already wrapped in ApiResponse<T>
        }

        /// <summary>
        /// Updates the active status of a user account.
        /// </summary>
        /// <param name="id">User identifier.</param>
        /// <param name="body">Target active flag.</param>
        [HttpPut("users/{id:guid}/status")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusRequest body, CancellationToken ct = default)
        {
            _logger.LogInformation("Admin updating user {UserId} IsActive={IsActive}", id, body.IsActive);

            var result = await _mediator.Send(new UpdateUserStatusCommand(id, body.IsActive), ct);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}

// --------------------------------------------------------------------------------------
// LoanWise.Api - BorrowersRiskController
// Author: Faz Ahmed
// Purpose: Risk & KYC endpoints for borrowers (self), lenders, and admins.
// Notes:
//  - Clean Architecture + CQRS (MediatR).
//  - Enforces borrower ownership: borrowers can only see their own risk summary.
//  - Admin-only KYC verification & borrower listing by KYC status.
//  - Uniform ApiResponse<T> envelopes; explicit Produces/Consumes for Swagger.
// --------------------------------------------------------------------------------------

using LoanWise.Application.DTOs.Admin;
using LoanWise.Application.DTOs.Borrowers;
using LoanWise.Application.Features.Admin.Borrowers.Queries.ListByKycStatus;
using LoanWise.Application.Features.Borrowers.Commands.VerifyKyc;
using LoanWise.Application.Features.Borrowers.Queries.GetRiskSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace LoanWise.Api.Controllers;

[ApiController]
[Route("api")]
[Produces("application/json")]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class BorrowersRiskController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<BorrowersRiskController> _logger;

    public BorrowersRiskController(IMediator mediator, ILogger<BorrowersRiskController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // Helpers
    private Guid? GetCurrentUserId() =>
        Guid.TryParse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value, out var id) ? id : null;

    private bool IsBorrowerOnly() => User.IsInRole("Borrower") && !User.IsInRole("Admin") && !User.IsInRole("Lender");

    /// <summary>
    /// Gets a borrower's risk summary.
    /// </summary>
    /// <param name="borrowerId">Borrower ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>
    /// Roles:
    /// - <b>Borrower</b>: may view only their own summary.
    /// - <b>Lender</b>/<b>Admin</b>: may view any borrower.
    /// </remarks>
    [HttpGet("borrowers/{borrowerId:guid}/risk-summary")]
    [Authorize(Roles = "Borrower,Lender,Admin")]
    [ProducesResponseType(typeof(ApiResponse<BorrowerRiskSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BorrowerRiskSummaryDto>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRiskSummary(Guid borrowerId, CancellationToken ct = default)
    {
        if (IsBorrowerOnly() && GetCurrentUserId() is Guid uid && uid != borrowerId)
        {
            _logger.LogWarning("Borrower {UserId} attempted to access risk summary for {BorrowerId}", uid, borrowerId);
            return Forbid();
        }

        var result = await _mediator.Send(new GetBorrowerRiskSummaryQuery(borrowerId), ct);
        return Ok(result); // mediator returns ApiResponse<T>
    }

    /// <summary>
    /// Triggers verification of a borrower's KYC (Admin only).
    /// </summary>
    /// <param name="borrowerId">Borrower ID.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPost("admin/kyc/{borrowerId:guid}/verify")]
    [Authorize(Roles = "Admin")]
    [ApiExplorerSettings(GroupName = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyKyc(Guid borrowerId, CancellationToken ct = default)
    {
        _logger.LogInformation("Admin {Admin} requested KYC verification for borrower {BorrowerId}", User?.Identity?.Name, borrowerId);

        var result = await _mediator.Send(new VerifyKycCommand(borrowerId), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // -------------------------------
    // Admin: list borrowers by KYC status
    // -------------------------------

    public sealed class ListByKycQuery
    {
        /// <summary>One or more KYC statuses (e.g., Verified,Pending). Accepts CSV or repeated keys.</summary>
        [FromQuery(Name = "status"), Required]
        public string[] Status { get; init; } = Array.Empty<string>();

        /// <summary>Page number (1-based).</summary>
        [Range(1, int.MaxValue)]
        public int Page { get; init; } = 1;

        /// <summary>Page size (1..100).</summary>
        [Range(1, 100)]
        public int PageSize { get; init; } = 25;

        /// <summary>Optional text search (name/email/etc.).</summary>
        public string? Search { get; init; }

        /// <summary>Sort field (score|verifiedAt|name|risk|status|scoreAt|lastScoreAt).</summary>
        public string? SortBy { get; init; }

        /// <summary>Sort direction (asc|desc). Defaults to desc.</summary>
        [RegularExpression("^(?i)(asc|desc)$")]
        public string SortDir { get; init; } = "desc";
    }

    /// <summary>
    /// Lists borrowers filtered by KYC status with paging/sorting (Admin only).
    /// </summary>
    /// <remarks>
    /// Examples:
    /// <ul>
    ///   <li><code>/api/admin/borrowers/by-kyc?status=Verified,Pending&amp;sortBy=scoreAt&amp;sortDir=desc&amp;page=1&amp;pageSize=25</code></li>
    ///   <li><code>/api/admin/borrowers/by-kyc?status=Verified&amp;status=Pending&amp;sortBy=name&amp;sortDir=asc&amp;search=ali</code></li>
    /// </ul>
    /// </remarks>
    [HttpGet("admin/borrowers/by-kyc")]
    [Authorize(Roles = "Admin")]
    [ApiExplorerSettings(GroupName = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<BorrowerKycListResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListByKyc([FromQuery] ListByKycQuery q, CancellationToken ct = default)
    {
        // Accept CSV or repeated keys
        var statusesCsv = string.Join(',', q.Status ?? Array.Empty<string>());

        var res = await _mediator.Send(
            new ListBorrowersByKycStatusQuery(
                statusesCsv,
                q.Page,
                q.PageSize,
                q.Search,
                q.SortBy,
                q.SortDir),
            ct);

        return Ok(res);
    }
}

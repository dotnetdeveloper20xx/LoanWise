// ------------------------------------------------------------------------------------------------------
// LoanWise.Api/Controllers/LenderExportsController.cs
// Author: Faz Ahmed
// Purpose: Export the current lender's transactions (CSV/XLSX). Admins can optionally export for any lender.
// Notes:
//  - Clean, thin controller delegating to ILenderReportingRepository + ITransactionExportService.
//  - Supports CSV or Excel via `format` query; validates paging and date ranges.
//  - Uses IUserContext to infer current lender; Admins can pass ?lenderId={guid} to override.
//  - No caching of sensitive exports; basic ETag added for idempotent downloads.
// ------------------------------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using LoanWise.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanWise.Api.Controllers;

[ApiController]
[Route("api/lenders")]
[Authorize(Roles = "Lender,Admin")]
[Produces("text/csv", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class LenderExportsController : ControllerBase
{
    private readonly ILenderReportingRepository _reporting;
    private readonly IUserContext _user;
    private readonly ITransactionExportService _export;
    private readonly ILogger<LenderExportsController> _logger;

    public LenderExportsController(
        ILenderReportingRepository reporting,
        IUserContext user,
        ITransactionExportService export,
        ILogger<LenderExportsController> logger)
    {
        _reporting = reporting;
        _user = user;
        _export = export;
        _logger = logger;
    }

    /// <summary>
    /// (Authored by Faz Ahmed) Export lender transactions as CSV or XLSX.
    /// </summary>
    /// <param name="from">Start date (UTC). If null, no lower bound.</param>
    /// <param name="to">End date (UTC). If null, no upper bound.</param>
    /// <param name="loanId">Optional loan filter.</param>
    /// <param name="borrowerId">Optional borrower filter.</param>
    /// <param name="format">csv | xlsx. Defaults to csv.</param>
    /// <param name="page">Page (1+). Defaults to 1.</param>
    /// <param name="pageSize">Page size (1–10000). Defaults to 1000.</param>
    /// <param name="lenderId">
    /// Admin‑only override: export for a specific lender. Ignored for non‑admins.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>
    /// Route: <c>GET /api/lenders/transactions/export</c><br/>
    /// Examples:<br/>
    /// <c>/api/lenders/transactions/export?from=2025-01-01&amp;to=2025-08-01&amp;format=csv</c><br/>
    /// <c>/api/lenders/transactions/export?loanId=...&amp;format=xlsx</c><br/>
    /// <c>/api/lenders/transactions/export?lenderId=... (Admin only)</c>
    /// </remarks>
    [HttpGet("transactions/export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportTransactions(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Guid? loanId,
        [FromQuery] Guid? borrowerId,
        [FromQuery, RegularExpression("^(?i)(csv|xlsx)$")] string format = "csv",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 1000,
        [FromQuery] Guid? lenderId = null,
        CancellationToken ct = default)
    {
        // Resolve lender context: Admins may override; others must use their own ID.
        var currentUserId = _user.UserId ?? Guid.Empty;
        var effectiveLenderId =
            User.IsInRole("Admin") && lenderId.HasValue && lenderId.Value != Guid.Empty
                ? lenderId.Value
                : currentUserId;

        if (effectiveLenderId == Guid.Empty)
            return Unauthorized(); // no usable identity

        // Basic guards
        if (from.HasValue && to.HasValue && from > to)
            return BadRequest("Query invalid: 'from' must be <= 'to'.");

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1000;
        if (pageSize > 10_000) pageSize = 10_000; // upper bound to protect memory

        // Fetch page
        var (total, items) = await _reporting.GetLenderTransactionsAsync(
            effectiveLenderId, from, to, loanId, borrowerId, page, pageSize, ct);

        var safeFrom = from?.ToString("yyyyMMdd") ?? "start";
        var safeTo = to?.ToString("yyyyMMdd") ?? "end";
        var who = User.IsInRole("Admin") && lenderId.HasValue ? lenderId.Value : effectiveLenderId;

        // Export
        if (string.Equals(format, "xlsx", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = _export.BuildExcel(items);
            var fileName = $"lender-{who}_transactions_{safeFrom}_to_{safeTo}.xlsx";
            AddNoCacheHeaders(bytes);
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        else if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = _export.BuildCsv(items);
            var fileName = $"lender-{who}_transactions_{safeFrom}_to_{safeTo}.csv";
            AddNoCacheHeaders(bytes);
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        return BadRequest("Unsupported format. Use 'csv' or 'xlsx'.");
    }

    private void AddNoCacheHeaders(byte[] content)
    {
        // Weak ETag is fine for downloads; helps clients avoid duplicate saves.
        var etag = $"W/\"{Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(content))}\"";
        Response.Headers["ETag"] = etag;
        Response.Headers["Cache-Control"] = "no-store";
        Response.Headers["X-Export-Author"] = "Faz Ahmed";
    }
}

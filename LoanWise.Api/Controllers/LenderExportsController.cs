using System.Text;
using LoanWise.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanWise.Api.Controllers;

[ApiController]
[Route("api/lenders")]
[Authorize(Roles = "Lender,Admin")]
public sealed class LenderExportsController : ControllerBase
{
    private readonly ILenderReportingRepository _reporting;
    private readonly IUserContext _user;
    private readonly ITransactionExportService _export;

    public LenderExportsController(ILenderReportingRepository reporting, IUserContext user, ITransactionExportService export)
    {
        _reporting = reporting; _user = user; _export = export;
    }

    // GET /api/lenders/transactions/export?from=...&to=...&loanId=...&borrowerId=...&format=csv|xlsx
    [HttpGet("transactions/export")]
    public async Task<IActionResult> ExportTransactions(
           [FromQuery] DateTime? from, [FromQuery] DateTime? to,
           [FromQuery] Guid? loanId, [FromQuery] Guid? borrowerId,
           [FromQuery] string format = "csv", [FromQuery] int page = 1, [FromQuery] int pageSize = 1000,
           CancellationToken ct = default)
    {
        var lenderId = _user.UserId ?? Guid.Empty;
        if (lenderId == Guid.Empty && !User.IsInRole("Admin")) return Unauthorized();

        var (total, items) = await _reporting.GetLenderTransactionsAsync(
            lenderId, from, to, loanId, borrowerId, page, pageSize, ct);

        var safeFrom = from?.ToString("yyyyMMdd") ?? "start";
        var safeTo = to?.ToString("yyyyMMdd") ?? "end";

        if (string.Equals(format, "xlsx", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = _export.BuildExcel(items);
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"lender-transactions_{safeFrom}_to_{safeTo}.xlsx");
        }
        else
        {
            var bytes = _export.BuildCsv(items);
            return File(bytes, "text/csv", $"lender-transactions_{safeFrom}_to_{safeTo}.csv");
        }
    }

    private static string BuildCsv(IEnumerable<LoanWise.Application.DTOs.Lenders.LenderTransactionDto> items)
    {
        var sb = new StringBuilder();
        sb.AppendLine("OccurredAtUtc,Type,LoanId,LoanRef,Borrower,Amount,Description");
        foreach (var t in items)
        {
            // Basic CSV escaping with quotes
            sb.AppendLine(string.Join(',', new[]
            {
                t.OccurredAtUtc.ToString("u"),
                Csv(t.Type),
                t.LoanId.ToString(),
                Csv(t.LoanRef),
                Csv(t.BorrowerName),
                t.Amount.ToString("0.00"),
                Csv(t.Description)
            }));
        }
        return sb.ToString();

        static string Csv(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "\"\"";
            var needsQuotes = s.Contains(',') || s.Contains('"') || s.Contains('\n');
            var escaped = s.Replace("\"", "\"\"");
            return needsQuotes ? $"\"{escaped}\"" : escaped;
        }
    }

    private static byte[] AddUtf8Bom(string text)
    {
        var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        return utf8.GetPreamble().Concat(utf8.GetBytes(text)).ToArray();
    }

    // Excel (ClosedXML)
    private static byte[] BuildExcel(IEnumerable<LoanWise.Application.DTOs.Lenders.LenderTransactionDto> items)
    {
        using var wb = new ClosedXML.Excel.XLWorkbook();
        var ws = wb.AddWorksheet("Transactions");

        // Header
        var row = 1;
        ws.Cell(row, 1).Value = "OccurredAtUtc";
        ws.Cell(row, 2).Value = "Type";
        ws.Cell(row, 3).Value = "LoanId";
        ws.Cell(row, 4).Value = "LoanRef";
        ws.Cell(row, 5).Value = "Borrower";
        ws.Cell(row, 6).Value = "Amount";
        ws.Cell(row, 7).Value = "Description";
        ws.Range(row, 1, row, 7).Style.Font.Bold = true;

        // Rows
        foreach (var t in items)
        {
            row++;
            ws.Cell(row, 1).Value = t.OccurredAtUtc;
            ws.Cell(row, 1).Style.DateFormat.Format = "yyyy-mm-dd hh:mm";
            ws.Cell(row, 2).Value = t.Type;
            ws.Cell(row, 3).SetValue(t.LoanId.ToString());
            ws.Cell(row, 4).Value = t.LoanRef;
            ws.Cell(row, 5).Value = t.BorrowerName;
            ws.Cell(row, 6).Value = t.Amount;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 7).Value = t.Description;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}

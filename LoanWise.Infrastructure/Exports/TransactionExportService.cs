using System.Text;
using ClosedXML.Excel;
using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.DTOs.Lenders;

namespace LoanWise.Infrastructure.Exports;

public sealed class TransactionExportService : ITransactionExportService
{
    public byte[] BuildCsv(IEnumerable<LenderTransactionDto> items)
    {
        var sb = new StringBuilder();
        sb.AppendLine("OccurredAtUtc,Type,LoanId,LoanRef,Borrower,Amount,Description");
        foreach (var t in items)
        {
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
        return AddUtf8Bom(sb.ToString());

        static string Csv(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "\"\"";
            var needsQuotes = s.Contains(',') || s.Contains('"') || s.Contains('\n');
            var escaped = s.Replace("\"", "\"\"");
            return needsQuotes ? $"\"{escaped}\"" : escaped;
        }

        static byte[] AddUtf8Bom(string text)
        {
            var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            return utf8.GetPreamble().Concat(utf8.GetBytes(text)).ToArray();
        }
    }

    public byte[] BuildExcel(IEnumerable<LenderTransactionDto> items)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Transactions");

        var row = 1;
        ws.Cell(row, 1).Value = "OccurredAtUtc";
        ws.Cell(row, 2).Value = "Type";
        ws.Cell(row, 3).Value = "LoanId";
        ws.Cell(row, 4).Value = "LoanRef";
        ws.Cell(row, 5).Value = "Borrower";
        ws.Cell(row, 6).Value = "Amount";
        ws.Cell(row, 7).Value = "Description";
        ws.Range(row, 1, row, 7).Style.Font.Bold = true;

        foreach (var t in items)
        {
            row++;
            ws.Cell(row, 1).Value = t.OccurredAtUtc;
            ws.Cell(row, 1).Style.DateFormat.Format = "yyyy-mm-dd hh:mm";
            ws.Cell(row, 2).Value = t.Type;
            ws.Cell(row, 3).Value = t.LoanId.ToString();
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

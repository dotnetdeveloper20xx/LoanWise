// File: LoanWise.Infrastructure/Exports/RepaymentPlanPdfService.cs
using LoanWise.Application.Common.Interfaces;              // IRepaymentPlanPdfService
using AppExports = LoanWise.Application.DTOs.Exports;      // alias to the Application DTOs
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LoanWise.Infrastructure.Exports
{
    public sealed class RepaymentPlanPdfService : IRepaymentPlanPdfService
    {
        public byte[] Render(AppExports.RepaymentPlanDoc m)   // <-- use Application DTO here
        {
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(36);
                    page.Size(PageSizes.A4);

                    page.Header().Row(r =>
                    {
                        r.RelativeItem().Column(c =>
                        {
                            c.Item().Text("LoanWise").FontSize(18).SemiBold();
                            c.Item().Text("Repayment Plan").FontSize(14);
                        });
                        r.ConstantItem(160).AlignRight().Column(c =>
                        {
                            c.Item().Text($"Generated: {m.GeneratedAtUtc:u}").FontSize(9).Light();
                            c.Item().Text($"Loan: {m.LoanId}").FontSize(9).Light();
                        });
                    });

                    page.Content().Column(c =>
                    {
                        c.Spacing(10);

                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text($"Borrower: {m.BorrowerName}").Bold();
                            r.RelativeItem().AlignRight().Text($"Amount: £{m.Amount:N2}");
                        });

                        // If your RepaymentPlanDoc has AnnualInterestRate; otherwise remove this block.
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text($"Term: {m.DurationInMonths} months");
                            r.RelativeItem().AlignRight().Text($"APR: {m.AnnualInterestRate:P2}");
                        });

                        c.Item().LineHorizontal(1);

                        c.Item().Table(t =>
                        {
                            t.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(40);   // #
                                cols.ConstantColumn(110);  // Due Date
                                cols.RelativeColumn();     // Status
                                cols.ConstantColumn(100);  // Amount
                                cols.ConstantColumn(120);  // Paid On
                            });

                            t.Header(h =>
                            {
                                h.Cell().Text("#").SemiBold();
                                h.Cell().Text("Due Date").SemiBold();
                                h.Cell().Text("Status").SemiBold();
                                h.Cell().AlignRight().Text("Amount").SemiBold();
                                h.Cell().Text("Paid On").SemiBold();
                            });

                            foreach (var (idx, line) in m.Lines.Select((x, i) => (i + 1, x)))
                            {
                                t.Cell().Text(idx.ToString());
                                t.Cell().Text(line.DueDate.ToString("yyyy-MM-dd"));
                                t.Cell().Text(line.IsPaid
                                    ? "Paid"
                                    : (line.DueDate.Date < DateTime.UtcNow.Date ? "Overdue" : "Scheduled"));
                                t.Cell().AlignRight().Text($"£{line.Amount:N2}");
                                t.Cell().Text(line.PaidOnUtc?.ToString("yyyy-MM-dd") ?? "-");
                            }
                        });
                    });

                    page.Footer()
                        .AlignRight()
                        .Text(t =>
                        {
                            t.DefaultTextStyle(TextStyle.Default.FontSize(9));
                            t.Span("Page ");
                            t.CurrentPageNumber();
                            t.Span(" of ");
                            t.TotalPages();
                        });
                });
            }).GeneratePdf();
        }
    }
}

using LoanWise.Application.Common.Interfaces;
using AppExports = LoanWise.Application.DTOs.Exports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LoanWise.Infrastructure.Exports;

/// <summary>
/// Renders a simple, professional Loan Agreement PDF using QuestPDF.
/// Contract (input DTO) lives in Application; implementation stays in Infrastructure.
/// </summary>
public sealed class LoanAgreementPdfService : ILoanAgreementPdfService
{
    /// <summary>
    /// Render the agreement to a PDF byte array.
    /// </summary>
    /// <param name="m">Loan agreement document model (Application DTO).</param>
    public byte[] Render(AppExports.LoanAgreementDoc m)
    {
        // Community license setting for QuestPDF (required by the library).
        QuestPDF.Settings.License = LicenseType.Community;

        // Local helpers
        static string D(DateTime? dt) => dt?.ToString("yyyy-MM-dd") ?? "-"; // uniform date formatting
        var sections = m.Sections ?? new List<AppExports.AgreementSection>(); // be defensive

        // Create and render the PDF
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                // Page setup
                page.Margin(36);                 // 0.5 inch-ish margins
                page.Size(PageSizes.A4);         // standard A4; change if needed
                page.DefaultTextStyle(TextStyle.Default.FontSize(11)); // default body font size

                // ── Header ─────────────────────────────────────────────────────────────
                page.Header().Row(r =>
                {
                    r.RelativeItem().Text("LoanWise").FontSize(18).SemiBold();

                    r.RelativeItem().AlignRight().Column(c =>
                    {
                        c.Item().Text("Loan Agreement").FontSize(14).SemiBold();
                        c.Item().Text($"Agreement Date: {m.AgreementDateUtc:u}").FontSize(9).Light();
                        c.Item().Text($"Loan ID: {m.LoanId}").FontSize(9).Light();
                    });
                });

                // ── Content ────────────────────────────────────────────────────────────
                page.Content().Column(c =>
                {
                    c.Spacing(12); // vertical distance between sibling items on this page

                    // Parties block
                    c.Item().Shrink().Padding(8).Border(1).Column(cc =>
                    {
                        cc.Spacing(6);
                        cc.Item().Text("Parties").SemiBold();
                        cc.Item().Text($"Borrower: {m.BorrowerName}");
                        cc.Item().Text($"Lender: {m.LenderEntityName}");
                    });

                    // Summary block
                    c.Item().Shrink().Padding(8).Border(1).Column(cc =>
                    {
                        cc.Spacing(6);
                        cc.Item().Text("Summary").SemiBold();

                        cc.Item().Grid(grid =>
                        {
                            // 4 columns = label, value, label, value
                            grid.Columns(4);

                            // helper to add a key/value pair row
                            void KV(string key, string value)
                            {
                                grid.Item().Text(key).Light();
                                grid.Item().Text(value);
                            }

                            KV("Principal Amount", $"£{m.PrincipalAmount:N2}");
                            KV("Term", $"{m.DurationInMonths} month(s)");

                            // Show APR if provided as a fraction (e.g., 0.12 = 12.00%)
                            if (m.AnnualInterestRate > 0)
                                KV("APR", $"{m.AnnualInterestRate:P2}");

                            KV("Repayments", $"{m.RepaymentCount}");
                            KV("Disbursed On", D(m.DisbursementDateUtc));
                            KV("First Payment Due", D(m.FirstPaymentDueDateUtc));

                            if (m.EstimatedMonthlyPayment.HasValue)
                                KV("Est. Monthly Payment", $"£{m.EstimatedMonthlyPayment.Value:N2}");
                        });
                    });

                    // Legal sections (title + body)
                    foreach (var s in sections)
                    {
                        c.Item().Text(s.Title).SemiBold().FontSize(12);
                        c.Item().Text(s.Body).LineHeight(1.25f);
                        // No Gap() in current API; spacing is handled by parent column (c.Spacing(12)).
                    }

                    // Signatures block
                    c.Item().Text("Signatures").SemiBold().FontSize(12);

                    c.Item().Row(r =>
                    {
                        // Borrower signature column
                        r.RelativeItem().Column(cc =>
                        {
                            cc.Spacing(4);
                            cc.Item().Text(!string.IsNullOrWhiteSpace(m.BorrowerSignature.Title)
                                ? m.BorrowerSignature.Title
                                : "Borrower").Italic();

                            cc.Item().LineHorizontal(1);
                            cc.Item().Text(m.BorrowerSignature.Name);

                            if (m.BorrowerSignature.SignedOnUtc.HasValue)
                                cc.Item().Text($"Signed On: {D(m.BorrowerSignature.SignedOnUtc)}");
                        });

                        // Lender signature column
                        r.RelativeItem().Column(cc =>
                        {
                            cc.Spacing(4);
                            cc.Item().Text(!string.IsNullOrWhiteSpace(m.LenderSignature.Title)
                                ? m.LenderSignature.Title
                                : "Authorized Representative").Italic();

                            cc.Item().LineHorizontal(1);
                            cc.Item().Text(m.LenderSignature.Name);

                            if (m.LenderSignature.SignedOnUtc.HasValue)
                                cc.Item().Text($"Signed On: {D(m.LenderSignature.SignedOnUtc)}");
                        });
                    });
                });

                // ── Footer (page numbers) ─────────────────────────────────────────────
                page.Footer()
                    .AlignRight()
                    .Text(t =>
                    {
                        t.DefaultTextStyle(TextStyle.Default.FontSize(9));
                        t.Span("Page "); t.CurrentPageNumber();
                        t.Span(" of "); t.TotalPages();
                    });
            });
        }).GeneratePdf();
    }
}

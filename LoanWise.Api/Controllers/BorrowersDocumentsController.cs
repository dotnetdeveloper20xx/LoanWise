// --------------------------------------------------------------------------------------
// LoanWise.Api - BorrowersDocumentsController
// Author: Faz Ahmed
// Purpose: Secure generation & delivery of borrower-facing PDF documents
//          (Repayment Plan + Loan Agreement).
// Notes:
//  - Aligns with Clean Architecture: controllers stay thin; business logic lives in services.
//  - Enforces role/ownership checks: Borrower (owner) or Admin only.
//  - Adds cache control, ETag, and consistent content-disposition for PDFs.
//  - Ready for Serilog/App Insights enrichment via ILogger (wire in Program.cs).
// --------------------------------------------------------------------------------------

using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.DTOs.Exports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using AppExports = LoanWise.Application.DTOs.Exports;

namespace LoanWise.Api.Controllers;

[ApiController]
[Route("api/borrowers")]
[Authorize(Roles = "Borrower,Admin")]
[Produces("application/pdf")]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class BorrowersDocumentsController : ControllerBase
{
    private readonly ILoanRepository _loans;
    private readonly IUserRepository _users;
    private readonly IRepaymentPlanPdfService _repaymentPlanPdf;
    private readonly ILoanAgreementPdfService _agreementPdf;
    private readonly ILogger<BorrowersDocumentsController> _logger;

    public BorrowersDocumentsController(
        ILoanRepository loans,
        IUserRepository users,
        IRepaymentPlanPdfService repaymentPlanPdf,
        ILoanAgreementPdfService agreementPdf,
        ILogger<BorrowersDocumentsController> logger)
    {
        _loans = loans;
        _users = users;
        _repaymentPlanPdf = repaymentPlanPdf;
        _agreementPdf = agreementPdf;
        _logger = logger;
    }

    // Utility: current user id from JWT
    private Guid? GetCurrentUserId() =>
        Guid.TryParse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value, out var id)
            ? id : null;

    // Utility: ensure the caller is the borrower (owner) or an Admin
    private bool CanAccessBorrower(Guid borrowerId) =>
        User.IsInRole("Admin") || (GetCurrentUserId() is Guid uid && uid == borrowerId);

    /// <summary>
    /// Generates a PDF of the loan's repayment plan for the borrower (or Admin).
    /// </summary>
    [HttpGet("loans/{loanId:guid}/repayment-plan.pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRepaymentPlanPdf([FromRoute, Required] Guid loanId, CancellationToken ct = default)
    {
        var loan = await _loans.GetByIdAsync(loanId, ct);
        if (loan is null)
        {
            _logger.LogWarning("Repayment plan requested for missing loan {LoanId}", loanId);
            return NotFound();
        }

        if (!CanAccessBorrower(loan.BorrowerId))
        {
            _logger.LogWarning("Unauthorized repayment plan access for loan {LoanId} by user {User}", loanId, GetCurrentUserId());
            return Forbid();
        }

        var borrower = await _users.GetByIdAsync(loan.BorrowerId);

        var doc = new AppExports.RepaymentPlanDoc
        {
            LoanId = loan.Id,
            BorrowerName = borrower?.FullName ?? "Borrower",
            Amount = loan.Amount.Value,
            DurationInMonths = loan.DurationInMonths,
            GeneratedAtUtc = DateTime.UtcNow,
            Lines = loan.Repayments
                .OrderBy(r => r.DueDate)
                .Select((r, i) => new AppExports.RepaymentLine(
                    i + 1,
                    r.DueDate,
                    r.RepaymentAmount,
                    r.IsPaid,
                    r.PaidOn // nullable
                ))
                .ToList()
        };

        var bytes = _repaymentPlanPdf.Render(doc);

        // ETag for idempotent downloads (weak ETag is enough for binary snapshots)
        var etag = $"W/\"{Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(bytes))}\"";
        Response.Headers["ETag"] = etag;
        Response.Headers["Cache-Control"] = "no-store"; // sensitive

        var fileName = $"Loan_{loan.Id}_RepaymentPlan.pdf";
        return File(bytes, "application/pdf", fileName);
    }

    /// <summary>
    /// Generates a PDF of the loan agreement for the borrower (or Admin).
    /// </summary>
    [HttpGet("loans/{loanId:guid}/agreement.pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLoanAgreementPdf([FromRoute, Required] Guid loanId, CancellationToken ct = default)
    {
        var loan = await _loans.GetByIdAsync(loanId, ct);
        if (loan is null)
        {
            _logger.LogWarning("Loan agreement requested for missing loan {LoanId}", loanId);
            return NotFound();
        }

        if (!CanAccessBorrower(loan.BorrowerId))
        {
            _logger.LogWarning("Unauthorized agreement access for loan {LoanId} by user {User}", loanId, GetCurrentUserId());
            return Forbid();
        }

        var borrower = await _users.GetByIdAsync(loan.BorrowerId);

        var firstRepayment = loan.Repayments.OrderBy(r => r.DueDate).FirstOrDefault();
        var doc = new LoanAgreementDoc
        {
            LoanId = loan.Id,
            BorrowerName = borrower?.FullName ?? "Borrower",
            LenderEntityName = "LoanWise Lenders",
            AgreementDateUtc = DateTime.UtcNow,
            PrincipalAmount = loan.Amount.Value,
            DurationInMonths = loan.DurationInMonths,
            DisbursementDateUtc = loan.DisbursedAtUtc,
            FirstPaymentDueDateUtc = firstRepayment?.DueDate,
            RepaymentCount = loan.Repayments.Count,
            EstimatedMonthlyPayment = firstRepayment?.RepaymentAmount,
            Sections = new List<AgreementSection>
            {
                new() { Title = "1. Loan and Purpose", Body = "The Lender(s) agree to lend the Principal Amount to the Borrower for the stated purpose. Disbursement will occur once the loan is fully funded and approved." },
                new() { Title = "2. Interest and Fees", Body = "Interest accrues at the Annual Interest Rate on the outstanding principal balance. Any applicable platform or processing fees are disclosed separately." },
                new() { Title = "3. Repayment", Body = "The Borrower agrees to repay the loan in equal scheduled installments according to the repayment schedule. Early repayment is permitted without penalty unless otherwise specified." },
                new() { Title = "4. Default", Body = "If the Borrower fails to make a payment when due, the loan may be deemed in default. The Borrower may be liable for reasonable collection costs and any applicable default interest as permitted by law." },
                new() { Title = "5. Representations", Body = "The Borrower confirms that the information provided is accurate and complete. The Borrower agrees to promptly update the Lender with any material changes." },
                new() { Title = "6. Governing Law", Body = "This Agreement is governed by the laws of the applicable jurisdiction. Any disputes will be subject to the exclusive jurisdiction of the courts in that jurisdiction." },
                new() { Title = "7. Entire Agreement", Body = "This document constitutes the entire agreement between the parties and supersedes all prior discussions or understandings." }
            },
            BorrowerSignature = new SignatureBlock { Name = borrower?.FullName ?? "Borrower", Title = "Borrower" },
            LenderSignature = new SignatureBlock { Name = "Authorized Representative", Title = "Lender Representative" }
        };

        var bytes = _agreementPdf.Render(doc);

        var etag = $"W/\"{Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(bytes))}\"";
        Response.Headers["ETag"] = etag;
        Response.Headers["Cache-Control"] = "no-store";

        var fileName = $"Loan_{loan.Id}_Agreement.pdf";
        return File(bytes, "application/pdf", fileName);
    }
}

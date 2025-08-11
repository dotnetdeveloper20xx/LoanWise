using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.DTOs.Exports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppExports = LoanWise.Application.DTOs.Exports; // alias to the Application DTOs


namespace LoanWise.Api.Controllers;

[ApiController]
[Route("api/borrowers")]
[Authorize(Roles = "Borrower,Admin")]
public sealed class BorrowersDocumentsController : ControllerBase
{
    private readonly ILoanRepository _loans;
    private readonly IUserRepository _users;
    private readonly IRepaymentPlanPdfService _pdf;
    private readonly ILoanAgreementPdfService _agreementPdf;

    public BorrowersDocumentsController(ILoanRepository loans, 
        IUserRepository users, 
        IRepaymentPlanPdfService pdf,
        ILoanAgreementPdfService agreementPdf)
    {
        _loans = loans; _users = users; _pdf = pdf; _agreementPdf = agreementPdf;
    }

    [HttpGet("loans/{loanId:guid}/repayment-plan.pdf")]
    public async Task<IActionResult> GetRepaymentPlanPdf(Guid loanId, CancellationToken ct)
    {
        var loan = await _loans.GetByIdAsync(loanId, ct);
        if (loan is null) return NotFound();

        // TODO: enforce borrower/admin authorization check here

        var borrower = await _users.GetByIdAsync(loan.BorrowerId); // or pass ct if your repo supports it

        var doc = new AppExports.RepaymentPlanDoc
        {
            LoanId = loan.Id,
            BorrowerName = borrower?.FullName ?? "Borrower",
            Amount = loan.Amount.Value,
            // AnnualInterestRate = loan.AnnualInterestRate, // include if you have it
            DurationInMonths = loan.DurationInMonths,
            GeneratedAtUtc = DateTime.UtcNow,
            Lines = loan.Repayments
                .OrderBy(r => r.DueDate)
                .Select((r, i) => new AppExports.RepaymentLine(
                    i + 1,
                    r.DueDate,
                    r.RepaymentAmount,
                    r.IsPaid,
                    r.PaidOn            // NOTE: use PaidOn (nullable), not PaidOnUtc
                ))
                .ToList()
        };

        var bytes = _pdf.Render(doc); // IRepaymentPlanPdfService
        return File(bytes, "application/pdf", $"Loan_{loan.Id}_RepaymentPlan.pdf");
    }

    [HttpGet("loans/{loanId:guid}/agreement.pdf")]
    public async Task<IActionResult> GetLoanAgreementPdf(Guid loanId, CancellationToken ct)
    {
        var loan = await _loans.GetByIdAsync(loanId, ct);
        if (loan is null) return NotFound();

        // Optional: only borrower (owner) or admin can fetch
        // ...your auth check here...

        var borrower = await _users.GetByIdAsync(loan.BorrowerId);

        // Derivations (adjust property names to your model)
        var firstDue = loan.Repayments.OrderBy(r => r.DueDate).FirstOrDefault()?.DueDate;
        var monthly = loan.Repayments.OrderBy(r => r.DueDate).FirstOrDefault()?.RepaymentAmount;

        var doc = new LoanAgreementDoc
        {
            LoanId = loan.Id,
            BorrowerName = borrower?.FullName ?? "Borrower",
            LenderEntityName = "LoanWise Lenders",
            AgreementDateUtc = DateTime.UtcNow,
            PrincipalAmount = loan.Amount.Value,        
            DurationInMonths = loan.DurationInMonths,
            DisbursementDateUtc = loan.DisbursedAtUtc,
            FirstPaymentDueDateUtc = firstDue,
            RepaymentCount = loan.Repayments.Count,
            EstimatedMonthlyPayment = monthly,

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
        Response.Headers["Cache-Control"] = "no-store";
        return File(bytes, "application/pdf", $"Loan_{loan.Id}_Agreement.pdf");
    }
}

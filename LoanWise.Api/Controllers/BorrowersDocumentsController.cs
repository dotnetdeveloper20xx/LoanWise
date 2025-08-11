using LoanWise.Application.Common.Interfaces;
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

    public BorrowersDocumentsController(ILoanRepository loans, IUserRepository users, IRepaymentPlanPdfService pdf)
    {
        _loans = loans; _users = users; _pdf = pdf;
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


}

namespace LoanWise.Application.DTOs.Exports;

public sealed class LoanAgreementDoc
{
    // Parties / context
    public Guid LoanId { get; init; }
    public string BorrowerName { get; init; } = default!;
    public string LenderEntityName { get; init; } = "LoanWise Lenders"; // display name for the platform/lenders
    public DateTime AgreementDateUtc { get; init; }

    // Summary
    public decimal PrincipalAmount { get; init; }
    public decimal AnnualInterestRate { get; init; }    // e.g., 0.12m for 12% (optional; remove if you don't track APR)
    public int DurationInMonths { get; init; }
    public DateTime? DisbursementDateUtc { get; init; }
    public DateTime? FirstPaymentDueDateUtc { get; init; }
    public int RepaymentCount { get; init; }
    public decimal? EstimatedMonthlyPayment { get; init; }

    // Legal text
    public List<AgreementSection> Sections { get; init; } = new();

    // Signature blocks
    public SignatureBlock BorrowerSignature { get; init; } = new();
    public SignatureBlock LenderSignature { get; init; } = new();
}

public sealed class AgreementSection
{
    public string Title { get; init; } = default!;
    public string Body { get; init; } = default!;  // plain text; keep it simple for now
}

public sealed class SignatureBlock
{
    public string Name { get; init; } = default!;
    public string Title { get; init; } = "";       // e.g., "Borrower" / "Authorized Representative"
    public DateTime? SignedOnUtc { get; init; }    // filled later if you capture signatures
}

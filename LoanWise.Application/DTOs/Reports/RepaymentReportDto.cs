namespace LoanWise.Application.DTOs.Reports
{
    public sealed record RepaymentReportDto(
        Guid RepaymentId,
        Guid LoanId,
        string BorrowerName,
        DateTime DueDate,
        bool IsPaid,
        DateTime? PaidOn,
        decimal RepaymentAmount,
        string LoanStatus
    );
}

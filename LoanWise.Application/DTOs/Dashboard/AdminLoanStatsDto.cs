namespace LoanWise.Application.DTOs.Dashboard
{
    public class AdminLoanStatsDto
    {
        public int TotalLoans { get; set; }
        public int ApprovedCount { get; set; }
        public int FundedCount { get; set; }
        public int DisbursedCount { get; set; }
        public int CompletedCount { get; set; }
        public int OverdueRepaymentCount { get; set; }
    }
}

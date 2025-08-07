namespace LoanWise.Application.DTOs.Dashboard
{
    public class LenderPortfolioDto
    {
        public decimal TotalFunded { get; set; }
        public int NumberOfLoansFunded { get; set; }
        public decimal TotalReceived { get; set; } // Placeholder for future expansion
        public decimal OutstandingBalance { get; set; } // Placeholder for future expansion
    }
}

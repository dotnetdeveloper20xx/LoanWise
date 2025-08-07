using System;

namespace LoanWise.Application.DTOs.Dashboard
{
    public class BorrowerDashboardDto
    {
        public int TotalLoans { get; set; }
        public int FundedLoans { get; set; }
        public int DisbursedLoans { get; set; }
        public decimal TotalOutstanding { get; set; }

        public Guid? UpcomingRepaymentId { get; set; }
        public DateTime? NextRepaymentDueDate { get; set; }
        public decimal? NextRepaymentAmount { get; set; }
    }
}

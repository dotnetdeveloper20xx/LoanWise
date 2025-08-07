using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanWise.Application.DTOs
{
    public class LenderPortfolioDto
    {
        public decimal TotalFunded { get; set; }
        public int NumberOfLoansFunded { get; set; }
        public decimal TotalReceived { get; set; } // Placeholder for future expansion
        public decimal OutstandingBalance { get; set; } // Placeholder for future expansion
    }
}

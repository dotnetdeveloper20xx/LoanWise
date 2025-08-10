namespace LoanWise.Domain.Entities
{
    /// <summary>
    /// A proportional slice of a borrower repayment that belongs to a specific lender.
    /// </summary>
    public class LenderRepayment
    {
        public Guid Id { get; set; }
        public Guid LoanId { get; set; }
        public Guid RepaymentId { get; set; }
        public Guid LenderId { get; set; }
        public decimal Amount { get; set; }         // decimal, currency = loan currency
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}

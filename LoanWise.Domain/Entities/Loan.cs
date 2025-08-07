using LoanWise.Domain.Enums;
using LoanWise.Domain.ValueObjects;

namespace LoanWise.Domain.Entities
{
    /// <summary>
    /// Represents a loan application made by a borrower.
    /// </summary>
    public class Loan
    {
        private readonly List<Funding> _fundings = new();
        private readonly List<Repayment> _repayments = new();

        public Guid Id { get; private set; }
        public Guid BorrowerId { get; private set; }
        public User Borrower { get; private set; }
        public Money Amount { get; private set; }
        public int DurationInMonths { get; private set; }
        public LoanPurpose Purpose { get; private set; }
        public LoanStatus Status { get; private set; }
        public RiskLevel RiskLevel { get; private set; }

        public IReadOnlyCollection<Funding> Fundings => _fundings.AsReadOnly();
        public IReadOnlyCollection<Repayment> Repayments => _repayments.AsReadOnly();

        private Loan() { }

        public Loan(Guid id, Guid borrowerId, Money amount, int durationInMonths, LoanPurpose purpose)
        {
            Id = id;
            BorrowerId = borrowerId;
            Amount = amount;
            DurationInMonths = durationInMonths;
            Purpose = purpose;
            Status = LoanStatus.Pending;
            RiskLevel = RiskLevel.Unknown;
        }

        public void Approve(RiskLevel riskLevel)
        {
            if (Status != LoanStatus.Pending)
                throw new InvalidOperationException("Only pending loans can be approved.");

            RiskLevel = riskLevel;
            Status = LoanStatus.Approved;
        }

        public void Reject()
        {
            if (Status != LoanStatus.Pending)
                throw new InvalidOperationException("Only pending loans can be rejected.");

            Status = LoanStatus.Rejected;
        }

        public void AddFunding(Funding funding)
        {
            _fundings.Add(funding);
        }

        public void AddRepayment(Repayment repayment)
        {
            _repayments.Add(repayment);
        }

        /// <summary>
        /// Checks if the loan is fully funded.
        /// </summary>
        public bool IsFullyFunded()
        {
            var total = _fundings.Sum(f => f.Amount.Value);
            return total >= Amount.Value;
        }

        /// <summary>
        /// Updates loan status to Funded if fully funded and previously Approved.
        /// </summary>
        public void UpdateFundingStatus()
        {
            if (Status == LoanStatus.Approved && IsFullyFunded())
            {
                Status = LoanStatus.Funded;
            }
        }

        /// <summary>
        /// Marks the loan as disbursed after it has been fully funded.
        /// </summary>
        public void Disburse()
        {
            if (Status != LoanStatus.Funded)
                throw new InvalidOperationException("Only fully funded loans can be disbursed.");

            Status = LoanStatus.Disbursed;
        }

        public void GenerateRepaymentSchedule()
        {
            if (Status != LoanStatus.Disbursed)
                throw new InvalidOperationException("Repayment schedule can only be generated for disbursed loans.");

            var monthlyInstallment = Math.Round(Amount.Value / DurationInMonths, 2);

            for (int i = 1; i <= DurationInMonths; i++)
            {
                var dueDate = DateTime.UtcNow.Date.AddMonths(i);
                var repayment = new Repayment(
                    id: Guid.NewGuid(),
                    loanId: this.Id,
                    dueDate: dueDate,
                    amount: new Money(monthlyInstallment)
                );

                _repayments.Add(repayment);
            }
        }

    }
}

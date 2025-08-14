using LoanWise.Domain.Common;
using LoanWise.Domain.Enums;
using LoanWise.Domain.Events;
using LoanWise.Domain.ValueObjects;

namespace LoanWise.Domain.Entities
{
    /// <summary>
    /// Represents a loan application made by a borrower.
    /// Aggregate root; raises domain events for lifecycle transitions.
    /// </summary>
    public class Loan : Entity
    {
        private readonly List<Funding> _fundings = new();
        private readonly List<Repayment> _repayments = new();

        public Guid Id { get; private set; }
        public Guid BorrowerId { get; private set; }

        // Navigation can be null at creation; EF will populate when needed
        public User? Borrower { get; private set; }

        public decimal Amount { get; private set; }
        public int DurationInMonths { get; private set; }
        public LoanPurpose Purpose { get; private set; }

        public LoanStatus Status { get; private set; }
        public RiskLevel RiskLevel { get; private set; }

        public string? RejectedReason { get; private set; }
        public DateTime? RejectedAtUtc { get; private set; }
        public DateTime? ApprovedAtUtc { get; private set; }
        public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
        public DateTime? DisbursedAtUtc { get; private set; }

        public IReadOnlyCollection<Funding> Fundings => _fundings.AsReadOnly();
        public IReadOnlyCollection<Repayment> Repayments => _repayments.AsReadOnly();

        public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

        // EF
        private Loan() { }

        public Loan(Guid id, Guid borrowerId, decimal amount, int durationInMonths, LoanPurpose purpose)
        {
            if (amount == 0) throw new ArgumentOutOfRangeException(nameof(durationInMonths), "Loan amount must be > 0.");
            if (durationInMonths <= 0) throw new ArgumentOutOfRangeException(nameof(durationInMonths), "Duration must be > 0.");

            Id = id;
            BorrowerId = borrowerId;
            Amount = amount;
            DurationInMonths = durationInMonths;
            Purpose = purpose;

            Status = LoanStatus.Pending;
            RiskLevel = RiskLevel.Unknown;
            CreatedAtUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Approve a pending loan with a calculated risk level.
        /// Raises LoanApprovedEvent.
        /// </summary>
        public void Approve(RiskLevel riskLevel)
        {
            if (Status != LoanStatus.Pending)
                throw new InvalidOperationException("Only pending loans can be approved.");

            RiskLevel = riskLevel;
            Status = LoanStatus.Approved;
            ApprovedAtUtc = DateTime.UtcNow;

            AddDomainEvent(new LoanApprovedEvent(Id, BorrowerId));
        }

        /// <summary>
        /// Reject a pending loan with a reason.
        /// Raises LoanRejectedEvent.
        /// </summary>
        public void Reject(string reason)
        {
            if (Status != LoanStatus.Pending)
                throw new InvalidOperationException("Only pending loans can be rejected.");

            Status = LoanStatus.Rejected;
            RejectedReason = reason;
            RejectedAtUtc = DateTime.UtcNow;

            AddDomainEvent(new LoanRejectedEvent(Id, BorrowerId, reason));
        }

        /// <summary>
        /// Add a new funding contribution. Also checks if the loan has just become fully funded
        /// and, if appropriate, transitions to Funded and raises LoanFundedEvent.
        /// </summary>
        public void AddFunding(Funding funding)
        {
            if (funding is null) throw new ArgumentNullException(nameof(funding));
            if (Status != LoanStatus.Approved && Status != LoanStatus.Funded && Status != LoanStatus.Disbursed)
                throw new InvalidOperationException("Funding can only be added to approved/funded/disbursed loans.");

            var wasFullyFunded = IsFullyFunded();

            _fundings.Add(funding);
            AddDomainEvent(new FundingAddedEvent(Id, funding.LenderId, funding.Amount));

            // If we just crossed the threshold (and weren't already marked Funded), flip state & raise event.
            if (!wasFullyFunded && IsFullyFunded() && Status == LoanStatus.Approved)
            {
                Status = LoanStatus.Funded;

                var lastFunding = Fundings.Last(); // Or track the one just added
                AddDomainEvent(new LoanFundedEvent(
                    Id,
                    lastFunding.Id,
                    lastFunding.LenderId,
                    lastFunding.Amount,
                    true
                ));
            }
        }

        public void AddRepayment(Repayment repayment)
        {
            if (repayment is null) throw new ArgumentNullException(nameof(repayment));
            _repayments.Add(repayment);
        }

        /// <summary>
        /// Checks if the loan is fully funded by comparing total funding vs target amount.
        /// </summary>
        public bool IsFullyFunded()
        {
            var total = _fundings.Sum(f => f.Amount);
            return total >= Amount;
        }

        /// <summary>
        /// Re-evaluates funding status. If loan is Approved and now fully funded, transitions to Funded and raises event.
        /// Useful after bulk operations or seed scripts.
        /// </summary>
        public void UpdateFundingStatus(Funding latestFunding)
        {
            if (Status == LoanStatus.Approved && IsFullyFunded())
            {
                Status = LoanStatus.Funded;
                AddDomainEvent(new LoanFundedEvent(
                    Id,
                    latestFunding.Id,
                    latestFunding.LenderId,
                    latestFunding.Amount,
                    true
                ));
            }
        }


        /// <summary>
        /// Marks the loan as disbursed. Only valid from Funded.
        /// Raises LoanDisbursedEvent.
        /// </summary>
        public void Disburse()
        {
            if (Status != LoanStatus.Funded)
                throw new InvalidOperationException("Only fully funded loans can be disbursed.");

            Status = LoanStatus.Disbursed;
            DisbursedAtUtc = DateTime.UtcNow;

            AddDomainEvent(new LoanDisbursedEvent(Id, DisbursedAtUtc.Value));
        }

        /// <summary>
        /// Creates an equal installment repayment schedule after disbursement.
        /// </summary>
        public void GenerateRepaymentSchedule()
        {
            if (Status != LoanStatus.Disbursed)
                throw new InvalidOperationException("Repayment schedule can only be generated for disbursed loans.");

            var monthlyInstallment = Math.Round(Amount / DurationInMonths, 2, MidpointRounding.AwayFromZero);

            for (int i = 1; i <= DurationInMonths; i++)
            {
                var dueDate = DateTime.UtcNow.Date.AddMonths(i);
                var repayment = new Repayment(
                    id: Guid.NewGuid(),
                    loanId: Id,
                    dueDate: dueDate,
                    amount: monthlyInstallment
                );

                _repayments.Add(repayment);
            }
        }

        /// <summary>
        /// Optional helper to scan for overdue repayments. We keep event raising out for now;
        /// overdue alerts are better emitted by a scheduled job that publishes events daily.
        /// </summary>
        public void MarkOverdueRepayments(DateTime currentDate)
        {
            foreach (var repayment in _repayments)
            {
                _ = repayment.IsOverdue(currentDate);
                // If you want inline events instead of scheduler:
                // if (repayment.IsOverdue(currentDate))
                //     AddDomainEvent(new RepaymentOverdueEvent(Id, BorrowerId, repayment.Id, repayment.DueDate, repayment.RepaymentAmount));
            }
        }

        // --------- Legacy helpers (prefer Approve/Reject which raise events) ---------

        /// <summary>
        /// Prefer <see cref="Approve(RiskLevel)"/> which raises domain events.
        /// </summary>
        public void SetApproved()
        {
            if (Status == LoanStatus.Approved) return;
            if (Status != LoanStatus.Pending)
                throw new InvalidOperationException("Only pending loans can be approved.");

            Status = LoanStatus.Approved;
            ApprovedAtUtc = DateTime.UtcNow;
            AddDomainEvent(new LoanApprovedEvent(Id, BorrowerId));
        }

        /// <summary>
        /// Prefer <see cref="Reject(string)"/> which raises domain events.
        /// </summary>
        public void SetRejected(string? reason)
        {
            if (Status == LoanStatus.Rejected) return;
            if (Status != LoanStatus.Pending)
                throw new InvalidOperationException("Only pending loans can be rejected.");

            Status = LoanStatus.Rejected;
            RejectedReason = reason;
            RejectedAtUtc = DateTime.UtcNow;
            AddDomainEvent(new LoanRejectedEvent(Id, BorrowerId, reason ?? string.Empty));
        }
    }
}

using System;
using System.Collections.Generic;

namespace LoanWise.Domain.Entities
{
    /// <summary>
    /// Represents a platform user — can be a Borrower, Lender, or Admin.
    /// </summary>
    public class User
    {
        private readonly List<Loan> _loans = new();
        private readonly List<Funding> _fundings = new();
        private readonly List<VerificationDocument> _documents = new();
        private readonly List<SystemEvent> _events = new();

        /// <summary>
        /// Unique identifier for the user.
        /// </summary>
        public Guid Id { get;  set; }

        /// <summary>
        /// Full name of the user.
        /// </summary>
        public string FullName { get;  set; }

        /// <summary>
        /// Email address (used as login identity).
        /// </summary>
        public string Email { get;  set; }

        /// <summary>
        /// Hashed password (stored securely).
        /// </summary>
        public string PasswordHash { get;  set; }

        /// <summary>
        /// Role of the user: Borrower, Lender, Admin.
        /// </summary>
        public UserRole Role { get;  set; }

        /// <summary>
        /// Mock KYC / credit score value.
        /// </summary>
        public int? CreditScore { get; private set; }

        /// <summary>
        /// Risk tier derived from credit score.
        /// </summary>
        public RiskTier? Tier { get; private set; }

        /// <summary>
        /// Indicates whether mock KYC has been simulated.
        /// </summary>
        public bool KycVerified => CreditScore.HasValue;

        public IReadOnlyCollection<Loan> Loans => _loans.AsReadOnly();
        public IReadOnlyCollection<Funding> Fundings => _fundings.AsReadOnly();
        public IReadOnlyCollection<VerificationDocument> Documents => _documents.AsReadOnly();
        public IReadOnlyCollection<SystemEvent> Events => _events.AsReadOnly();

        public User() { }

        public User(Guid id, string fullName, string email, string passwordHash, UserRole role)
        {
            Id = id;
            FullName = fullName;
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
        }

        public void AddDocument(VerificationDocument document) => _documents.Add(document);
        public void AddEvent(SystemEvent systemEvent) => _events.Add(systemEvent);

        public void SimulateKyc()
        {
            CreditScore = new Random().Next(600, 750);
            Tier = CreditScore switch
            {
                < 620 => RiskTier.High,
                < 700 => RiskTier.Medium,
                _ => RiskTier.Low
            };
        }

        public void AssignCreditProfile(int score, RiskTier tier)
        {
            CreditScore = score;
            Tier = tier;
        }

        public void SetPassword(string hash)
        {
            PasswordHash = hash;
        }

    }

    public enum UserRole
    {
        Borrower = 0,
        Lender = 1,
        Admin = 2
    }

    
}

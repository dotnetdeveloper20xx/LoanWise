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
        public Guid Id { get; private set; }

        /// <summary>
        /// Full name of the user.
        /// </summary>
        public string FullName { get; private set; }

        /// <summary>
        /// Email address (used as login identity).
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Hashed password (stored securely).
        /// </summary>
        public string PasswordHash { get; private set; }

        /// <summary>
        /// Role of the user: Borrower, Lender, Admin.
        /// </summary>
        public UserRole Role { get; private set; }

        /// <summary>
        /// Navigation property to loans this user has applied for (as borrower).
        /// </summary>
        public IReadOnlyCollection<Loan> Loans => _loans.AsReadOnly();

        /// <summary>
        /// Navigation property to fundings this user made (as lender).
        /// </summary>
        public IReadOnlyCollection<Funding> Fundings => _fundings.AsReadOnly();

        /// <summary>
        /// Verification documents uploaded by the user (e.g., payslips).
        /// </summary>
        public IReadOnlyCollection<VerificationDocument> Documents => _documents.AsReadOnly();

        /// <summary>
        /// Audit or notification events associated with this user.
        /// </summary>
        public IReadOnlyCollection<SystemEvent> Events => _events.AsReadOnly();

        /// <summary>
        /// Required by EF Core for materialization.
        /// </summary>
        private User() { }

        /// <summary>
        /// Creates a new user with required fields.
        /// </summary>
        public User(Guid id, string fullName, string email, string passwordHash, UserRole role)
        {
            Id = id;
            FullName = fullName;
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
        }

        /// <summary>
        /// Adds a verification document to the user.
        /// </summary>
        public void AddDocument(VerificationDocument document)
        {
            _documents.Add(document);
        }

        /// <summary>
        /// Adds a system event (audit/log/notification) related to this user.
        /// </summary>
        public void AddEvent(SystemEvent systemEvent)
        {
            _events.Add(systemEvent);
        }
    }

    /// <summary>
    /// Roles assigned to users in the platform.
    /// </summary>
    public enum UserRole
    {
        Borrower = 0,
        Lender = 1,
        Admin = 2
    }
}

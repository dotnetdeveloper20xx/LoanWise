using LoanWise.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanWise.Application.Common.Interfaces
{
    /// <summary>
    /// Represents the abstraction of the EF Core DbContext used throughout the application.
    /// Enables dependency injection and testability.
    /// </summary>
    public interface IApplicationDbContext
    {
        // =========================
        // Core Lending
        // =========================

        /// <summary>
        /// Loans submitted by borrowers.
        /// </summary>
        DbSet<Loan> Loans { get; }

        /// <summary>
        /// Repayment entries for loans.
        /// </summary>
        DbSet<Repayment> Repayments { get; }

        /// <summary>
        /// Lender funding contributions.
        /// </summary>
        DbSet<Funding> Fundings { get; }

        /// <summary>
        /// Per-lender repayment allocations.
        /// </summary>
        DbSet<LenderRepayment> LenderRepayments { get; }

        // =========================
        // User & Security
        // =========================

        /// <summary>
        /// Application users (borrowers, lenders, admins).
        /// </summary>
        DbSet<User> Users { get; }

        /// <summary>
        /// Refresh tokens for JWT authentication.
        /// </summary>
        DbSet<RefreshToken> RefreshTokens { get; }

        // =========================
        // Operational / Compliance
        // =========================

        /// <summary>
        /// KYC and verification documents.
        /// </summary>
        DbSet<VerificationDocument> VerificationDocuments { get; }

        /// <summary>
        /// System-level logged events (audit, diagnostics).
        /// </summary>
        DbSet<SystemEvent> SystemEvents { get; }

        /// <summary>
        /// Credit profile simulations or imports.
        /// </summary>
        DbSet<CreditProfile> CreditProfiles { get; }

        /// <summary>
        /// Escrow ledger of virtual transactions.
        /// </summary>
        DbSet<EscrowTransaction> EscrowTransactions { get; }

        /// <summary>
        /// In-app notifications for users.
        /// </summary>
        DbSet<Notification> Notifications { get; }

        // =========================
        // Commit
        // =========================

        /// <summary>
        /// Persists changes to the database.
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}

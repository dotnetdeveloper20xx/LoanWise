using LoanWise.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;



namespace LoanWise.Application.Common.Interfaces
{
    /// <summary>
    /// Represents the abstraction of the EF Core DbContext used throughout the application.
    /// Enables dependency injection and testability.
    /// </summary>
    public interface IApplicationDbContext
    {
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
        /// KYC and verification documents.
        /// </summary>
        DbSet<VerificationDocument> VerificationDocuments { get; }

        /// <summary>
        /// System-level logged events (audit/notifications).
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

        DbSet<User> Users { get; }
        DbSet<RefreshToken> RefreshTokens { get; }     


        /// <summary>
        /// Persists changes to the database.
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}

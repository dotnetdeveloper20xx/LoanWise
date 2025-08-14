using LoanWise.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

public interface IApplicationDbContext
{
    // Core Lending
    DbSet<Loan> Loans { get; }
    DbSet<Repayment> Repayments { get; }
    DbSet<Funding> Fundings { get; }
    DbSet<LenderRepayment> LenderRepayments { get; }

    // Users & Security
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    // Ops / Compliance
    DbSet<VerificationDocument> VerificationDocuments { get; }
    DbSet<SystemEvent> SystemEvents { get; }
    DbSet<CreditProfile> CreditProfiles { get; }
    DbSet<EscrowTransaction> EscrowTransactions { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<BorrowerRiskSnapshot> BorrowerRiskSnapshots { get; }

    // ✨ NEW: allow repositories to manage entity state
    EntityEntry Entry(object entity);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
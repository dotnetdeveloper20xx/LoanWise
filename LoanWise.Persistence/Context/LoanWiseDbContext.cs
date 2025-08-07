using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanWise.Persistence.Context
{
    /// <summary>
    /// The EF Core database context for the LoanWise application.
    /// </summary>
    public class LoanWiseDbContext : DbContext, IApplicationDbContext
    {
        public LoanWiseDbContext(DbContextOptions<LoanWiseDbContext> options)
            : base(options) { }

        public DbSet<Loan> Loans => Set<Loan>();
        public DbSet<Repayment> Repayments => Set<Repayment>();
        public DbSet<Funding> Fundings => Set<Funding>();
        public DbSet<VerificationDocument> VerificationDocuments => Set<VerificationDocument>();
        public DbSet<SystemEvent> SystemEvents => Set<SystemEvent>();
        public DbSet<CreditProfile> CreditProfiles => Set<CreditProfile>();
        public DbSet<EscrowTransaction> EscrowTransactions => Set<EscrowTransaction>();

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => base.SaveChangesAsync(cancellationToken);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Optional: Apply configurations from Fluent API mappings
            base.OnModelCreating(modelBuilder);
        }
    }
}

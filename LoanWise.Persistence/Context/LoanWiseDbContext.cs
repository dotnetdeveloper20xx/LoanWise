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
            // Loan.Amount
            modelBuilder.Entity<Loan>(builder =>
            {
                builder.OwnsOne(l => l.Amount, amt =>
                {
                    amt.Property(a => a.Value).HasColumnName("AmountValue").IsRequired();
                    amt.Property(a => a.Currency).HasColumnName("AmountCurrency").HasDefaultValue("GBP").IsRequired();
                });
            });

            // EscrowTransaction.Amount
            modelBuilder.Entity<EscrowTransaction>(builder =>
            {
                builder.OwnsOne(e => e.Amount, amt =>
                {
                    amt.Property(a => a.Value).HasColumnName("EscrowAmountValue").IsRequired();
                    amt.Property(a => a.Currency).HasColumnName("EscrowAmountCurrency").HasDefaultValue("GBP").IsRequired();
                });
            });

            // Funding.Amount + prevent cascade delete
            modelBuilder.Entity<Funding>(builder =>
            {
                builder.OwnsOne(f => f.Amount, amt =>
                {
                    amt.Property(a => a.Value).HasColumnName("FundingAmountValue").IsRequired();
                    amt.Property(a => a.Currency).HasColumnName("FundingAmountCurrency").HasDefaultValue("GBP").IsRequired();
                });

                builder
                    .HasOne(f => f.Lender)
                    .WithMany() // or WithMany(l => l.Fundings) if reverse exists
                    .HasForeignKey(f => f.LenderId)
                    .OnDelete(DeleteBehavior.Restrict); // avoid cascade path issue
            });

            // Repayment.Amount
            modelBuilder.Entity<Repayment>(builder =>
            {
                builder.OwnsOne(r => r.Amount, amt =>
                {
                    amt.Property(a => a.Value).HasColumnName("RepaymentAmountValue").IsRequired();
                    amt.Property(a => a.Currency).HasColumnName("RepaymentAmountCurrency").HasDefaultValue("GBP").IsRequired();
                });
            });

            // CreditProfile
            modelBuilder.Entity<CreditProfile>(builder =>
            {
                builder.HasKey(c => c.UserId);

                builder
                    .HasOne(c => c.User)
                    .WithOne()
                    .HasForeignKey<CreditProfile>(c => c.UserId);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}

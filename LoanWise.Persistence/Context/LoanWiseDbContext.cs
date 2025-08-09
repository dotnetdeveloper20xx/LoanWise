using System.Linq;
using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Common;
using LoanWise.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanWise.Persistence.Context
{
    /// <summary>
    /// The EF Core database context for the LoanWise application.
    /// </summary>
    public class LoanWiseDbContext : DbContext, IApplicationDbContext
    {
        private readonly IDomainEventDispatcher? _eventDispatcher;

        // Runtime constructor with domain event dispatcher
        public LoanWiseDbContext(
            DbContextOptions<LoanWiseDbContext> options,
            IDomainEventDispatcher eventDispatcher)
            : base(options)
        {
            _eventDispatcher = eventDispatcher;
        }

        // Design-time constructor for EF tools (no dispatcher needed)
        public LoanWiseDbContext(DbContextOptions<LoanWiseDbContext> options)
            : base(options)
        {
            _eventDispatcher = null;
        }

        public DbSet<Loan> Loans => Set<Loan>();
        public DbSet<Repayment> Repayments => Set<Repayment>();
        public DbSet<Funding> Fundings => Set<Funding>();
        public DbSet<VerificationDocument> VerificationDocuments => Set<VerificationDocument>();
        public DbSet<SystemEvent> SystemEvents => Set<SystemEvent>();
        public DbSet<CreditProfile> CreditProfiles => Set<CreditProfile>();
        public DbSet<EscrowTransaction> EscrowTransactions => Set<EscrowTransaction>();
        public DbSet<User> Users { get; set; } = default!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = default!;
        public DbSet<Notification> Notifications { get; set; } = default!;

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Collect domain events BEFORE saving
            var entitiesWithEvents = ChangeTracker
                .Entries<Entity>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity)
                .ToList();

            var domainEvents = entitiesWithEvents
                .SelectMany(e => e.DomainEvents)
                .ToList();

            // Clear events on the entities
            entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

            var result = await base.SaveChangesAsync(cancellationToken);

            // Dispatch AFTER commit (simple approach; for guaranteed delivery use Outbox)
            if (domainEvents.Count > 0 && _eventDispatcher is not null)
            {
                await _eventDispatcher.DispatchAsync(domainEvents, cancellationToken);
            }

            return result;
        }

        // TODO: move these into configuration classes.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Loan.Amount (Money VO)
            modelBuilder.Entity<Loan>(builder =>
            {
                builder.OwnsOne(l => l.Amount, amt =>
                {
                    amt.Property(a => a.Value)
                       .HasColumnName("AmountValue")
                       .IsRequired();

                    amt.Property(a => a.Currency)
                       .HasColumnName("AmountCurrency")
                       .HasDefaultValue("GBP")
                       .IsRequired();
                });
            });

            // EscrowTransaction.Amount (Money VO)
            modelBuilder.Entity<EscrowTransaction>(builder =>
            {
                builder.OwnsOne(e => e.Amount, amt =>
                {
                    amt.Property(a => a.Value)
                       .HasColumnName("EscrowAmountValue")
                       .IsRequired();

                    amt.Property(a => a.Currency)
                       .HasColumnName("EscrowAmountCurrency")
                       .HasDefaultValue("GBP")
                       .IsRequired();
                });
            });

            // Funding.Amount (Money VO) + prevent cascade delete to Lender
            modelBuilder.Entity<Funding>(builder =>
            {
                builder.OwnsOne(f => f.Amount, amt =>
                {
                    amt.Property(a => a.Value)
                       .HasColumnName("FundingAmountValue")
                       .IsRequired();

                    amt.Property(a => a.Currency)
                       .HasColumnName("FundingAmountCurrency")
                       .HasDefaultValue("GBP")
                       .IsRequired();
                });

                builder
                    .HasOne(f => f.Lender)
                    .WithMany()
                    .HasForeignKey(f => f.LenderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Repayment.Amount: you said this is now a DECIMAL property named RepaymentAmount
            modelBuilder.Entity<Repayment>(builder =>
            {
                builder.Property(r => r.RepaymentAmount)
                       .HasColumnName("RepaymentAmount")
                       .HasColumnType("decimal(18,2)")
                       .IsRequired();
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

            // RefreshTokens
            modelBuilder.Entity<RefreshToken>(builder =>
            {
                builder.HasKey(rt => rt.Id);

                builder.Property(rt => rt.TokenHash)
                       .IsRequired()
                       .HasMaxLength(256);

                builder.HasIndex(rt => new { rt.UserId, rt.TokenHash })
                       .IsUnique();

                builder.HasOne<User>()
                       .WithMany() // or .WithMany(u => u.RefreshTokens) if you add a nav property
                       .HasForeignKey(rt => rt.UserId);
            });

            // Prevent cascading deletes globally unless explicitly configured
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var foreignKey in entityType.GetForeignKeys())
                {
                    if (!foreignKey.IsOwnership && foreignKey.DeleteBehavior == DeleteBehavior.Cascade)
                    {
                        foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
                    }
                }
            }
        }
    }
}

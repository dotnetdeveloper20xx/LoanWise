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


        // Core lending
        public DbSet<Loan> Loans => Set<Loan>();
        public DbSet<Funding> Fundings => Set<Funding>();
        public DbSet<Repayment> Repayments => Set<Repayment>();

        // User & security
        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        // Support & operations
        public DbSet<VerificationDocument> VerificationDocuments => Set<VerificationDocument>();
        public DbSet<CreditProfile> CreditProfiles => Set<CreditProfile>();
        public DbSet<SystemEvent> SystemEvents => Set<SystemEvent>();
        public DbSet<Notification> Notifications => Set<Notification>();

        // Financial operations
        public DbSet<EscrowTransaction> EscrowTransactions => Set<EscrowTransaction>();

        public DbSet<LenderRepayment> LenderRepayments => Set<LenderRepayment>();

        public DbSet<BorrowerRiskSnapshot> BorrowerRiskSnapshots => Set<BorrowerRiskSnapshot>();
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
             

            // Funding.Amount (Money VO) + prevent cascade delete to Lender
            modelBuilder.Entity<Funding>(builder =>
            {               

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

            modelBuilder.Entity<LenderRepayment>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
                b.HasIndex(x => new { x.LenderId, x.LoanId });
                b.HasIndex(x => x.RepaymentId).IsUnique(false);

                b.HasOne<Loan>().WithMany().HasForeignKey(x => x.LoanId);
                b.HasOne<Repayment>().WithMany().HasForeignKey(x => x.RepaymentId);
                b.HasOne<User>().WithMany().HasForeignKey(x => x.LenderId);
            });


            modelBuilder.Entity<BorrowerRiskSnapshot>(b =>
            {
                b.HasKey(x => x.BorrowerId);
                b.Property(x => x.RiskTier).HasMaxLength(32);
                b.Property(x => x.KycStatus).HasMaxLength(32);
                b.Property(x => x.FlagsJson).HasMaxLength(2000);
                b.HasIndex(x => x.KycStatus);
                b.HasIndex(x => x.LastVerifiedAtUtc);
                b.HasIndex(x => x.LastScoreAtUtc);
            });

            modelBuilder.Entity<BorrowerRiskSnapshot>(b =>
            {
                b.HasIndex(x => x.KycStatus);
                b.HasIndex(x => x.LastVerifiedAtUtc);
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

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Common;
using LoanWise.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanWise.Persistence.Context
{
    /// <summary>
    /// EF Core DbContext for LoanWise.
    /// Implements <see cref="IApplicationDbContext"/> for DI/testability.
    /// </summary>
    public class LoanWiseDbContext : DbContext, IApplicationDbContext
    {
        private readonly IDomainEventDispatcher? _eventDispatcher;

        // Runtime constructor (with domain event dispatcher)
        public LoanWiseDbContext(
            DbContextOptions<LoanWiseDbContext> options,
            IDomainEventDispatcher eventDispatcher)
            : base(options)
        {
            _eventDispatcher = eventDispatcher;
        }

        // Design-time constructor (EF tools)
        public LoanWiseDbContext(DbContextOptions<LoanWiseDbContext> options)
            : base(options)
        {
            _eventDispatcher = null;
        }

        // ============== DbSets (match IApplicationDbContext) ==============

        // Core lending
        public DbSet<Loan> Loans => Set<Loan>();
        public DbSet<Repayment> Repayments => Set<Repayment>();
        public DbSet<Funding> Fundings => Set<Funding>();
        public DbSet<LenderRepayment> LenderRepayments => Set<LenderRepayment>();

        // Users & security
        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        // Operations / compliance
        public DbSet<VerificationDocument> VerificationDocuments => Set<VerificationDocument>();
        public DbSet<SystemEvent> SystemEvents => Set<SystemEvent>();
        public DbSet<CreditProfile> CreditProfiles => Set<CreditProfile>();
        public DbSet<EscrowTransaction> EscrowTransactions => Set<EscrowTransaction>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<BorrowerRiskSnapshot> BorrowerRiskSnapshots => Set<BorrowerRiskSnapshot>();

        // ============== SaveChanges with domain events ==============

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Gather domain events BEFORE saving
            var entitiesWithEvents = ChangeTracker
                .Entries<Entity>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity)
                .ToList();

            var domainEvents = entitiesWithEvents
                .SelectMany(e => e.DomainEvents)
                .ToList();

            // Clear them so they don't dispatch twice
            entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

            // Commit
            var result = await base.SaveChangesAsync(cancellationToken);

            // Dispatch AFTER commit (for guaranteed delivery, consider outbox)
            if (domainEvents.Count > 0 && _eventDispatcher is not null)
            {
                await _eventDispatcher.DispatchAsync(domainEvents, cancellationToken);
            }

            return result;
        }

        // Explicit interface implementation (helps certain analyzers)
        Task<int> IApplicationDbContext.SaveChangesAsync(CancellationToken cancellationToken)
            => SaveChangesAsync(cancellationToken);

        // ============== Model configuration ==============

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Loan
            modelBuilder.Entity<Loan>(b =>
            {
                b.Property(x => x.Amount).HasPrecision(18, 2);

                // Concurrency token if RowVersion exists on base Entity
                b.Property(l => l.RowVersion).IsRowVersion();
            });

            // Funding
            modelBuilder.Entity<Funding>(b =>
            {
                b.Property(x => x.Amount).HasPrecision(18, 2);

                b.HasOne(f => f.Lender)
                 .WithMany()
                 .HasForeignKey(f => f.LenderId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // Repayment (RepaymentAmount is decimal column)
            modelBuilder.Entity<Repayment>(b =>
            {
                b.Property(r => r.RepaymentAmount)
                 .HasColumnName("RepaymentAmount")
                 .HasColumnType("decimal(18,2)")
                 .IsRequired();
            });

            // CreditProfile (1:1 with User)
            modelBuilder.Entity<CreditProfile>(b =>
            {
                b.HasKey(c => c.UserId);

                b.HasOne(c => c.User)
                 .WithOne()
                 .HasForeignKey<CreditProfile>(c => c.UserId);
            });

            // RefreshToken
            modelBuilder.Entity<RefreshToken>(b =>
            {
                b.HasKey(rt => rt.Id);

                b.Property(rt => rt.TokenHash)
                 .IsRequired()
                 .HasMaxLength(256);

                b.HasIndex(rt => new { rt.UserId, rt.TokenHash }).IsUnique();

                b.HasOne<User>()
                 .WithMany() // or add nav on User if desired
                 .HasForeignKey(rt => rt.UserId);
            });

            // LenderRepayment
            modelBuilder.Entity<LenderRepayment>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
                b.HasIndex(x => new { x.LenderId, x.LoanId });
                b.HasIndex(x => x.RepaymentId);

                b.HasOne<Loan>().WithMany().HasForeignKey(x => x.LoanId);
                b.HasOne<Repayment>().WithMany().HasForeignKey(x => x.RepaymentId);
                b.HasOne<User>().WithMany().HasForeignKey(x => x.LenderId);
            });

            // BorrowerRiskSnapshot
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

            // Global delete behavior: no cascade unless explicitly configured
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var fk in entityType.GetForeignKeys())
                {
                    if (!fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade)
                        fk.DeleteBehavior = DeleteBehavior.Restrict;
                }
            }
        }
    }
}

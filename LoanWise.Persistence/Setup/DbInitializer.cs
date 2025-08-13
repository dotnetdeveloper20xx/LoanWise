using LoanWise.Domain.Entities;
using LoanWise.Domain.Enums;
using LoanWise.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace LoanWise.Persistence.Setup
{
    /// <summary>
    /// Seeds rich data for local/QA. Order: Users -> Loans -> Fundings -> Disburse+Repayments.
    /// Uses domain methods for all state transitions; avoids touching internal fields directly.
    /// </summary>
    public static class DbInitializer
    {
        public static async Task InitializeAsync(LoanWiseDbContext db, ILogger? logger = null, CancellationToken ct = default)
        {
            // --- Apply migrations only if there are pending ones ---
            try
            {
                var pending = await db.Database.GetPendingMigrationsAsync(ct);
                if (pending.Any())
                {
                    logger?.LogInformation(
                        "Applying {Count} pending migrations to DB '{DbName}' on '{DataSource}'...",
                        pending.Count(),
                        db.Database.GetDbConnection().Database,
                        db.Database.GetDbConnection().DataSource
                    );

                    await db.Database.MigrateAsync(ct);

                    logger?.LogInformation("Migrations applied successfully.");
                }
                else
                {
                    logger?.LogInformation(
                        "No pending migrations for DB '{DbName}' on '{DataSource}'.",
                        db.Database.GetDbConnection().Database,
                        db.Database.GetDbConnection().DataSource
                    );
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Automatic migration check/apply failed. Continuing without applying migrations.");
            }

            // Idempotent guard (skip if data already present)
            if (await db.Users.AnyAsync(ct))
            {
                logger?.LogInformation("DbInitializer: Users already exist. Skipping seed.");
                return;
            }

            logger?.LogInformation("DbInitializer: Seeding started.");

            var rand = new Random(20250813);
            string SeedPasswordHash() => HashPassword("P@ssw0rd!"); // dev/test only

            // 1) USERS ----------------------------------------------------------
            var admin = new User
            {
                Id = Guid.NewGuid(),
                FullName = "System Admin",
                Email = "admin@loanwise.local",
                Role = UserRole.Admin,
                IsActive = true,
                PasswordHash = SeedPasswordHash()
            };
            admin.AssignCreditProfile(score: 820, Domain.Entities.RiskTier.High);

            var borrowers = Enumerable.Range(1, 10).Select(i =>
            {
                var u = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = $"Borrower {i}",
                    Email = $"borrower{i}@loanwise.local",
                    Role = UserRole.Borrower,
                    IsActive = true,
                    PasswordHash = SeedPasswordHash()
                };
                u.AssignCreditProfile(score: rand.Next(560, 780), Domain.Entities.RiskTier.Low);
                return u;
            }).ToList();

            var lenders = Enumerable.Range(1, 8).Select(i =>
            {
                var u = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = $"Lender {i}",
                    Email = $"lender{i}@loanwise.local",
                    Role = UserRole.Lender,
                    IsActive = true,
                    PasswordHash = SeedPasswordHash()
                };
                u.AssignCreditProfile(score: rand.Next(620, 800), Domain.Entities.RiskTier.Medium);
                return u;
            }).ToList();

            await db.Users.AddAsync(admin, ct);
            await db.Users.AddRangeAsync(borrowers, ct);
            await db.Users.AddRangeAsync(lenders, ct);
            await db.SaveChangesAsync(ct);
            logger?.LogInformation("Seeded users (1 admin, {Borrowers} borrowers, {Lenders} lenders).", borrowers.Count, lenders.Count);

            // 2) LOANS (Pending -> Approve) ------------------------------------
            var purposes = Enum.GetValues<LoanPurpose>();
            var loans = new List<Loan>();

            foreach (var b in borrowers)
            {
                var loanCount = rand.Next(1, 3); // 1–2 per borrower
                for (int i = 0; i < loanCount; i++)
                {
                    decimal amount = rand.Next(2, 21) * 1000m; // £2k–£20k
                    int months = new[] { 6, 12, 18, 24, 36 }[rand.Next(5)];
                    var purpose = purposes[rand.Next(purposes.Length)];

                    var loan = new Loan(
                        id: Guid.NewGuid(),
                        borrowerId: b.Id,
                        amount: amount,
                        durationInMonths: months,
                        purpose: purpose
                    );

                    var risk = amount switch
                    {
                        <= 5000m => RiskLevel.Low,
                        <= 12000m => RiskLevel.Medium,
                        _ => RiskLevel.High
                    };
                    loan.Approve(risk);
                    loans.Add(loan);
                }
            }

            await db.Loans.AddRangeAsync(loans, ct);
            await db.SaveChangesAsync(ct);
            logger?.LogInformation("Seeded and approved {LoanCount} loans.", loans.Count);

            // 3) FUNDINGS (domain: AddFunding + UpdateFundingStatus) -----------
            var allFundings = new List<Funding>();

            foreach (var loan in loans)
            {
                // ~80% of loans receive some funding
                if (rand.NextDouble() >= 0.8) continue;

                var lendersForLoan = lenders
                    .OrderBy(_ => rand.Next())
                    .Take(rand.Next(1, Math.Min(4, lenders.Count)))
                    .ToList();

                foreach (var lender in lendersForLoan)
                {
                    var contributed = allFundings.Where(f => f.LoanId == loan.Id).Sum(f => f.Amount);
                    var left = loan.Amount - contributed;
                    if (left <= 0) break;

                    var chunk = Math.Min(left, rand.Next(1, 8) * 500m); // £500–£3500
                    if (chunk <= 0) break;

                    var funding = new Funding(
                        id: Guid.NewGuid(),
                        lenderId: lender.Id,
                        loanId: loan.Id,
                        amount: chunk,
                        fundedOn: DateTime.UtcNow.AddDays(-rand.Next(0, 20))
                    );

                    loan.AddFunding(funding);          // raises FundingAddedEvent
                    loan.UpdateFundingStatus(funding); // may raise LoanFundedEvent
                    allFundings.Add(funding);

                    if (loan.IsFullyFunded()) break;
                }
            }

            await db.Fundings.AddRangeAsync(allFundings, ct);
            await db.SaveChangesAsync(ct);
            logger?.LogInformation("Added {FundingCount} funding records.", allFundings.Count);

            // 4) DISBURSE & GENERATE REPAYMENTS --------------------------------
            var fundedLoans = await db.Loans.Where(l => l.Status == LoanStatus.Funded).ToListAsync(ct);
            var disbursedCount = 0;

            foreach (var loan in fundedLoans)
            {
                if (rand.NextDouble() < 0.55)
                {
                    loan.Disburse();                   // raises LoanDisbursedEvent
                    loan.GenerateRepaymentSchedule();  // creates repayments inside aggregate
                    disbursedCount++;
                }
            }

            await db.SaveChangesAsync(ct);

            // Mark a subset of early repayments as paid/overdue (null-safe borrower lookup)
            var repayEntries = await (
                from r in db.Repayments
                join l in db.Loans on r.LoanId equals l.Id into lj
                from l in lj.DefaultIfEmpty() // left join (safety)
                where r.DueDate < DateTime.UtcNow.AddMonths(3)
                select new
                {
                    Repayment = r,
                    BorrowerId = l != null ? l.BorrowerId : Guid.Empty
                }
            ).ToListAsync(ct);

            foreach (var entry in repayEntries)
            {
                var r = entry.Repayment;
                var borrowerId = entry.BorrowerId;

                if (r.DueDate < DateTime.UtcNow.AddDays(-7) && rand.NextDouble() < 0.35)
                {
                    // leave unpaid (overdue)
                }
                else if (r.DueDate < DateTime.UtcNow && rand.NextDouble() < 0.55 && borrowerId != Guid.Empty)
                {
                    r.MarkAsPaid(r.DueDate.AddDays(rand.Next(0, 7)), borrowerId);
                }
            }

            await db.SaveChangesAsync(ct);
            logger?.LogInformation("Disbursed {Disbursed} loans and generated repayment schedules.", disbursedCount);
            logger?.LogInformation("DbInitializer: Seeding completed successfully.");
        }

        // Simple SHA-256 for seed-only passwords (replace with your app's real hasher if needed)
        private static string HashPassword(string raw)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return Convert.ToBase64String(bytes);
        }
    }
}

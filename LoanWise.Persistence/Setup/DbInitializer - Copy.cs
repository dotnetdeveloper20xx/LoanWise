using System;
using System.Linq;
using System.Collections.Generic;
using LoanWise.Domain.Entities;
using LoanWise.Domain.Enums;
using LoanWise.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LoanWise.Persistence.Setup
{
    /// <summary>
    /// Seeds rich data for local/QA. Order strictly enforced:
    /// 1) Users (Admin, Borrowers, Lenders)
    /// 2) Loans (created + approved)
    /// 3) Fundings (partial + full)
    /// 4) Disburse funded subset + generate repayments
    /// 5) Randomly mark some repayments paid/overdue
    ///
    /// Notes:
    /// - Uses domain methods only (Approve/Disburse/AddFunding/GenerateRepaymentSchedule/MarkAsPaid).
    /// - Calls SaveChanges between parent -> child transitions.
    /// - "Top-up" behavior: if rows already exist, we add more (idempotent-ish).
    /// - Passwords use Identity's PasswordHasher<User>.
    /// </summary>
    public static class DbInitializer1
    {
        public static async Task InitializeAsync(
            LoanWiseDbContext db,
            ILogger? logger = null,
            CancellationToken ct = default)
        {
            // --- Apply pending migrations (best-effort) -----------------------
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
                    logger?.LogInformation("Migrations applied.");
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

            logger?.LogInformation("DbInitializer: Seeding started.");

            // Improve bulk seeding performance
            var originalAutoDetect = db.ChangeTracker.AutoDetectChangesEnabled;
            db.ChangeTracker.AutoDetectChangesEnabled = false;

            try
            {
                var rand = new Random(20250813);
                var passwordHasher = new PasswordHasher<User>();
                string PlainPassword = "P@ssw0rd!"; // dev/test only

                // --- Targets ----------------------------------------------------
                const int targetBorrowers = 3;   // <-- updated
                const int targetLenders = 2;   // <-- updated
                const int targetLoansMin = 18;  // trimmed for small user set
                const int targetLoansMax = 36;

                // -----------------------------------------------------------------
                // 1) USERS (parents)
                // -----------------------------------------------------------------
                var existingUsers = await db.Users.AsNoTracking().ToListAsync(ct);
                var existingBorrowers = existingUsers.Where(u => u.Role == UserRole.Borrower).ToList();
                var existingLenders = existingUsers.Where(u => u.Role == UserRole.Lender).ToList();
                var adminExists = existingUsers.Any(u => u.Role == UserRole.Admin);

                var toAddUsers = new List<User>();

                if (!adminExists)
                {
                    var admin = new User
                    {
                        Id = Guid.NewGuid(),
                        FullName = "System Admin",
                        Email = "admin@loanwise.local",
                        Role = UserRole.Admin,
                        IsActive = true
                    };
                    admin.PasswordHash = passwordHasher.HashPassword(admin, PlainPassword);
                    admin.AssignCreditProfile(score: 820, LoanWise.Domain.Entities.RiskTier.High);
                    toAddUsers.Add(admin);
                }

                int borrowersNeeded = Math.Max(0, targetBorrowers - existingBorrowers.Count);
                for (int i = 1; i <= borrowersNeeded; i++)
                {
                    var u = new User
                    {
                        Id = Guid.NewGuid(),
                        FullName = $"Borrower {existingBorrowers.Count + i}",
                        Email = $"borrower{existingBorrowers.Count + i}@loanwise.local",
                        Role = UserRole.Borrower,
                        IsActive = true
                    };
                    u.PasswordHash = passwordHasher.HashPassword(u, PlainPassword);
                    u.AssignCreditProfile(score: rand.Next(560, 780), LoanWise.Domain.Entities.RiskTier.Low);
                    toAddUsers.Add(u);
                }

                int lendersNeeded = Math.Max(0, targetLenders - existingLenders.Count);
                for (int i = 1; i <= lendersNeeded; i++)
                {
                    var u = new User
                    {
                        Id = Guid.NewGuid(),
                        FullName = $"Lender {existingLenders.Count + i}",
                        Email = $"lender{existingLenders.Count + i}@loanwise.local",
                        Role = UserRole.Lender,
                        IsActive = true
                    };
                    u.PasswordHash = passwordHasher.HashPassword(u, PlainPassword);
                    u.AssignCreditProfile(score: rand.Next(620, 800), LoanWise.Domain.Entities.RiskTier.Medium);
                    toAddUsers.Add(u);
                }

                if (toAddUsers.Count > 0)
                {
                    await db.Users.AddRangeAsync(toAddUsers, ct);
                    await db.SaveChangesAsync(ct);
                    logger?.LogInformation(
                        "Seeded users: +{Admins} admin(s), +{Borrowers} borrowers, +{Lenders} lenders.",
                        toAddUsers.Count(u => u.Role == UserRole.Admin),
                        toAddUsers.Count(u => u.Role == UserRole.Borrower),
                        toAddUsers.Count(u => u.Role == UserRole.Lender));
                }
                else
                {
                    logger?.LogInformation("Users already meet/exceed targets. No new users added.");
                }

                // reload small projections used below
                var borrowers = await db.Users
                    .AsNoTracking()
                    .Where(u => u.Role == UserRole.Borrower && u.IsActive)
                    .Select(u => new { u.Id })
                    .ToListAsync(ct);

                var lenders = await db.Users
                    .AsNoTracking()
                    .Where(u => u.Role == UserRole.Lender && u.IsActive)
                    .Select(u => new { u.Id })
                    .ToListAsync(ct);

                if (borrowers.Count == 0 || lenders.Count == 0)
                {
                    logger?.LogWarning("Insufficient parent data (borrowers: {B}, lenders: {L}). Aborting seed.", borrowers.Count, lenders.Count);
                    return;
                }

                // -----------------------------------------------------------------
                // 2) LOANS (children of Users, but parents of Fundings/Repayments)
                // -----------------------------------------------------------------
                var purposes = Enum.GetValues<LoanPurpose>();
                int existingLoansCount = await db.Loans.AsNoTracking().CountAsync(ct);
                int targetLoans = rand.Next(targetLoansMin, targetLoansMax + 1);
                int loansNeeded = Math.Max(0, targetLoans - existingLoansCount);

                var newLoans = new List<Loan>();
                while (loansNeeded > 0)
                {
                    // randomly pick a borrower for each loan
                    var borrowerId = borrowers[rand.Next(borrowers.Count)].Id;

                    decimal amount = rand.Next(2, 31) * 1000m;        // £2k–£30k (tighter for small dataset)
                    int months = new[] { 6, 12, 18, 24, 36, 48 }[rand.Next(6)];
                    var purpose = purposes[rand.Next(purposes.Length)];

                    var loan = new Loan(
                        id: Guid.NewGuid(),
                        borrowerId: borrowerId,
                        amount: amount,
                        durationInMonths: months,
                        purpose: purpose
                    );

                    var risk = amount switch
                    {
                        <= 5000m => RiskLevel.Low,
                        <= 12000m => RiskLevel.Medium,
                        <= 20000m => RiskLevel.Medium,
                        _ => RiskLevel.High
                    };

                    loan.Approve(risk);
                    newLoans.Add(loan);
                    loansNeeded--;
                }

                if (newLoans.Count > 0)
                {
                    await db.Loans.AddRangeAsync(newLoans, ct);
                    await db.SaveChangesAsync(ct);
                    logger?.LogInformation("Seeded and approved {LoanCount} new loans (total now ~{Total}).", newLoans.Count, existingLoansCount + newLoans.Count);
                }
                else
                {
                    logger?.LogInformation("Loans already meet/exceed target (~{Total}). No new loans added.", existingLoansCount);
                }

                // Get a working set of loans (both existing & newly added) for funding
                var loansForFunding = await db.Loans
                    .AsNoTracking()
                    .Where(l => l.Status == LoanStatus.Approved || l.Status == LoanStatus.Funded)
                    .Select(l => new { l.Id, l.Amount })
                    .ToListAsync(ct);

                // -----------------------------------------------------------------
                // 3) FUNDINGS (children of Loans & Users)
                // -----------------------------------------------------------------
                var existingFundingCount = await db.Fundings.AsNoTracking().CountAsync(ct);
                var newFundings = new List<Funding>();

                foreach (var loan in loansForFunding.OrderBy(_ => rand.Next()))
                {
                    // Skip some to keep a mix of funded vs not fully funded
                    if (rand.NextDouble() < 0.10) continue;

                    // target: 1–2 funders per loan (since we only have 2 lenders)
                    int funders = rand.Next(1, Math.Min(3, lenders.Count + 1));
                    decimal contributedSoFar = 0m;

                    var lenderPool = lenders.OrderBy(_ => rand.Next()).Take(funders).ToList();
                    foreach (var lender in lenderPool)
                    {
                        var remaining = loan.Amount - contributedSoFar;
                        if (remaining <= 0) break;

                        // Allocate 15–60% of remaining (bounded)
                        var allocation = Math.Max(250m, Math.Min(remaining, Math.Round(remaining * (decimal)(0.15 + rand.NextDouble() * 0.45), 2)));

                        var funding = new Funding(
                            id: Guid.NewGuid(),
                            lenderId: lender.Id,
                            loanId: loan.Id,
                            amount: allocation,
                            fundedOn: DateTime.UtcNow.AddDays(-rand.Next(0, 20))
                        );

                        newFundings.Add(funding);
                        contributedSoFar += allocation;

                        // 50% chance to stop early (simulate partial funding)
                        if (rand.NextDouble() < 0.50) break;
                    }
                }

                if (newFundings.Count > 0)
                {
                    await db.Fundings.AddRangeAsync(newFundings, ct);
                    await db.SaveChangesAsync(ct);
                    logger?.LogInformation("Added {FundingCount} funding records (total before was {Existing}).", newFundings.Count, existingFundingCount);
                }
                else
                {
                    logger?.LogInformation("No additional funding required.");
                }

                // Reflect funding inside aggregates (update statuses)
                var trackLoans = await db.Loans
                    .Include(l => l.Fundings)
                    .Where(l => l.Status == LoanStatus.Approved || l.Status == LoanStatus.Funded)
                    .ToListAsync(ct);

                foreach (var loan in trackLoans)
                {
                    foreach (var funding in loan.Fundings)
                    {
                        loan.UpdateFundingStatus(funding);
                    }
                }
                await db.SaveChangesAsync(ct);

                // -----------------------------------------------------------------
                // 4) DISBURSE funded subset + generate repayments
                // -----------------------------------------------------------------
                var fundedLoans = await db.Loans
                    .Where(l => l.Status == LoanStatus.Funded)
                    .ToListAsync(ct);

                int disbursedCount = 0;
                foreach (var loan in fundedLoans)
                {
                    if (rand.NextDouble() < 0.55)
                    {
                        loan.Disburse();
                        loan.GenerateRepaymentSchedule();
                        disbursedCount++;
                    }
                }
                await db.SaveChangesAsync(ct);

                logger?.LogInformation("Disbursed {Disbursed} loans and generated repayment schedules.", disbursedCount);

                // -----------------------------------------------------------------
                // 5) Randomly mark some repayments paid/overdue
                // -----------------------------------------------------------------
                var repayEntries = await (
                    from r in db.Repayments
                    join l in db.Loans on r.LoanId equals l.Id into lj
                    from l in lj.DefaultIfEmpty()
                    select new
                    {
                        RepaymentId = r.Id,
                        LoanId = r.LoanId,
                        DueDate = r.DueDate,
                        IsPaid = r.IsPaid,
                        BorrowerId = l != null ? l.BorrowerId : Guid.Empty
                    }
                ).ToListAsync(ct);

                var toUpdateRepayments = await db.Repayments
                    .Where(r => repayEntries.Any(x => x.RepaymentId == r.Id))
                    .ToListAsync(ct);

                var lookup = repayEntries.ToDictionary(x => x.RepaymentId, x => x);

                int paidCount = 0, overdueLeftUnpaid = 0;
                foreach (var r in toUpdateRepayments)
                {
                    var meta = lookup[r.Id];
                    if (meta.IsPaid) continue;

                    if (meta.DueDate < DateTime.UtcNow.AddMonths(3))
                    {
                        if (meta.DueDate < DateTime.UtcNow.AddDays(-7) && rand.NextDouble() < 0.35)
                        {
                            overdueLeftUnpaid++;
                        }
                        else if (meta.DueDate <= DateTime.UtcNow && rand.NextDouble() < 0.55 && meta.BorrowerId != Guid.Empty)
                        {
                            var paidOn = meta.DueDate.AddDays(rand.Next(0, 10));
                            r.MarkAsPaid(paidOn, meta.BorrowerId);
                            paidCount++;
                        }
                    }
                }

                await db.SaveChangesAsync(ct);
                logger?.LogInformation("Repayments updated: {Paid} marked paid, {Overdue} left overdue.", paidCount, overdueLeftUnpaid);

                logger?.LogInformation("DbInitializer: Seeding completed successfully.");
            }
            finally
            {
                db.ChangeTracker.AutoDetectChangesEnabled = originalAutoDetect;
            }
        }
    }
}

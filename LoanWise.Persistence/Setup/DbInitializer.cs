using LoanWise.Domain.Entities;
using LoanWise.Domain.Enums;
using LoanWise.Domain.ValueObjects;
using LoanWise.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LoanWise.Persistence.Setup
{
    /// <summary>
    /// Seeds initial data into the LoanWise database.
    /// </summary>
    public static class DbInitializer
    {
        public static async Task InitializeAsync(LoanWiseDbContext context, ILogger logger)
        {
            await context.Database.MigrateAsync();

            // short-circuit if already seeded
            if (await context.Users.AnyAsync())
            {
                logger.LogInformation("Users already exist. Skipping seeding.");
                return;
            }

            await using var tx = await context.Database.BeginTransactionAsync();

            try
            {
                logger.LogInformation("Seeding users...");

                const string demoPassword = "demo123"; // demo/testing only
                var hasher = new PasswordHasher<User>();

                // 1) Create users (Borrowers, Lenders, Admin) with hashed passwords
                var newUsers = new List<User>();

                // Borrowers
                for (int i = 1; i <= 10; i++)
                {
                    var u = new User
                    {
                        Id = Guid.NewGuid(),
                        FullName = $"Borrower {i}",
                        Email = $"borrower{i}@demo.com",
                        Role = UserRole.Borrower
                    };
                    u.PasswordHash = hasher.HashPassword(u, demoPassword);
                    newUsers.Add(u);
                }

                // Lenders
                for (int i = 1; i <= 5; i++)
                {
                    var u = new User
                    {
                        Id = Guid.NewGuid(),
                        FullName = $"Lender {i}",
                        Email = $"lender{i}@demo.com",
                        Role = UserRole.Lender
                    };
                    u.PasswordHash = hasher.HashPassword(u, demoPassword);
                    newUsers.Add(u);
                }

                // Admin
                var admin = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "Admin",
                    Email = "admin@loanwise.com",
                    Role = UserRole.Admin
                };
                admin.PasswordHash = hasher.HashPassword(admin, demoPassword);
                newUsers.Add(admin);

                context.Users.AddRange(newUsers);
                await context.SaveChangesAsync(); // ensure IDs are persisted

                // 2) Re-query borrowers & lenders from DB to avoid FK issues
                var borrowers = await context.Users
                    .AsNoTracking()
                    .Where(u => u.Role == UserRole.Borrower)
                    .ToListAsync();

                var lenderIds = await context.Users
                    .AsNoTracking()
                    .Where(u => u.Role == UserRole.Lender)
                    .Select(u => u.Id)
                    .ToListAsync();

                if (lenderIds.Count == 0)
                {
                    logger.LogWarning("No lenders found after seeding; skipping funding/loans.");
                    await tx.CommitAsync();
                    return;
                }

                logger.LogInformation("Seeding loans and related entities...");

                var rnd = new Random();
                var purposes = Enum.GetValues(typeof(LoanPurpose)).Cast<LoanPurpose>().ToArray();
                var riskLevels = Enum.GetValues(typeof(RiskLevel)).Cast<RiskLevel>().ToArray();

                var loans = new List<Loan>();

                foreach (var borrower in borrowers)
                {
                    // 3 loans per borrower
                    for (int i = 0; i < 3; i++)
                    {
                        var amount = new Money(rnd.Next(1000, 20000));
                        var duration = rnd.Next(6, 24);
                        var purpose = purposes[rnd.Next(purposes.Length)];

                        var loan = new Loan(Guid.NewGuid(), borrower.Id, amount, duration, purpose);
                        loan.Approve(riskLevels[rnd.Next(riskLevels.Length)]);

                        // pick up to 3 distinct lenders from DB
                        var pickedLenders = lenderIds
                            .OrderBy(_ => rnd.Next())
                            .Take(Math.Min(3, lenderIds.Count))
                            .ToList();

                        foreach (var lenderId in pickedLenders)
                        {
                            var fundingAmount = rnd.Next(500, Math.Max(600, (int)(amount.Value / 2)));
                            var funding = new Funding(
                                Guid.NewGuid(),
                                loan.Id,
                                lenderId, // FK guaranteed to exist
                                new Money(fundingAmount),
                                DateTime.UtcNow.AddDays(-i)
                            );
                            loan.AddFunding(funding);
                        }

                        loan.UpdateFundingStatus();

                        if (loan.IsFullyFunded())
                        {
                            loan.Disburse();
                            loan.GenerateRepaymentSchedule();
                        }

                        loans.Add(loan);
                    }
                }

                // Add loans (cascades should insert Fundings/Repayments if configured)
                context.Loans.AddRange(loans);
                await context.SaveChangesAsync();

                await tx.CommitAsync();

                logger.LogInformation(
                    "Seed completed: {Borrowers} borrowers, {Lenders} lenders, {Loans} loans.",
                    borrowers.Count, lenderIds.Count, loans.Count);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                logger.LogError(ex, "Seeding failed and was rolled back.");
                throw;
            }
        }
    }
}

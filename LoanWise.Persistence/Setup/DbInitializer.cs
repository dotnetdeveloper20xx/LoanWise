using LoanWise.Domain.Entities;
using LoanWise.Domain.Enums;
using LoanWise.Domain.ValueObjects;
using LoanWise.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LoanWise.Persistence.Setup
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(LoanWiseDbContext context, ILogger logger)
        {
            // Apply migrations
            if ((await context.Database.GetPendingMigrationsAsync()).Any())
            {
                logger.LogInformation("Applying migrations...");
                await context.Database.MigrateAsync();
            }

            const string demoPassword = "demo123";
            var hasher = new PasswordHasher<User>();
            var rnd = new Random();

            // =====================================================
            // PHASE 1: USERS
            // =====================================================
            logger.LogInformation("Seeding users...");

            // Borrowers
            for (int i = 1; i <= 10; i++)
            {
                var email = $"borrower{i}@demo.com";
                if (!await context.Users.AnyAsync(u => u.Email == email))
                {
                    var user = new User
                    {
                        Id = Guid.NewGuid(),
                        FullName = $"Borrower {i}",
                        Email = email,
                        Role = UserRole.Borrower
                    };
                    user.PasswordHash = hasher.HashPassword(user, demoPassword);
                    context.Users.Add(user);
                }
            }

            // Lenders
            for (int i = 1; i <= 5; i++)
            {
                var email = $"lender{i}@demo.com";
                if (!await context.Users.AnyAsync(u => u.Email == email))
                {
                    var user = new User
                    {
                        Id = Guid.NewGuid(),
                        FullName = $"Lender {i}",
                        Email = email,
                        Role = UserRole.Lender
                    };
                    user.PasswordHash = hasher.HashPassword(user, demoPassword);
                    context.Users.Add(user);
                }
            }

            // Admin
            var adminEmail = "admin@loanwise.com";
            if (!await context.Users.AnyAsync(u => u.Email == adminEmail))
            {
                var admin = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "Admin",
                    Email = adminEmail,
                    Role = UserRole.Admin
                };
                admin.PasswordHash = hasher.HashPassword(admin, demoPassword);
                context.Users.Add(admin);
            }

            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var borrowers = await context.Users.AsNoTracking()
                .Where(u => u.Role == UserRole.Borrower)
                .ToListAsync();

            var lenderIds = await context.Users.AsNoTracking()
                .Where(u => u.Role == UserRole.Lender)
                .Select(u => u.Id)
                .ToListAsync();

            if (!borrowers.Any() || !lenderIds.Any())
            {
                logger.LogWarning("No borrowers or lenders found, aborting loan/funding seeding.");
                return;
            }

            // =====================================================
            // PHASE 2: LOANS
            // =====================================================
            logger.LogInformation("Seeding loans...");

            var purposes = Enum.GetValues(typeof(LoanPurpose)).Cast<LoanPurpose>().ToArray();
            var riskLevels = Enum.GetValues(typeof(RiskLevel)).Cast<RiskLevel>().ToArray();
            var loansToInsert = new List<Loan>();

            foreach (var borrower in borrowers)
            {
                for (int i = 0; i < 3; i++)
                {
                    var amount = new Money(rnd.Next(1000, 20000));
                    var duration = rnd.Next(6, 24);
                    var purpose = purposes[rnd.Next(purposes.Length)];

                    var loan = new Loan(Guid.NewGuid(), borrower.Id, amount, duration, purpose);
                    loan.Approve(riskLevels[rnd.Next(riskLevels.Length)]);
                    loansToInsert.Add(loan);
                }
            }

            context.Loans.AddRange(loansToInsert);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            // Pull persisted loans from DB
            var persistedLoans = await context.Loans
                .AsNoTracking()
                .Select(l => new { l.Id, l.Amount })
                .ToListAsync();

            logger.LogInformation("Loans in DB after save: {count}", persistedLoans.Count);
            foreach (var loan in persistedLoans)
                logger.LogInformation("DB LoanId: {id}", loan.Id);

            if (!persistedLoans.Any())
            {
                logger.LogError("No loans persisted, aborting funding creation.");
                return;
            }

            // =====================================================
            // PHASE 3: FUNDINGS
            // =====================================================
            logger.LogInformation("Seeding fundings...");

            var fundingsToInsert = new List<Funding>();

            foreach (var loan in persistedLoans)
            {
                var pickedLenders = lenderIds
                    .OrderBy(_ => rnd.Next())
                    .Take(Math.Min(3, lenderIds.Count))
                    .ToList();

                foreach (var lenderId in pickedLenders)
                {
                    var fundingAmount = rnd.Next(500, Math.Max(600, (int)(loan.Amount.Value / 2)));

                    fundingsToInsert.Add(new Funding(
                        Guid.NewGuid(),
                        loan.Id,        // LoanId from DB
                        lenderId,       // LenderId from DB
                        new Money(fundingAmount),
                        DateTime.UtcNow
                    ));
                }
            }

            // Debug check: ensure all funding LoanIds exist in DB
            var dbLoanIds = persistedLoans.Select(l => l.Id).ToHashSet();
            var invalidFundingIds = fundingsToInsert.Select(f => f.LoanId).Where(id => !dbLoanIds.Contains(id)).Distinct().ToList();

            if (invalidFundingIds.Any())
            {
                logger.LogError("Aborting: Found {count} funding LoanIds not in DB: {ids}",
                    invalidFundingIds.Count, string.Join(",", invalidFundingIds));
                return;
            }

            foreach (var f in fundingsToInsert)
                logger.LogInformation("Funding -> LoanId: {id}", f.LoanId);

            context.Fundings.AddRange(fundingsToInsert);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            // =====================================================
            // FINAL: UPDATE LOAN STATUS & GENERATE REPAYMENTS
            // =====================================================
            logger.LogInformation("Updating loans with funding status and generating repayments...");

            var loansWithFundings = await context.Loans
                .Include(l => l.Fundings)
                .ToListAsync();

            foreach (var loan in loansWithFundings)
            {
                loan.UpdateFundingStatus();
                if (loan.IsFullyFunded())
                {
                    loan.Disburse();
                    loan.GenerateRepaymentSchedule();
                }
            }

            await context.SaveChangesAsync();

            logger.LogInformation(
                "Seeding completed: {Borrowers} borrowers, {Lenders} lenders, {Loans} loans, {Fundings} fundings.",
                borrowers.Count, lenderIds.Count, loansToInsert.Count, fundingsToInsert.Count
            );
        }
    }
}

using LoanWise.Domain.Entities;
using LoanWise.Domain.Enums;
using LoanWise.Domain.ValueObjects;
using LoanWise.Persistence.Context;
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

            if (!context.Users.Any())
            {
                logger.LogInformation("Seeding users...");

                const string demoPassword = "demo123"; // Simple password for demo/testing

                var borrowers = Enumerable.Range(1, 10).Select(i => new User
                {
                    Id = Guid.NewGuid(),
                    FullName = $"Borrower {i}",
                    Email = $"borrower{i}@demo.com",
                    Role = UserRole.Borrower,
                    PasswordHash = demoPassword
                }).ToList();

                var lenders = Enumerable.Range(1, 5).Select(i => new User
                {
                    Id = Guid.NewGuid(),
                    FullName = $"Lender {i}",
                    Email = $"lender{i}@demo.com",
                    Role = UserRole.Lender,
                    PasswordHash = demoPassword
                }).ToList();

                var admins = new List<User>
                {
                    new User
                    {
                        Id = Guid.NewGuid(),
                        FullName = "Admin",
                        Email = "admin@loanwise.com",
                        Role = UserRole.Admin,
                        PasswordHash = demoPassword
                    }
                };

                context.Users.AddRange(borrowers);
                context.Users.AddRange(lenders);
                context.Users.AddRange(admins);

                await context.SaveChangesAsync();

                logger.LogInformation("Seeding loans and related entities...");

                var random = new Random();
                var purposes = Enum.GetValues(typeof(LoanPurpose)).Cast<LoanPurpose>().ToList();
                var riskLevels = Enum.GetValues(typeof(RiskLevel)).Cast<RiskLevel>().ToList();

                var loans = new List<Loan>();

                foreach (var borrower in borrowers)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var amount = new Money(random.Next(1000, 20000));
                        var duration = random.Next(6, 24);
                        var purpose = purposes[random.Next(purposes.Count)];

                        var loan = new Loan(Guid.NewGuid(), borrower.Id, amount, duration, purpose);
                        loan.Approve(riskLevels[random.Next(riskLevels.Count)]);

                        foreach (var lender in lenders.OrderBy(x => random.Next()).Take(3))
                        {
                            var fundingAmount = random.Next(500, (int)(amount.Value / 2));
                            var funding = new Funding(Guid.NewGuid(), loan.Id, lender.Id, new Money(fundingAmount), DateTime.UtcNow.AddDays(-i));
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

                context.Loans.AddRange(loans);
                await context.SaveChangesAsync();

                logger.LogInformation("Seeded {Borrowers} borrowers, {Lenders} lenders, {Loans} loans.",
                    borrowers.Count, lenders.Count, loans.Count);
            }
        }
    }
}

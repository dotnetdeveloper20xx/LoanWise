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

            if (!context.Loans.Any())
            {
                var testBorrowerId = Guid.NewGuid();

                var loan = new Loan(
                    id: Guid.NewGuid(),
                    borrowerId: testBorrowerId,
                    amount: new Money(10000),
                    durationInMonths: 12,
                    purpose: LoanPurpose.Business
                );

                context.Loans.Add(loan);

                logger.LogInformation("Seeded 1 test loan for borrower {BorrowerId}", testBorrowerId);
            }

            // Add other test entities as needed
            await context.SaveChangesAsync();
        }
    }
}

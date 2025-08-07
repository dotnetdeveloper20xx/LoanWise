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
        //public static async Task InitializeAsync(LoanWiseDbContext context, ILogger logger)
        //{
        //    await context.Database.MigrateAsync();

        //    if (!context.Set<User>().Any())
        //    {
        //        var borrowerId = Guid.NewGuid();

        //        var borrower = new User
        //        {
        //            Id = borrowerId,
        //            Email = "borrower@test.com",
        //            Role = "Borrower"
        //        };

        //        context.Add(borrower);

        //        // Seed a loan for that borrower
        //        var loan = new Loan(
        //            id: Guid.NewGuid(),
        //            borrowerId: borrowerId,
        //            amount: new Money(10000),
        //            durationInMonths: 12,
        //            purpose: LoanPurpose.Business
        //        );

        //        context.Loans.Add(loan);

        //        logger.LogInformation("Seeded test borrower and loan: {BorrowerId}", borrowerId);
        //    }

        //    await context.SaveChangesAsync();
        //}
    }

}

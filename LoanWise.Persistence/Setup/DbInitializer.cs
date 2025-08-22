using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LoanWise.Domain.Entities;
using LoanWise.Domain.Enums;
using LoanWise.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LoanWise.Persistence.Setup
{
    /// <summary>
    /// Minimal initializer: ensure database is migrated and a single Admin user exists.
    /// Does nothing else.
    /// </summary>
    public static class DbInitializer
    {
        // You can change these defaults or load from configuration if desired.
        private const string AdminName = "System Admin";
        private const string AdminEmail = "admin@loanwise.local";
        private const string AdminPassword = "P@ssw0rd!"; // dev/test only

        public static async Task InitializeAsync(
            LoanWiseDbContext db,
            ILogger? logger = null,
            CancellationToken ct = default)
        {
            // 0) Apply pending migrations (best-effort)
            try
            {
                var pending = await db.Database.GetPendingMigrationsAsync(ct);
                if (pending.Any())
                {
                    logger?.LogInformation(
                        "Applying {Count} pending migrations to DB '{DbName}' on '{DataSource}'...",
                        pending.Count(),
                        db.Database.GetDbConnection().Database,
                        db.Database.GetDbConnection().DataSource);

                    await db.Database.MigrateAsync(ct);
                    logger?.LogInformation("Migrations applied.");
                }
                else
                {
                    logger?.LogInformation(
                        "No pending migrations for DB '{DbName}' on '{DataSource}'.",
                        db.Database.GetDbConnection().Database,
                        db.Database.GetDbConnection().DataSource);
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Automatic migration check/apply failed. Continuing without applying migrations.");
            }

            logger?.LogInformation("DbInitializer: Minimal admin seeding started.");

            // 1) Ensure a single Admin user exists
            var admin = await db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Role == UserRole.Admin, ct);

            if (admin != null)
            {
                // Admin already exists — do nothing else.
                logger?.LogInformation("Admin user already present ({Email}). No further seeding.", admin.Email);
                return;
            }

            var passwordHasher = new PasswordHasher<User>();

            var newAdmin = new User
            {
                Id = Guid.NewGuid(),
                FullName = AdminName,
                Email = AdminEmail,
                Role = UserRole.Admin,
                IsActive = true
            };

            newAdmin.PasswordHash = passwordHasher.HashPassword(newAdmin, AdminPassword);

            // (Optional) Assign a neutral credit profile if your domain requires it
            // newAdmin.AssignCreditProfile(score: 800, RiskTier.Low);

            await db.Users.AddAsync(newAdmin, ct);
            await db.SaveChangesAsync(ct);

            logger?.LogInformation("Admin user created: {Email}", AdminEmail);

            // 2) Done. No borrowers, lenders, loans, fundings, or repayments are seeded.
            logger?.LogInformation("DbInitializer: Minimal admin seeding completed.");
        }
    }
}


using LoanWise.Application.Common.Interfaces;
using LoanWise.Infrastructure.Repositories;
using LoanWise.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LoanWise.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Configures EF Core and persistence-related services.
    /// </summary>
    public static class AddPersistenceServices
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext with SQL Server (or other provider)
            services.AddDbContext<LoanWiseDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Register the ApplicationDbContext abstraction
            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<LoanWiseDbContext>());

            // Register repository implementations
            services.AddScoped<ILoanRepository, LoanRepository>();

            return services;
        }
    }
}

using LoanWise.Application.Common.Interfaces;
using LoanWise.Infrastructure.Common;
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
            // Register domain event dispatcher
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            // Register DbContextOptions for LoanWiseDbContext
            services.AddDbContext<LoanWiseDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            // Register DbContext manually with constructor dependencies
            services.AddScoped<LoanWiseDbContext>(provider =>
            {
                var options = provider.GetRequiredService<DbContextOptions<LoanWiseDbContext>>();
                var dispatcher = provider.GetRequiredService<IDomainEventDispatcher>();
                return new LoanWiseDbContext(options, dispatcher);
            });

            // Register abstraction
            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<LoanWiseDbContext>());

            // Register repositories
            services.AddScoped<ILoanRepository, LoanRepository>();

            return services;
        }
    }
}

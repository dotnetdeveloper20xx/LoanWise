
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace LoanWise.Persistence.DependencyInjection;

public static class AddPersistenceServices
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration config)
    {
        //services.AddDbContext<LoanWiseDbContext>(options =>
        //    options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        //services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<LoanWiseDbContext>());

        return services;
    }
}

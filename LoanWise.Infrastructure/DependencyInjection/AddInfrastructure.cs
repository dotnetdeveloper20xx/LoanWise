using LoanWise.Application.Common.Interfaces;
using LoanWise.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace LoanWise.Infrastructure.DependencyInjection;

public static class AddInfrastructureServices
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {

        services.AddScoped<ILoanRepository, LoanRepository>();

        //services.AddSingleton<IEmailService, SendGridEmailService>();
        //services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();
        // Add more services as needed

        return services;
    }
}

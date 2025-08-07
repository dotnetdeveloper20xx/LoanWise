using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using LoanWise.Infrastructure.Identity;
using LoanWise.Infrastructure.Repositories;
using LoanWise.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace LoanWise.Infrastructure.DependencyInjection;

public static class AddInfrastructureServices
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {

        services.AddScoped<ILoanRepository, LoanRepository>();
        services.AddScoped<IFundingRepository, FundingRepository>();

        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IPasswordService, PasswordService>();

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();


        return services;
    }
}

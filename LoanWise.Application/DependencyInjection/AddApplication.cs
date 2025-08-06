using Microsoft.Extensions.DependencyInjection;
using MediatR;
using FluentValidation;
using System.Reflection;

namespace LoanWise.Application.DependencyInjection;

public static class AddApplicationServices
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        //services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}

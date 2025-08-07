using FluentValidation;
using LoanWise.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace LoanWise.Application.DependencyInjection;

public static class AddApplicationServices
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));


        //services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}

using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using LoanWise.Infrastructure.Common;
using LoanWise.Infrastructure.Credit;
using LoanWise.Infrastructure.Email;
using LoanWise.Infrastructure.Exports;
using LoanWise.Infrastructure.Identity;
using LoanWise.Infrastructure.Kyc;
using LoanWise.Infrastructure.Notifications;
using LoanWise.Infrastructure.Repositories;
using LoanWise.Infrastructure.Services;
using LoanWise.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SendGrid;

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

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // Email (SendGrid)
        services.Configure<SendGridOptions>(config.GetSection("Email"));
        services.AddSingleton<ISendGridClient>(sp =>
        {
            var opts = sp.GetRequiredService<
                Microsoft.Extensions.Options.IOptions<SendGridOptions>>().Value;
            return new SendGridClient(opts.ApiKey);
        });
        services.AddScoped<EmailNotificationService>();

        services.AddScoped<ILenderReportingRepository, LenderReportingRepository>();

        services.Configure<KycOptions>(config.GetSection("Kyc"));
        services.AddScoped<IKycService, MockKycService>();
        services.AddScoped<ICreditScoringService, MockCreditScoringService>();

        services.AddScoped<IBorrowerRiskRepository, BorrowerRiskRepository>();

        services.AddSingleton<IRepaymentPlanPdfService, RepaymentPlanPdfService>();      
        services.AddSingleton<ITransactionExportService, TransactionExportService>();

        services.AddSingleton<ILoanAgreementPdfService, LoanAgreementPdfService>();
        return services;
    }
}

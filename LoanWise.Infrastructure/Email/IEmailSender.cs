// LoanWise.Infrastructure/Email/IEmailSender.cs
public interface IEmailSender
{
    Task SendToUserAsync(Guid userId, string subject, string body, CancellationToken ct = default);
}
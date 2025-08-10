using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace LoanWise.Infrastructure.Email
{
    public sealed class SendGridEmailSender : IEmailSender
    {
        private readonly ISendGridClient _client;
        public SendGridEmailSender(IOptions<LoanWise.Infrastructure.Email.SendGridOptions> options)
            => _client = new SendGridClient(options.Value.ApiKey);

        public async Task SendToUserAsync(Guid userId, string subject, string body, CancellationToken ct = default)
        {
            // TODO: look up user's email by userId (via repository or UserService)
            var to = new EmailAddress("user@example.com"); // replace with lookup
            var from = new EmailAddress("no-reply@loanwise.app", "LoanWise");
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: body, htmlContent: null);
            await _client.SendEmailAsync(msg, ct);
        }
    }
}

// File: LoanWise.Infrastructure/Notifications/EmailNotificationService.cs
using LoanWise.Application.Common.Interfaces;           // INotificationService, IUserRepository
using LoanWise.Infrastructure.Email;                   // EmailSenderOptions
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace LoanWise.Infrastructure.Notifications
{
    /// <summary>
    /// Sends notifications via email using SendGrid.
    /// Looks up the user's email via IUserRepository (Clean Architecture friendly).
    /// </summary>
    public sealed class EmailNotificationService : INotificationService
    {
        private readonly IUserRepository _users;
        private readonly ISendGridClient _sendGrid;
        private readonly SendGridOptions _options;
        private readonly ILogger<EmailNotificationService> _logger;

        public EmailNotificationService(
            IUserRepository users,
            ISendGridClient sendGridClient,
            IOptions<SendGridOptions> options,
            ILogger<EmailNotificationService> logger)
        {
            _users = users;
            _sendGrid = sendGridClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task NotifyBorrowerAsync(Guid borrowerId, string title, string message, CancellationToken ct = default)
        {
            var email = await GetEmailForUserAsync(borrowerId, ct);
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("EmailNotificationService: borrower {BorrowerId} not found or has no email.", borrowerId);
                return;
            }

            await SendAsync(email, title, message, ct);
        }

        public async Task NotifyLenderAsync(Guid lenderId, string title, string message, CancellationToken ct = default)
        {
            var email = await GetEmailForUserAsync(lenderId, ct);
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("EmailNotificationService: lender {LenderId} not found or has no email.", lenderId);
                return;
            }

            await SendAsync(email, title, message, ct);
        }

        // --- helpers ---

        private Task<string?> GetEmailForUserAsync(Guid userId, CancellationToken ct)
            => _users.GetEmailByIdAsync(userId, ct);

        private async Task SendAsync(string toEmail, string subject, string body, CancellationToken ct)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("EmailNotificationService disabled; would send to {To}: {Subject}", toEmail, subject);
                return;
            }

            var from = new EmailAddress(_options.FromEmail, _options.FromName);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(
                from,
                to,
                subject,
                plainTextContent: body,
                htmlContent: _options.SendHtml ? $"<p>{System.Net.WebUtility.HtmlEncode(body)}</p>" : null
            );

            var response = await _sendGrid.SendEmailAsync(msg, ct);

            if ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300)
            {
                _logger.LogInformation("Email sent to {To} with subject '{Subject}'.", toEmail, subject);
            }
            else
            {
                var respBody = await response.Body.ReadAsStringAsync(ct);
                _logger.LogError("Failed to send email to {To}. Status: {Status}. Body: {Body}",
                    toEmail, response.StatusCode, respBody);
            }
        }
    }
}

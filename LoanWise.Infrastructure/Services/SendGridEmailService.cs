using LoanWise.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace LoanWise.Infrastructure.Services
{
    /// <summary>
    /// Sends emails using SendGrid.
    /// </summary>
    public class SendGridEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string _apiKey;

        public SendGridEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _apiKey = _configuration["SendGrid:ApiKey"]
                   ?? throw new ArgumentNullException("Missing SendGrid:ApiKey from config");
            _fromEmail = _configuration["SendGrid:FromEmail"] ?? "no-reply@loanwise.com";
            _fromName = _configuration["SendGrid:FromName"] ?? "LoanWise";
        }

        public async Task SendEmailAsync(string toEmail, string subject, string messageBody)
        {
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, messageBody, null);

            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Body.ReadAsStringAsync();
                throw new Exception($"SendGrid failed: {response.StatusCode}, {errorBody}");
            }
        }
    }
}

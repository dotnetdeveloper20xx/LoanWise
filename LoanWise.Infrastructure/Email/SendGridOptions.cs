namespace LoanWise.Infrastructure.Email
{
    /// <summary>
    /// Configuration options for SendGrid email sending.
    /// Bound from the "Email" section in appsettings.json.
    /// </summary>
    public sealed class SendGridOptions
    {
        public bool Enabled { get; set; } = true;
        public bool SendHtml { get; set; } = false;
        public string FromEmail { get; set; } = "no-reply@loanwise.app";
        public string FromName { get; set; } = "LoanWise";
        public string ApiKey { get; set; } = string.Empty;
    }
}

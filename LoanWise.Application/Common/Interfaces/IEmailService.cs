using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanWise.Application.Common.Interfaces
{
    /// <summary>
    /// Contract for sending emails.
    /// </summary>
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string messageBody);
    }
}

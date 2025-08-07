using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Events;
using MediatR;

namespace LoanWise.Infrastructure.Notifications.Handlers
{
    /// <summary>
    /// Handles LoanFundedEvent by notifying the borrower via email.
    /// </summary>
    public class LoanFundedEventHandler : INotificationHandler<LoanFundedEvent>
    {
        private readonly IEmailService _emailService;
        private readonly IUserRepository _userRepository;

        public LoanFundedEventHandler(IEmailService emailService, IUserRepository userRepository)
        {
            _emailService = emailService;
            _userRepository = userRepository;
        }

        public async Task Handle(LoanFundedEvent notification, CancellationToken cancellationToken)
        {
            var borrower = await _userRepository.GetByIdAsync(notification.BorrowerId);

            if (borrower is null || string.IsNullOrWhiteSpace(borrower.Email))
                return;

            var subject = "🎉 Your loan has been fully funded!";
            var message = $"""
                Hi {borrower.FullName},

                Good news! Your loan request has been fully funded by our lender community.
                It will now proceed to disbursement and repayment schedule generation.

                Amount Funded: £{notification.AmountFunded:N2}
                Loan ID: {notification.LoanId}

                Thank you for using LoanWise.
                """;

            await _emailService.SendEmailAsync(borrower.Email, subject, message);
        }
    }
}

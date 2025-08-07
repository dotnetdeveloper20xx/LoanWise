using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using LoanWise.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LoanWise.Application.Features.Loans.Commands.ApplyLoan
{
    /// <summary>
    /// Handles ApplyLoanCommand and persists the new loan.
    /// </summary>
    public class ApplyLoanCommandHandler : IRequestHandler<ApplyLoanCommand, Guid>
    {
        private readonly ILoanRepository _loanRepository;
        private readonly ILogger<ApplyLoanCommandHandler> _logger;

        public ApplyLoanCommandHandler(
            ILoanRepository loanRepository,
            ILogger<ApplyLoanCommandHandler> logger)
        {
            _loanRepository = loanRepository;
            _logger = logger;
        }

        public async Task<Guid> Handle(ApplyLoanCommand request, CancellationToken cancellationToken)
        {
            var loan = new Loan(
                id: Guid.NewGuid(),
                borrowerId: request.BorrowerId,
                amount: new Money(request.Amount),
                durationInMonths: request.DurationInMonths,
                purpose: request.Purpose
            );

            // Optional: store metadata like Description or MonthlyIncome elsewhere if needed

            await _loanRepository.AddAsync(loan, cancellationToken);

            _logger.LogInformation("New loan application created: {LoanId}", loan.Id);

            return loan.Id;
        }
    }
}

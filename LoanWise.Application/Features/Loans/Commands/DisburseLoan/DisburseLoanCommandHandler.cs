using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.Features.Loans.Commands.DisburseLoan;
using LoanWise.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Common.Models;

public class DisburseLoanCommandHandler : IRequestHandler<DisburseLoanCommand, ApiResponse<Guid>>
{
    private readonly ILoanRepository _loanRepository;
    private readonly ILogger<DisburseLoanCommandHandler> _logger;
    private readonly IMediator _mediator; // keep injected, see note below

    public DisburseLoanCommandHandler(
        ILoanRepository loanRepository,
        ILogger<DisburseLoanCommandHandler> logger,
        IMediator mediator)
    {
        _loanRepository = loanRepository;
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<ApiResponse<Guid>> Handle(DisburseLoanCommand request, CancellationToken ct)
    {
        // Load with repayments to support idempotency
        var loan = await _loanRepository.GetByIdWithRepaymentsAsync(request.LoanId, ct);
        if (loan is null)
        {
            _logger.LogWarning("Disbursement failed: Loan {LoanId} not found", request.LoanId);
            return ApiResponse<Guid>.FailureResult("Loan not found.");
        }

        // Friendly pre-check (Disburse() also guards, but this gives a clearer message)
        if (loan.Status == LoanStatus.Rejected)
            return ApiResponse<Guid>.FailureResult("Cannot disburse a rejected loan.");

        // Already disbursed? Be idempotent.
        if (loan.Status == LoanStatus.Disbursed)
        {
            var count = loan.Repayments?.Count ?? 0;
            if (count == 0)
            {
                loan.GenerateRepaymentSchedule();
                await _loanRepository.UpdateAsync(loan, ct);

                _logger.LogInformation("Loan {LoanId} was already disbursed; generated missing schedule ({Count} installments).",
                    loan.Id, loan.Repayments?.Count);

                return ApiResponse<Guid>.SuccessResult(
                    loan.Id,
                    $"Repayment schedule generated ({loan.Repayments?.Count} installments)."
                );
            }

            _logger.LogInformation("Loan {LoanId} already disbursed; schedule exists ({Count} installments).",
                loan.Id, count);

            return ApiResponse<Guid>.SuccessResult(
                loan.Id,
                $"Loan already disbursed; {count} installments exist."
            );
        }

        try
        {
            // Funded -> Disbursed + schedule
            loan.Disburse();                  // raises LoanDisbursedEvent inside the aggregate
            loan.GenerateRepaymentSchedule(); // will throw if not Disbursed

            await _loanRepository.UpdateAsync(loan, ct);

            _logger.LogInformation("Loan {LoanId} disbursed and schedule generated ({Count} installments).",
                loan.Id, loan.Repayments.Count);

            // IMPORTANT:
            // If your DbContext dispatches domain events on SaveChanges, DO NOT publish manually here.
            // If it does NOT, uncomment the following line to publish the event explicitly:
            // await _mediator.Publish(new LoanDisbursedEvent(loan.Id, loan.DisbursedAtUtc!.Value), ct);

            return ApiResponse<Guid>.SuccessResult(
                loan.Id,
                $"Loan disbursed; {loan.Repayments.Count} installments generated."
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Disbursement rejected for loan {LoanId}: {Message}", loan.Id, ex.Message);
            return ApiResponse<Guid>.FailureResult(ex.Message);
        }
    }
}

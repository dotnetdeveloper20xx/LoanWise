using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.Features.Loans.Commands.DisburseLoan;
using LoanWise.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore; // for DbUpdateConcurrencyException
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Common.Models;

public class DisburseLoanCommandHandler : IRequestHandler<DisburseLoanCommand, ApiResponse<Guid>>
{
    private readonly ILoanRepository _loanRepository;
    private readonly ILogger<DisburseLoanCommandHandler> _logger;
    private readonly IMediator _mediator; // keep injected for domain notifications if not dispatched by DbContext

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
        // Load tracked (no AsNoTracking) so EF has original values/concurrency token.
        var loan = await _loanRepository.GetByIdWithRepaymentsAsync(request.LoanId, ct);
        if (loan is null)
        {
            _logger.LogWarning("Disbursement failed: Loan {LoanId} not found", request.LoanId);
            return ApiResponse<Guid>.FailureResult("Loan not found.");
        }

        // Friendly guardrails (aggregate will also protect itself).
        if (loan.Status == LoanStatus.Rejected)
            return ApiResponse<Guid>.FailureResult("Cannot disburse a rejected loan.");

        // Idempotency: if already disbursed, ensure schedule exists and return success.
        if (loan.Status == LoanStatus.Disbursed)
        {
            var existing = loan.Repayments?.Count ?? 0;
            if (existing == 0)
            {
                loan.GenerateRepaymentSchedule(); // safe now because status is Disbursed
                try
                {
                    // IMPORTANT: UpdateAsync must NOT call DbSet.Update() for tracked entities.
                    await _loanRepository.UpdateAsync(loan, ct);

                    _logger.LogInformation(
                        "Loan {LoanId} already disbursed; generated missing schedule ({Count} installments).",
                        loan.Id, loan.Repayments?.Count ?? 0);

                    return ApiResponse<Guid>.SuccessResult(
                        loan.Id,
                        $"Repayment schedule generated ({loan.Repayments?.Count ?? 0} installments)."
                    );
                }
                catch (DbUpdateConcurrencyException)
                {
                    _logger.LogWarning("Concurrency conflict while generating missing schedule for loan {LoanId}", loan.Id);
                    return ApiResponse<Guid>.FailureResult("Conflict: loan was modified by another process. Please retry.");
                }
            }

            _logger.LogInformation("Loan {LoanId} already disbursed; schedule exists ({Count} installments).",
                loan.Id, existing);

            return ApiResponse<Guid>.SuccessResult(
                loan.Id,
                $"Loan already disbursed; {existing} installments exist."
            );
        }

        // Normal path: Funded -> Disbursed + schedule
        try
        {
            loan.Disburse();                  // aggregate state change (may raise domain event)
            loan.GenerateRepaymentSchedule(); // requires Disbursed state

            // Persist tracked changes. Implementation detail:
            // - If entity is tracked, UpdateAsync should simply SaveChangesAsync(ct).
            await _loanRepository.UpdateAsync(loan, ct);

            _logger.LogInformation("Loan {LoanId} disbursed; schedule generated ({Count} installments).",
                loan.Id, loan.Repayments.Count);

            // If your DbContext does NOT dispatch domain events on SaveChanges,
            // publish them explicitly here via _mediator.Publish(...).

            return ApiResponse<Guid>.SuccessResult(
                loan.Id,
                $"Loan disbursed; {loan.Repayments.Count} installments generated."
            );
        }
        catch (InvalidOperationException ex)
        {
            // e.g., Disburse() guard failures
            _logger.LogWarning("Disbursement rejected for loan {LoanId}: {Message}", loan.Id, ex.Message);
            return ApiResponse<Guid>.FailureResult(ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Map to a clear, user-facing message; your middleware can translate to HTTP 409.
            _logger.LogWarning("Concurrency conflict while disbursing loan {LoanId}", loan.Id);
            return ApiResponse<Guid>.FailureResult("Conflict: loan was modified by another process. Please retry.");
        }
    }
}

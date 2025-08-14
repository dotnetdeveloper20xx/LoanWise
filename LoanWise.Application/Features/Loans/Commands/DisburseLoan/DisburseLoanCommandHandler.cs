using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.Features.Loans.Commands.DisburseLoan;
using LoanWise.Domain.Entities;
using LoanWise.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Common.Models;

public sealed class DisburseLoanCommandHandler
    : IRequestHandler<DisburseLoanCommand, ApiResponse<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<DisburseLoanCommandHandler> _log;

    public DisburseLoanCommandHandler(
        IApplicationDbContext db,
        ILogger<DisburseLoanCommandHandler> log)
    {
        _db = db;
        _log = log;
    }

    public async Task<ApiResponse<Guid>> Handle(DisburseLoanCommand request, CancellationToken ct)
    {
        // We need the concrete DbContext to use transactions & tracking APIs
        if (_db is not DbContext ef)
        {
            _log.LogError("IApplicationDbContext is not backed by a DbContext.");
            return ApiResponse<Guid>.FailureResult("Server configuration error.");
        }

        try
        {
            await using var tx = await ef.Database.BeginTransactionAsync(ct);

            // Load TRACKED (no AsNoTracking here on a command path)
            var loan = await ef.Set<Loan>()
                .FirstOrDefaultAsync(l => l.Id == request.LoanId, ct);

            if (loan is null)
                return ApiResponse<Guid>.FailureResult("Loan not found.");

            // Idempotent fast-path: already disbursed → ensure schedule exists & exit
            if (loan.Status == LoanStatus.Disbursed)
            {
                var hasSchedule = await ef.Set<Repayment>()
                    .AnyAsync(r => r.LoanId == loan.Id, ct);

                if (!hasSchedule)
                {
                    var newItems = BuildEqualInstallments(loan);
                    await ef.Set<Repayment>().AddRangeAsync(newItems, ct);
                    await ef.SaveChangesAsync(ct); // inserts only the new repayments
                }

                await tx.CommitAsync(ct);
                var count = await ef.Set<Repayment>().CountAsync(r => r.LoanId == loan.Id, ct);
                return ApiResponse<Guid>.SuccessResult(
                    loan.Id, $"Loan already disbursed; {count} installments exist.");
            }

            // Guard: must be funded
            if (loan.Status != LoanStatus.Funded)
            {
                await tx.RollbackAsync(ct);
                return ApiResponse<Guid>.FailureResult("Loan is not fully funded yet.");
            }

            // State transition (raises domain event inside aggregate)
            loan.Disburse();

            // Persist the loan row first (updates RowVersion)
            await ef.SaveChangesAsync(ct);

            // Create schedule ONLY if it doesn't exist (INSERTs; no mass UPDATEs)
            var exists = await ef.Set<Repayment>().AnyAsync(r => r.LoanId == loan.Id, ct);
            if (!exists)
            {
                var items = BuildEqualInstallments(loan);
                await ef.Set<Repayment>().AddRangeAsync(items, ct);
                await ef.SaveChangesAsync(ct);
            }

            await tx.CommitAsync(ct);

            var total = await ef.Set<Repayment>().CountAsync(r => r.LoanId == loan.Id, ct);
            return ApiResponse<Guid>.SuccessResult(
                loan.Id, $"Loan disbursed; {total} installments generated.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Someone bumped RowVersion while we were saving the loan.
            _log.LogWarning(ex, "Concurrency while disbursing {LoanId}", request.LoanId);

            // Reload and return a clean outcome if another process already finished.
            if (_db is DbContext ef2)
            {
                var reloaded = await ef2.Set<Loan>()
                    .AsTracking()
                    .FirstOrDefaultAsync(l => l.Id == request.LoanId, ct);

                if (reloaded is not null && reloaded.Status == LoanStatus.Disbursed)
                {
                    var hasSchedule = await ef2.Set<Repayment>().AnyAsync(r => r.LoanId == reloaded.Id, ct);
                    if (!hasSchedule)
                    {
                        var items = BuildEqualInstallments(reloaded);
                        await ef2.Set<Repayment>().AddRangeAsync(items, ct);
                        await ef2.SaveChangesAsync(ct);
                    }

                    var total = await ef2.Set<Repayment>().CountAsync(r => r.LoanId == reloaded.Id, ct);
                    return ApiResponse<Guid>.SuccessResult(
                        reloaded.Id,
                        $"Loan already disbursed by another process; {total} installments exist.");
                }
            }

            // If we’re here, it’s a real edit conflict the client should just retry.
            return ApiResponse<Guid>.FailureResult(
                "Conflict: loan was modified by another process. Please retry.");
        }
        catch (InvalidOperationException ex)
        {
            // Domain guard failures (e.g., calling Disburse in the wrong state)
            _log.LogWarning("Disbursement rejected: {Message}", ex.Message);
            return ApiResponse<Guid>.FailureResult(ex.Message);
        }
    }

    private static IEnumerable<Repayment> BuildEqualInstallments(Loan loan)
    {
        // Build repayments without touching the navigation collection to avoid EF marking existing rows Modified
        var monthly = Math.Round(loan.Amount / loan.DurationInMonths, 2, MidpointRounding.AwayFromZero);
        var start = (loan.DisbursedAtUtc ?? DateTime.UtcNow).Date;

        for (int i = 1; i <= loan.DurationInMonths; i++)
        {
            yield return new Repayment(
                id: Guid.NewGuid(),
                loanId: loan.Id,
                dueDate: start.AddMonths(i),
                amount: monthly);
        }
    }
}

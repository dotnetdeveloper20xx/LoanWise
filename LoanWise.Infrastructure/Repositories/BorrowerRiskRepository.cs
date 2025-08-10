using System.Text.Json;
using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanWise.Persistence.Repositories;

internal sealed class BorrowerRiskRepository : IBorrowerRiskRepository
{
    private readonly IApplicationDbContext _db;
    public BorrowerRiskRepository(IApplicationDbContext db) => _db = db;

    public Task<BorrowerRiskSnapshot?> GetAsync(Guid borrowerId, CancellationToken ct = default)
        => _db.BorrowerRiskSnapshots.FirstOrDefaultAsync(x => x.BorrowerId == borrowerId, ct);

    public async Task UpsertAsync(BorrowerRiskSnapshot snapshot, CancellationToken ct = default)
    {
        var existing = await _db.BorrowerRiskSnapshots
            .FirstOrDefaultAsync(x => x.BorrowerId == snapshot.BorrowerId, ct);

        if (existing is null)
        {
            snapshot.UpdatedAtUtc = DateTime.UtcNow;
            await _db.BorrowerRiskSnapshots.AddAsync(snapshot, ct);
        }
        else
        {
            existing.CreditScore = snapshot.CreditScore;
            existing.RiskTier = snapshot.RiskTier;
            existing.KycStatus = snapshot.KycStatus;
            existing.FlagsJson = snapshot.FlagsJson;
            existing.LastVerifiedAtUtc = snapshot.LastVerifiedAtUtc ?? existing.LastVerifiedAtUtc; // only overwrite if provided
            existing.LastScoreAtUtc = snapshot.LastScoreAtUtc;
            existing.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
    }

    // helpers (optional)
    public static string ToFlagsJson(IEnumerable<string> flags) => JsonSerializer.Serialize(flags);
}

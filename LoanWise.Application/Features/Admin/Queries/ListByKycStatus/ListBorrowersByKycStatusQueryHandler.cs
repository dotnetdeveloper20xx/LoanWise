// LoanWise.Application/Features/Admin/Borrowers/Queries/ListByKycStatus/ListBorrowersByKycStatusQueryHandler.cs
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.DTOs.Admin;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Admin.Borrowers.Queries.ListByKycStatus;

public sealed class ListBorrowersByKycStatusQueryHandler
    : IRequestHandler<ListBorrowersByKycStatusQuery, ApiResponse<BorrowerKycListResult>>
{
    private static readonly HashSet<string> Allowed =
        new(StringComparer.OrdinalIgnoreCase) { "Verified", "Pending", "Failed", "Unknown" };

    private readonly IApplicationDbContext _db;
    public ListBorrowersByKycStatusQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<BorrowerKycListResult>> Handle(ListBorrowersByKycStatusQuery q, CancellationToken ct)
    {
        // Parse statuses (CSV), validate, and normalize (lowercase for case-insensitive compare)
        var statuses = (q.StatusesCsv ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => Allowed.Contains(s))
            .Select(s => s.ToLowerInvariant())
            .Distinct()
            .ToArray();

        if (statuses.Length == 0)
            return ApiResponse<BorrowerKycListResult>.FailureResult("Provide at least one valid status: Verified, Pending, Failed, Unknown.");

        var page = q.Page < 1 ? 1 : q.Page;
        var pageSize = Math.Clamp(q.PageSize, 1, 200);
        var sortBy = (q.SortBy ?? "").Trim().ToLowerInvariant();
        var asc = string.Equals(q.SortDir, "asc", StringComparison.OrdinalIgnoreCase);

        // Base: multi-status filter (case-insensitive)
        var baseQuery = _db.BorrowerRiskSnapshots.AsNoTracking()
            .Where(s => statuses.Contains(s.KycStatus.ToLower()));

        // Join to users (for name/email search)
        var joined = from s in baseQuery
                     join u in _db.Users.AsNoTracking() on s.BorrowerId equals u.Id
                     select new
                     {
                         s.BorrowerId,
                         u.FullName,
                         u.Email,
                         s.KycStatus,
                         s.CreditScore,
                         s.RiskTier,
                         s.LastVerifiedAtUtc,
                         s.LastScoreAtUtc,
                         s.FlagsJson
                     };

        // SEARCH (name/email)
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim();
            var pattern = $"%{EscapeLike(term)}%";
            joined = joined.Where(x =>
                EF.Functions.Like(x.FullName, pattern) ||
                EF.Functions.Like(x.Email, pattern));
        }

        var total = await joined.CountAsync(ct);

        // SORT (added scoreAt/lastScoreAt)
        joined = sortBy switch
        {
            "score" => asc
                ? joined.OrderBy(x => x.CreditScore).ThenBy(x => x.BorrowerId)
                : joined.OrderByDescending(x => x.CreditScore).ThenByDescending(x => x.BorrowerId),

            "verifiedat" => asc
                ? joined.OrderBy(x => x.LastVerifiedAtUtc.HasValue)
                        .ThenBy(x => x.LastVerifiedAtUtc)
                        .ThenBy(x => x.BorrowerId)
                : joined.OrderByDescending(x => x.LastVerifiedAtUtc.HasValue)
                        .ThenByDescending(x => x.LastVerifiedAtUtc)
                        .ThenByDescending(x => x.BorrowerId),

            "scoreat" or "lastscoreat" => asc
                ? joined.OrderBy(x => x.LastScoreAtUtc).ThenBy(x => x.BorrowerId)
                : joined.OrderByDescending(x => x.LastScoreAtUtc).ThenByDescending(x => x.BorrowerId),

            "name" => asc
                ? joined.OrderBy(x => x.FullName).ThenBy(x => x.BorrowerId)
                : joined.OrderByDescending(x => x.FullName).ThenByDescending(x => x.BorrowerId),

            "risk" => asc
                ? joined.OrderBy(x => x.RiskTier).ThenBy(x => x.BorrowerId)
                : joined.OrderByDescending(x => x.RiskTier).ThenByDescending(x => x.BorrowerId),

            "status" => asc
                ? joined.OrderBy(x => x.KycStatus).ThenBy(x => x.BorrowerId)
                : joined.OrderByDescending(x => x.KycStatus).ThenByDescending(x => x.BorrowerId),

            _ => joined.OrderByDescending(x => x.LastVerifiedAtUtc.HasValue)
                       .ThenByDescending(x => x.LastVerifiedAtUtc)
                       .ThenByDescending(x => x.LastScoreAtUtc)
        };

        // PAGE + MATERIALIZE
        var rows = await joined
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        // Parse FlagsJson after materialization
        var items = rows.Select(r => new BorrowerKycListItemDto
        {
            BorrowerId = r.BorrowerId,
            BorrowerName = r.FullName,
            KycStatus = r.KycStatus,
            CreditScore = r.CreditScore,
            RiskTier = r.RiskTier,
            LastVerifiedAtUtc = r.LastVerifiedAtUtc,
            LastScoreAtUtc = r.LastScoreAtUtc,
            Flags = ParseFlags(r.FlagsJson)
        }).ToList();

        return ApiResponse<BorrowerKycListResult>.SuccessResult(
            new BorrowerKycListResult { Total = total, Items = items });
    }

    private static string[] ParseFlags(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return Array.Empty<string>();
        try { return JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>(); }
        catch { return Array.Empty<string>(); }
    }

    private static string EscapeLike(string input) =>
        input.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
}

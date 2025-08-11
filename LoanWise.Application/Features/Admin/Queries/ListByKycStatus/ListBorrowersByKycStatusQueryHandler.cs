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
    private static readonly HashSet<string> AllowedStatuses =
        new(StringComparer.OrdinalIgnoreCase) { "Verified", "Pending", "Failed", "Unknown" };

    private readonly IApplicationDbContext _db;

    public ListBorrowersByKycStatusQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<BorrowerKycListResult>> Handle(ListBorrowersByKycStatusQuery q, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q.Status) || !AllowedStatuses.Contains(q.Status))
            return ApiResponse<BorrowerKycListResult>.FailureResult(
                "Invalid KYC status. Use Verified | Pending | Failed | Unknown.");

        var page = q.Page < 1 ? 1 : q.Page;
        var pageSize = Math.Clamp(q.PageSize, 1, 200);
        var sortBy = (q.SortBy ?? "").Trim().ToLowerInvariant();
        var asc = string.Equals(q.SortDir, "asc", StringComparison.OrdinalIgnoreCase);

        // Filter by KYC status (case-insensitive)
        var baseQuery = _db.BorrowerRiskSnapshots.AsNoTracking()
            .Where(s => s.KycStatus.ToLower() == q.Status.ToLower());

        // Join to Users to enable name/email search and display
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

        // SEARCH: borrower name or email
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim();
            var pattern = $"%{EscapeLike(term)}%";
            joined = joined.Where(x =>
                EF.Functions.Like(x.FullName, pattern) ||
                EF.Functions.Like(x.Email, pattern));
        }

        // Total after filters
        var total = await joined.CountAsync(ct);

        // SORT
        joined = (sortBy) switch
        {
            "score" => asc
                ? joined.OrderBy(x => x.CreditScore).ThenBy(x => x.BorrowerId)
                : joined.OrderByDescending(x => x.CreditScore).ThenByDescending(x => x.BorrowerId),

            "verifiedat" => asc
                // asc: nulls first (use HasValue to control null ordering)
                ? joined.OrderBy(x => x.LastVerifiedAtUtc.HasValue)
                        .ThenBy(x => x.LastVerifiedAtUtc)
                        .ThenBy(x => x.BorrowerId)
                // desc: nulls last
                : joined.OrderByDescending(x => x.LastVerifiedAtUtc.HasValue)
                        .ThenByDescending(x => x.LastVerifiedAtUtc)
                        .ThenByDescending(x => x.BorrowerId),

            "name" => asc
                ? joined.OrderBy(x => x.FullName).ThenBy(x => x.BorrowerId)
                : joined.OrderByDescending(x => x.FullName).ThenByDescending(x => x.BorrowerId),

            "risk" => asc
                ? joined.OrderBy(x => x.RiskTier).ThenBy(x => x.BorrowerId)
                : joined.OrderByDescending(x => x.RiskTier).ThenByDescending(x => x.BorrowerId),

            "status" => asc
                ? joined.OrderBy(x => x.KycStatus).ThenBy(x => x.BorrowerId)
                : joined.OrderByDescending(x => x.KycStatus).ThenByDescending(x => x.BorrowerId),

            _ => // default sort: most recently verified, then most recent score
                joined.OrderByDescending(x => x.LastVerifiedAtUtc.HasValue)
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

    // Escape SQL LIKE wildcards for EF.Functions.Like
    private static string EscapeLike(string input) =>
        input.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
}

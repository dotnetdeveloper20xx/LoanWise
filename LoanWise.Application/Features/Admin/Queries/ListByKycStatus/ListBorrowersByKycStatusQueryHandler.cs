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

        var baseQuery = _db.BorrowerRiskSnapshots.AsNoTracking()
            .Where(s => s.KycStatus.ToLower() == q.Status.ToLower());

        var total = await baseQuery.CountAsync(ct);

        // Project raw fields (including FlagsJson) and materialize
        var rows = await (
            from s in baseQuery
            join u in _db.Users.AsNoTracking() on s.BorrowerId equals u.Id
            orderby s.LastVerifiedAtUtc descending, s.LastScoreAtUtc descending
            select new
            {
                s.BorrowerId,
                BorrowerName = u.FullName, // adjust if different
                s.KycStatus,
                s.CreditScore,
                s.RiskTier,
                s.LastVerifiedAtUtc,
                s.LastScoreAtUtc,
                s.FlagsJson
            })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        // Parse FlagsJson AFTER materialization
        var items = rows.Select(r => new BorrowerKycListItemDto
        {
            BorrowerId = r.BorrowerId,
            BorrowerName = r.BorrowerName,
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
        catch { return Array.Empty<string>(); } // be defensive against bad data
    }
}

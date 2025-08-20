using LoanWise.Application.Common.Interfaces;
using LoanWise.Application.DTOs.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Admin.Queries.GetUsers
{
    public sealed class GetUsersQueryHandler
    : IRequestHandler<GetUsersQuery, ApiResponse<PaginatedResult<UserListDto>>>
    {
        private readonly IApplicationDbContext _db;

        public GetUsersQueryHandler(IApplicationDbContext db) => _db = db;

        public async Task<ApiResponse<PaginatedResult<UserListDto>>> Handle(
            GetUsersQuery request, CancellationToken ct)
        {
            var query = _db.Users.AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.Trim();
                query = query.Where(u => u.FullName.Contains(s) || u.Email.Contains(s));
            }

            // Role
            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                var r = request.Role.Trim();
                query = query.Where(u => u.Role.ToString() == r);
            }

            // Status filter — ONLY apply when explicitly provided
            // null => include ALL users (active + inactive)
            if (request.IsActive is not null)
                query = query.Where(u => u.IsActive == request.IsActive.Value);

            // Sorting
            var sortBy = (request.SortBy ?? "fullName").ToLowerInvariant();
            var desc = string.Equals(request.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

            // Note: Only use CreatedAtUtc if your entity has it (see note below)
            query = sortBy switch
            {
                "email" => (desc ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email)),
                "role" => (desc ? query.OrderByDescending(u => u.Role) : query.OrderBy(u => u.Role)),
                "status" => (desc ? query.OrderByDescending(u => u.IsActive) : query.OrderBy(u => u.IsActive)),
                "createdat" => (desc ? query.OrderByDescending(u => u.CreatedAtUtc) : query.OrderBy(u => u.CreatedAtUtc)), // only if exists
                _ => (desc ? query.OrderByDescending(u => u.FullName) : query.OrderBy(u => u.FullName)),
            };

            var total = await query.CountAsync(ct);

            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(u => new UserListDto(
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.Role.ToString(),
                    u.IsActive
                ))
                .ToListAsync(ct);

            var result = new PaginatedResult<UserListDto>(items, total, request.Page, request.PageSize);
            return ApiResponse<PaginatedResult<UserListDto>>.SuccessResult(result);
        }
    }

}


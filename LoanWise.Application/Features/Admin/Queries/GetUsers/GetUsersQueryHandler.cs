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
            GetUsersQuery request,
            CancellationToken cancellationToken)
        {
            var query = _db.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
                query = query.Where(u => u.FullName.Contains(request.Search) || u.Email.Contains(request.Search));

            if (!string.IsNullOrWhiteSpace(request.Role))
                query = query.Where(u => u.Role.ToString() == request.Role);

            var total = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(u => u.FullName)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(u => new UserListDto(
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.Role.ToString(),
                    u.IsActive
                ))
                .ToListAsync(cancellationToken);

            var result = new PaginatedResult<UserListDto>(items, total, request.Page, request.PageSize);

            return ApiResponse<PaginatedResult<UserListDto>>.SuccessResult(result);
        }
    }
}


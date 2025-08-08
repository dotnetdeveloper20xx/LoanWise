using LoanWise.Application.DTOs.Users;
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Admin.Queries.GetUsers
{
    /// <summary>
    /// Request for paginated list of users, with optional search and role filter.
    /// </summary>
    public sealed record GetUsersQuery(
        int Page = 1,
        int PageSize = 20,
        string? Search = null,
        string? Role = null
    ) : IRequest<ApiResponse<PaginatedResult<UserListDto>>>;

}

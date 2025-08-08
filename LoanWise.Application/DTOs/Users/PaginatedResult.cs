namespace LoanWise.Application.DTOs.Users
{
    public sealed record PaginatedResult<T>(
         List<T> Items,
         int TotalCount,
         int Page,
         int PageSize
     );
}

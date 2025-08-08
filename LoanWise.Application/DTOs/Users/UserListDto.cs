namespace LoanWise.Application.DTOs.Users
{
    public sealed record UserListDto(
        Guid Id,
        string FullName,
        string Email,
        string Role,
        bool IsActive
    );
}

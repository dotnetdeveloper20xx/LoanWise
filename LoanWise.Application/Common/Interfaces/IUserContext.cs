using System;

namespace LoanWise.Application.Common.Interfaces
{
    public interface IUserContext
    {
        Guid? UserId { get; }
        string? Email { get; }
        string? Role { get; }
        bool IsAuthenticated { get; }
        bool IsInRole(string v);
    }
}

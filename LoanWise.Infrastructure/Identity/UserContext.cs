using LoanWise.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace LoanWise.Infrastructure.Identity
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public Guid? UserId =>
            Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        public string? Email => User?.FindFirstValue(ClaimTypes.Email);

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
        public string? Role => User?.FindFirstValue(ClaimTypes.Role);

        public bool IsInRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role) || string.IsNullOrWhiteSpace(Role))
                return false;

            // Compare case-insensitively
            return string.Equals(Role, role, StringComparison.OrdinalIgnoreCase);
        }
    }
}

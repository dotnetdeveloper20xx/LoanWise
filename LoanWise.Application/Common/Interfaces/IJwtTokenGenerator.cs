using LoanWise.Domain.Entities;

namespace LoanWise.Application.Common.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }
}

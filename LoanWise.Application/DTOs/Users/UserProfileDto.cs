using LoanWise.Domain.Entities;

namespace LoanWise.Application.DTOs.Users
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public UserRole Role { get; set; }
        public int? CreditScore { get; set; }
        public RiskTier? RiskTier { get; set; }
        public bool KycVerified { get; set; }
    }
}

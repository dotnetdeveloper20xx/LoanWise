namespace LoanWise.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string TokenHash { get; set; } = default!;
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? RevokedAtUtc { get; set; }
        public string CreatedByIp { get; set; } = default!;
        public string? ReplacedByTokenHash { get; set; }

        public bool IsRevoked => RevokedAtUtc.HasValue;
        public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}

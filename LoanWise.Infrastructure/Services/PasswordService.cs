using LoanWise.Domain.Entities;
using Microsoft.AspNetCore.Identity;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _hasher = new();

    public string HashPassword(string plain, User user)
        => _hasher.HashPassword(user, plain);

    public bool VerifyPassword(string hash, string plain, User user)
        => _hasher.VerifyHashedPassword(user, hash, plain) == PasswordVerificationResult.Success;
}

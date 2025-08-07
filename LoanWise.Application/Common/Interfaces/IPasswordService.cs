
using LoanWise.Domain.Entities;

public interface IPasswordService
{
    string HashPassword(string plain, User user);
    bool VerifyPassword(string hash, string plain, User user);
}

// LoanWise.Application/Common/Interfaces/IKycService.cs
namespace LoanWise.Application.Common.Interfaces
{
    
    public interface IKycService
    {
        Task<KycResult> VerifyAsync(Guid borrowerId, CancellationToken ct = default);
        Task<KycSnapshot> GetStatusAsync(Guid borrowerId, CancellationToken ct = default);
    }

    public sealed record KycResult(Guid BorrowerId, string Status, string[] Flags, DateTime VerifiedAtUtc);
    public sealed record KycSnapshot(Guid BorrowerId, string Status, string[] Flags, DateTime? LastVerifiedAtUtc);

}

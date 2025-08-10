// LoanWise.Infrastructure/Kyc/KycOptions.cs
namespace LoanWise.Infrastructure.Kyc;

public sealed class KycOptions
{
    public string Mode { get; set; } = "Mock";  // future: "Live"
    public double PassRate { get; set; } = 0.9; // 90% pass by default
}

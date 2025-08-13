namespace LoanWise.Application.DTOs.Fundings
{

    public sealed record FundingResultDto(
      Guid LoanId,
      Guid FundingId,
      Guid LenderId,
      decimal requestedAmount,
      decimal appliedAmount,
      decimal remainingBefore,
      decimal remainingAfter,
      bool fullyFunded
  );
}

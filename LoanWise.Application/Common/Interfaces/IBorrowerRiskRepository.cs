using LoanWise.Domain.Entities;

namespace LoanWise.Application.Common.Interfaces;

public interface IBorrowerRiskRepository
{
    Task<BorrowerRiskSnapshot?> GetAsync(Guid borrowerId, CancellationToken ct = default);
    Task UpsertAsync(BorrowerRiskSnapshot snapshot, CancellationToken ct = default);
}

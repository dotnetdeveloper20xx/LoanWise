using LoanWise.Domain.Entities;

namespace LoanWise.Application.Common.Interfaces
{
    /// <summary>
    /// Contract for funding data access.
    /// </summary>
    public interface IFundingRepository
    {
        Task AddAsync(Funding funding, CancellationToken cancellationToken = default);
        Task<List<Funding>> GetByLoanIdAsync(Guid loanId, CancellationToken cancellationToken = default);
    }
}

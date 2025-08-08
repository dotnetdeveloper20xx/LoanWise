using LoanWise.Domain.Entities;
using LoanWise.Domain.Enums;

namespace LoanWise.Application.Common.Interfaces
{
    /// <summary>
    /// Abstraction for data access operations related to loans.
    /// </summary>
    public interface ILoanRepository
    {
        /// <summary>Adds a new loan to the persistence store.</summary>
        Task AddAsync(Loan loan, CancellationToken cancellationToken);

        /// <summary>Updates an existing loan in the persistence store.</summary>
        Task UpdateAsync(Loan loan, CancellationToken cancellationToken);

        /// <summary>Gets a loan by its unique ID, including borrower, fundings, and repayments.</summary>
        Task<Loan?> GetByIdAsync(Guid loanId, CancellationToken cancellationToken);

        /// <summary>Retrieves all open loan requests (not yet fully funded or disbursed).</summary>
        Task<IReadOnlyList<Loan>> GetOpenLoansAsync(CancellationToken cancellationToken);

        /// <summary>Retrieves all loans submitted by a specific borrower.</summary>
        Task<IReadOnlyList<Loan>> GetLoansByBorrowerAsync(Guid borrowerId, CancellationToken cancellationToken);

        /// <summary>Retrieves loans by status (e.g., Pending, Approved, Funded, Disbursed, Completed).</summary>
        Task<IReadOnlyList<Loan>> GetByStatusAsync(LoanStatus status, CancellationToken cancellationToken);

        /// <summary>Saves changes if the repository uses a Unit of Work pattern.</summary>
        Task SaveChangesAsync(CancellationToken cancellationToken);

        /// <summary>Retrieves all loans including their fundings for projection purposes.</summary>
        Task<IReadOnlyList<Loan>> GetAllIncludingFundingsAsync(CancellationToken cancellationToken);

        /// <summary>Retrieves loans with their repayment collections eagerly loaded.</summary>
        Task<IReadOnlyList<Loan>> GetLoansWithRepaymentsAsync(CancellationToken cancellationToken);

        /// <summary>Retrieves all loans including repayments (admin stats, reporting, etc.).</summary>
        Task<IReadOnlyList<Loan>> GetAllIncludingRepaymentsAsync(CancellationToken cancellationToken);
    }
}

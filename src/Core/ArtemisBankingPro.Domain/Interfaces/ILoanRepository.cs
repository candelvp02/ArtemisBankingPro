using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;

namespace ArtemisBankingPro.Domain.Interfaces;

public interface ILoanRepository : IGenericRepository<Loan>
{
    Task<Loan?> GetByLoanNumberAsync(string loanNumber);
    Task<Loan?> GetWithInstallmentsAsync(int loanId);
    Task<bool> HasActiveLoanAsync(string userId);
    Task<bool> LoanNumberExistsAsync(string loanNumber);
    Task<IReadOnlyList<Loan>> GetActiveLoansByUserAsync(string userId, LoanStatus status);
}
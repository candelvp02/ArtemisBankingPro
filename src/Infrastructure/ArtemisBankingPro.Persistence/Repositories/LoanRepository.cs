using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBankingPro.Persistence.Repositories;

public class LoanRepository : GenericRepository<Loan>, ILoanRepository
{
    public LoanRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Loan?> GetByLoanNumberAsync(string loanNumber) =>
        await DbSet.FirstOrDefaultAsync(l => l.LoanNumber == loanNumber);

    public async Task<Loan?> GetWithInstallmentsAsync(int loanId) =>
        await DbSet
            .Include(l => l.Installments)
            .Include(l => l.ApplicationUser)
            .FirstOrDefaultAsync(l => l.Id == loanId);

    public async Task<bool> HasActiveLoanAsync(string userId) =>
        await DbSet.AnyAsync(l => l.ApplicationUserId == userId && l.Status == LoanStatus.Active);

    public async Task<bool> LoanNumberExistsAsync(string loanNumber) =>
        await DbSet.AnyAsync(l => l.LoanNumber == loanNumber);

    public async Task<IReadOnlyList<Loan>> GetActiveLoansByUserAsync(string userId, LoanStatus status) =>
        await DbSet
            .Where(l => l.ApplicationUserId == userId && l.Status == status)
            .ToListAsync();
}
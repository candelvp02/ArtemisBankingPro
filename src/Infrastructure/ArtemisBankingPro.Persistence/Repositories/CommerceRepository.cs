using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBankingPro.Persistence.Repositories;

public class CommerceRepository : GenericRepository<Commerce>, ICommerceRepository
{
    public CommerceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Commerce?> GetByRncAsync(string rnc) =>
        await DbSet.FirstOrDefaultAsync(c => c.Rnc == rnc);

    public async Task<Commerce?> GetByEmailAsync(string email) =>
        await DbSet.FirstOrDefaultAsync(c => c.Email == email);

    public async Task<Commerce?> GetByApplicationUserIdAsync(string userId) =>
        await DbSet.FirstOrDefaultAsync(c => c.ApplicationUserId == userId);
}
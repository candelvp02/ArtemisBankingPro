using ArtemisBankingPro.Domain.Entities;

namespace ArtemisBankingPro.Domain.Interfaces;

public interface ICommerceRepository : IGenericRepository<Commerce>
{
    Task<Commerce?> GetByRncAsync(string rnc);
    Task<Commerce?> GetByEmailAsync(string email);
    Task<Commerce?> GetByApplicationUserIdAsync(string userId);
}
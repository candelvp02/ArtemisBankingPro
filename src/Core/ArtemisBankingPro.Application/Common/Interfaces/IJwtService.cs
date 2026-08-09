namespace ArtemisBankingPro.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(string userId, string userName, string role);
}
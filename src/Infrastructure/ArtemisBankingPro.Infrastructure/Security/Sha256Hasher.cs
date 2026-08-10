using System.Security.Cryptography;
using System.Text;
using ArtemisBankingPro.Application.Common.Interfaces;

namespace ArtemisBankingPro.Infrastructure.Security;

public class Sha256Hasher : IPasswordHasher
{
    public string Hash(string plainText)
    {
        var bytes = Encoding.UTF8.GetBytes(plainText);
        var hashBytes = SHA256.HashData(bytes);

        return Convert.ToHexString(hashBytes);
    }

    public bool Verify(string plainText, string hash)
    {
        var computedHash = Hash(plainText);

        return string.Equals(computedHash, hash, StringComparison.OrdinalIgnoreCase);
    }
}
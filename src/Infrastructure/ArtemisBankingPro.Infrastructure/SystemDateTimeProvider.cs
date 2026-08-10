using ArtemisBankingPro.Application.Common.Interfaces;

namespace ArtemisBankingPro.Infrastructure;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
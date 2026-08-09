namespace ArtemisBankingPro.Domain.Exceptions;

public class HighRiskClientException : DomainException
{
    public HighRiskClientException(string message) : base(message)
    {
    }
}
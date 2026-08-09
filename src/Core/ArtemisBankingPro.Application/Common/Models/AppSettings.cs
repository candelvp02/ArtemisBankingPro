namespace ArtemisBankingPro.Application.Common.Models;

public class AppSettings
{
    public const string SectionName = "AppSettings";

    public string WebAppBaseUrl { get; set; } = string.Empty;
}
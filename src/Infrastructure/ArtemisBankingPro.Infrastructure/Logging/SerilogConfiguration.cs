using Microsoft.Extensions.Configuration;
using Serilog;

namespace ArtemisBankingPro.Infrastructure.Logging;

public static class SerilogConfiguration
{
    public static LoggerConfiguration BuildLoggerConfiguration(IConfiguration configuration, string logFileName)
    {
        return new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                path: $"Logs/{logFileName}-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
    }
}
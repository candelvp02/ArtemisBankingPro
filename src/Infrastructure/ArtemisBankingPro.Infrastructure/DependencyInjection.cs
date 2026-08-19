using ArtemisBankingPro.Application.Common.Interfaces;
using ArtemisBankingPro.Domain.Interfaces;
using ArtemisBankingPro.Infrastructure.Email;
using ArtemisBankingPro.Infrastructure.Identity;
using ArtemisBankingPro.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArtemisBankingPro.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));

        services.AddHttpContextAccessor();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IJwtService, JwtTokenGenerator>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IPasswordHasher, Sha256Hasher>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        return services;
    }
}
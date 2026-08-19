using System.Reflection;
using ArtemisBankingPro.Application.Common.Behaviors;
using ArtemisBankingPro.Application.Common.Models;
using ArtemisBankingPro.Application.Services.Implementations;
using ArtemisBankingPro.Application.Services.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArtemisBankingPro.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddAutoMapper(assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        services.Configure<AppSettings>(configuration.GetSection(AppSettings.SectionName));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ISavingsAccountService, SavingsAccountService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<ICreditCardService, CreditCardService>();

        return services;
    }
}
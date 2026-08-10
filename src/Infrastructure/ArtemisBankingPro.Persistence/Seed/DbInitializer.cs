using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ArtemisBankingPro.Persistence.Seed;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureCreatedAsync();

        await SeedRolesAsync(roleManager);
        await SeedDefaultUserAsync(userManager, "admin", "admin@artemisbank.com", "Admin123!", UserRole.Administrator);
        await SeedDefaultUserAsync(userManager, "cashier", "cashier@artemisbank.com", "Cashier123!", UserRole.Cashier);
        await SeedDefaultUserAsync(userManager, "client", "client@artemisbank.com", "Client123!", UserRole.Client);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in Enum.GetNames<UserRole>())
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedDefaultUserAsync(
        UserManager<ApplicationUser> userManager,
        string userName,
        string email,
        string password,
        UserRole role)
    {
        if (await userManager.FindByNameAsync(userName) is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true,
            FirstName = userName,
            LastName = "Seed",
            Cedula = Guid.NewGuid().ToString("N")[..11]
        };

        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role.ToString());
        }
    }
}
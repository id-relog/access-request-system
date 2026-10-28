using AccessRequestSystem.API.Auth;
using Microsoft.AspNetCore.Identity;

namespace AccessRequestSystem.API;

public static class DataSeeder
{
    /// <summary>Создаёт роли Employee/Administrator и одного администратора</summary>
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser<Guid>>>();

        foreach (var role in new[] { Roles.Employee, Roles.Administrator })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = role });
        }

        const string adminEmail = "admin@company.local";
        const string adminPassword = "Admin123!";

        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new IdentityUser<Guid>
            {
                Id = Guid.NewGuid(),
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, Roles.Administrator);
        }
    }
}

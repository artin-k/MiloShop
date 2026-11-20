using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiloShop.Domain.Entities;

namespace MiloShop.Infrastructure.Persistence
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider services, IConfiguration config)
        {
            using var scope = services.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1) Ensure roles exist
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

            // 2) Assign Admin role to the configured email (if user exists)
            var adminEmail = config["Admin:Email"]?.Trim();

            if (string.IsNullOrWhiteSpace(adminEmail))
                return; // no admin configured

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
                await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

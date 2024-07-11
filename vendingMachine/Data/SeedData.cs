using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

public class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var logger = serviceProvider.GetRequiredService<ILogger<SeedData>>();

        string[] roleNames = { "Admin", "User" };
        IdentityResult roleResult;

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                logger.LogInformation($"Creating role {roleName}");
                roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!roleResult.Succeeded)
                {
                    logger.LogError($"Failed to create role {roleName}");
                }
            }
        }

        var admin = new IdentityUser
        {
            UserName = "admin",
            Email = "admin@example.com",
        };

        string adminPassword = "Admin123!";

        var user = await userManager.FindByEmailAsync(admin.Email);

        if (user == null)
        {
            logger.LogInformation("Creating admin user");
            var createPowerUser = await userManager.CreateAsync(admin, adminPassword);
            if (createPowerUser.Succeeded)
            {
                var addToRoleResult = await userManager.AddToRoleAsync(admin, "Admin");
                if (!addToRoleResult.Succeeded)
                {
                    logger.LogError("Failed to add admin user to Admin role");
                }
            }
            else
            {
                logger.LogError("Failed to create admin user");
            }
        }
    }
}

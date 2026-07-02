using Microsoft.AspNetCore.Identity;

namespace Sabara.Web.Data
{
    public static class DbInitializer
    {
        public const string AdminRole = "Admin";
        public const string EmployeeRole = "Employee";

        public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var roleName in new[] { AdminRole, EmployeeRole })
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            const string adminEmail = "Admin@test.com";
            const string adminPassword = "123456";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(adminUser, AdminRole);
            }
            else if (!await userManager.IsInRoleAsync(adminUser, AdminRole))
            {
                await userManager.AddToRoleAsync(adminUser, AdminRole);
            }
        }
    }
}

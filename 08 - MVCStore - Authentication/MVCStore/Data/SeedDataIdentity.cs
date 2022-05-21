using Microsoft.AspNetCore.Identity;

namespace MVCStore.Data
{
    public static class SeedDataIdentity
    {
        private const string adminEmail = "admin@test.com";
        private const string adminPassword = "Secret123$";
        public static async Task EnsurePopulatedAsync(IApplicationBuilder app)
        {
            var serviceProvider = app.ApplicationServices
            .CreateScope().ServiceProvider;

            using (var userManager = serviceProvider
                .GetRequiredService<UserManager<IdentityUser>>())
            {
                IdentityUser user = await userManager.FindByEmailAsync(adminEmail);

                if (user == null)
                {
                    user = new IdentityUser { UserName = adminEmail, Email = adminEmail };
                    await userManager.CreateAsync(user, adminPassword);
                }

                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                var roleName = "ProductManagement";

                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));

                var adminWithRoleEmail = "adminRole@test.com";
                var adminWithRolePassword = "Secret123$";

                IdentityUser adminWithRole = await userManager.FindByEmailAsync(adminWithRoleEmail);
                if (adminWithRole == null)
                {
                    adminWithRole = new IdentityUser { UserName = adminWithRoleEmail, Email = adminWithRoleEmail };
                    await userManager.CreateAsync(adminWithRole, adminWithRolePassword);
                    await userManager.AddToRoleAsync(adminWithRole, roleName);
                }
            }
        }
    }
}

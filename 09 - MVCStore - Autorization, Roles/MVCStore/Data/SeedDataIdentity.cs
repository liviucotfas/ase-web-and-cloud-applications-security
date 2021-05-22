using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCStore.Data
{
    public static class SeedDataIdentity
    {
        private const string adminEmail = "admin@test.com";
        private const string adminPassword = "Secret123$";
        public static async Task EnsurePopulated(IApplicationBuilder app)
        {
            var serviceProvider = app.ApplicationServices
            .CreateScope().ServiceProvider;

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var roleName = "ProductManagement";
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole(roleName));

            using (var userManager = serviceProvider
                .GetRequiredService<UserManager<IdentityUser>>())
            {
                IdentityUser user = await userManager.FindByEmailAsync(adminEmail);

                if (user == null)
                {
                    user = new IdentityUser { UserName = adminEmail, Email = adminEmail };
                    await userManager.CreateAsync(user, adminPassword);
                }

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

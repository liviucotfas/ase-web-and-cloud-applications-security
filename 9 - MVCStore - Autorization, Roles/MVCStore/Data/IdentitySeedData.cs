using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCStore.Data
{
    public static class IdentitySeedData
    {
        private const string adminEmail = "admin@test.com";
        private const string adminPassword = "Secret123$";
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var userManager = serviceProvider
                .GetRequiredService<UserManager<IdentityUser>>())
            {
                IdentityUser user = await userManager.FindByEmailAsync(adminEmail);

                if (user == null)
                {
                    user = new IdentityUser { UserName = adminEmail, Email = adminEmail };
                    var result = await userManager.CreateAsync(user, adminPassword);
                }

                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                var roleName = "ProductManagement";
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));

                var adminWithRoleEmail = "adminRole2@test.com";
                IdentityUser adminWithRole = await userManager.FindByEmailAsync(adminWithRoleEmail);
                if (adminWithRole == null)
                {
                    adminWithRole = new IdentityUser { UserName = adminWithRoleEmail, Email = adminWithRoleEmail };
                    await userManager.CreateAsync(adminWithRole, adminPassword);
                    await userManager.AddToRoleAsync(adminWithRole, roleName);
                }
            }
        }
    }
}

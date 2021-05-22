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

            using (var userManager = serviceProvider
                .GetRequiredService<UserManager<IdentityUser>>())
            {
                IdentityUser user = await userManager.FindByEmailAsync(adminEmail);

                if (user == null)
                {
                    user = new IdentityUser { UserName = adminEmail, Email = adminEmail };
                    await userManager.CreateAsync(user, adminPassword);
                }
            }
        }
    }
}

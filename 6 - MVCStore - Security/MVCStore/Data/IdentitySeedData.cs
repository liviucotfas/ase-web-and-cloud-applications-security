using Microsoft.AspNetCore.Builder;
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
		private const string adminUser = "Admin";
		private const string adminPassword = "Secret123$";

		public static async void EnsurePopulated(IApplicationBuilder app)
		{
			var roleManager = app.ApplicationServices
				.GetRequiredService<RoleManager<IdentityRole>>();

			if (!await roleManager.RoleExistsAsync("ProductManagement"))
				await roleManager.CreateAsync(new IdentityRole("ProductManagement"));

			var userManager = app.ApplicationServices
				.GetRequiredService<UserManager<IdentityUser>>();

			IdentityUser user = await userManager.FindByIdAsync(adminUser);
			if (user == null)
			{
				user = new IdentityUser("Admin");
				await userManager.CreateAsync(user, adminPassword);
			}

			IdentityUser adminProducts = await userManager.FindByIdAsync("AdminProductManagament");
			if (adminProducts == null)
			{
				adminProducts = new IdentityUser("AdminProductManagament");
				await userManager.CreateAsync(adminProducts, adminPassword);
				await userManager.AddToRoleAsync(adminProducts, "ProductManagement");
			}
		}
	}
}

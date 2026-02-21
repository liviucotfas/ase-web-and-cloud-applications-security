using Microsoft.AspNetCore.Identity;

namespace MVCStore.Data
{
	public static class SeedDataIdentity
	{
		private const string adminEmail = "admin@test.com";
		private const string adminPassword = "Secret123$";
		public static async Task EnsurePopulatedAsync(WebApplication app)
		{
			var serviceProvider = app.Services
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

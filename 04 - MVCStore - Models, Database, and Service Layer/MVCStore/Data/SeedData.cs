using Microsoft.EntityFrameworkCore;
using MVCStore.Models;

namespace MVCStore.Data
{
	public static class SeedData
	{
		public static void EnsurePopulated(WebApplication app)
		{
			using (var scope = app.Services.CreateScope())
			{
				var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

				// Automatically applies pending migrations during development
				// WARNING: Not recommended for production - use migration tools or CI/CD instead
				//if (context.Database.GetPendingMigrations().Any())
				//{
				//	context.Database.Migrate();
				//}

				if (!context.Categories.Any())
				{
					context.Categories.AddRange(
						new Category
						{
							Name = "Security Software",
							Products = new List<Product>()
						},
						new Category
						{
							Name = "Training & Certification",
							Products = new List<Product>()
						}
					);

					context.SaveChanges();
				}

				if (!context.Products.Any())
				{
					var securitySoftware = context.Categories.First(c => c.Name == "Security Software");
					var training = context.Categories.First(c => c.Name == "Training & Certification");

					context.Products.AddRange(
						new Product
						{
							Name = "Enterprise Antivirus License",
							Price = 299.99m,
							CategoryID = securitySoftware.CategoryID
						},
						new Product
						{
							Name = "Password Manager Pro",
							Price = 49.99m,
							CategoryID = securitySoftware.CategoryID
						},
						new Product
						{
							Name = "Certified Ethical Hacker (CEH) Course",
							Price = 1299.00m,
							CategoryID = training.CategoryID
						},
						new Product
						{
							Name = "Security Awareness Training",
							Price = 199.00m,
							CategoryID = training.CategoryID
						}
					);

					context.SaveChanges();
				}
			}
		}
	}
}

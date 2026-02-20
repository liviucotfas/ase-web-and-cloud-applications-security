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

                if (context.Database.GetPendingMigrations().Any())
				{
					context.Database.Migrate();
				}

				if (!context.Products.Any())
				{
					context.Products.AddRange(
					new Product
					{
						Name = "Kayak",
						Description = "A boat for one person",
						Category = "Watersports",
						Price = 275
					},
					new Product
					{
						Name = "Lifejacket",
						Description = "Protective and fashionable",
						Category = "Watersports",
						Price = 48.95m
					},
					new Product
					{
						Name = "Soccer Ball",
						Description = "FIFA-approved size and weight",
						Category = "Soccer",
						Price = 19.50m
					}
					);

					context.SaveChanges();
				}
			}
		}
	}
}

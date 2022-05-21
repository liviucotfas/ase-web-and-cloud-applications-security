using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MVCStore.Models;

namespace MVCStore.Data
{
    public static class SeedData
    {
        public static void EnsurePopulated(IApplicationBuilder app)
        {
            ApplicationDbContext context = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

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
                    Price = 275
                },
                new Product
                {
                    Name = "Lifejacket",
                    Description = "Protective and fashionable",
                    Price = 48.95m
                },
                new Product
                {
                    Name = "Soccer Ball",
                    Description = "FIFA-approved size and weight",
                    Price = 19.50m
                }
                );

                context.SaveChanges();
            }
        }
    }
}
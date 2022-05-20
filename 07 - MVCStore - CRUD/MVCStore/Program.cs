using MVCStore.Data;
using Microsoft.EntityFrameworkCore;

namespace MVCStore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<ApplicationDbContext>(opts => {
                opts.UseSqlServer(
                builder.Configuration["ConnectionStrings:DefaultConnection"]);
            });

            builder.Services.AddScoped<IStoreRepository, EFStoreRepository>();

            // Configure the HTTP request pipeline.
            var app = builder.Build();
            app.UseStaticFiles();
            // !!!! new/updated code {
            app.MapControllerRoute("pagination",
                "Products/Page{productPage}",
                new { Controller = "Home", action = "Index" });
            //}
            app.MapDefaultControllerRoute();

            SeedData.EnsurePopulated(app);

            app.Run();
        }
    }
}
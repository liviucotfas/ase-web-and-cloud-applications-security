using MVCStore.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace MVCStore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<ApplicationDbContext>(opts => {
                opts.UseSqlServer(
                builder.Configuration["ConnectionStrings:DefaultConnection"]);
            });

            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
        .AddRoleManager<RoleManager<IdentityRole>>()
        .AddDefaultUI()
        .AddEntityFrameworkStores<ApplicationDbContext>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddScoped<IStoreRepository, EFStoreRepository>();

            // Configure the HTTP request pipeline.
            var app = builder.Build();
            app.UseStaticFiles();

            // !!!! new/updated code {
            app.UseAuthentication();
            app.UseAuthorization();
            //}

            app.MapControllerRoute("pagination",
                "Products/Page{productPage}",
                new { Controller = "Home", action = "Index" });
            app.MapDefaultControllerRoute();
            app.MapRazorPages();

            SeedData.EnsurePopulated(app);
            Task.Run(async () =>
            {
                await SeedDataIdentity.EnsurePopulatedAsync(app);
            }).Wait();

            app.Run();
        }
    }
}
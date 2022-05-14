using MVCStore.Data;
using Microsoft.EntityFrameworkCore;

namespace MVCStore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<ApplicationDbContext>(opts => {
                opts.UseSqlServer(
                builder.Configuration["ConnectionStrings:DefaultConnection"]);
            });

            builder.Services.AddScoped<IStoreRepository, EFStoreRepository>();

            var app = builder.Build();
            app.UseStaticFiles();
            app.MapDefaultControllerRoute();

            // !!!! new/updated code {
            SeedData.EnsurePopulated(app);
            //}

            app.Run();
        }
    }
}
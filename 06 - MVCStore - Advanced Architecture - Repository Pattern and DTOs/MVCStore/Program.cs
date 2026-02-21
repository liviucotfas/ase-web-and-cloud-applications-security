using Microsoft.EntityFrameworkCore;
using MVCStore.Data;
using MVCStore.Repositories;
using MVCStore.Services;

namespace MVCStore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddControllersWithViews();

			// Configure Entity Framework Core to use SQL Server
			builder.Services.AddDbContext<ApplicationDbContext>(opts =>
			{
				opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
				// Enable sensitive data logging only in development
				if (builder.Environment.IsDevelopment())
				{
					opts.EnableSensitiveDataLogging();
				}
			});

			// Register Repository layer
			builder.Services.AddScoped<IProductRepository, ProductRepository>();

			// Register Service layer (now depends on repository)
			builder.Services.AddScoped<IProductService, ProductService>();

			var app = builder.Build();
			//app.MapGet("/", () => "Hello World!");

			// Configure the HTTP request pipeline.
			if (!app.Environment.IsDevelopment())
			{
				//app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			// Redirects HTTP requests to HTTPS automatically
			// Enhances security by ensuring all traffic uses encrypted connections
			app.UseHttpsRedirection();
			// Adds endpoint routing to the pipeline
			// Matches incoming requests to available endpoints (controllers, actions, etc.)
			// Must be placed before UseAuthorization() and endpoint mapping methods
			app.UseRouting();

			// Enables authorization middleware
			// Checks if the user is authorized to access the requested resource
			// Should be placed after UseRouting() and before endpoint mapping methods
			app.UseAuthorization();

			// Maps static file assets (CSS, JavaScript, images) with optimization
			app.MapStaticAssets();

			// Seed the database
			SeedData.EnsurePopulated(app);

			// Defines the default routing pattern for MVC controllers
			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}")
				// Chains static asset support to the route
				// Enables fingerprinting for static files referenced in views served by this route
				.WithStaticAssets();

			app.Run();
        }
    }
}
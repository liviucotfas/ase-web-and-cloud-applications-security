# MVCStore - Authentication

<!-- vscode-markdown-toc -->
* 1. [Objectives](#Objectives)
* 2. [Prerequisites](#Prerequisites)
* 3. [Changing the Context Class](#ChangingtheContextClass)
* 4. [Creating and Applying the Database Migration](#CreatingandApplyingtheDatabaseMigration)
* 5. [Defining the Seed Data](#DefiningtheSeedData)
* 6. [Applying a Basic Authorization Policy](#ApplyingaBasicAuthorizationPolicy)
* 7. [Bibliography](#Bibliography)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

##  1. <a name='Objectives'></a>Objectives
- Adding ASP.NET Core Identity to an existing MVC application
- Extending `ApplicationDbContext` to support Identity
- Creating and applying Identity database migrations
- Seeding an initial admin user programmatically
- Restricting access to controllers with the `[Authorize]` attribute
- Scaffolding Login, Register, and LogOut Razor Pages

##  2. <a name='Prerequisites'></a>Prerequisites

Before starting this lab, you must have completed **Lab 07: CRUD**. Your project should already have:

- `Models/DTOs/ProductDto.cs`, `CategoryDto.cs`, `MappingExtensions.cs`
- `Repositories/IProductRepository.cs`, `ProductRepository.cs`
- `Repositories/ICategoryRepository.cs`, `CategoryRepository.cs`
- `Services/IProductService.cs`, `ProductService.cs`
- `Controllers/AdminController.cs` with full CRUD actions
- `Views/Admin/` — `Index.cshtml`, `Create.cshtml`, `Edit.cshtml`
- `Views/Shared/_AdminLayout.cshtml`
- `Program.cs` registering `IProductRepository`, `ICategoryRepository`, and `IProductService`

##  3. <a name='ChangingtheContextClass'></a>Changing the Context Class

1. Install the NuGet package `Microsoft.AspNetCore.Identity.EntityFrameworkCore`.

2. Change the `ApplicationDbContext` class as follows. Add `using Microsoft.AspNetCore.Identity.EntityFrameworkCore;` and `using Microsoft.AspNetCore.Identity;` to the existing usings.

	```csharp
	public class ApplicationDbContext : IdentityDbContext<IdentityUser>
	{
	    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

	    public DbSet<Product> Products { get; set; }
	    public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);
        }
	}
	```

	> The `ApplicationDbContext` class is derived from `IdentityDbContext`, which provides Identity-specific features for Entity Framework Core. For the type parameter, we used the `IdentityUser` class, which is the built-in class used to represent users.

	> **Important**: Keep both `Products` and `Categories` `DbSet` properties. Identity adds its own tables (users, roles, claims, etc.) alongside your existing tables.

3. Install the NuGet package `Microsoft.AspNetCore.Identity.UI`. Then update `Program.cs` with the highlighted changes:

	```csharp
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
	            if (builder.Environment.IsDevelopment())
	            {
	                opts.EnableSensitiveDataLogging();
	            }
	        });

	        // !!!! new/updated code {
	        builder.Services.AddDefaultIdentity<IdentityUser>(options =>
	                options.SignIn.RequireConfirmedAccount = false)
	            .AddEntityFrameworkStores<ApplicationDbContext>();
	        // }

	        // Register Repository and Service layers (unchanged from Lab 06/07)
	        builder.Services.AddScoped<IProductRepository, ProductRepository>();
	        builder.Services.AddScoped<IProductService, ProductService>();
	        builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

	        var app = builder.Build();

	        if (!app.Environment.IsDevelopment())
	        {
	            app.UseHsts();
	        }

	        app.UseHttpsRedirection();
	        app.UseRouting();

	        // !!!! new/updated code {
	        app.UseAuthentication();
	        app.UseAuthorization();
	        // }

	        app.MapStaticAssets();

	        // Seed the database
	        SeedData.EnsurePopulated(app);

	        app.MapControllerRoute(
	                name: "default",
	                pattern: "{controller=Home}/{action=Index}/{id?}")
	            .WithStaticAssets();

	        // !!!! new/updated code {
	        app.MapRazorPages();
	        // }

	        app.Run();
	    }
	}
	```

	> The `AddDefaultIdentity` method adds a set of common identity services to the application, including a default UI, token providers, and configures authentication to use identity cookies. Further reading: https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.identityservicecollectionuiextensions.adddefaultidentity

	> **Note**: `app.UseAuthentication()` must appear **before** `app.UseAuthorization()` in the middleware pipeline. Placing them in the wrong order will cause authorization to silently fail.

	> **Note**: `MapStaticAssets()` replaces `UseStaticFiles()` in .NET 9 and enables fingerprinting for static files, improving caching behaviour. `WithStaticAssets()` chains this support to a specific route.

##  4. <a name='CreatingandApplyingtheDatabaseMigration'></a>Creating and Applying the Database Migration

1. Add a new migration to our application by running the following command.

	```
	Add-Migration Identity
	```
2. Apply the migration by calling the following command.

	```
	Update-Database
	```

##  5. <a name='DefiningtheSeedData'></a>Defining the Seed Data

1. Add a new class called `SeedDataIdentity` to the `Data` folder

	```C#
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
	```

2. Because `EnsurePopulatedAsync` must be awaited, change the `Main` method signature from `void` to `async Task`. Then call `SeedDataIdentity.EnsurePopulatedAsync` after `SeedData.EnsurePopulated(app)` and before `app.Run()`:

	```csharp
	SeedData.EnsurePopulated(app);
	await SeedDataIdentity.EnsurePopulatedAsync(app);

	app.Run();
	```

***Question**
- What changes should be made to the application in order to store additional details for a User?
	> Futher reading: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/add-user-data

##  6. <a name='ApplyingaBasicAuthorizationPolicy'></a>Applying a Basic Authorization Policy

1. The `[Authorize]` attribute is used to restrict access to action methods or controllers. Decorate the `AdminController` with this attribute.

2. Run the application and try to access any action on the `AdminController`

4. Scaffold Register, Login, and LogOut

	> Futher reading: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity

	Make sure to have the following code in the selected layout view

	```CSHTML
	@RenderSection("Scripts", required: false)
	```
5. Check the code in the Register, Login, and LogOut pages. Change their default layout to `_AdminLayout` by modifying in the folder `Areas/Indentity/Pages` the `_ViewStart.cshtml`.

6. Notice that a file called `_LoginPartial.cshtml` has been added to the `Views/Shared` folder.

7. Include the `LoginPartial` in the `_AdminLayout` file

	```HTML
    <nav class="navbar bg-light navbar-expand-sm">
        <div class="container-fluid">
            <span class="navbar-brand mb-0">MVCStore</span>
             <!-- !!!! new/updated code { -->
            <div class="d-sm-inline-flex justify-content-between">
                <partial name="_LoginPartial" />
            </div>
            <!-- } -->
        </div>
    </nav>
	```

    > `d-inline-flex` creates an inline flexbox container: https://getbootstrap.com/docs/5.2/utilities/display/#notation. For `sm` check https://getbootstrap.com/docs/5.2/layout/breakpoints/#available-breakpoints .

##  7. <a name='Bibliography'></a>Bibliography

- [Introduction to Identity on ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [AddDefaultIdentity API reference](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.identityservicecollectionuiextensions.adddefaultidentity)
- [IdentityDbContext Class](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.entityframeworkcore.identitydbcontext)
- [Scaffold Identity in ASP.NET Core projects](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/scaffold-identity)
- [Add, download, and delete user data to Identity in an ASP.NET Core project](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/add-user-data)
- [Authorization in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/introduction)
- [Simple authorization with the [Authorize] attribute](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/simple)
- [ASP.NET Core Middleware — Authentication and Authorization order](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/)
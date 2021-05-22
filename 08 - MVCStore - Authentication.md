# MVCStore - Security

<!-- vscode-markdown-toc -->
* 1. [Objectives](#Objectives)
* 2. [Changing the Context Class](#ChangingtheContextClass)
* 3. [Creating and Applying the Database Migration](#CreatingandApplyingtheDatabaseMigration)
* 4. [Defining the Seed Data](#DefiningtheSeedData)
* 5. [Applying a Basic Authorization Policy](#ApplyingaBasicAuthorizationPolicy)
* 6. [Bibliography](#Bibliography)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

##  1. <a name='Objectives'></a>Objectives
- implementing the identity framework;

##  2. <a name='ChangingtheContextClass'></a>Changing the Context Class

1. Install the NuGet package "Microsoft.AspNetCore.Identity.EntityFrameworkCore".
2. Change the `ApplicationDbContext` class as follows.

	```C#
	public class ApplicationDbContext : IdentityDbContext<IdentityUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options) { }
		
		public DbSet<Product> Products { get; set; }
	}
	```

	> The `ApplicationDbContext` class is derived from `IdentityDbContext`, which provides Identity-specific features for Entity Framework Core. For the type parameter, we used the `IdentityUser` class, which is the built-in class used to represent users. 

2. Install the package "Microsoft.AspNetCore.Identity.UI". In the `Startup` class make the following changes in the `ConfigureServices` method.

	```C#
	public void ConfigureServices(IServiceCollection services)
    {
		services.AddControllersWithViews();

		services.AddDbContext<ApplicationDbContext>(options =>
		options.UseSqlServer(
			Configuration.GetConnectionString("DefaultConnection")));

		// !!!! new/updated code
		services.AddDefaultIdentity<IdentityUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
		//}

		services.AddScoped<IStoreRepository, EFStoreRepository>();
    }
	```
	> The `AddDefaultIdentity` method adds a set of common identity services to the application, including a default UI, token providers, and configures authentication to use identity cookies. Further reading: https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.identityservicecollectionuiextensions.adddefaultidentity

3. Also update the `Configure` method as follows.

	```C#
	//......
 	app.UseHttpsRedirection();
	app.UseStaticFiles();

	app.UseRouting();

	// !!!! new/updated code
	app.UseAuthentication();
	app.UseAuthorization();
	//}

	app.UseEndpoints(endpoints => {
		endpoints.MapControllerRoute("pagination", "Products/Page{productPage}", new { Controller = "Home", action = "Index" });
		endpoints.MapDefaultControllerRoute();
		// !!!! new/updated code
		endpoints.MapRazorPages();
		//}
	});
	```

##  3. <a name='CreatingandApplyingtheDatabaseMigration'></a>Creating and Applying the Database Migration

1. Add a new migration to our application by running the following command.

	```
	Add-Migration Identity
	```
2. Apply the migration by calling the following command.

	```
	Update-Database
	```

##  4. <a name='DefiningtheSeedData'></a>Defining the Seed Data

1. Add a new class called `SeedDataIdentity` to the `Data` folder

	```C#
	public static class SeedDataIdentity
    {
        private const string adminEmail = "admin@test.com";
        private const string adminPassword = "Secret123$";
        public static async Task EnsurePopulated(IApplicationBuilder app)
        {
            var serviceProvider = app.ApplicationServices
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

2. Call the `EnsurePopulated` method in the `Configure` method of the `Startup` class. 

	```C#
	Task.Run(async () =>
		{
			await SeedDataIdentity.Initialize(app);
		}).Wait(); 
	```

***Question**
- What changes should be made to the application in order to store additional details for a User?
	> Futher reading: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/add-user-data

##  5. <a name='ApplyingaBasicAuthorizationPolicy'></a>Applying a Basic Authorization Policy

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
 	<div class="bg-dark text-white p-2">
        <span class="navbar-brand ml-2">MVC STORE</span>
        <!-- !!!! new/updated code { -->
        <div style="background-color:white;display:inline-block;">
            <partial name="_LoginPartial" />
        </div>
        <!-- } -->
    </div>
	```

##  6. <a name='Bibliography'></a>Bibliography
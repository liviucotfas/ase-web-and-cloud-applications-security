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
		services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
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
5. Check the code in the Register, Login, and LogOut pages

6. Create a partial view called `_LoginPartial.cshtml` in the `Views/Shared` folder.

	```CSHTML
	@using Microsoft.AspNetCore.Identity
	@inject SignInManager<IdentityUser> SignInManager
	@inject UserManager<IdentityUser> UserManager

	<ul class="navbar-nav">
	@if (SignInManager.IsSignedIn(User))
	{
		<li class="nav-item">
			<a  class="nav-link text-dark" asp-area="Identity" asp-page="/Account/Manage/Index" title="Manage">Hello @User.Identity.Name!</a>
		</li>
		<li class="nav-item">
			<form  class="form-inline" asp-area="Identity" asp-page="/Account/Logout" asp-route-returnUrl="@Url.Action("Index", "Home", new { area = "" })">
				<button  type="submit" class="nav-link btn btn-link text-dark">Logout</button>
			</form>
		</li>
	}
	else
	{
		<li class="nav-item">
			<a class="nav-link text-dark" asp-area="Identity" asp-page="/Account/Register">Register</a>
		</li>
		<li class="nav-item">
			<a class="nav-link text-dark" asp-area="Identity" asp-page="/Account/Login">Login</a>
		</li>
	}
	</ul>
	```
7. Include the LoginPartial in the _Layout file

	```HTML
 	<header>
        <nav class="navbar navbar-expand-sm navbar-toggleable-sm navbar-light bg-white border-bottom box-shadow mb-3">
            <div class="container">
                <a class="navbar-brand" asp-area="" asp-controller="Home" asp-action="Index">MVCStore</a>
                <button class="navbar-toggler" type="button" data-toggle="collapse" data-target=".navbar-collapse" aria-controls="navbarSupportedContent"
                        aria-expanded="false" aria-label="Toggle navigation">
                    <span class="navbar-toggler-icon"></span>
                </button>
                <div class="navbar-collapse collapse d-sm-inline-flex flex-sm-row-reverse">
                    <partial name="_LoginPartial" />
                    <ul class="navbar-nav flex-grow-1">
                        <li class="nav-item">
                            <a class="nav-link text-dark" asp-area="" asp-controller="Home" asp-action="Index">Home</a>
                        </li>
                    </ul>
                </div>
            </div>
        </nav>
    </header>
	```

##  6. <a name='Bibliography'></a>Bibliography
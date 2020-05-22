# MVCStore - Security

## Changing the Context Class

1. Change the `ApplicationDbContext` class as follows.

	```C#
	public class ApplicationDbContext : IdentityDbContext<IdentityUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options) { }
		
		public DbSet<Product> Products { get; set; }
	}
	```

	> The AppIdentityDbContext class is derived from IdentityDbContext, which provides Identity-specific features for Entity Framework Core. For the type parameter, we used the IdentityUser class, which is the built-in class used to represent users. 

2. Install the package "Microsoft.AspNetCore.Identity.UI". In the `Startup` class make the following changes in the `ConfigureServices` method.

	```C#
	public void ConfigureServices(IServiceCollection services)
    {
        /*services.AddTransient
                <IProductRepository, 
                FakeProductRepository>();*/

		services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

		
		//new lines{
		services.AddDefaultIdentity<IdentityUser>()
              .AddEntityFrameworkStores<ApplicationDbContext>();
		//}new lines

		services.AddTransient
                <IProductRepository, 
                EFProductRepository>();

		//new lines{
        services.AddControllersWithViews();
        services.AddRazorPages();
		//}new lines
    }
	```

2. Also update the `Configure` method as follows.

	```C#
	//......
 	app.UseHttpsRedirection();
	app.UseStaticFiles();

	app.UseRouting();

	//new lines{
	app.UseAuthentication();
	app.UseAuthorization();
	//}	

	app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}"
				);
			
			//new lines{
			endpoints.MapRazorPages();
			//}	
		});
	```

## Creating and Applying the Database Migration

1. Add a new migration to our application by running the following command.

	```
	Add-Migration Identity
	```

## Defining the Seed Data

1. Add a new class called `IdentitySeedData` to the `Data` folder

	```C#
	public static class IdentitySeedData
    {
        private const string adminEmail = "admin@test.com";
        private const string adminPassword = "Secret123$";
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
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

2. Call the `EnsurePopulated` method in the `Main` method of the `Program` class. 

	```C#
	Task.Run(async () =>
		{
			await IdentitySeedData.Initialize(services);
		}).Wait(); 
	```

***Question**
- What changes should be made to the application in order to store additional details for a User?
	> Futher reading: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/add-user-data

## Applying a Basic Authorization Policy

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
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

2. In the `Startup` class make the following changes in the `ConfigureServices` method.

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
            
        services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
    }
	```

2. Also update the `Configure` method as follows.

	```C#
	//......
 	app.UseHttpsRedirection();
	app.UseStaticFiles();

	//new{
	app.UseAuthentication(); 
	//}	
	//....
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
	> Futher reading: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/add-user-data?view=aspnetcore-2.1


## Applying a Basic Authorization Policy

1. The `[Authorize]` attribute is used to restrict access to action methods or controllers. Decorate the `AdminController` with this attribute.

2. Run the application and try to access any action on the `AdminController`

3. Create a partial view called `_LoginPartial.cshtml` in the `Views/Shared` folder.

	```CSHTML
	@using Microsoft.AspNetCore.Identity

	@inject SignInManager<IdentityUser> SignInManager
	@inject UserManager<IdentityUser> UserManager

	@if (SignInManager.IsSignedIn(User))
	{
		<form asp-area="Identity" asp-page="/Account/Logout" asp-route-returnUrl="@Url.Action("Index", "Home", new { area = "" })" method="post" id="logoutForm" class="navbar-right">
			<ul class="nav navbar-nav navbar-right">
				<li>
					<a asp-area="Identity" asp-page="/Account/Manage/Index" title="Manage">Hello @UserManager.GetUserName(User)!</a>
				</li>
				<li>
					<button type="submit" class="btn btn-link navbar-btn navbar-link">Logout</button>
				</li>
			</ul>
		</form>
	}
	else
	{
		<ul class="nav navbar-nav navbar-right">
			<li><a asp-area="Identity" asp-page="/Account/Register">Register</a></li>
			<li><a asp-area="Identity" asp-page="/Account/Login">Login</a></li>
		</ul>
	}
	```

4. Scaffold Register, Login, and LogOut

	> Futher reading: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-2.1

	Make sure to have the following code in the selected layout view

	```CSHTML
	@RenderSection("Scripts", required: false)
	```
5. Check the code in the Register, Login, and LogOut pages
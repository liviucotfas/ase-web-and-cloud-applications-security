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
        private const string adminUser = "Admin";
        private const string adminPassword = "Secret123$";
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var userManager = serviceProvider
                .GetRequiredService<UserManager<IdentityUser>>())
            {
                IdentityUser user = await userManager.FindByIdAsync(adminUser);

                if (user == null)
                {
                    user = new IdentityUser(adminUser);
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

## Creating the Account Controller and Views

1. Add a folder called `ViewModels` to your project
2. Add a view model to represent the user’s credentials to the `ViewModels` folder
	```C#
	public class LoginViewModel {
		[Required]
		public string Name { get; set; }
		[Required]
		[UIHint("password")]
		public string Password { get; set; }
		public string ReturnUrl { get; set; } = "/";
	}
	```
3. Add a new class called `AccountController.cs` to the `Controllers` folder.

	```C#
	[Authorize]
    public class AccountController : Controller
    {
        private UserManager<IdentityUser> userManager;
        private SignInManager<IdentityUser> signInManager;
        public AccountController(UserManager<IdentityUser> userMgr,
        SignInManager<IdentityUser> signInMgr)
        {
            userManager = userMgr;
            signInManager = signInMgr;
        }
        [AllowAnonymous]
        public ViewResult Login(string returnUrl)
        {
            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl
            });
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginModel)
        {
            if (ModelState.IsValid)
            {
                IdentityUser user =
                await userManager.FindByNameAsync(loginModel.Name);
                if (user != null)
                {
                    await signInManager.SignOutAsync();
                    if ((await signInManager.PasswordSignInAsync(user,
                    loginModel.Password, false, false)).Succeeded)
                    {
                        return Redirect(loginModel?.ReturnUrl ?? "/Admin/Index");
                    }
                }
            }
            ModelState.AddModelError("", "Invalid name or password");
            return View(loginModel);
        }
        public async Task<RedirectResult> Logout(string returnUrl = "/")
        {
            await signInManager.SignOutAsync();
            return Redirect(returnUrl);
        }
    }
	```

4. Update the `_ViewImports.cshtml` file to also include the `MVCStore.ViewModels` namespace.

	```HTML
	@using MVCStore
	@using MVCStore.Models
	@using MVCStore.ViewModels
	@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
	```

4. Add a view for the `Login` action

	```HTML
	@model LoginViewModel
	@{
		ViewBag.Title = "Log In";
		Layout = "_AdminLayout";
	}
	<div class="text-danger" asp-validation-summary="All"></div>
	<form asp-action="Login" asp-controller="Account" method="post">
		<input type="hidden" asp-for="ReturnUrl" />
		<div class="form-group">
			<label asp-for="Name"></label>
			<div><span asp-validation-for="Name" class="text-danger"></span></div>
			<input asp-for="Name" class="form-control" />
		</div>
		<div class="form-group">
			<label asp-for="Password"></label>
			<div><span asp-validation-for="Password" class="text-danger"></span></div>
			<input asp-for="Password" class="form-control" />
		</div>
		<button class="btn btn-primary" type="submit">Log In</button>
	</form>
	```

5. Change to the shared administration layout to add a button that will log the current user out by sending a request to the `Logout` action

	```HTML
	<!DOCTYPE html>
	<html>
	<head>
		<meta name="viewport" content="width=device-width" />
		<link rel="stylesheet" asp-href-include="lib/bootstrap/dist/css/*.min.css" />
		<title>@ViewBag.Title</title>
		<style>
			.input-validation-error {
				border-color: red;
				background-color: #fee;
			}
		</style>
		<script src="/lib/jquery/dist/jquery.min.js"></script>
		<script src="/lib/jquery-validation/dist/jquery.validate.min.js"></script>
		<script src="/lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js">
		</script>
	</head>
	<body class="m-1 p-1">
		<div class="bg-info p-2 row">
			<div class="col">
				<h4>@ViewBag.Title</h4>
			</div>
			<div class="col-2">
				<a class="btn btn-sm btn-primary"
				asp-action="Logout" asp-controller="Account">Log Out</a>
			</div>
		</div>
		@if (TempData["message"] != null)
		{
			<div class="alert alert-success mt-1">@TempData["message"]</div>
		}
		@RenderBody()
	</body>
	</html>
	```

# Roles

1. Update the `EnsurePopulated` method in the `IdentitySeedData` class as follows.

	```C#
	public static class IdentitySeedData
	{
		private const string adminUser = "Admin";
		private const string adminPassword = "Secret123$";

		public static async void EnsurePopulated(IApplicationBuilder app)
		{
			var roleManager = app.ApplicationServices
				.GetRequiredService<RoleManager<IdentityRole>>();

			if (!await roleManager.RoleExistsAsync("ProductManagement"))
				await roleManager.CreateAsync(new IdentityRole("ProductManagement"));

			var userManager = app.ApplicationServices
				.GetRequiredService<UserManager<IdentityUser>>();

			IdentityUser user = await userManager.FindByIdAsync(adminUser);
			if (user == null)
			{
				user = new IdentityUser("Admin");
				await userManager.CreateAsync(user, adminPassword);
			}

			IdentityUser adminProducts = await userManager.FindByIdAsync("AdminProductManagament");
			if (adminProducts == null)
			{
				adminProducts = new IdentityUser("AdminProductManagament");
				await userManager.CreateAsync(adminProducts, adminPassword);
				await userManager.AddToRoleAsync(adminProducts, "ProductManagement");
			}
		}
	}
	```

2. Update the last few lines in the `Index.cshtml` corresponding to the `AdminController` as follows.

	```HTML
	<a asp-action="Create" class="btn btn-primary
	   @if(!User.IsInRole("ProductManagement")){
		@: disabled
	   }
	   "
	   >Add Product</a>
	```

3. You can decorate the actions that will only be available to users that have the `ProductManagement` role as follows.

	```C#
	[Authorize(Roles = "ProductManagement")]
	```
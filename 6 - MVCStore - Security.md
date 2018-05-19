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

        services.AddDbContext<ApplicationDbContext>(
                options =>options.UseSqlServer(
            Configuration["ConnectionStrings:DefaultConnection"]));
		
		//new lines{
		services.AddIdentity<IdentityUser, IdentityRole>()
		.AddEntityFrameworkStores<ApplicationDbContext>()
		.AddDefaultTokenProviders(); 
		//}new lines

		services.AddTransient
                <IProductRepository, 
                EFProductRepository>();
            
        services.AddMvc();
    }
	```

2. Also update the `Configure` method as follows.

	```C#
	//......
	app.UseStatusCodePages();
	app.UseStaticFiles();
	app.UseSession();
	app.UseAuthentication(); 
	app.UseMvc(routes => {
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
	public static class IdentitySeedData {
        private const string adminUser = "Admin";
        private const string adminPassword = "Secret123$";
        public static async void EnsurePopulated(IApplicationBuilder app) {
            UserManager<IdentityUser> userManager = app.ApplicationServices
                .GetRequiredService<UserManager<IdentityUser>>();
            IdentityUser user = await userManager.FindByIdAsync(adminUser);
            if (user == null) {
                user = new IdentityUser("Admin");
                await userManager.CreateAsync(user, adminPassword);
            }
        }
    }
	```

2. Call the `EnsurePopulated` method in the `Configure` method of the `Startup` class. 

	```C#
	IdentitySeedData.EnsurePopulated(app); 
	```

## Applying a Basic Authorization Policy

1. The `[Authorize]` attribute is used to restrict access to action methods or controllers. Decorate the `AdminController` with this attribute.

## Creating the Account Controller and Views

1. Add a folder called `ViewModels` to your project
2. Add a view model to represent the user’s credentials to the `ViewModels` folder
	```C#
	public class LoginModel {
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
			return View(new LoginModel
			{
				ReturnUrl = returnUrl
			});
		}
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginModel loginModel)
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
	@model LoginModel
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
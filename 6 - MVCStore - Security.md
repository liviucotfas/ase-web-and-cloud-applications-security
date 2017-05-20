# CRUD ASP.NET MVC Core Application

## Adding the Identity Package to the Project

Microsoft.AspNetCore.Identity.EntityFrameworkCore

```
public class ApplicationDbContext : IdentityDbContext<IdentityUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options) { }
		public DbSet<Product> Products { get; set; }
	}
```

```
services.AddIdentity<IdentityUser, IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();
```

```
app.UseIdentity();
```

```
string adminUser = "Admin";
string adminPassword = "Secret123$";
UserManager<IdentityUser> userManager = app.ApplicationServices.GetRequiredService<UserManager<IdentityUser>>();
IdentityUser user = await userManager.FindByIdAsync(adminUser);
if (user == null)
{
    user = new IdentityUser("Admin");
    await userManager.CreateAsync(user, adminPassword);
}
```

Add-Migration Initial

Update-Database

## Applying a Basic Authorization Policy

The `[Authorize]` attribute is used to restrict access to action methods

## Creating the Account Controller and Views

```
public class LoginModel {
[Required]
public string Name { get; set; }
[Required]
[UIHint("password")]
public string Password { get; set; }
public string ReturnUrl { get; set; } = "/";
}
```

```
[Authorize]
public class AccountController : Controller {
private UserManager<IdentityUser> userManager;
private SignInManager<IdentityUser> signInManager;
public AccountController(UserManager<IdentityUser> userMgr,
SignInManager<IdentityUser> signInMgr) {
userManager = userMgr;
signInManager = signInMgr;
}
[AllowAnonymous]
public ViewResult Login(string returnUrl) {
return View(new LoginModel {
ReturnUrl = returnUrl
});
}
[HttpPost]
[AllowAnonymous]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Login(LoginModel loginModel) {
if (ModelState.IsValid) {
IdentityUser user =
await userManager.FindByNameAsync(loginModel.Name);
if (user != null) {
await signInManager.SignOutAsync();
if ((await signInManager.PasswordSignInAsync(user,
loginModel.Password, false, false)).Succeeded) {
return Redirect(loginModel?.ReturnUrl ?? "/Admin/Index");
}
}
}
ModelState.AddModelError("", "Invalid name or password");
return View(loginModel);
}
public async Task<RedirectResult> Logout(string returnUrl = "/") {
await signInManager.SignOutAsync();
return Redirect(returnUrl);
}
}
}
```

Login.cshtml File in the Views/Account Folder

```
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

_AdminLayout.cshtml File

```
DOCTYPE html>
<html>
<head>
<meta name="viewport" content="width=device-width" />
<link rel="stylesheet" asp-href-include="lib/bootstrap/dist/css/*.min.css" />
<title>@ViewBag.Title</title>
<style>
.input-validation-error { border-color: red; background-color: #fee ; }
</style>
<script asp-src-include="lib/jquery/**/jquery.min.js"></script>
<script asp-src-include="lib/jquery-validation/**/jquery.validate.min.js">
</script>
<script asp-src-include="lib/jquery-validation-unobtrusive/**/*.min.js"></script>
</head>
<body class="panel panel-default">
<div class="panel-heading">
<h4>
@ViewBag.Title

<!--- Added -->
<a class="btn btn-sm btn-primary pull-right"asp-action="Logout" asp-controller="Account">Log Out</a>
</h4>
</div>
<div class="panel-body">
@if (TempData["message"] != null) {
<div class="alert alert-success">@TempData["message"]</div>
}
@RenderBody()
</div>
</body>
</html>


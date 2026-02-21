# MVCStore - Authorization, Roles

<!-- vscode-markdown-toc -->
* 1. [Objectives](#Objectives)
* 2. [Prerequisites](#Prerequisites)
* 3. [Introduction](#Introduction)
* 4. [Roles](#Roles)
* 5. [Bibliography](#Bibliography)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

##  1. <a name='Objectives'></a>Objectives
- Understanding the difference between authentication and authorization
- Implementing role-based authorization in ASP.NET Core
- Creating and seeding Identity roles programmatically
- Switching from `AddDefaultIdentity` to `AddIdentity` to enable role support
- Protecting controller actions with the `[Authorize(Roles = "...")]` attribute
- Conditionally rendering UI elements based on the current user's roles

##  2. <a name='Prerequisites'></a>Prerequisites

Before starting this lab, you must have completed **Lab 08: Authentication**. Your project should already have:

- ASP.NET Core Identity configured (`AddDefaultIdentity`, `UseAuthentication`, `UseAuthorization`)
- `ApplicationDbContext` extending `IdentityDbContext<IdentityUser>`
- `SeedDataIdentity` class in the `Data` folder seeding an initial admin user
- `Views/Shared/_LoginPartial.cshtml` scaffolded and included in `_AdminLayout.cshtml`
- `AdminController` decorated with `[Authorize]`

##  3. <a name='Introduction'></a>Introduction
**Authorization** refers to the process that determines what a user is able to do. For example, an administrative user is allowed to create a document library, add documents, edit documents, and delete them. A non-administrative user working with the library is only authorized to read the documents.

**Authorization** is orthogonal and independent from authentication. However, authorization requires an authentication mechanism. Authentication is the process of ascertaining who a user is. Authentication may create one or more identities for the current user.

##  4. <a name='Roles'></a>Roles

1. Update the `EnsurePopulatedAsync` method in the `SeedDataIdentity` class to also create a role.

	```C#
	var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

	var roleName = "ProductManagement";

	if (!await roleManager.RoleExistsAsync(roleName))
		await roleManager.CreateAsync(new IdentityRole(roleName));
	```

2. Add a user with the "ProductManagement" role

	```C#
	var adminWithRoleEmail = "adminRole@test.com";
	var adminWithRolePassword = "Secret123$";

	IdentityUser adminWithRole = await userManager.FindByEmailAsync(adminWithRoleEmail);
	if (adminWithRole == null)
	{
		adminWithRole = new IdentityUser { UserName = adminWithRoleEmail, Email = adminWithRoleEmail };
		await userManager.CreateAsync(adminWithRole, adminWithRolePassword);
		await userManager.AddToRoleAsync(adminWithRole, roleName);
	}
	```
3. `AddDefaultIdentity` does not support roles. Replace it with `AddIdentity` in `Program.cs`:

	```csharp
	public class Program
	{
	    public static async Task Main(string[] args)
	    {
	        var builder = WebApplication.CreateBuilder(args);

	        // ... other service registrations ...

	        // !!!! new/updated code {
	        // Replace AddDefaultIdentity with AddIdentity to enable role support
	        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
	                options.SignIn.RequireConfirmedAccount = false)
	            .AddRoleManager<RoleManager<IdentityRole>>()
	            .AddDefaultUI()
	            .AddDefaultTokenProviders()
	            .AddEntityFrameworkStores<ApplicationDbContext>();
	        // }

	        // ... rest of configuration ...
	    }
	}
	```

	> **Why replace `AddDefaultIdentity`?** `AddDefaultIdentity` is a convenience method that does not register `RoleManager`. To use roles you need `AddIdentity`, which gives you full control. `.AddDefaultUI()` restores the scaffolded Login/Register pages, and `.AddDefaultTokenProviders()` restores password reset token support.

4. Update the last few lines in the `Index.cshtml` corresponding to the `AdminController` as follows.

	```HTML
	<td class="text-center">
		<a asp-action="Edit" class="btn btn-sm btn-warning"
		asp-route-productId="@item.ProductID">
			Edit
		</a>
		@if (User.IsInRole("ProductManagement"))
		{
			<form 
				asp-action="Delete" 
				method="post" style="display: inline">
				<input type="hidden" name="ProductId" value="@item.ProductID" />
				<button type="submit" class="btn btn-danger btn-sm">
					Delete
				</button>
			</form>
		}
    </td>
	```

5. Decorate the actions that should only be available to users with the `ProductManagement` role. Apply this to the `Delete` action (and optionally `Create` and `Edit`):

	```C#
	[Authorize(Roles = "ProductManagement")]
	```

##  5. <a name='Bibliography'></a>Bibliography

- [Authorization in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/introduction)
- [Role-based authorization in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/roles)
- [AddIdentity vs AddDefaultIdentity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration)
- [RoleManager<TRole> Class](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.rolemanager-1)
- [IdentityRole Class](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.identityrole)
- [User.IsInRole in Razor Views](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/views)
- [Authorize attribute — Roles](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/roles#adding-role-checks)
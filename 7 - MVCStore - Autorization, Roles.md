# MVCStore - Authorization, Roles

<!-- vscode-markdown-toc -->
* 1. [Documentation](#Documentation)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

##  1. <a name='Documentation'></a>Documentation
- Authorization Overview: https://docs.microsoft.com/en-us/aspnet/core/security/authorization/introduction?view=aspnetcore-2.1

## Introduction
Authorization refers to the process that determines what a user is able to do. For example, an administrative user is allowed to create a document library, add documents, edit documents, and delete them. A non-administrative user working with the library is only authorized to read the documents.

Authorization is orthogonal and independent from authentication. However, authorization requires an authentication mechanism. Authentication is the process of ascertaining who a user is. Authentication may create one or more identities for the current user.

## Roles

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
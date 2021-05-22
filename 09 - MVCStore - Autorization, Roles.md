# MVCStore - Authorization, Roles

<!-- vscode-markdown-toc -->
* 1. [Objectives](#Objectives)
* 2. [Documentation](#Documentation)
* 3. [Introduction](#Introduction)
* 4. [Roles](#Roles)
* 5. [Bibliography](#Bibliography)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

##  1. <a name='Objectives'></a>Objectives

##  2. <a name='Documentation'></a>Documentation
- Authorization Overview: https://docs.microsoft.com/en-us/aspnet/core/security/authorization/introduction

##  3. <a name='Introduction'></a>Introduction
Authorization refers to the process that determines what a user is able to do. For example, an administrative user is allowed to create a document library, add documents, edit documents, and delete them. A non-administrative user working with the library is only authorized to read the documents.

Authorization is orthogonal and independent from authentication. However, authorization requires an authentication mechanism. Authentication is the process of ascertaining who a user is. Authentication may create one or more identities for the current user.

##  4. <a name='Roles'></a>Roles

1. Update the `EnsurePopulated` method in the `SeedDataIdentity` class to also create a role.

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
3. Modify the `ConfigureServices` in the `Startup` class as follows

	```C#
		services.AddIdentity<IdentityUser, IdentityRole>()
			.AddRoleManager<RoleManager<IdentityRole>>()
			.AddDefaultUI()
			.AddEntityFrameworkStores<ApplicationDbContext>();
	```

4. Update the last few lines in the `Index.cshtml` corresponding to the `AdminController` as follows.

	```HTML
	<a asp-action="Create" class="btn btn-primary
	   @if(!User.IsInRole("ProductManagement")){
		@: disabled
	   }
	   "
	   >Add Product</a>
	```

5. You can decorate the actions that will only be available to users that have the `ProductManagement` role as follows.

	```C#
	[Authorize(Roles = "ProductManagement")]
	```

##  5. <a name='Bibliography'></a>Bibliography
# MVCStore - Setup and Configuration

<!-- vscode-markdown-toc -->
* 1. [Objectives](#Objectives)
* 2. [Creating the Projects](#CreatingtheProjects)
* 3. [Creating the Folder Structure](#CreatingtheFolderStructure)
* 4. [Configuring the Application](#ConfiguringtheApplication)
* 5. [Configuring the Razor View Engine](#ConfiguringtheRazorViewEngine)
* 6. [Creating the Controller and View](#CreatingtheControllerandView)
* 7. [Looking Ahead](#LookingAhead)
* 8. [Bibliography](#Bibliography)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

##  1. <a name='Objectives'></a>Objectives
- configuring the folder structure;
- configuring services and dependency injection;
- configuring middleware components;
- configuring the Razor View Engine (importing types, layout page);
- creating the default controller, action and view;
- setting up a unit test project for future labs.

##  2. <a name='CreatingtheProjects'></a>Creating the Projects

1. To create the project, select `New > Project` from the Visual Studio `File` menu and choose the `ASP.NET Core Empty`. Name the project `MVCStore`. Check the "Do not use top-level statements" checkbox.

    > **Why "Empty"?** Starting with an empty template helps you understand how ASP.NET Core works from the ground up. You'll configure the MVC framework yourself rather than using a pre-configured template.

2. Run the project. Why do you think that we are seeing the "Hello World!" text?

    > **Answer**: The default Program.cs includes `app.MapGet("/", () => "Hello World!");` which creates a minimal endpoint. You'll replace this with the MVC framework in the next steps.

### Setting Up the Test Project

3. Right-click on the solution item in the Solution Explorer and select **Add > New Project** from the popup menu. Select **xUnit Test Project** from the list of project templates and set the name of the project to `MVCStore.Tests`. Click OK to create the unit test project.

    > **Why a test project now?** Unit testing is a professional development practice that verifies your code works correctly. While we won't write tests in this lab, you'll start unit testing in later labs:
    > - **Lab 05**: Unit test controllers
    > - **Lab 06**: Unit test services with mocking (using the Repository Pattern)
    > 
    > Setting up the test project now ensures your solution structure is complete from the start and follows industry best practices.

4. Add a reference to the `MVCStore` project from the test project.

    > **How**: Right-click on the `MVCStore.Tests` project → Add → Project Reference → Select `MVCStore`

5. Install the **Moq** NuGet package in the test project.

    > **What is Moq?** Moq is a mocking library that allows you to create fake implementations of interfaces for testing purposes. For example, you can create a mock service that returns test data without accessing a real database. You'll use this extensively in **Lab 06** when testing services that depend on repositories.

##  3. <a name='CreatingtheFolderStructure'></a>Creating the Folder Structure

6. Add the following folders to the `MVCStore` project:

    | Name | Description | When You'll Use It |
    | ------------- |-------------| ------------- |
    | Models | Domain model classes (entities) | **Lab 04**: Product, Category |
    | Controllers | Controller classes that handle HTTP requests | **Lab 05**: HomeController with pagination |
    | Views | Razor view files (.cshtml) | **Lab 05**: Index views, layouts |

    > **Note**: In later labs, you'll add more folders:
    > - **Data** (Lab 04): DbContext and migrations
    > - **Services** (Lab 04): Business logic layer
    > - **Repositories** (Lab 06): Data access abstraction
    > - **DTOs** (Lab 06): Data transfer objects
    > - **ViewModels** (Lab 05): View-specific data models

##  4. <a name='ConfiguringtheApplication'></a>Configuring the Application

7. Open the `Program.cs` file.

8. Modify the `Main` method in the `Program` class as follows to enable the MVC framework:

    ```csharp
    namespace MVCStore
    {
        public class Program
        {
            public static void Main(string[] args)
            {
                var builder = WebApplication.CreateBuilder(args);

                // Add services to the container.
                builder.Services.AddControllersWithViews();

                var app = builder.Build();
                // app.MapGet("/", () => "Hello World!");  // Remove this line

                // Configure the HTTP request pipeline.
                if (!app.Environment.IsDevelopment())
                {
                    app.UseExceptionHandler("/Home/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }

                // Redirects HTTP requests to HTTPS automatically
                // Enhances security by ensuring all traffic uses encrypted connections
                app.UseHttpsRedirection();
                
                // Adds endpoint routing to the pipeline
                // Matches incoming requests to available endpoints (controllers, actions, etc.)
                // Must be placed before UseAuthorization() and endpoint mapping methods
                app.UseRouting();

                // Enables authorization middleware
                // Checks if the user is authorized to access the requested resource
                // Should be placed after UseRouting() and before endpoint mapping methods
                app.UseAuthorization();

                // Maps static file assets (CSS, JavaScript, images) with optimization
                app.MapStaticAssets();

                // Defines the default routing pattern for MVC controllers
                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}")
                    // Chains static asset support to the route
                    // Enables fingerprinting for static files referenced in views served by this route
                    .WithStaticAssets();

                app.Run();
            }
        }
    }
    ```

### Understanding the Configuration

> **builder.Services** - Dependency Injection Container
> 
> The `builder.Services` property is used to set up objects, known as **services**, that can be used throughout the application and that are accessed through **dependency injection**.
>
> **Current Setup**: Right now, we're only registering MVC services with `AddControllersWithViews()`. 
>
> **Future Labs**: In **Lab 04**, you'll register your own services here:
> ```csharp
> // Lab 04 additions:
> builder.Services.AddDbContext<ApplicationDbContext>(...);  // Database
> builder.Services.AddScoped<IProductService, ProductService>();  // Business logic
> 
> // Lab 06 additions:
> builder.Services.AddScoped<IProductRepository, ProductRepository>();  // Data access
> ```
>
> This is where **dependency injection** configuration happens—one of ASP.NET Core's most powerful features for creating maintainable, testable code.

> **AddControllersWithViews()**
>
> This method configures the MVC services for the commonly used features with controllers and views. It combines the effects of:
> - `AddMvcCore()` - Core MVC services
> - `AddApiExplorer()` - API exploration
> - `AddAuthorization()` - Authorization services
> - `AddCors()` - Cross-Origin Resource Sharing
> - `AddDataAnnotations()` - Model validation
> - `AddFormatterMappings()` - Content negotiation
> - `AddCacheTagHelper()` - Cache tag helpers
> - `AddViews()` - View engine
> - `AddRazorViewEngine()` - Razor view processing
>
> Documentation: https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.mvcservicecollectionextensions.addcontrollerswithviews

> **app - Request Pipeline**
>
> ASP.NET Core receives HTTP requests and passes them along a **request pipeline**, which is populated with middleware components registered using the `app` object. Each middleware component is able to:
> - Inspect requests
> - Modify requests
> - Generate responses
> - Modify responses from other components
>
> **Middleware Order Matters!** The order in which you add middleware determines the order in which it processes requests:
> 
> ```
> Request Flow:
> Client → HTTPS Redirect → Routing → Authorization → Static Files → MVC Controller → Response
> ```
>
> One especially important middleware component provides the **endpoint routing** feature, which matches HTTP requests to application endpoints. The `MapControllerRoute` method registers the MVC Framework as a source of endpoints using a default convention for mapping requests to classes and methods.

> **MapStaticAssets()**
>
> This method enables support for serving static content (CSS, JavaScript, images, etc.) from the `wwwroot` folder with optimizations like:
> - Content fingerprinting for cache busting
> - Compression support
> - Content negotiation
>
> In **Lab 05**, you'll see this in action when adding Bootstrap for styling.

##  5. <a name='ConfiguringtheRazorViewEngine'></a>Configuring the Razor View Engine

> The **Razor view engine** is responsible for processing view files (which have the **.cshtml** extension) to generate HTML responses. Razor allows you to mix C# code with HTML markup using the `@` symbol.

### Setting Up View Imports

9. Add the Razor View Imports file:
   - Right-click the `Views` folder
   - Select **Add > New Item** from the pop-up menu
   - Select the **"Razor View Imports"** item from the **ASP.NET Core > Web > ASP.NET** category
   - The name should be `_ViewImports.cshtml`
   - Add the following statement to the file:

    ```cshtml
    @addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
    ```

    > The `@addTagHelper` statement enables ASP.NET Core's built-in tag helpers, which provide server-side processing for HTML elements. For example:
    > - `<a asp-controller="Home" asp-action="Index">` - Generates URLs
    > - `<form asp-action="Create">` - Handles form submission
    > - `<input asp-for="Name">` - Model binding and validation
    >
    > In **Lab 05**, you'll create a custom tag helper for pagination.

    > **Note**: As you add features in later labs, you'll expand this file:
    > ```cshtml
    > @using MVCStore.Models              // Lab 04: Use Product, Category
    > @using MVCStore.ViewModels          // Lab 05: Use view models
    > @addTagHelper *, MVCStore           // Lab 05: Custom tag helpers
    > ```

### Setting Up View Start

10. Add the Razor View Start file:
    - Right-click the `Views` folder
    - Select **Add > New Item** from the pop-up menu
    - Select the **"Razor View Start"** item from the **ASP.NET Core > Web > ASP.NET** category
    - The name should be `_ViewStart.cshtml`
    - Add the following statements to the file:

    ```cshtml
    @{
        Layout = "_Layout";
    }
    ```

    > The **view start** file is executed before any view in the `Views` folder. It tells Razor to use a layout file (master page) in the HTML that it generates, reducing duplication across views.
    >
    > Without this, you'd have to specify the layout in every view file. With it, all views automatically use the `_Layout.cshtml` layout.

### Creating the Layout

11. Create a `Shared` subfolder inside the `Views` folder.

12. Add a Razor Layout file named `_Layout.cshtml` to the `Views/Shared` folder with the content shown below:

    ```cshtml
    <!DOCTYPE html>
    <html>
    <head>
        <meta name="viewport" content="width=device-width" />
        <title>@ViewBag.Title</title>
    </head>
    <body>
        <div>
            @RenderBody()
        </div>
        @await RenderSectionAsync("Scripts", required: false)
    </body>
    </html>
    ```

    > This file defines a simple HTML document into which the contents of individual views will be inserted by the `@RenderBody()` expression.
    >
    > **How it works**:
    > 1. When a view is rendered, its content replaces `@RenderBody()`
    > 2. The `@ViewBag.Title` allows views to set the page title
    > 3. The `Scripts` section allows views to add page-specific JavaScript
    >
    > In **Lab 05**, you'll enhance this layout with Bootstrap for professional styling.

##  6. <a name='CreatingtheControllerandView'></a>Creating the Controller and View

### Creating the Controller

13. Add a class file named `HomeController.cs` in the `Controllers` folder and use it to define the class shown below. This is a minimal controller that contains just enough functionality to produce a response.

    ```csharp
    using Microsoft.AspNetCore.Mvc;

    namespace MVCStore.Controllers
    {
        public class HomeController : Controller
        {
            public IActionResult Index()
            {
                return View();
            }
        }
    }
    ```

    > The `MapControllerRoute` method configured in `Program.cs` tells ASP.NET Core how to match URLs to controller classes. The default pattern `{controller=Home}/{action=Index}/{id?}` means:
    > - Requests to `/` or `/Home` or `/Home/Index` all route to this `Index` action
    > - The `controller=Home` sets the default controller
    > - The `action=Index` sets the default action
    > - The `{id?}` makes the id parameter optional

    > **Current Implementation**: The `Index` action method doesn't do anything useful yet—it just returns the result of calling the `View()` method, which is inherited from the `Controller` base class. This tells ASP.NET Core to render the default view associated with the action method.

    > **Controller Evolution**: This is a minimal controller to verify your MVC setup. In future labs, this controller will evolve significantly:
    >
    > **Lab 04**: Add dependency injection
    > ```csharp
    > public class HomeController : Controller
    > {
    >     private readonly IProductService _productService;
    >     
    >     public HomeController(IProductService productService)  // Constructor injection
    >     {
    >         _productService = productService;
    >     }
    >     
    >     public async Task<IActionResult> Index()
    >     {
    >         var products = await _productService.GetAllProductsAsync();
    >         return View(products);
    >     }
    > }
    > ```
    >
    > **Lab 05**: Add pagination
    > ```csharp
    > public async Task<IActionResult> Index(int productPage = 1)
    > {
    >     var products = await _productService.GetProductsPageAsync(productPage, PageSize);
    >     // ... pagination logic
    >     return View(products);
    > }
    > ```

### Understanding the Controller Base Class

> By inheriting from `Controller`, the `HomeController` class gains access to a wide range of useful functionality:
>
> **View Methods**:
> - `View()` - Renders the default view
> - `PartialView()` - Renders a partial view (view component)
> - `ViewComponent()` - Renders a view component
>
> **Result Methods**:
> - `RedirectToAction()` - Redirects to another action
> - `Json()` - Returns JSON data (for APIs)
> - `Content()` - Returns plain text
> - `File()` - Returns file downloads
>
> **HTTP Context**:
> - `Request` - Information about the incoming request
> - `Response` - Control over the outgoing response
> - `User` - Information about the authenticated user
> - `HttpContext` - Full context of the HTTP request/response
>
> **Model Binding and Validation**:
> - `ModelState` - Validation state of model binding
> - `TryValidateModel()` - Manually validate models
>
> **Routing Helpers**:
> - `Url.Action()` - Generate URLs to actions
> - `RouteData` - Information about the current route
>
> **Dependency Injection**:
> - Constructor parameters are automatically resolved from the service container (you'll see this in **Lab 04**)
>
> This makes the `Controller` base class the foundation for handling web requests in ASP.NET Core MVC.

### Creating the View

14. Create a `Home` subfolder inside the `Views` folder.

15. Add a Razor View file named `Index.cshtml` to the `Views/Home` folder with the content shown below:

    ```cshtml
    @{
        ViewBag.Title = "Home";
    }

    <h4>Welcome to MVCStore</h4>
    <p>Your application is running successfully!</p>
    ```

    > This view will be rendered inside the `_Layout.cshtml` layout at the `@RenderBody()` location. The `ViewBag.Title` sets the page title in the layout's `<title>` tag.

### Running the Application

16. Run the application (F5 or Ctrl+F5).

    > You should now see "Welcome to MVCStore" instead of "Hello World!". This confirms that:
    > - ✅ MVC framework is configured
    > - ✅ Routing is working
    > - ✅ Controller is found and executed
    > - ✅ View is rendered with the layout

    > **Troubleshooting**: If you still see "Hello World!", make sure you commented out or removed the `app.MapGet("/", () => "Hello World!");` line from Program.cs.

##  7. <a name='LookingAhead'></a>Looking Ahead

Congratulations! You've set up a basic MVC application structure. While simple, this foundation is crucial for everything that follows.

### What You've Built

✅ **Project Structure**: Organized folders for MVC pattern  
✅ **Dependency Injection**: Service container configured (you'll use this extensively)  
✅ **Request Pipeline**: Middleware configured in proper order  
✅ **Routing**: URL-to-controller mapping established  
✅ **View Engine**: Razor configured with layouts  
✅ **Test Project**: Ready for unit testing  

### Upcoming Labs: Building on This Foundation

#### **Lab 04: Models, Database, and Service Layer**
You'll add database integration to your application:
- Install Entity Framework Core
- Create domain models (`Product`, `Category`)
- Set up `ApplicationDbContext` for database access
- Create a service layer (`IProductService`, `ProductService`)
- Use migrations to create the database
- Seed initial data

**Key Evolution**: Your simple `HomeController` will use dependency injection to receive an `IProductService` instance, allowing it to display real data from a database.

#### **Lab 05: Controllers, Actions, Views**
You'll expand your MVC knowledge with advanced features:
- Pagination for large datasets
- Custom tag helpers for reusable UI components
- View models for complex page data
- Unit testing controllers with Moq

**Key Evolution**: The `Index` action will accept parameters, use services with pagination, and pass view models to sophisticated views.

#### **Lab 06: Advanced Architecture - Repository Pattern and DTOs**
You'll refactor your code using professional patterns:
- **DTOs (Data Transfer Objects)**: Separate classes for data transfer with validation
- **Repository Pattern**: Abstraction layer for data access
- **Separation of Concerns**: Clear boundaries between layers
- **SOLID Principles**: Professional architecture practices

**Key Evolution**: You'll understand **why** these patterns matter by first building without them, then refactoring to add them. This "learn by refactoring" approach shows the real value of architectural patterns.

#### **Lab 07 and Beyond**
- Complete CRUD operations with proper architecture
- Authentication and authorization
- Security vulnerabilities and protections (CSRF, CORS)

### The Learning Journey

This lab gave you a working MVC application, but you might wonder:
- "Why use dependency injection?" → You'll see in **Lab 04** when injecting services
- "How do I test this?" → You'll learn in **Lab 05** with unit tests
- "Isn't this too simple?" → You'll refactor in **Lab 06** for production-ready code

Each lab builds on the previous one, adding complexity only when you're ready and showing **why** each pattern or feature is needed.

### Key Takeaways

1. **Separation of Concerns**: Models, Views, and Controllers each have distinct responsibilities
2. **Dependency Injection**: Services are configured centrally and injected where needed
3. **Middleware Pipeline**: Order matters for security and functionality
4. **Convention over Configuration**: Default routing, view locations, etc.
5. **Foundation for Growth**: This simple setup scales to complex, production applications

> **Pro Tip**: Keep this simple application structure in mind as you add features in later labs. The core patterns (DI, middleware, MVC) remain the same even as the application grows in complexity.

##  8. <a name='Bibliography'></a>Bibliography

- [ASP.NET Core MVC Overview](https://docs.microsoft.com/en-us/aspnet/core/mvc/overview)
- [Dependency Injection in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
- [Razor Syntax Reference](https://docs.microsoft.com/en-us/aspnet/core/mvc/views/razor)
- [Routing in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing)
- [ASP.NET Core Middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/)
- [Unit Testing in .NET](https://docs.microsoft.com/en-us/dotnet/core/testing/)
- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)

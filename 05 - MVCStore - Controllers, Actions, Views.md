# MVCStore - Controllers, Actions and Views

<!-- vscode-markdown-toc -->
* 1. [Prerequisites](#Prerequisites)
* 2. [Objectives](#Objectives)
* 3. [Preparing the Controller](#PreparingtheController)
* 4. [Unit Testing the HomeController](#UnitTestingtheHomeController)
* 5. [Updating the View](#UpdatingtheView)
* 6. [Looking Ahead: Lab 06](#LookingAhead)
* 7. [Bibliography](#Bibliography)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

##  1. <a name='Prerequisites'></a>Prerequisites

Before starting this lab, you should have completed **Lab 04: Models, Database, and Service Layer**. You should have:
- A working `ProductService` that uses `ApplicationDbContext` directly
- Domain models (`Product`, `Category`) with navigation properties
- An `IProductService` interface registered with dependency injection
- A functioning database with seed data
- Basic understanding of how services work in ASP.NET Core

> **Note**: This lab uses the simple service layer from Lab 04, where services access `DbContext` directly. In **Lab 06: Advanced Architecture**, you'll learn to refactor this code using DTOs and the Repository Pattern for better testability and maintainability.

##  2. <a name='Objectives'></a>Objectives
- using dependency injection in controllers;
- unit testing controller actions;
- working with view models and model binding;
- understanding the limitations of current architecture (preparation for Lab 06).

##  3. <a name='PreparingtheController'></a>Preparing the Controller

1. The `HomeController` as highlighted below.

    ```C#
    using Microsoft.AspNetCore.Mvc;
    using MVCStore.Services;

    namespace MVCStore.Controllers
    {
        public class HomeController : Controller
        {
            private readonly IProductService _productService;
            
            public HomeController(IProductService productService)
            {
                _productService = productService;
            }
            
            public async Task<IActionResult> Index(CancellationToken ct)
            {
                var products = await _productService.GetAllProductsAsync(ct);
                return View(products);
            }
        }
    }
    ```

    > When ASP.NET Core needs to create a new instance of the `HomeController` class to handle an HTTP request, it will inspect the constructor and see that it requires an object that implements the `IProductService` interface. 
    
    > To determine what implementation class should be used, ASP.NET Core consults the configuration in the `Program` class, which tells it that `ProductService` should be used and that a new instance should be created for every request (via `AddScoped`). 
    
    > ASP.NET Core creates a new `ProductService` object (which receives an `ApplicationDbContext` in its constructor) and uses it to invoke the `HomeController` constructor to create the controller object that will process the HTTP request.
    
    > This is known as **dependency injection**, and it allows the `HomeController` object to access the application's data through the `IProductService` interface without knowing which implementation class has been configured. 

    > **Current Implementation**: The service layer (from Lab 04) handles business logic and uses `ApplicationDbContext` directly for data access. This is a straightforward approach that works well for learning and simple applications.

    > **Future Enhancement**: In Lab 06, you'll learn to add a Repository layer between the service and `DbContext`, and use DTOs instead of domain models. This provides better testability, security, and flexibility.

##  4. <a name='UnitTestingtheHomeController'></a>Unit Testing the HomeController

2. We can unit test that the controller is accessing the service correctly by creating a mock service, injecting it into the constructor of the `HomeController` class, and then calling the `Index` method to get the response that contains the list of products. We then compare the `Product` objects we get to what we would expect from the test data in the mock implementation.

    > **Note**: Make sure you have the required NuGet packages in your test project:
    > - `xunit`
    > - `xunit.runner.visualstudio`
    > - `Moq`
    > - `Microsoft.AspNetCore.Mvc`

    Create a test class `HomeControllerTests`:

    ```C#
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using MVCStore.Controllers;
    using MVCStore.Models;
    using MVCStore.Services;
    using Xunit;

    namespace MVCStore.Tests
    {
        public class HomeControllerTests
        {
            [Fact]
            public async Task Can_Use_Service()
            {
                // Arrange
                Mock<IProductService> mock = new Mock<IProductService>();
                mock.Setup(m => m.GetAllProductsAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Product>
                    {
                        new Product { ProductID = 1, Name = "P1", Price = 100m, CategoryID = 1 },
                        new Product { ProductID = 2, Name = "P2", Price = 200m, CategoryID = 1 }
                    });
                
                HomeController controller = new HomeController(mock.Object);
                
                // Act
                IEnumerable<Product>? result = (await controller.Index(CancellationToken.None) as ViewResult)?.ViewData.Model as IEnumerable<Product>;
                
                // Assert
                Product[] prodArray = result?.ToArray() ?? Array.Empty<Product>();
                Assert.True(prodArray.Length == 2);
                Assert.Equal("P1", prodArray[0].Name);
                Assert.Equal("P2", prodArray[1].Name);
            }
        }
    }
    ```

    > If you are not able to run the test due to Windows Smart Screen, you can run the test from the command line using `dotnet test` in the test project directory.

    > **Testing Challenges**: Notice that this test requires creating full `Product` domain models with all their properties. This approach has some limitations:
    > 
    > 1. **Dependency on Domain Models**: Tests depend on the exact structure of domain models. If you change `Product`, tests may break.
    > 2. **No Business Logic Testing**: We're testing the controller, but not the service's business logic (that would require a real database).
    > 3. **Limited Scenarios**: Hard to test validation, error handling, or security concerns at this level.
    > 
    > In **Lab 06**, you'll learn how DTOs and the Repository Pattern make testing easier by:
    > - Allowing you to mock the repository layer and test services without a database
    > - Using DTOs that are independent of domain model changes
    > - Testing business logic and validation in isolation

##  5. <a name='UpdatingtheView'></a>Updating the View

3. The `Index` action method passes the collection of `Product` objects from the service to the `View` method, which means these objects will be the view model that Razor uses when it generates HTML content from the view. Make the changes to the view shown below to generate content using the `Product` view model objects.

    Update `Views/Home/Index.cshtml`:

    ```CSHTML
    @model IEnumerable<Product>

    @{
        ViewData["Title"] = "Products";
    }

    <h1>Products</h1>

    <div class="row">
        @foreach (var p in Model ?? Enumerable.Empty<Product>()) 
        {
            <div class="col-md-4 mb-3">
                <div class="card">
                    <div class="card-body">
                        <h5 class="card-title">@p.Name</h5>
                        <p class="card-text">
                            <strong>Price:</strong> @p.Price.ToString("c")<br />
                            <strong>Category:</strong> @(p.Category?.Name ?? "Unknown")
                        </p>
                    </div>
                </div>
            </div>
        }
    </div>
    ```

    > **Note**: You might need to add the following lines to the `_ViewImports.cshtml` file:

    ```CSHTML
    @using MVCStore
    @using MVCStore.Models
    @addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
    ```

    > The `@model` expression at the top of the file specifies that the view expects to receive a sequence of `Product` objects from the action method as its model data. We use an `@foreach` expression to work through the sequence and generate a simple set of HTML elements for each `Product` object that is received.
    
    > The view doesn't know where the `Product` objects came from, how they were obtained, or whether they represent all the products known to the application. Instead, the view deals only with how details of each `Product` are displayed using HTML elements.

    > **Current Approach**: We're passing domain models (`Product`) directly to the view. This works but has security implications:
    > - Views have access to all properties of the domain model
    > - Risk of over-posting attacks if used in forms
    > - Changes to domain models affect views
    > 
    > In **Lab 06**, you'll learn to use DTOs that contain only the data needed for the view, providing better security and separation of concerns.

##  6. <a name='LookingAhead'></a>Looking Ahead: Lab 06

In this lab, you've worked directly with domain models (`Product`) in your views and controllers, and your service layer accesses `DbContext` directly. While this approach works and is easy to understand, it has some limitations:

### Current Architecture Issues

1. **Security Concerns**
   - Views have access to all properties of domain models
   - Risk of over-posting attacks when using models in forms
   - No validation layer between user input and database

2. **Testing Challenges**
   - Service layer requires a real database to test (uses `DbContext`)
   - Controller tests use domain models directly
   - Business logic is mixed with data access

3. **Tight Coupling**
   - Service layer is tightly coupled to Entity Framework Core
   - Hard to swap data sources (e.g., to use MongoDB or an API)
   - Changes to domain models affect controllers and views

4. **No Separation of Concerns**
   - Data access and business logic in the same layer (service)
   - Violates Single Responsibility Principle
   - Hard to reuse business logic in different contexts (Web API, console app, etc.)

### What You'll Learn in Lab 06

In **Lab 06: Advanced Architecture - Repository Pattern and DTOs**, you'll refactor the code you've built to address these issues:

1. **Data Transfer Objects (DTOs)**
   - Separate classes for data transfer between layers
   - Built-in validation using Data Annotations
   - Protection against over-posting attacks
   - Independent of domain model changes

2. **Repository Pattern**
   - Abstraction layer for data access
   - Easy to mock for unit testing
   - Centralized query logic
   - Flexibility to change data sources

3. **Improved Service Layer**
   - Pure business logic (no database code)
   - Uses repositories for data access
   - Works with DTOs instead of domain models
   - Easy to unit test without a database

4. **SOLID Principles**
   - Single Responsibility: Each layer has one job
   - Dependency Inversion: Depend on abstractions, not implementations
   - Practical application of professional patterns

### Example: What Will Change

**Current Controller (Lab 05)**:
```csharp
public async Task<IActionResult> Create(Product product)  // Domain model
{
    await _productService.CreateProductAsync(product);
    return RedirectToAction("Index");
}
```

**Refactored Controller (Lab 06)**:
```csharp
public async Task<IActionResult> Create(CreateProductDto dto)  // DTO with validation
{
    if (!ModelState.IsValid)
        return View(dto);
    
    await _productService.CreateProductAsync(dto);
    return RedirectToAction("Index");
}
```

**Benefits**:
- ✅ Built-in validation
- ✅ Protection against over-posting
- ✅ Separation of concerns
- ✅ Easier to test

This refactoring will prepare you for building production-ready applications with proper architecture and security!

##  7. <a name='Bibliography'></a>Bibliography

- [Dependency Injection in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
- [Tag Helpers in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/mvc/views/tag-helpers/intro)
- [Unit Testing in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/test/)
- [View Models vs Domain Models](https://docs.microsoft.com/en-us/aspnet/core/mvc/views/overview)
- [Moq Documentation](https://github.com/moq/moq4)
- [xUnit Documentation](https://xunit.net/)

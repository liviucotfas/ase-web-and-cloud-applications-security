# CRUD - Create, Read, Update and Delete

<!-- vscode-markdown-toc -->
* 1. [Objectives](#Objectives)
* 2. [Prerequisites](#Prerequisites)
* 3. [Creating a CRUD Controller](#CreatingaCRUDController)
* 4. [Displaying the Products](#DisplayingtheProducts)
* 5. [Editing the Products](#EditingtheProducts)
* 6. [Creating New Products](#CreatingNewProducts)
* 7. [Deleting Products](#DeletingProducts)
* 8. [Unit Testing](#UnitTesting)
* 9. [Bibliography](#Bibliography)

<!-- vscode-markdown-toc-config
    numbering=true
    autoSave=true
    /vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

##  1. <a name='Objectives'></a>Objectives
- Retrieving items from the database using the service and repository layers
- Adding new items to the database via DTOs
- Updating existing items in the database via DTOs
- Deleting existing items from the database
- Unit testing the Admin controller by mocking `IProductService`

##  2. <a name='Prerequisites'></a>Prerequisites

Before starting this lab, you must have completed **Lab 06: Repository Pattern and DTOs**. Your project should already have:

- `Models/DTOs/ProductDto.cs` — `ProductListItemDto`, `CreateProductDto`, `UpdateProductDto`, `ProductDetailsDto`
- `Models/DTOs/CategoryDto.cs` — `CategoryDto`
- `Models/DTOs/MappingExtensions.cs` — mapping extension methods
- `Repositories/IProductRepository.cs` and `Repositories/ProductRepository.cs`
- `Services/IProductService.cs` and `Services/ProductService.cs` that use DTOs and depend on `IProductRepository`
- `Program.cs` registering `IProductRepository` and `IProductService`

You also need to complete **Lab 06 – Assignment 1** (Category Architecture) before starting. The following interfaces and classes must exist:

- `Repositories/ICategoryRepository.cs`
- `Repositories/CategoryRepository.cs`

If you have not yet created the category repository, add it now:

`Repositories/ICategoryRepository.cs`:

```csharp
using MVCStore.Models;

namespace MVCStore.Repositories
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllAsync(CancellationToken ct = default);
        Task<Category?> GetByIdAsync(int id, CancellationToken ct = default);
    }
}
```

`Repositories/CategoryRepository.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using MVCStore.Data;
using MVCStore.Models;

namespace MVCStore.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<List<Category>> GetAllAsync(CancellationToken ct = default)
            => _context.Categories.OrderBy(c => c.Name).AsNoTracking().ToListAsync(ct);

        public Task<Category?> GetByIdAsync(int id, CancellationToken ct = default)
            => _context.Categories.FirstOrDefaultAsync(c => c.CategoryID == id, ct);
    }
}
```

Register `ICategoryRepository` in `Program.cs`:

```csharp
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
```

##  3. <a name='CreatingaCRUDController'></a>Creating a CRUD Controller

1. Add a new controller to the `Controllers` folder called `AdminController`. It depends on both `IProductService` (for CRUD operations) and `ICategoryRepository` (to populate category dropdowns).

    ```csharp
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using MVCStore.Models.DTOs;
    using MVCStore.Repositories;
    using MVCStore.Services;

    namespace MVCStore.Controllers
    {
        public class AdminController : Controller
        {
            private readonly IProductService _productService;
            private readonly ICategoryRepository _categoryRepository;

            public AdminController(IProductService productService, ICategoryRepository categoryRepository)
            {
                _productService = productService;
                _categoryRepository = categoryRepository;
            }

            public async Task<IActionResult> Index(CancellationToken ct = default)
            {
                var products = await _productService.GetAllProductsAsync(ct);
                return View(products);
            }
        }
    }
    ```

    > The controller declares dependencies on `IProductService` and `ICategoryRepository`, both resolved by dependency injection. `IProductService` handles all product business logic via the repository layer; `ICategoryRepository` is used only to populate the category `<select>` on the create/edit forms.

##  4. <a name='DisplayingtheProducts'></a>Displaying the Products

2. Add a new layout file called `_AdminLayout.cshtml` to the `Views/Shared` folder. Include a TempData area to display success messages after operations.

    ```cshtml
    <!DOCTYPE html>

    <html lang="en">
    <head>
        <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">
        <title>@ViewBag.Title</title>
        <link href="/lib/bootstrap/dist/css/bootstrap.min.css" rel="stylesheet" />
    </head>
    <body>
        <nav class="navbar bg-light navbar-expand-sm">
            <div class="container-fluid">
                <span class="navbar-brand mb-0">MVCStore Admin</span>
            </div>
        </nav>

        <div class="container-fluid mt-3">
            @if (TempData["message"] != null)
            {
                <div class="alert alert-success">@TempData["message"]</div>
            }
            @RenderBody()
        </div>

        @await RenderSectionAsync("Scripts", required: false)
    </body>
    </html>
    ```

3. Add a view `Views/Admin/Index.cshtml` for the `Index` action. The model is `IEnumerable<ProductListItemDto>` — the lightweight DTO returned by `IProductService.GetAllProductsAsync`.

    ```cshtml
    @using MVCStore.Models.DTOs
    @model IEnumerable<ProductListItemDto>

    @{
        ViewBag.Title = "Admin – Products";
        Layout = "~/Views/Shared/_AdminLayout.cshtml";
    }

    <h1>Products</h1>

    <a asp-action="Create" class="btn btn-primary mb-3">Add Product</a>

    <table class="table table-striped table-bordered table-sm">
        <thead>
            <tr>
                <th class="text-end">ID</th>
                <th>Name</th>
                <th>Category</th>
                <th class="text-end">Price</th>
                <th class="text-center">Actions</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var item in Model)
            {
                <tr>
                    <td class="text-end">@item.ProductID</td>
                    <td>@item.Name</td>
                    <td>@item.CategoryName</td>
                    <td class="text-end">@item.Price.ToString("C")</td>
                    <td class="text-center">
                        <a asp-action="Edit" asp-route-productId="@item.ProductID"
                           class="btn btn-sm btn-warning">Edit</a>
                        <form asp-action="Delete" method="post" style="display:inline">
                            @Html.AntiForgeryToken()
                            <input type="hidden" name="productId" value="@item.ProductID" />
                            <button type="submit" class="btn btn-sm btn-danger">Delete</button>
                        </form>
                    </td>
                </tr>
            }
        </tbody>
    </table>
    ```

    > The view model is now `ProductListItemDto` (not the `Product` domain model). Notice the `CategoryName` column — this is a flattened property provided by the DTO, avoiding an extra round-trip to load the `Category` navigation property in the view layer.

##  5. <a name='EditingtheProducts'></a>Editing the Products

4. Add the `Edit` GET action to `AdminController`. `GetProductByIdAsync` returns `ProductDetailsDto?`, which must be mapped to `UpdateProductDto` for the edit form. Category options are loaded from `ICategoryRepository` and stored in `ViewBag.Categories` as a `SelectList`.

    ```csharp
    public async Task<IActionResult> Edit(int productId, CancellationToken ct = default)
    {
        var product = await _productService.GetProductByIdAsync(productId, ct);
        if (product is null)
        {
            return NotFound();
        }

        var dto = new UpdateProductDto
        {
            ProductID = product.ProductID,
            Name = product.Name,
            Price = product.Price,
            CategoryID = product.Category.CategoryID
        };

        await PopulateCategoriesAsync(dto.CategoryID, ct);
        return View(dto);
    }

    // Private helper – loads categories into ViewBag.Categories
    private async Task PopulateCategoriesAsync(int selectedId = 0, CancellationToken ct = default)
    {
        var categories = await _categoryRepository.GetAllAsync(ct);
        ViewBag.Categories = new SelectList(categories, "CategoryID", "Name", selectedId);
    }
    ```

5. Add the view `Views/Admin/Edit.cshtml`. The model is `UpdateProductDto`.

    ```cshtml
    @using MVCStore.Models.DTOs
    @model UpdateProductDto

    @{
        ViewBag.Title = "Edit Product";
        Layout = "~/Views/Shared/_AdminLayout.cshtml";
    }

    <h1>Edit Product</h1>
    <hr />

    <form asp-action="Edit" method="post">
        @Html.AntiForgeryToken()
        <div asp-validation-summary="ModelOnly" class="text-danger"></div>
        <input type="hidden" asp-for="ProductID" />

        <div class="mb-3">
            <label asp-for="Name" class="form-label"></label>
            <input asp-for="Name" class="form-control" />
            <span asp-validation-for="Name" class="text-danger"></span>
        </div>
        <div class="mb-3">
            <label asp-for="Price" class="form-label"></label>
            <input asp-for="Price" class="form-control" />
            <span asp-validation-for="Price" class="text-danger"></span>
        </div>
        <div class="mb-3">
            <label asp-for="CategoryID" class="form-label"></label>
            <select asp-for="CategoryID" asp-items="ViewBag.Categories" class="form-select">
                <option value="">-- Select Category --</option>
            </select>
            <span asp-validation-for="CategoryID" class="text-danger"></span>
        </div>

        <input type="submit" value="Save" class="btn btn-primary" />
        <a asp-action="Index" class="btn btn-secondary">Cancel</a>
    </form>
    ```

6. Add the `Edit` POST action to `AdminController`. It receives an `UpdateProductDto` and calls `IProductService.UpdateProductAsync`.

    ```csharp
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateProductDto dto, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(dto.CategoryID, ct);
            return View(dto);
        }

        try
        {
            await _productService.UpdateProductAsync(dto, ct);
            TempData["message"] = $"{dto.Name} has been saved.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            await PopulateCategoriesAsync(dto.CategoryID, ct);
            return View(dto);
        }
    }
    ```

    > `UpdateProductAsync` and `CreateProductAsync` in `IProductService` already exist from Lab 06 — they validate business rules and delegate to `IProductRepository`. No changes to the service layer are needed.

##  6. <a name='CreatingNewProducts'></a>Creating New Products

7. Add the `Create` GET action to `AdminController`.

    ```csharp
    public async Task<IActionResult> Create(CancellationToken ct = default)
    {
        await PopulateCategoriesAsync(ct: ct);
        return View(new CreateProductDto());
    }
    ```

8. Add the view `Views/Admin/Create.cshtml`. The model is `CreateProductDto` (no `ProductID` field).

    ```cshtml
    @using MVCStore.Models.DTOs
    @model CreateProductDto

    @{
        ViewBag.Title = "New Product";
        Layout = "~/Views/Shared/_AdminLayout.cshtml";
    }

    <h1>New Product</h1>
    <hr />

    <form asp-action="Create" method="post">
        @Html.AntiForgeryToken()
        <div asp-validation-summary="ModelOnly" class="text-danger"></div>

        <div class="mb-3">
            <label asp-for="Name" class="form-label"></label>
            <input asp-for="Name" class="form-control" />
            <span asp-validation-for="Name" class="text-danger"></span>
        </div>
        <div class="mb-3">
            <label asp-for="Price" class="form-label"></label>
            <input asp-for="Price" class="form-control" />
            <span asp-validation-for="Price" class="text-danger"></span>
        </div>
        <div class="mb-3">
            <label asp-for="CategoryID" class="form-label"></label>
            <select asp-for="CategoryID" asp-items="ViewBag.Categories" class="form-select">
                <option value="">-- Select Category --</option>
            </select>
            <span asp-validation-for="CategoryID" class="text-danger"></span>
        </div>

        <input type="submit" value="Create" class="btn btn-primary" />
        <a asp-action="Index" class="btn btn-secondary">Cancel</a>
    </form>
    ```

9. Add the `Create` POST action.

    ```csharp
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductDto dto, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(dto.CategoryID, ct);
            return View(dto);
        }

        try
        {
            var created = await _productService.CreateProductAsync(dto, ct);
            TempData["message"] = $"{created.Name} has been created.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            await PopulateCategoriesAsync(dto.CategoryID, ct);
            return View(dto);
        }
    }
    ```

    > `CreateProductDto` and `UpdateProductDto` are **separate DTOs** (not the domain `Product`). This prevents over-posting — a user cannot supply a `ProductID` on the create form to override the database-generated key.

##  7. <a name='DeletingProducts'></a>Deleting Products

10. Add the `Delete` POST action to `AdminController`. Because `IProductService.DeleteProductAsync` returns `Task` (not the deleted product), fetch the product name first so it can appear in the confirmation message.

    ```csharp
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int productId, CancellationToken ct = default)
    {
        var product = await _productService.GetProductByIdAsync(productId, ct);
        if (product is not null)
        {
            await _productService.DeleteProductAsync(productId, ct);
            TempData["message"] = $"{product.Name} was deleted.";
        }
        return RedirectToAction(nameof(Index));
    }
    ```

##  8. <a name='UnitTesting'></a>Unit Testing

11. Unit test the `AdminController` by mocking `IProductService`. Because the controller now also depends on `ICategoryRepository`, create a mock for that too (it is not called by `Delete`, but it is required by the constructor).

    ```csharp
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using MVCStore.Controllers;
    using MVCStore.Models.DTOs;
    using MVCStore.Repositories;
    using MVCStore.Services;

    namespace MVCStore.Tests
    {
        public class AdminControllerTests
        {
            private static AdminController CreateController(
                Mock<IProductService> serviceMock,
                Mock<ICategoryRepository>? repoMock = null)
            {
                repoMock ??= new Mock<ICategoryRepository>();
                return new AdminController(serviceMock.Object, repoMock.Object);
            }

            [Fact]
            public async Task Index_Returns_All_Products()
            {
                // Arrange
                var products = new List<ProductListItemDto>
                {
                    new() { ProductID = 1, Name = "P1", CategoryName = "Cat1" },
                    new() { ProductID = 2, Name = "P2", CategoryName = "Cat2" }
                };
                var mockService = new Mock<IProductService>();
                mockService.Setup(s => s.GetAllProductsAsync(It.IsAny<CancellationToken>()))
                           .ReturnsAsync(products);

                var controller = CreateController(mockService);

                // Act
                var result = await controller.Index() as ViewResult;

                // Assert
                var model = Assert.IsAssignableFrom<IEnumerable<ProductListItemDto>>(result!.Model);
                Assert.Equal(2, model.Count());
            }

            [Fact]
            public async Task Delete_Calls_Service_And_Sets_TempData()
            {
                // Arrange
                var productDetails = new ProductDetailsDto
                {
                    ProductID = 2,
                    Name = "Test Product",
                    Price = 9.99m,
                    Category = new CategoryDto { CategoryID = 1, Name = "Cat" }
                };
                var mockService = new Mock<IProductService>();
                mockService.Setup(s => s.GetProductByIdAsync(2, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(productDetails);
                mockService.Setup(s => s.DeleteProductAsync(2, It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);

                var controller = CreateController(mockService);

                // Act
                await controller.Delete(2);

                // Assert – service was called and TempData contains the product name
                mockService.Verify(s => s.DeleteProductAsync(2, It.IsAny<CancellationToken>()), Times.Once);
                Assert.Equal("Test Product was deleted.", controller.TempData["message"]);
            }

            [Fact]
            public async Task Delete_NonExistent_Product_Does_Not_Call_DeleteAsync()
            {
                // Arrange
                var mockService = new Mock<IProductService>();
                mockService.Setup(s => s.GetProductByIdAsync(99, It.IsAny<CancellationToken>()))
                           .ReturnsAsync((ProductDetailsDto?)null);

                var controller = CreateController(mockService);

                // Act
                await controller.Delete(99);

                // Assert
                mockService.Verify(s => s.DeleteProductAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }
    }
    ```

##  9. <a name='Bibliography'></a>Bibliography
- [ASP.NET Core MVC Controllers](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/actions)
- [Tag Helpers in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/mvc/views/tag-helpers/intro)
- [Model Validation in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation)
- [Prevent Cross-Site Request Forgery (XSRF/CSRF)](https://docs.microsoft.com/en-us/aspnet/core/security/anti-request-forgery)
- [TempData in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/app-state#tempdata)
- [SelectList and DropDownList in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/mvc/views/working-with-forms#the-select-tag-helper)
- [Unit Testing Controllers in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/testing)
- [Moq Documentation](https://github.com/moq/moq4)
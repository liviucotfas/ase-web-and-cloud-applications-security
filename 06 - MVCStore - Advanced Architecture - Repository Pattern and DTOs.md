# MVCStore - Advanced Architecture: Repository Pattern and DTOs

<!-- vscode-markdown-toc -->
* 1. [Objectives](#Objectives)
* 2. [Prerequisites](#Prerequisites)
* 3. [Introduction: Why Advanced Patterns?](#IntroductionWhyAdvancedPatterns)
* 4. [Understanding DTOs (Data Transfer Objects)](#UnderstandingDTOs)
* 5. [Creating Data Transfer Objects](#CreatingDataTransferObjects)
* 6. [Understanding the Repository Pattern](#UnderstandingtheRepositoryPattern)
* 7. [Creating the Repository Layer](#CreatingtheRepositoryLayer)
* 8. [Refactoring the Service Layer](#RefactoringtheServiceLayer)
* 9. [Updating Dependency Injection](#UpdatingDependencyInjection)
* 10. [Testing the Refactored Architecture](#TestingtheRefactoredArchitecture)
* 11. [Understanding SOLID Principles](#UnderstandingSOLIDPrinciples)
* 12. [Assignments](#Assignments)
* 13. [Bibliography](#Bibliography)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

##  1. <a name='Objectives'></a>Objectives
- Understanding the limitations of direct DbContext access in services
- Implementing Data Transfer Objects (DTOs) for validation and security
- Implementing the Repository Pattern for data access abstraction
- Creating mapping logic between entities and DTOs
- Applying SOLID principles in practice
- Refactoring existing code for better testability and maintainability

##  2. <a name='Prerequisites'></a>Prerequisites

Before starting this lab, you should have completed **Lab 05: Controllers, Actions, and Views** and have:
- A working MVCStore application with Entity Framework Core
- Domain models (`Product`, `Category`)
- A database context (`ApplicationDbContext`)
- A service layer (`ProductService`) that uses `DbContext` directly
- Controllers, views, and paging functionality
- Tag helpers for pagination
- Basic understanding of dependency injection

##  3. <a name='IntroductionWhyAdvancedPatterns'></a>Introduction: Why Advanced Patterns?

### Current Architecture Problems

In Lab 04, you created a service layer that works directly with `ApplicationDbContext`:

```csharp
public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;  // Direct DbContext dependency
    
    public Task<List<Product>> GetAllProductsAsync()
    {
        return _context.Products.ToListAsync();  // Returns domain models
    }
}
```

**Problems with this approach:**

1. **Tight Coupling**: Service is tightly coupled to Entity Framework Core
2. **Testing Difficulty**: Hard to unit test without a real database
3. **Security Risks**: Domain models exposed directly to controllers/views (over-posting attacks)
4. **No Validation Layer**: Validation mixed with database operations
5. **Violation of SOLID**: Single Responsibility Principle violated (service does both business logic and data access)

### The Solution: Layered Architecture

We'll introduce two patterns to solve these problems:

```
Controller/View
     ↓ (uses DTOs)
Service Layer (Business Logic)
     ↓ (uses Domain Models)
Repository Layer (Data Access)
     ↓
DbContext
     ↓
Database
```

**Benefits:**
- ✅ Separation of Concerns
- ✅ Easier Testing (mock repositories)
- ✅ Better Security (DTOs protect domain models)
- ✅ Flexible (easy to swap data sources)

##  4. <a name='UnderstandingDTOs'></a>Understanding DTOs (Data Transfer Objects)

### What are DTOs?

DTOs are simple objects designed to transfer data between layers of an application. They differ from domain models:

| Aspect | Domain Model | DTO |
|--------|-------------|-----|
| **Purpose** | Represents business entities | Transfers data between layers |
| **Location** | `Models` folder | `Models/DTOs` folder |
| **Validation** | Usually none | Rich validation attributes |
| **Navigation Props** | Yes (EF relationships) | No (flattened data) |
| **Mutability** | Mutable | Often immutable |

### Why Use DTOs?

**1. Security - Prevent Over-Posting**

Without DTOs:
```csharp
// Controller receives Product entity directly - DANGEROUS!
[HttpPost]
public async Task<IActionResult> Create(Product product)  // Can modify ANY property!
{
    await _service.CreateProductAsync(product);
    return RedirectToAction("Index");
}
```

Attacker can send:
```json
{
  "name": "Hacked Product",
  "price": 0.01,
  "categoryID": 1,
  "productID": 999999,  // ← Can override database ID!
  "isAdmin": true       // ← Can add extra properties if model changes
}
```

With DTOs:
```csharp
// Controller receives DTO - SAFE!
[HttpPost]
public async Task<IActionResult> Create(CreateProductDto dto)  // Only allows specific fields
{
    await _service.CreateProductAsync(dto);
    return RedirectToAction("Index");
}
```

**2. Validation - Centralized and Reusable**

```csharp
public class CreateProductDto
{
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(100, MinimumLength = 2)]
    public required string Name { get; set; }

    [Range(0.01, 999999.99, ErrorMessage = "Price must be between 0.01 and 999,999.99")]
    public decimal Price { get; set; }
}
```

**3. Performance - Only Transfer What's Needed**

```csharp
// List view only needs basic info
public class ProductListItemDto
{
    public int ProductID { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string CategoryName { get; set; }  // Flattened!
}
```

##  5. <a name='CreatingDataTransferObjects'></a>Creating Data Transfer Objects

### Step 1: Create DTOs Folder

1. Create a folder: `Models/DTOs`

### Step 2: Create Product DTOs

Create `ProductDto.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace MVCStore.Models.DTOs
{
    /// <summary>
    /// DTO for displaying product details
    /// </summary>
    public class ProductDto
    {
        public int ProductID { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CategoryID { get; set; }
        public string? CategoryName { get; set; }  // Flattened from Category.Name
    }

    /// <summary>
    /// DTO for creating a new product
    /// </summary>
    public class CreateProductDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 100 characters")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be between 0.01 and 999,999.99")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category")]
        [Display(Name = "Category")]
        public int CategoryID { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing product
    /// </summary>
    public class UpdateProductDto
    {
        [Required]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, MinimumLength = 2)]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 999999.99)]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Range(1, int.MaxValue)]
        public int CategoryID { get; set; }
    }

    /// <summary>
    /// Lightweight DTO for product listings
    /// </summary>
    public class ProductListItemDto
    {
        public int ProductID { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for product details view with full category information
    /// </summary>
    public class ProductDetailsDto
    {
        public int ProductID { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public CategoryDto Category { get; set; } = null!;
    }
}
```

### Step 3: Create Category DTOs

Create `CategoryDto.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace MVCStore.Models.DTOs
{
    public class CategoryDto
    {
        public int CategoryID { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(50, MinimumLength = 2)]
        public required string Name { get; set; }
    }

    public class UpdateCategoryDto
    {
        [Required]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(50, MinimumLength = 2)]
        public required string Name { get; set; }
    }
}
```

### Step 4: Create Mapping Extensions

Create `MappingExtensions.cs`:

```csharp
namespace MVCStore.Models.DTOs
{
    /// <summary>
    /// Extension methods for mapping between domain models and DTOs
    /// </summary>
    public static class MappingExtensions
    {
        // Product → DTOs
        public static ProductDto ToDto(this Product product)
        {
            return new ProductDto
            {
                ProductID = product.ProductID,
                Name = product.Name,
                Price = product.Price,
                CategoryID = product.CategoryID,
                CategoryName = product.Category?.Name
            };
        }

        public static ProductListItemDto ToListItemDto(this Product product)
        {
            return new ProductListItemDto
            {
                ProductID = product.ProductID,
                Name = product.Name,
                Price = product.Price,
                CategoryName = product.Category?.Name ?? "Unknown"
            };
        }

        public static ProductDetailsDto ToDetailsDto(this Product product)
        {
            return new ProductDetailsDto
            {
                ProductID = product.ProductID,
                Name = product.Name,
                Price = product.Price,
                Category = product.Category?.ToDto() ?? new CategoryDto()
            };
        }

        // DTOs → Product
        public static Product ToEntity(this CreateProductDto dto)
        {
            return new Product
            {
                Name = dto.Name,
                Price = dto.Price,
                CategoryID = dto.CategoryID
            };
        }

        public static void UpdateEntity(this UpdateProductDto dto, Product product)
        {
            product.Name = dto.Name;
            product.Price = dto.Price;
            product.CategoryID = dto.CategoryID;
        }

        // Category mappings
        public static CategoryDto ToDto(this Category category)
        {
            return new CategoryDto
            {
                CategoryID = category.CategoryID,
                Name = category.Name
            };
        }

        public static Category ToEntity(this CreateCategoryDto dto)
        {
            return new Category
            {
                Name = dto.Name
            };
        }

        public static void UpdateEntity(this UpdateCategoryDto dto, Category category)
        {
            category.Name = dto.Name;
        }
    }
}
```

> **Note**: These extension methods keep mapping logic centralized and reusable. They follow the principle of having conversion logic in one place.

##  6. <a name='UnderstandingtheRepositoryPattern'></a>Understanding the Repository Pattern

### What is the Repository Pattern?

The Repository Pattern provides an abstraction layer between the business logic (service layer) and data access (Entity Framework Core). It treats data access as a collection-like interface.

**Without Repository:**
```csharp
Service → DbContext → Database
```

**With Repository:**
```csharp
Service → Repository → DbContext → Database
```

### Benefits

1. **Testability**: Easy to mock repositories for unit testing
2. **Flexibility**: Can swap data sources (SQL Server → MongoDB) without changing service code
3. **Centralized Queries**: All database queries for an entity in one place
4. **Single Responsibility**: Repository handles data access, service handles business logic

### When to Use Repositories?

✅ **Use when:**
- You need to unit test services without a database
- You might switch data sources in the future
- You have complex queries that need to be reused
- You want strict separation between layers

❌ **Don't use when:**
- Simple CRUD applications with no testing requirements
- You're already using specifications or other query patterns
- Over-engineering for a small project

##  7. <a name='CreatingtheRepositoryLayer'></a>Creating the Repository Layer

### Step 1: Create Repository Interface

Create folder `Repositories` and add `IProductRepository.cs`:

```csharp
using MVCStore.Models;

namespace MVCStore.Repositories
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllAsync(CancellationToken ct = default);
        Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Product> AddAsync(Product product, CancellationToken ct = default);
        Task UpdateAsync(Product product, CancellationToken ct = default);
        Task DeleteAsync(Product product, CancellationToken ct = default);
        Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    }
}
```

> **Design Note**: The interface works with **domain models** (`Product`), not DTOs. This keeps the repository focused on data access only.

### Step 2: Implement Repository

Create `ProductRepository.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using MVCStore.Data;
using MVCStore.Models;

namespace MVCStore.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<List<Product>> GetAllAsync(CancellationToken ct = default)
        {
            return _context.Products
                .Include(p => p.Category)  // Eager load related data
                .OrderBy(p => p.Name)
                .AsNoTracking()            // Read-only for performance
                .ToListAsync(ct);
        }

        public Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductID == id, ct);
        }

        public async Task<Product> AddAsync(Product product, CancellationToken ct = default)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync(ct);
            return product;
        }

        public async Task UpdateAsync(Product product, CancellationToken ct = default)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Product product, CancellationToken ct = default)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync(ct);
        }

        public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        {
            return _context.Products.AnyAsync(p => p.ProductID == id, ct);
        }
    }
}
```

>**Key Points:**
> - `Include()` eagerly loads the `Category` to prevent N+1 queries
> - `AsNoTracking()` improves performance for read-only operations
> - All EF Core specific logic is contained here
> 
> **Note on Paging**: This implementation doesn't include database-level paging methods. For this lab, paging is handled in the service layer using LINQ on the in-memory collection returned by `GetAllAsync()`. In production applications with large datasets, you should add paging methods to the repository that use `.Skip()` and `.Take()` at the database level for better performance.

##  8. <a name='RefactoringtheServiceLayer'></a>Refactoring the Service Layer

### Step 1: Update Service Interface

Update `IProductService.cs` to use DTOs:

```csharp
using MVCStore.Models.DTOs;

namespace MVCStore.Services
{
    public interface IProductService
    {
        Task<List<ProductListItemDto>> GetAllProductsAsync(CancellationToken ct = default);
        Task<List<ProductListItemDto>> GetProductsPageAsync(int pageNumber, int pageSize, CancellationToken ct = default);
        Task<int> GetProductCountAsync(CancellationToken ct = default);
        Task<ProductDetailsDto?> GetProductByIdAsync(int id, CancellationToken ct = default);
        Task<ProductDto> CreateProductAsync(CreateProductDto dto, CancellationToken ct = default);
        Task UpdateProductAsync(UpdateProductDto dto, CancellationToken ct = default);
        Task DeleteProductAsync(int id, CancellationToken ct = default);
    }
}
```

> **Notice**: Methods now accept and return **DTOs**, not domain models!

### Step 2: Refactor Service Implementation

Update `ProductService.cs`:

```csharp
using MVCStore.Models.DTOs;
using MVCStore.Repositories;

namespace MVCStore.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;  // Now depends on repository, not DbContext!
        }

        public async Task<List<ProductListItemDto>> GetAllProductsAsync(CancellationToken ct = default)
        {
            var products = await _productRepository.GetAllAsync(ct);
            return products.Select(p => p.ToListItemDto()).ToList();  // Map to DTO
        }

        public async Task<List<ProductListItemDto>> GetProductsPageAsync(int pageNumber, int pageSize, CancellationToken ct = default)
        {
            var products = await _productRepository.GetAllAsync(ct);
            return products
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => p.ToListItemDto())
                .ToList();
        }

        public async Task<int> GetProductCountAsync(CancellationToken ct = default)
        {
            var products = await _productRepository.GetAllAsync(ct);
            return products.Count;
        }

        public async Task<ProductDetailsDto?> GetProductByIdAsync(int id, CancellationToken ct = default)
        {
            var product = await _productRepository.GetByIdAsync(id, ct);
            return product?.ToDetailsDto();  // Map to DTO
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            // Business validation (separate from DTO validation)
            ValidateProductDto(dto.Name, dto.Price);

            var product = dto.ToEntity();  // Convert DTO to entity
            var created = await _productRepository.AddAsync(product, ct);
            
            return created.ToDto();  // Return DTO
        }

        public async Task UpdateProductAsync(UpdateProductDto dto, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            // Business validation
            ValidateProductDto(dto.Name, dto.Price);

            var product = await _productRepository.GetByIdAsync(dto.ProductID, ct);
            if (product is null)
            {
                throw new InvalidOperationException($"Product with ID {dto.ProductID} not found.");
            }

            dto.UpdateEntity(product);  // Update existing entity
            await _productRepository.UpdateAsync(product, ct);
        }

        public async Task DeleteProductAsync(int id, CancellationToken ct = default)
        {
            var product = await _productRepository.GetByIdAsync(id, ct);
            if (product is not null)
            {
                await _productRepository.DeleteAsync(product, ct);
            }
        }

        // Private helper for business validation
        private static void ValidateProductDto(string name, decimal price)
        {
            if (price < 0)
            {
                throw new InvalidOperationException("Price cannot be negative.");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Product name is required.");
            }

            if (name.Length > 100)
            {
                throw new InvalidOperationException("Product name cannot exceed 100 characters.");
            }
        }
    }
}
```

> **Key Changes:**
> - Service now depends on `IProductRepository` instead of `ApplicationDbContext`
> - All methods work with DTOs
> - Business logic separated from data access
> - Easy to unit test by mocking `IProductRepository`

##  9. <a name='UpdatingDependencyInjection'></a>Updating Dependency Injection

Update `Program.cs` to register the repository:

```csharp
using Microsoft.EntityFrameworkCore;
using MVCStore.Data;
using MVCStore.Repositories;
using MVCStore.Services;

public static void Main(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddControllersWithViews();

    builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    {
        opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        if (builder.Environment.IsDevelopment())
        {
            opts.EnableSensitiveDataLogging();
        }
    });

    // Register Repository layer
    builder.Services.AddScoped<IProductRepository, ProductRepository>();

    // Register Service layer (now depends on repository)
    builder.Services.AddScoped<IProductService, ProductService>();

    var app = builder.Build();

    // ... rest of configuration
}
```

### Update View Imports

Update `Views\_ViewImports.cshtml` to include the DTOs namespace so Razor views can recognize DTO types:

```cshtml
@using MVCStore
@using MVCStore.Models
@using MVCStore.Models.DTOs
@using MVCStore.ViewModels
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, MVCStore
```

> **Important**: Without adding `@using MVCStore.Models.DTOs`, your views will not be able to reference DTO types like `ProductListItemDto` or `ProductDetailsDto`.

##  10. <a name='TestingtheRefactoredArchitecture'></a>Testing the Refactored Architecture

### Update Your Controller

The `HomeController` already uses the service layer, so it will automatically work with DTOs. The controller doesn't need to know whether it's receiving `Product` entities or `ProductListItemDto` objects - it just passes them to the view:

```csharp
using Microsoft.AspNetCore.Mvc;
using MVCStore.Services;
using MVCStore.ViewModels;

namespace MVCStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        public int PageSize = 4;  // Number of products per page

        public HomeController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index(int productPage = 1, CancellationToken ct = default)
        {
            var products = await _productService.GetProductsPageAsync(productPage, PageSize, ct);
            var totalProducts = await _productService.GetProductCountAsync(ct);

            ViewBag.PagingInfo = new PagingInfoViewModel
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItems = totalProducts
            };

            return View(products);  // Returns List<ProductListItemDto>
        }
    }
}
```

> **Note**: The controller code remains unchanged because it works through the `IProductService` abstraction. This demonstrates the power of dependency inversion!

### Update the View

Update `Views\Home\Index.cshtml` to use the DTO:

```cshtml
@model IEnumerable<ProductListItemDto>

@{
    ViewData["Title"] = "Products";
}

<h1>Products</h1>

<div class="row">
    @foreach (var p in Model ?? Enumerable.Empty<ProductListItemDto>())
    {
        <div class="col-md-3 mb-3">
            <div class="card">
                <div class="card-body">
                    <h5 class="card-title">@p.Name</h5>
                    <p class="card-text">
                        <strong>Price:</strong> @p.Price.ToString("c")<br />
                        <strong>Category:</strong> @p.CategoryName
                    </p>
                </div>
            </div>
        </div>
    }
</div>

<div class="text-center mt-4" page-model="@ViewBag.PagingInfo" page-action="Index"></div>
```

> **Key Changes in View:**
> - Changed `@model` from `IEnumerable<Product>` to `IEnumerable<ProductListItemDto>`
> - Changed `@p.Category?.Name ?? "Unknown"` to `@p.CategoryName` (flattened property)
> - No need for null-conditional operator on CategoryName since it's a string property with a default value

### Update Unit Tests

Update `HomeControllerTests.cs` to use DTOs in mock setups:

```csharp
using Microsoft.AspNetCore.Mvc;
using Moq;
using MVCStore.Controllers;
using MVCStore.Models.DTOs;
using MVCStore.Services;
using MVCStore.ViewModels;

namespace MVCStore.Tests
{
    public class HomeControllerTests
    {
        [Fact]
        public async Task Can_Use_Service()
        {
            // Arrange
            Mock<IProductService> mock = new Mock<IProductService>();
            mock.Setup(m => m.GetProductsPageAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProductListItemDto>
                {
                    new ProductListItemDto { ProductID = 1, Name = "P1", Price = 100m, CategoryName = "Category1" },
                    new ProductListItemDto { ProductID = 2, Name = "P2", Price = 200m, CategoryName = "Category1" }
                });
            mock.Setup(m => m.GetProductCountAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(2);

            HomeController controller = new HomeController(mock.Object);

            // Act
            IEnumerable<ProductListItemDto>? result = (await controller.Index() as ViewResult)?.ViewData.Model as IEnumerable<ProductListItemDto>;

            // Assert
            ProductListItemDto[] prodArray = result?.ToArray() ?? Array.Empty<ProductListItemDto>();
            Assert.True(prodArray.Length == 2);
            Assert.Equal("P1", prodArray[0].Name);
            Assert.Equal("P2", prodArray[1].Name);
        }
    }
}
```

> **Testing Benefits**: Tests now mock the repository instead of the DbContext, making them faster and more focused on business logic.

### Run and Test

1. Start the application
2. Navigate to the home page to see the paginated product list
3. Test the pagination by clicking through pages
4. All products now display using DTOs, providing better security and separation of concerns

##  11. <a name='UnderstandingSOLIDPrinciples'></a>Understanding SOLID Principles

Your refactored architecture now follows SOLID principles:

### **S - Single Responsibility Principle**
- **Repository**: Only handles data access
- **Service**: Only handles business logic
- **Controller**: Only handles HTTP concerns
- **DTOs**: Only transfer data

### **O - Open/Closed Principle**
You can extend functionality by:
- Adding new DTOs without changing existing ones
- Creating new repositories without modifying existing ones

### **L - Liskov Substitution Principle**
Any implementation of `IProductRepository` can replace `ProductRepository`:
```csharp
// Could swap to MongoDB, Redis, etc. without changing service
services.AddScoped<IProductRepository, MongoDbProductRepository>();
```

### **I - Interface Segregation Principle**
Interfaces are focused and specific:
- `IProductRepository` only has product-related methods
- `IProductService` only has product service methods

### **D - Dependency Inversion Principle**
High-level modules (services) depend on abstractions (interfaces), not concrete implementations:
```csharp
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;  // ← Abstraction, not ProductRepository
}
```

##  12. <a name='Assignments'></a>Assignments

### Assignment 1: Implement Category Architecture

Apply the same patterns to categories:

1. Create `ICategoryRepository` and `CategoryRepository`
2. Create `ICategoryService` and `CategoryService` using DTOs
3. Update `CategoryController` to use DTOs
4. Test all CRUD operations

### Assignment 2: Unit Testing with Mocking

Create a unit test project and write tests for `ProductService`:

```csharp
using Moq;
using Xunit;

public class ProductServiceTests
{
    [Fact]
    public async Task CreateProductAsync_NegativePrice_ThrowsException()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var service = new ProductService(mockRepo.Object);
        
        var dto = new CreateProductDto
        {
            Name = "Test",
            Price = -10,
            CategoryID = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateProductAsync(dto));
    }
}
```

### Assignment 3: Understanding the Architecture

Answer the following:

1. Draw a diagram showing the flow of data from Controller → Service → Repository → Database
2. Explain why using DTOs prevents over-posting attacks
3. What would you need to change if you wanted to use MongoDB instead of SQL Server?
4. How does the Repository Pattern make unit testing easier?
5. Name three SOLID principles demonstrated in this architecture and explain how

### Assignment 4: Add Caching

Implement a caching layer in the service:

```csharp
public class CachedProductService : IProductService
{
    private readonly IProductService _innerService;
    private readonly IMemoryCache _cache;

    // Implement caching logic...
}
```

##  13. <a name='Bibliography'></a>Bibliography

- [Repository Pattern in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/repository-pattern)
- [DTOs in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-web-api)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [Unit Testing in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/test/)
- [Dependency Injection Best Practices](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines)
- [Martin Fowler - Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)

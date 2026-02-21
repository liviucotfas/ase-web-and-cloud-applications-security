# MVCStore - Models, Database, and Service Layer

<!-- vscode-markdown-toc -->
* 1. [Objectives](#Objectives)
* 2. [Installing the Entity Framework](#InstallingtheEntityFramework)
* 3. [Defining the Connection String](#DefiningtheConnectionString)
* 4. [Starting the Data Model](#StartingtheDataModel)
* 5. [Creating the Database Context Class](#CreatingtheDatabaseContextClass)
* 6. [Configuring Entity Framework Core](#ConfiguringEntityFrameworkCore)
* 7. [Creating a Service Layer](#CreatingaServiceLayer)
* 8. [Creating the Database Migration](#CreatingtheDatabaseMigration)
* 9. [Creating Seed Data](#CreatingSeedData)
* 10. [Deleting the database](#Deletingthedatabase)
* 11. [Assignments](#Assignments)
* 12. [Bibliography](#Bibliography)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

##  1. <a name='Objectives'></a>Objectives
- configuring the types of the columns using attributes;
- creating the Database Context class;
- configuring Entity Framework Core;
- creating a service layer for business logic and data access;
- using database migrations;
- seeding data;
- understanding dependency injection.

##  2. <a name='InstallingtheEntityFramework'></a>Installing the Entity Framework

The MVCStore application will store its data in a SQL Server LocalDB database, which is accessed using Entity Framework Core.

> **Entity Framework Core** is the Microsoft object-to-relational mapping (ORM) framework, and it is the most widely used method of accessing databases in ASP.NET Core projects.

1. Install the packages `Microsoft.EntityFrameworkCore.Tools` and `Microsoft.EntityFrameworkCore.SqlServer`.

##  3. <a name='DefiningtheConnectionString'></a>Defining the Connection String
2. Create an empty database (example "ism-mvcstore") using the SQL Server Explorer panel.

3. Modify the `appsettings.json` (or `appsettings.Development.json`) file in order to store the connection string.

    > A connection string specifies the location and name of the database and provides configuration settings for how the application should connect to the database server.

    ```json
    {
        "ConnectionStrings": {
            "DefaultConnection": "...."
        }
    }
    ```

> **Security Warning:** Never store passwords or other sensitive data in source code or configuration files. Production secrets must not be used for development or test environments, and secrets must not be deployed with the application. Use secure secret management solutions for production credentials. See [ASP.NET Core App Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for implementation guidance.

##  4. <a name='StartingtheDataModel'></a>Starting the Data Model

4. Add the following `Category` class to the `Models` folder:

    ```C#
    using System.ComponentModel.DataAnnotations.Schema;

    namespace MVCStore.Models
    {
        public class Category
        {
            public int CategoryID { get; set; }
            public required string Name { get; set; }

            // Navigation property
            public ICollection<Product> Products { get; set; } = new List<Product>();
        }
    }
    ```

    > The `Category` class represents a product category. The `Products` navigation property establishes a one-to-many relationship: one category can have many products. The collection is initialized to prevent null reference exceptions.

5. Add the following `Product` class to the `Models` folder:

    ```C#
    using System.ComponentModel.DataAnnotations.Schema;

    namespace MVCStore.Models
    {
        public class Product
        {
            public int ProductID { get; set; }
            public required string Name { get; set; }
            
            [Column(TypeName = "decimal(8, 2)")]
            public decimal Price { get; set; }

            public int CategoryID { get; set; }
            
            // Navigation property
            public Category? Category { get; set; }
        }
    }
    ```
    
    > The `Price` property has been decorated with the `Column` attribute to specify the SQL data type that will be used to store values for this property. Not all C# types map neatly onto SQL types, and this attribute ensures the database uses an appropriate type for the application data.

    > The `CategoryID` property is a foreign key that establishes the relationship between `Product` and `Category`. Each product belongs to one category. The nullable `Category` navigation property allows Entity Framework Core to automatically load the related category when needed.

##  5. <a name='CreatingtheDatabaseContextClass'></a>Creating the Database Context Class

> The database context class is the bridge between the application and EF Core and provides access to the application's data using model objects.

6. Add a `Data` folder to the project. Add a class file called `ApplicationDbContext.cs` to the `Data` folder and define the class shown below.

    ```C#
    using Microsoft.EntityFrameworkCore;
    using MVCStore.Models;

    namespace MVCStore.Data
    {
        public class ApplicationDbContext : DbContext
        {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
            
            public DbSet<Product> Products { get; set; }
            public DbSet<Category> Categories { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                // Configure relationships
                modelBuilder.Entity<Product>()
                    .HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryID)
                    .OnDelete(DeleteBehavior.Restrict);
            }
        }
    }
    ```

    > The `DbContext` base class provides access to Entity Framework Core's underlying functionality. The `Products` property will provide access to the `Product` table in the database, and the `Categories` property will provide access to the `Category` table.

    > The `OnModelCreating` method is overridden to configure the relationship between `Product` and `Category`. The `DeleteBehavior.Restrict` prevents cascading deletes, meaning you cannot delete a category if it has associated products. This is important for maintaining referential integrity in your database.

##  6. <a name='ConfiguringEntityFrameworkCore'></a>Configuring Entity Framework Core

Entity Framework Core must be configured so that it knows the type of database to which it will connect, which connection string describes that connection, and which context class will present the data in the database.

7. Modify the `Main` method in the `Program` class as follows.

    ```C#
    using Microsoft.EntityFrameworkCore;
    using MVCStore.Data;

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllersWithViews();
        
        // Configure Entity Framework Core to use SQL Server
        builder.Services.AddDbContext<ApplicationDbContext>(opts =>
        {
            opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            // Enable sensitive data logging only in development
            if (builder.Environment.IsDevelopment())
            {
                opts.EnableSensitiveDataLogging();
            }
        });
        
        var app = builder.Build();
        // ... rest of configuration
    }
    ```

    > The `IConfiguration` interface provides access to the ASP.NET Core configuration system, which includes the contents of the `appsettings.json` file. Entity Framework Core is configured with the `AddDbContext` method, which registers the database context class as a service. The `UseSqlServer` method declares that SQL Server is being used.
    
    > The `EnableSensitiveDataLogging()` method is called only in development environments to help with debugging by logging parameter values in SQL queries. **This should never be enabled in production for security reasons**, as it could expose sensitive information in logs.

##  7. <a name='CreatingaServiceLayer'></a>Creating a Service Layer

> The service layer provides a clean separation between controllers and data access. Services encapsulate business logic and database operations, making the application easier to test and maintain. This approach keeps controllers thin and focused on handling HTTP requests.

8. Create a `Services` folder in the project. Add a file called `IProductService.cs` and define the interface as follows:

    ```C#
    using MVCStore.Models;

    namespace MVCStore.Services
    {
        public interface IProductService
        {
            Task<List<Product>> GetAllProductsAsync(CancellationToken ct = default);
            Task<Product?> GetProductByIdAsync(int id, CancellationToken ct = default);
            Task CreateProductAsync(Product product, CancellationToken ct = default);
            Task UpdateProductAsync(Product product, CancellationToken ct = default);
            Task DeleteProductAsync(int id, CancellationToken ct = default);
        }
    }
    ```
    
    > This interface defines the contract for product-related operations. It uses async methods with `CancellationToken` support for better performance and cancellation support.

    > A class that depends on the `IProductService` interface can perform product operations without needing to know the implementation details.

9. Create the service implementation named `ProductService.cs` in the `Services` folder:

    ```C#
    using Microsoft.EntityFrameworkCore;
    using MVCStore.Data;
    using MVCStore.Models;

    namespace MVCStore.Services
    {
        public class ProductService : IProductService
        {
            private readonly ApplicationDbContext _context;

            public ProductService(ApplicationDbContext context)
            {
                _context = context;
            }

            public Task<List<Product>> GetAllProductsAsync(CancellationToken ct = default)
            {
                return _context.Products
                    .Include(p => p.Category)
                    .OrderBy(p => p.Name)
                    .ToListAsync(ct);
            }

            public Task<Product?> GetProductByIdAsync(int id, CancellationToken ct = default)
            {
                return _context.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.ProductID == id, ct);
            }

            public async Task CreateProductAsync(Product product, CancellationToken ct = default)
            {
                // Business validation
                if (product.Price < 0)
                {
                    throw new InvalidOperationException("Price cannot be negative.");
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync(ct);
            }

            public async Task UpdateProductAsync(Product product, CancellationToken ct = default)
            {
                // Business validation
                if (product.Price < 0)
                {
                    throw new InvalidOperationException("Price cannot be negative.");
                }

                _context.Products.Update(product);
                await _context.SaveChangesAsync(ct);
            }

            public async Task DeleteProductAsync(int id, CancellationToken ct = default)
            {
                var product = await GetProductByIdAsync(id, ct);
                if (product is not null)
                {
                    _context.Products.Remove(product);
                    await _context.SaveChangesAsync(ct);
                }
            }
        }
    }
    ```
    
    > The service implementation uses the `ApplicationDbContext` directly to perform data access operations. This is a straightforward approach that works well for simple applications.

    > The service layer is where business logic and validation rules are enforced (e.g., checking if the price is negative). This keeps controllers thin and focused on handling HTTP requests.

    > Note the use of `Include(p => p.Category)` to eagerly load the related category data. This is called **eager loading** and prevents the N+1 query problem.

10. Add the statement shown below to the `Main` method of the `Program` class to register the service with dependency injection:
    
    ```C#
    using Microsoft.EntityFrameworkCore;
    using MVCStore.Data;
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

        // Register the Product Service
        builder.Services.AddScoped<IProductService, ProductService>();

        var app = builder.Build();
        // ... rest of configuration
    }
    ```
    
    > ASP.NET Core supports dependency injection, which allows objects to be accessed throughout the application. The `AddScoped` method creates a service where each HTTP request gets its own service instance. This is the recommended lifetime for services that use Entity Framework Core.

11. Update the `HomeController` to use the service:

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

    > The controller receives an `IProductService` instance through constructor injection and uses it to retrieve products. This keeps the controller thin and focused on handling HTTP requests, while all business logic and data access is handled by the service.

12. Update the `Index.cshtml` view in the `Views/Home` folder to display the products:

    ```razor
    @model List<MVCStore.Models.Product>

    <div class="container mt-4">
        <h2>Security Products & Training</h2>
        <p class="text-muted">Browse our selection of security software and training courses</p>

        @if (Model != null && Model.Any())
        {
            <table class="table table-striped table-hover">
                <thead class="table-dark">
                    <tr>
                        <th>Product Name</th>
                        <th>Category</th>
                        <th>Price</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var product in Model)
                    {
                        <tr>
                            <td>@product.Name</td>
                            <td>@product.Category?.Name</td>
                            <td>@product.Price.ToString("C")</td>
                        </tr>
                    }
                </tbody>
            </table>
        }
        else
        {
            <div class="alert alert-info" role="alert">
                <h4 class="alert-heading">No products found</h4>
                <p>The product catalog is currently empty. Please seed the database with sample data.</p>
            </div>
        }
    </div>
    ```

    > The view is strongly typed to `List<MVCStore.Models.Product>`, which provides IntelliSense and compile-time type checking.

    > The view uses Bootstrap classes for styling and displays products in a table format. The `?.` operator safely handles the nullable `Category` navigation property, preventing null reference exceptions if the category is not loaded.

    > The `ToString("C")` method formats the price as currency according to the current culture settings.

##  8. <a name='CreatingtheDatabaseMigration'></a>Creating the Database Migration

> Entity Framework Core is able to generate the schema for the database using the data model classes through a feature called migrations. When you prepare a migration, Entity Framework Core creates a C# class that contains the SQL commands required to prepare the database.

13. Run one of the following commands to generate the initial migration.

    Package Manager Console panel:
    ```
    Add-Migration Initial
    ```

    Alternative using .NET CLI:
    ```
    dotnet ef migrations add Initial
    ```

14. Run the following command to update the database.

    Package Manager Console panel:
    ```
    Update-Database
    ```

    Alternative using .NET CLI: 
    ```
    dotnet ef database update
    ```

15. Check the tables that have been created in the database. You should see both `Products` and `Categories` tables, with a foreign key relationship between them.

##  9. <a name='CreatingSeedData'></a>Creating Seed Data

16. To populate the database and provide some sample data, add a class file called `SeedData.cs` to the `Data` folder.

    ```C#
    using Microsoft.EntityFrameworkCore;
    using MVCStore.Models;

    namespace MVCStore.Data
    {
        public static class SeedData
        {
            public static void EnsurePopulated(WebApplication app)
            {
                using (var scope = app.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Automatically applies pending migrations during development
				    // WARNING: Not recommended for production - use migration tools or CI/CD instead
                    /*if (context.Database.GetPendingMigrations().Any())
                    {
                        context.Database.Migrate();
                    }*/
                    
                    if (!context.Categories.Any())
                    {
                        context.Categories.AddRange(
                            new Category
                            {
                                Name = "Security Software",
                                Products = new List<Product>()
                            },
                            new Category
                            {
                                Name = "Training & Certification",
                                Products = new List<Product>()
                            }
                        );

                        context.SaveChanges();
                    }

                    if (!context.Products.Any())
                    {
                        var securitySoftware = context.Categories.First(c => c.Name == "Security Software");
                        var training = context.Categories.First(c => c.Name == "Training & Certification");

                        context.Products.AddRange(
                            new Product
                            {
                                Name = "Enterprise Antivirus License",
                                Price = 299.99m,
                                CategoryID = securitySoftware.CategoryID
                            },
                            new Product
                            {
                                Name = "Password Manager Pro",
                                Price = 49.99m,
                                CategoryID = securitySoftware.CategoryID
                            },
                            new Product
                            {
                                Name = "Certified Ethical Hacker (CEH) Course",
                                Price = 1299.00m,
                                CategoryID = training.CategoryID
                            },
                            new Product
                            {
                                Name = "Security Awareness Training",
                                Price = 199.00m,
                                CategoryID = training.CategoryID
                            }
                        );

                        context.SaveChanges();
                    }
                }
            }
        }
    }
    ```

    > The static `EnsurePopulated` method receives a `WebApplication` argument and uses it to access the application's services, including the Entity Framework Core database context service.

    > The method applies any pending migrations, then checks if the database is empty using the `Any()` method. If the database is empty, it populates it with sample data relevant to a security course context.

    > **Important:** The seeding process follows a two-step approach:
    > 1. **Seed categories first**: Categories are created and saved to ensure they have valid `CategoryID` values
    > 2. **Seed products with category references**: After categories exist, products are created with valid `CategoryID` foreign keys

17. Call the `EnsurePopulated` method in the `Main` method of the `Program` class:

    ```C#
    using Microsoft.EntityFrameworkCore;
    using MVCStore.Data;
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

        builder.Services.AddScoped<IProductService, ProductService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();
        app.MapStaticAssets();

        // Seed the database
        SeedData.EnsurePopulated(app);

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.Run();
    }
    ```

##  10. <a name='Deletingthedatabase'></a>Deleting the database

Whenever you want to delete the content of the database, you can use the following query:

```SQL
/* Azure friendly */
/* Drop all Foreign Key constraints */
DECLARE @name VARCHAR(128)
DECLARE @constraint VARCHAR(254)
DECLARE @SQL VARCHAR(254)

SELECT @name = (SELECT TOP 1 TABLE_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'FOREIGN KEY' ORDER BY TABLE_NAME)

WHILE @name is not null
BEGIN
    SELECT @constraint = (SELECT TOP 1 CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'FOREIGN KEY' AND TABLE_NAME = @name ORDER BY CONSTRAINT_NAME)
    WHILE @constraint IS NOT NULL
    BEGIN
        SELECT @SQL = 'ALTER TABLE [dbo].[' + RTRIM(@name) +'] DROP CONSTRAINT [' + RTRIM(@constraint) +']'
        EXEC (@SQL)
        PRINT 'Dropped FK Constraint: ' + @constraint + ' on ' + @name
        SELECT @constraint = (SELECT TOP 1 CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'FOREIGN KEY' AND CONSTRAINT_NAME <> @constraint AND TABLE_NAME = @name ORDER BY CONSTRAINT_NAME)
    END
SELECT @name = (SELECT TOP 1 TABLE_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'FOREIGN KEY' ORDER BY TABLE_NAME)
END
GO

/* Drop all Primary Key constraints */
DECLARE @name VARCHAR(128)
DECLARE @constraint VARCHAR(254)
DECLARE @SQL VARCHAR(254)

SELECT @name = (SELECT TOP 1 TABLE_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'PRIMARY KEY' ORDER BY TABLE_NAME)

WHILE @name IS NOT NULL
BEGIN
    SELECT @constraint = (SELECT TOP 1 CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND TABLE_NAME = @name ORDER BY CONSTRAINT_NAME)
    WHILE @constraint is not null
    BEGIN
        SELECT @SQL = 'ALTER TABLE [dbo].[' + RTRIM(@name) +'] DROP CONSTRAINT [' + RTRIM(@constraint)+']'
        EXEC (@SQL)
        PRINT 'Dropped PK Constraint: ' + @constraint + ' on ' + @name
        SELECT @constraint = (SELECT TOP 1 CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND CONSTRAINT_NAME <> @constraint AND TABLE_NAME = @name ORDER BY CONSTRAINT_NAME)
    END
SELECT @name = (SELECT TOP 1 TABLE_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'PRIMARY KEY' ORDER BY TABLE_NAME)
END
GO

/* Drop all tables */
DECLARE @name VARCHAR(128)
DECLARE @SQL VARCHAR(254)

SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] = 'U' AND category = 0 ORDER BY [name])

WHILE @name IS NOT NULL
BEGIN
    SELECT @SQL = 'DROP TABLE [dbo].[' + RTRIM(@name) +']'
    EXEC (@SQL)
    PRINT 'Dropped Table: ' + @name
    SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] = 'U' AND category = 0 AND [name] > @name ORDER BY [name])
END
GO
```

## 11. <a name='Assignments'></a>Assignments

### Assignment 1: Configuring Secret Manager for MVCStore

Learn how to use the Secret Manager tool (https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) to securely store sensitive configuration data during development. Configure the Secret Manager tool for your MVCStore project to securely store your database connection string. Ensure that:

1. Secret Manager is initialized for the project
2. Your database connection string is stored as a secret (not in `appsettings.json`)
3. Your application successfully connects to the database using the stored secret
4. Sensitive data is not committed to source control

Explain why using Secret Manager is important for development environments and how it differs from storing secrets in configuration files.

### Assignment 2: Extending the Service Layer

Create a `CategoryService` that provides CRUD operations for categories:

1. Create `ICategoryService` interface with methods:
   - `GetAllCategoriesAsync()`
   - `GetCategoryByIdAsync(int id)`
   - `CreateCategoryAsync(Category category)`
   - `UpdateCategoryAsync(Category category)`
   - `DeleteCategoryAsync(int id)`

2. Implement `CategoryService` class
3. Register the service in `Program.cs`
4. Create a `CategoryController` that uses the service
5. Test all operations

### Assignment 3: Understanding Service Benefits

Answer the following questions:

1. Why do we create a service layer instead of using `ApplicationDbContext` directly in controllers?
2. What is the benefit of using interfaces (`IProductService`) with dependency injection?
3. How does the service layer help with testing?
4. What would happen if you needed to add caching to product queries? Where would you add this logic?

> **Looking Ahead:** In Lab 06, you'll learn advanced architectural patterns including the Repository Pattern and DTOs, which will make your application even more maintainable and testable.

##  12. <a name='Bibliography'></a>Bibliography

- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Dependency Injection in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
- [ASP.NET Core App Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Data Seeding in EF Core](https://docs.microsoft.com/en-us/ef/core/modeling/data-seeding)

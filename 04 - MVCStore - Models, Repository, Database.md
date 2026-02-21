# MVCStore - Models, Service Layer and Database

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
* 11. [Bibliography](#Bibliography)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

##  1. <a name='Objectives'></a>Objectives
- configuring the types of the columns using attributes;
- creating the Database Context class;
- creating a service layer for business logic and data access;
- using database migrations;
- seeding data.

##  2. <a name='InstallingtheEntityFramework'></a>Installing the Entity Framework

The MVCStore application will store its data in a SQL Server LocalDB database, which is accessed using Entity Framework Core.

> **Entity Framework** Core is the Microsoft object-to-relational mapping (ORM) framework, and it is the most widely used method of accessing databases in ASP.NET Core projects.

1. Install the packages `Microsoft.EntityFrameworkCore.Tools` and `Microsoft.EntityFrameworkCore.SqlServer`.

##  3. <a name='DefiningtheConnectionString'></a>Defining the Connection String
2. Create an empty database (example "ism-mvcstore") using the SQL Server Explorer panel.

3. Modify the `appsettings.json` (or `appsettings.Development.json`) file in order to store the connection string.

    > A connection string specifies the location and name of the database and provides configuration settings for how the application should connect to the database server.

    ```
    {
        "ConnectionStrings": {
            "DefaultConnection": "...."
        }
    }
    ```

> **Security Warning:** Never store passwords or other sensitive data in source code or configuration files. Production secrets must not be used for development or test environments, and secrets must not be deployed with the application. Use secure secret management solutions for production credentials. See [ASP.NET Core App Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for implementation guidance.

##  4. <a name='StartingtheDataModel'></a>Starting the Data Model
5. Add the following `Product` class to the `Models` folder

    ```C#
    public class Product
	{
        public int ProductID { get; set; }
        public required string Name { get; set; }
        [Column(TypeName = "decimal(8, 2)")]
        public decimal Price { get; set; }
	}
    ```
    >The `Price` property has been decorated with the `Column` attribute to specify the SQL data type that will be used to store values for this property. Not all C# types map neatly onto SQL types, and this attribute ensures the database uses an appropriate type for the application data.

##  5. <a name='CreatingtheDatabaseContextClass'></a>Creating the Database Context Class

> The database context class is the bridge between the application and the EF Core and provides access to the application's data using model objects.

6. Add a `Data` folder to the project. Add a class file called `ApplicationDbContext.cs` to the `Data` folder and defined the class shown below.

    ```C#
    public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options) { }
		public DbSet<Product> Products { get; set; }
	}
    ```

    The `DbContext` base class provides access to the Entity Framework Core's underlying functionality, and the `Products` property will provide access to the `Product` table in the database.

##  6. <a name='ConfiguringEntityFrameworkCore'></a>Configuring Entity Framework Core

Entity Framework Core must be configured so that it knows the type of database to which it will connect, which connection string describes that connection, and which context class will present the data in the database.

7. Modify the `Main` method in the `Program` class as follows.

    ```C#
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllersWithViews();
        builder.Services.AddDbContext<ApplicationDbContext>(opts => {
            opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });
        var app = builder.Build();
    }
    ```

    > The `IConfiguration` interface provides access to the ASP.NET Core configuration system, which includes the contents of the `appsettings.json` file. Access to the configuration data is through the `builder.Configuration` property, which allows the database connection string to be obtained. Entity Framework Core is configured with the `AddDbContext` method, which registers the database context class and configures the relationship with the database. The `UseSQLServer` method declares that SQL Server is being used.

    >Entity Framework Core is configured with the `AddDbContext` method, which registers the database context class and configures the relationship with the database. The `UseSQLServer` method declares that SQL Server is being used and the connection string is read via the `IConfiguration` object.

##  7. <a name='CreatingaServiceLayer'></a>Creating a Service Layer

> The service layer provides a clean separation between the presentation layer (controllers/pages) and data access layer (Entity Framework Core). Services encapsulate business logic, data access operations, and validation, making the application easier to test and maintain.

9. Create a `Services` folder in the project. Add a file called `IProductService.cs` and define the interface as follows:

    ```C#
    public interface IProductService
    {
        Task<List<Product>> GetAllProductsAsync(CancellationToken ct = default);
        Task<Product?> GetProductByIdAsync(int id, CancellationToken ct = default);
        Task CreateProductAsync(Product product, CancellationToken ct = default);
        Task UpdateProductAsync(Product product, CancellationToken ct = default);
        Task DeleteProductAsync(int id, CancellationToken ct = default);
    }
    ```
    
    > This interface defines the contract for product-related operations. It uses async methods with `CancellationToken` support for better performance and cancellation support.

    > A class that depends on the `IProductService` interface can perform product operations without needing to know the details of how data is stored or retrieved.

10. Create the service implementation named `ProductService.cs` in the `Services` folder:

    ```C#
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
                .OrderBy(p => p.Name)
                .ToListAsync(ct);
        }

        public Task<Product?> GetProductByIdAsync(int id, CancellationToken ct = default)
        {
            return _context.Products
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
    ```
    
    > The service implementation uses the `ApplicationDbContext` directly to perform data access operations. This is the recommended approach for modern ASP.NET Core applications, as Entity Framework Core's `DbContext` already implements the Unit of Work and Repository patterns.

    > The service layer is where business logic and validation rules are enforced (e.g., checking if the price is negative). This keeps controllers/pages thin and focused on handling HTTP requests.

11. Add the statement shown below to the `Main` method of the `Program` class to create a service for the `IProductService` interface that uses `ProductService` as the implementation class.
    
    > ASP.NET Core supports services that allow objects to be accessed throughout the application using dependency injection. One benefit of services is they allow classes to use interfaces without needing to know which implementation class is being used. Application components can access objects that implement the `IProductService` interface without knowing that it is the `ProductService` implementation class they are using. This makes it easy to change the implementation class the application uses without needing to make changes to the individual components. 
    
    ```C#
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllersWithViews();
        builder.Services.AddDbContext<ApplicationDbContext>(opts => {
            opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        // !!!! new/updated code {
        builder.Services.AddScoped<IProductService, ProductService>();
        //}

        var app = builder.Build();
    }
    ```
    
    > The `AddScoped` method creates a service where each HTTP request gets its own service instance, which is the recommended lifetime for services that use Entity Framework Core.

12. Update the `HomeController` to use the service:

    ```C#
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
    ```

    > The controller receives an `IProductService` instance through constructor injection and uses it to retrieve products. This keeps the controller thin and focused on handling HTTP requests, while all business logic and data access is handled by the service.

##  8. <a name='CreatingtheDatabaseMigration'></a>Creating the Database Migration

> Entity Framework Core is able to generate the schema for the database using the data model classes through a feature called migrations. When you prepare a migration, Entity Framework Core creates a C# class that contains the SQL commands required to prepare the database. If you need to modify your model classes, then you can create a new migration that contains the SQL commands required to reflect the changes.

13.  Run one of the following command to generate the initial migration.

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

15. Check the tables that have been created in the database.

##  9. <a name='CreatingSeedData'></a>Creating Seed Data

16. To populate the database and provide some sample data, let's add a class file called `SeedData.cs` to the `Data` folder.

    > Futher details: https://docs.microsoft.com/en-us/aspnet/core/tutorials/razor-pages/sql

    ```C#
    public static class SeedData
    {
        public static void EnsurePopulated(WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                /* Not recommended for production code, but useful during development to automatically create and update the database. In production, you should use the command line tools or CI/CD to apply migrations to the database.
                if (context.Database.GetPendingMigrations().Any())
                {
                    context.Database.Migrate();
                }*/
                
                if (!context.Products.Any())
                {
                    context.Products.AddRange(
                    new Product
                    {
                        Name = "Kayak",
                        Price = 275
                    },
                    new Product
                    {
                        Name = "Lifejacket",
                        Price = 48.95m
                    },
                    new Product
                    {
                        Name = "Soccer Ball",
                        Price = 19.50m
                    }
                    );

                    context.SaveChanges();
                }
            }
        }
    }
    ```

    >The static `EnsurePopulated` method receives an `WebApplication` argument, which is the interface used in the `Main` method of the `Program` class to register middleware components to handle HTTP requests. 
    
    >`WebApplication` also provides access to the application's services, including the Entity Framework Core database context service.

    > The `EnsurePopulated` method obtains a `ApplicationDbContext` object through the `WebApplication` interface and calls the `Database.Migrate` method if there are any pending migrations, which means that the database will be created and prepared so that it can store `Product` objects. Next, the number of `Product` objects in the database is checked. If there are no objects in the database, then the database is populated using a collection of `Product` objects using the `AddRange` method and then written to the database using the `SaveChanges` method.

17. Call the `EnsurePopulated` in the `Main` method of the `Program` class.

    ```C#
    public static void Main(string[] args)
    {
        ......

        // !!!! new/updated code {
        SeedData.EnsurePopulated(app);
        //}

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
    ```

##  10. <a name='Deletingthedatabase'></a>Deleting the database

Whenever you want to delete the content of the database, you can use the following query.

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

## 11. <a name='Assignments'></a>Assignment: Configuring Secret Manager for MVCStore

1. Learn how to use the Secret Manager tool (https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) to securely store sensitive configuration data during development, keeping secrets out of source control. Configure the Secret Manager tool for your MVCStore project to securely store your database connection string during development. Ensure that:
    1. Secret Manager is initialized for the project
    2. Your database connection string is stored as a secret (not in `appsettings.json`)
    3. Your application successfully connects to the database using the stored secret
    4. Sensitive data is not committed to source control
Explain why using Secret Manager is important for development environments and how it differs from storing secrets in configuration files.

##  12. <a name='Bibliography'></a>Bibliography

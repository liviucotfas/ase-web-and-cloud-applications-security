# MVCStore - Models, Repository and Database

<!-- vscode-markdown-toc -->
* 1. [Objectives](#Objectives)
* 2. [ 2. Installing the Entity Framework](#2.InstallingtheEntityFramework)
* 3. [3. Defining the Connection String](#DefiningtheConnectionString)
* 4. [Starting the Data Model](#StartingtheDataModel)
* 5. [Creating the Database Context Class](#CreatingtheDatabaseContextClass)
* 6. [Configuring Entity Framework Core](#ConfiguringEntityFrameworkCore)
* 7. [Creating a Repository](#CreatingaRepository)
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
- creating a repository;
- using database migrations;
- seeding data.

##  2. <a name='2.InstallingtheEntityFramework'></a> 2. Installing the Entity Framework

The MVCStore application will store its data in a SQL Server LocalDB database, which is accessed using Entity Framework Core.

> **Entity Framework** Core is the Microsoft object-to-relational mapping (ORM) framework, and it is the most widely used method of accessing databases in ASP.NET Core projects.

1. Install the packages `Microsoft.EntityFrameworkCore.Design` and `Microsoft.EntityFrameworkCore.SqlServer`.

##  3. <a name='DefiningtheConnectionString'></a>3. Defining the Connection String
2. Create an empty database using the SQL Server Explorer panel.

3. Modify the `appsettings.json` (or `appsettings.Development.json`) file in order to store the connection string.

    > A connection string specifies the location and name of the database and provides configuration settings for how the application should connect to the database server.

    ```
    {
         "ConnectionStrings": {
            "DefaultConnection": "...."
        }
    }
    ```
4. Update the connection string in `appsettings.json`

##  4. <a name='StartingtheDataModel'></a>Starting the Data Model
5. Add the following `Product` class to the `Models` folder

    ```C#
    public class Product
	{
	    public int ProductID { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
        [Column(TypeName = "decimal(8, 2)")]
		public decimal Price { get; set; }
		public string Category { get; set; }
	}
    ```
    >The `Price` property has been decorated with the `Column` attribute to specify the SQL data type that will be used to store values for this property. Not all C# types map neatly onto SQL types, and this attribute ensures the database uses an appropriate type for the application data.

##  5. <a name='CreatingtheDatabaseContextClass'></a>Creating the Database Context Class

> The database context class is the bridge between the application and the EF Core and provides access to the application’s data using model objects.

6. Add a `Data` folder to the project. Add a class file called `ApplicationDbContext.cs` to the `Data` folder and defined the class shown below.

    ```C#
    public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options) { }
		public DbSet<Product> Products { get; set; }
	}
    ```

    The `DbContext` base class provides access to the Entity Framework Core’s underlying functionality, and the `Products` property will provide access to the `Product` table in the database.

##  6. <a name='ConfiguringEntityFrameworkCore'></a>Configuring Entity Framework Core

Entity Framework Core must be configured so that it knows the type of database to which it will connect, which connection string describes that connection, and which context class will present the data in the database.

7. Add the following code to the `Startup` class

    ```C#
    private IConfiguration Configuration;
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    ```
8. Update the `ConfigureServices` method in the `Startup` class as follows.

    ```C#
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews();

        // !!!! new/updated code {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                Configuration.GetConnectionString("DefaultConnection")));
        //}
    }
    ```

    > The `IConfiguration` interface provides access to the ASP.NET Core configuration system, which includes the contents of the `appsettings.json` file. The constructor receives an `IConfiguration` object through its constructor and assigns it to the `Configuration` property, which is used to access the connection string.

    >Entity Framework Core is configured with the `AddDbContext` method, which registers the database context class and configures the relationship with the database. The `UseSQLServer` method declares that SQL Server is being used and the connection string is read via the `IConfiguration` object.

##  7. <a name='CreatingaRepository'></a>Creating a Repository

> The repository pattern is widely used, and it provides a consistent way to access the features presented by the database context class. It can reduce duplication and ensures that operations on the database are performed consistently.

9. Add a `Data` folder and define the `IStoreRepository` interface as follows.

    ```C#
    public interface IStoreRepository
	{
		IQueryable<Product> Products { get; }
	}
    ```
    > This interface uses `IQueryable<T>` to allow a caller to obtain a sequence of Product objects. The `IQueryable<T>` interface is derived from the more familiar `IEnumerable<T>` interface and represents a collection of objects that can be queried, such as those managed by a database.

    > A class that depends on the `IStoreRepository` interface can obtain `Product` objects without needing to know the details of how they are stored or how the implementation class will deliver them.

    > **Understanding Ienumerable<T> and Iqueryable<T> Interfaces**

    >The `IQueryable<T>` interface is useful because it allows a collection of objects to be queried efficiently. Later, when we add support for retrieving a subset of `Product` objects from a database, using the `IQueryable<T>` interface allows us to ask the database for just the objects that we require using standard LINQ statements and without needing to know what database server stores the data or how it processes the query. Without the `IQueryable<T>` interface, we would have to retrieve all of the `Product` objects from the database and then discard the ones we don’t want, which becomes an expensive operation as the amount of data used by an application increases. It is for this reason that the `IQueryable<T>` interface is typically used instead of `IEnumerable<T>` in database repository interfaces and classes.
    >However, care must be taken with the `IQueryable<T>` interface because each time the collection of objects is enumerated, the query will be evaluated again, which means that a new query will be sent to the database. This can undermine the efficiency gains of using `IQueryable<T>`. In such situations, you can convert `IQueryable<T>` to a more predictable form using the `ToList` or `ToArray` extension method.

10. Create an implementation of the repository interface,named `EFStoreRepository.cs` in the `Models` folder and
use it to define the class shown below.

    ```C#
    public class EFStoreRepository : IStoreRepository {
        private ApplicationDbContext context;
        
        public EFStoreRepository(ApplicationDbContext ctx) {
            context = ctx;
        }

        public IQueryable<Product> Products {
            get {
                return context.Products;
            }
        } 
    }
    ```
    > The repository implementation maps the `Products` property defined by the `IStoreRepository` interface onto the `Products` property defined by the `ApplicationDbContext` class. The `Products` property in the context class returns a `DbSet<Product>` object, which implements the `IQueryable<T>` interface and makes it easy to implement the repository interface when using Entity Framework Core.

11. Add the statement shown below to the `Startup` class to create a service for the `IStoreRepository` interface that uses `EFStoreRepository` as the implementation class
    > ASP.NET Core supports services that allow objects to be accessed throughout the application. One benefit of services is they allow classes to use interfaces without needing to know which implementation class is being used. Application components can access objects that implement the `IStoreRepository` interface without knowing that it is the `EFStoreRepository` implementation class they are using. This makes it easy to change the implementation class the application uses without needing to make changes to the individual components. 
    
    ```C#
    public void ConfigureServices(IServiceCollection services) {
        services.AddControllersWithViews();

        services.AddDbContext<StoreDbContext>(opts => {
        opts.UseSqlServer(
        Configuration["ConnectionStrings:SportsStoreConnection"]);
        });

        // !!!! new/updated code {
        services.AddScoped<IStoreRepository, EFStoreRepository>();
        //}
    }
    ```
    > The `AddScoped` method creates a service where each HTTP request gets its own repository object, which is the way that Entity Framework Core is typically used.

##  8. <a name='CreatingtheDatabaseMigration'></a>Creating the Database Migration

> Entity Framework Core is able to generate the schema for the database using the data model classes through a feature called migrations. When you prepare a migration, Entity Framework Core creates a C# class that contains the SQL commands required to prepare the database. If you need to modify your model classes, then you can create a new migration that contains the SQL commands required to reflect the changes.

12.  Run the following command to generate the initial migration using the `Package Manager Console` panel.

    ```
    Add-Migration Initial
    ```
    > If the command is not recognized, install the package `Microsoft.EntityFrameworkCore.Tools` 

13. Run the following command to update the database.

    ```
    Update-Database
    ```

14. Check the tables that have been created in the database.

##  9. <a name='CreatingSeedData'></a>Creating Seed Data

14. To populate the database and provide some sample data, let's add a class file called `SeedData.cs` to the `Data` folder.

    > Futher details: https://docs.microsoft.com/en-us/aspnet/core/tutorials/razor-pages/sql

    ```C#
   public class SeedData
    {
        public static void EnsurePopulated(IApplicationBuilder app)
        {
            ApplicationDbContext context = app.ApplicationServices
            .CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }
            if (!context.Products.Any())
            {
                context.Products.AddRange(
                new Product
                {
                    Name = "Kayak",
                    Description = "A boat for one person",
                    Category = "Watersports",
                    Price = 275
                },
                new Product
                {
                    Name = "Lifejacket",
                    Description = "Protective and fashionable",
                    Category = "Watersports",
                    Price = 48.95m
                },
                new Product
                {
                    Name = "Soccer Ball",
                    Description = "FIFA-approved size and weight",
                    Category = "Soccer",
                    Price = 19.50m
                });
            }
            context.SaveChanges();
        }
    }
    ```

    >The static `EnsurePopulated` method receives an `IApplicationBuilder `argument, which is the interface used in the `Configure` method of the `Startup` class to register middleware components to handle HTTP requests. 
    
    >`IApplicationBuilder` also provides access to the application’s services, including the Entity Framework Core database context service.

    > The `EnsurePopulated` method obtains a `ApplicationDbContext` object through the `IApplicationBuilder` interface and calls the `Database.Migrate` method if there are any pending migrations, which means that the database will be created and prepared so that it can store `Product` objects. Next, the number of `Product` objects in the database is checked. If there are no objects in the database, then the database is populated using a collection of `Product` objects using the `AddRange` method and then written to the database using the `SaveChanges` method.

15. Call the `EnsurePopulated` in the `Configure` method of the `Startup` class.

    ```C#
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStatusCodePages();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapDefaultControllerRoute();
        });

        // !!!! new/updated code {
        SeedData.EnsurePopulated(app);
        //}
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

##  11. <a name='Bibliography'></a>Bibliography

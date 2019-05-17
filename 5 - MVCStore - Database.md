# CRUD ASP.NET MVC Core Application

## Creating the Database Classes

> The database context class is the bridge between the application and the EF Core and provides access to the application’s data using model objects.

1. Add a class file called `ApplicationDbContext.cs` to the Data folder and defined the class shown bellow.

    ```C#
    public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options) { }
		public DbSet<Product> Products { get; set; }
	}
    ```

    The DbContext base class provides access to the Entity Framework Core’s underlying functionality, and the Products property will provide access to the Product objects in the database.

## Creating the Repository Class

4. The next step is to create a class that implements the IProductRepository interface and gets its data using Entity Framework Core.

    ```C#
    public class EFProductRepository : IProductRepository
	{
		private ApplicationDbContext context;
		
        public EFProductRepository(ApplicationDbContext ctx)
		{
			context = ctx;
		}

		public IQueryable<Product> Products()
        {
            return context.Products;
        }
	}
    ```

## Defining the Connection String

1. Add an `appsettings.json` file using the ASP.NET Configuration File item template in the ASP.NET section of the Add New Item window.

    > A connection string specifies the location and name of the database and provides configuration settings for how the application should connect to the database server.

    ```
    {
         "ConnectionStrings": {
            "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=_CHANGE_ME;Trusted_Connection=True;MultipleActiveResultSets=true"
        }
    }
    ```
2. Create an empty database

3. Update the connection string in `appsettings.json`


## Configuring the Application

1. Add the following code to the `Startup` class

    ```C#
    IConfiguration Configuration;
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    ```
2. Update the `ConfigureServices` method in the `Startup` class as follows.

    ```C#
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                Configuration.GetConnectionString("DefaultConnection")));

        services.AddTransient<IProductRepository, EFProductRepository>();

        services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
    }
    ```

## Creating and Applying the Database Migration

1. Run the following command to generate the initial migration.

    ```
    Add-Migration Initial
    ```

2. Run the following command to update the database.

    ```
    Update-Database
    ```

## Creating the Seed Data

3. To populate the database and provide some sample data, let's add a class file called `SeedData.cs` to the `Data` folder.

> Futher details: https://docs.microsoft.com/en-us/aspnet/core/tutorials/razor-pages/sql?view=aspnetcore-2.2

    ```C#
    public static class SeedData
	{
		public static void Initialize(IServiceProvider serviceProvider)
		{
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<ApplicationDbContext>>()))
            {
                // Look for any products.
                if (context.Products.Any())
                {
                    return;   // DB has been seeded
                }

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
                    },
                    new Product
                    {
                        Name = "Corner Flags",
                        Description = "Give your playing field a professional touch",
                        Category = "Soccer",
                        Price = 34.95m
                    },
                    new Product
                    {
                        Name = "Stadium",
                        Description = "Flat-packed 35,000-seat stadium",
                        Category = "Soccer",
                        Price = 79500
                    },
                    new Product
                    {
                        Name = "Thinking Cap",
                        Description = "Improve brain efficiency by 75%",
                        Category = "Chess",
                        Price = 16
                    },
                    new Product
                    {
                        Name = "Unsteady Chair",
                        Description = "Secretly give your opponent a disadvantage",
                        Category = "Chess",
                        Price = 29.95m
                    },
                    new Product
                    {
                        Name = "Human Chess Board",
                        Description = "A fun game for the family",
                        Category = "Chess",
                        Price = 75
                    },
                    new Product
                    {
                        Name = "Bling-Bling King",
                        Description = "Gold-plated, diamond-studded King",
                        Category = "Chess",
                        Price = 1200
                    }
                );

                context.SaveChanges();
            }
		}
	}
    ```

4. Let's update the `Main` method of the `Program` class as follows.

    ```C#
    public static void Main(string[] args)
    {
        var host  = CreateWebHostBuilder(args).Build();

        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            try
            {
                var context = services.
                    GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate();
                SeedData.Initialize(services);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred seeding the DB.");
            }
        }

        host.Run();
    }
    ```
3. Update the `ConfigureServices` method in the `Startup` class to use `EFProductRepository` instead of `FakeProductRepository`

    ```C#
    services.AddTransient<IProductRepository, EFProductRepository>();
    ```

# Deleting the database

1. Whenever you want to delete the content of the database, you can use the following query.

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

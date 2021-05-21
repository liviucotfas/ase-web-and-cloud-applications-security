# MVCStore - Models and Repository

<!-- vscode-markdown-toc -->
* 1. [Objectives](#Objectives)
* 2. [ 2. Installing the Entity Framework](#2.InstallingtheEntityFramework)
* 3. [3. Defining the Connection String](#DefiningtheConnectionString)
* 4. [Starting the Data Model](#StartingtheDataModel)
* 5. [Creating the Database Context Class](#CreatingtheDatabaseContextClass)
* 6. [Configuring Entity Framework Core](#ConfiguringEntityFrameworkCore)
* 7. [Creating a Repository](#CreatingaRepository)
* 8. [Creating the Database Migration](#CreatingtheDatabaseMigration)
* 9. [Creating a Fake Repository](#CreatingaFakeRepository)
* 10. [Registering the Repository Service](#RegisteringtheRepositoryService)
* 11. [Adding a Controller](#AddingaController)
* 12. [Setting the Default Route](#SettingtheDefaultRoute)
* 13. [Bibliography](#Bibliography)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

##  1. <a name='Objectives'></a>Objectives
- configuring the types of the columns using attributes;
- creating the Database Context class;

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

> Entity Framework Core is able to generate the schema for the database using the data model classes through a feature called migrations. When you prepare a migration, Entity Framework Core creates a C# class that contains the SQL commands required to prepare the database. If you need to modify your model classes, then you can create a new migration that contains the SQL commands required to reflect the changes. In this way, you don’t have to worry about manually writing and testing SQL commands and can just focus on the C# model classes in the application.

1. Run the following command to generate the initial migration using the `Package Manager Console` panel.

    ```
    Add-Migration Initial
    ```
    > If the command is not recognized, install the package `Microsoft.EntityFrameworkCore.Tools` 

2. Run the following command to update the database.

    ```
    Update-Database
    ```



##  9. <a name='CreatingaFakeRepository'></a>Creating a Fake Repository

3. Create the fake repository called `FakeProductRepository` in the `Data` folder.

     ```C#
    public class FakeProductRepository : IProductRepository
	{
		public IQueryable<Product> Products => new List<Product> {
            new Product { Name = "Windows 10", Price = 10 },
            new Product { Name = "Visual Studio", Price = 10 },
            new Product { Name = "Office 365", Price = 10 }
        }.AsQueryable();
	}
    ``` 

    > The `FakeProductRepository` class implements the `IProductRepository` interface by returning a fixed collection of `Product` objects as the value of the Products property. The `AsQueryable` method is used to convert the fixed collection of objects to an `IQueryable<Product>`, which is required to implement the `IProductRepository` interface and allows us to create a compatible fake repository without having to deal with real queries.    

##  10. <a name='RegisteringtheRepositoryService'></a>Registering the Repository Service

>MVC emphasizes the use of loosely coupled components, which means that you can make a change in one part of the application without having to make corresponding changes elsewhere. This approach categorizes parts of the application as services, which provide features that other parts of the application use. The class that provides a service can then be altered or replaced without requiring changes in the classes that use it.

4. Modify the `ConfigureServices` method in the `Startup` class as follows

    ```C#
    public void ConfigureServices(IServiceCollection services)
    {
        // !!!! add this line{ 
        services.AddTransient<IProductRepository, FakeProductRepository>();
        // }!!!!
        services.AddMvc();
    }
    ```

The statement added to the `ConfigureServices` method tells ASP.NET that when a component, such as a controller, needs an implementation of the `IProductRepository` interface, it should receive an instance of the `FakeProductRepository` class. The AddTransient method specifies that a new `FakeProductRepository` object should be created each time the `IProductRepository` interface is needed.

##  11. <a name='AddingaController'></a>Adding a Controller

7. Add a controler called `ProductController` to the `Controllers` folder

    ```C#
	public class ProductController : Controller
	{
		private IProductRepository repository;
		public ProductController(IProductRepository repo)
		{
			repository = repo;
		}
	}
    ```
    When MVC needs to create a new instance of the ProductController class to handle an HTTP request, it will inspect the constructor and see that it requires an object that implements the `IProductRepository` interface. To determine what implementation class should be used, MVC consults the configuration in the `Startup` class, which tells it that `FakeRepository` should be used and that a new instance should be created every time. MVC creates a new `FakeRepository` object and uses it to invoke the `ProductController` constructor in order to create the controller object that will process the HTTP request.

    This is known as **dependency injection**, and its approach allows the `ProductController` to access the application’s repository through the `IProductRepository` interface without having any need to know which implementation class has been configured. Later, we’ll replace the fake repository with the real one, and dependency injection means that the controller will continue to work without changes.

8. Add the following action to the `ProductController`

    ```C#
    public IActionResult List(){
        return View(repository.Products); 
    } 
    ```

9. Add a shared layout called `_Layout.cshtml` to the `Views/Shared` folder, by choosing the "Rayzor Layout" type of file in the "Add > New Item". It should be generated as follows.

    ```HTML
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

         @RenderSection("Scripts", required: false)
    </body>
    </html>
    ```
10. We need to configure the application so that the `_Layout.cshtml` file is applied by default. This is done by adding an "Razor View Start" file called `_ViewStart.cshtml` to the Views folder. Make sure that the content of the `_ViewStart.cshtml` file is as follows.

    ```CSHTML
    @{
        Layout = "_Layout";
    }
    ```
11. Add the view that will be used for displaying the products

    ```CSHTML
    @model IEnumerable<Product>
    @foreach (var p in Model) {
        <div>
            <h3>@p.Name</h3>
            @p.Description
            <h4>@p.Price.ToString("c")</h4>
        </div>
    }
    ```

    The view doesn’t know where the `Product` objects came from, how they were obtained, or whether or not they represent all of the products known to the application. Instead, the view deals only with how details of each `Product` is displayed using HTML elements

12. Notice that the `Product` class is not recognized. Add the following line to the `_ViewImports.cshtml` file.

    ```CSHTML
    @using MVCStore.Models
    ```

##  12. <a name='SettingtheDefaultRoute'></a>Setting the Default Route

10. Update the default route in the `Configure` method of the `Startup` class to match the code bellow

    ```C#
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute(
            name: "default",
            pattern: "{controller=Product}/{action=List}/{id?}");
    });
    ```
11. Run the application

##  13. <a name='Bibliography'></a>Bibliography
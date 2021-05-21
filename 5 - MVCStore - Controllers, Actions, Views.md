# Controllers, Actions and Views
<!-- vscode-markdown-toc -->
* 1. [Objectives](#Objectives)
* 2. [Bibliography](#Bibliography)
* 3. [Creating a Fake Repository](#CreatingaFakeRepository)
* 4. [Registering the Repository Service](#RegisteringtheRepositoryService)
* 5. [Adding a Controller](#AddingaController)
* 6. [Setting the Default Route](#SettingtheDefaultRoute)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

##  1. <a name='Objectives'></a>Objectives

##  2. <a name='Bibliography'></a>Bibliography

##  3. <a name='CreatingaFakeRepository'></a>Creating a Fake Repository

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

##  4. <a name='RegisteringtheRepositoryService'></a>Registering the Repository Service

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

##  5. <a name='AddingaController'></a>Adding a Controller

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

##  6. <a name='SettingtheDefaultRoute'></a>Setting the Default Route

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

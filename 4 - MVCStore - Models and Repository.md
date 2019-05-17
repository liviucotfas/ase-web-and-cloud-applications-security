# CRUD ASP.NET MVC Core Application

## Starting the Domain Model
1. Add the following Product class to the Models folder

    ```C#
    public class Product
	{
		public int ProductID { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public decimal Price { get; set; }
		public string Category { get; set; }
	}
    ```

## Creating a Repository

2. In the `Models` folder define the IProductRepository.cs interface as follows.

    ```C#
    public interface IProductRepository
	{
		IQueryable<Product> Products { get; }
	}
    ```
    > This interface uses IQueryable<T> to allow a caller to obtain a sequence of Product objects. The IQueryable<T> interface is derived from the more familiar IEnumerable<T> interface and represents a collection of objects that can be queried, such as those managed by a database.

    > A class that depends on the IProductRepository interface can obtain Product objects without needing to know the details of how they are stored or how the implementation class will deliver them.

    > **Understanding Ienumerable<T> and Iqueryable<T> Interfaces**
The IQueryable<T> interface is useful because it allows a collection of objects to be queried efficiently. Later, when we add support for retrieving a subset of Product objects from a database, using the IQueryable<T> interface allows us to ask the database for just the objects that we require using standard LINQ statements and without needing to know what database server stores the data or how it processes the query. Without the IQueryable<T> interface, we would have to retrieve all of the Product objects from the database and then discard the ones we don’t want, which becomes an expensive operation as the amount of data used by an application increases. It is for this reason that the IQueryable<T> interface is typically used instead of IEnumerable<T> in database repository interfaces and classes.
However, care must be taken with the IQueryable<T> interface because each time the collection of objects is enumerated, the query will be evaluated again, which means that a new query will be sent to the database. This can undermine the efficiency gains of using IQueryable<T>. In such situations, you can convert IQueryable<T> to a more predictable form using the ToList or ToArray extension method.

## Creating a Fake Repository

3. Create the fake repository called FakeProductRepository.cs to the `Models` folder.

     ```C#
    public class FakeProductRepository : IProductRepository
	{
		public IQueryable<Product> Products => new List<Product> {
            new Product { Name = "Football", Price = 25 },
            new Product { Name = "Surf board", Price = 179 },
            new Product { Name = "Running shoes", Price = 95 }
        }.AsQueryable<Product>();
	}
    ``` 

    > The FakeProductRepository class implements the IProductRepository interface by returning a fixed collection of Product objects as the value of the Products property. The AsQueryable method is used to convert the fixed collection of objects to an IQueryable<Product>, which is required to implement the IProductRepository interface and allows me to create a compatible fake repository without having to deal with real queries.    

## Registering the Repository Service

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

## Adding a Controller

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
    When MVC needs to create a new instance of the ProductController class to handle an HTTP request, it will inspect the constructor and see that it requires an object that implements the IProductRepository interface. To determine what implementation class should be used, MVC consults the configuration in the Startup class, which tells it that FakeRepository should be used and that a new instance should be created every time. MVC creates a new FakeRepository object and uses it to invoke the ProductController constructor in order to create the controller object that will process the HTTP request.

    This is known as **dependency injection**, and its approach allows the ProductController to access the application’s repository through the IProductRepository interface without having any need to know which implementation class has been configured. Later, we’ll replace the fake repository with the real one, and dependency injection means that the controller will continue to work without changes.

8. Add the following action to the `ProductController`

    ```C#
    public IActionResult List(){
        return View(repository.Products); 
    } 
    ```

9. Add a shared layout called `_Layout.cshtml` to the `Views/Shared` folder. It should be generated as follows.

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
    </body>
    </html>
    ```
10. We need to configure the application so that the _Layout.cshtml file is applied by default. This is done by adding an MVC View Start Page file called `_ViewStart.cshtml` to the Views folder. Make sure that the content of the `_ViewStart.cshtml` file is as follows.

    ```HTML
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

    The view doesn’t know where the Product objects came from, how they were obtained, or whether or not they represent all of the products known to the application. Instead, the view deals only with how details of each Product is displayed using HTML elements

## Setting the Default Route

10. Update the `Configure` method in the `Startup` class to match the code bellow

    ```C#
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
        loggerFactory.AddConsole();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // !!!! add these lines{ 
        app.UseStatusCodePages();
        app.UseStaticFiles();
        app.UseMvc(routes => {
            routes.MapRoute(
            name: "default",
            template: "{controller=Product}/{action=List}/{id?}");
        });
        // }!!!!
    }
    ```
11. Run the application
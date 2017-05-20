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

2. Added a new C# interface file called IProductRepository.cs to the Models folder and used it to define the following interface

    ```C#
    public interface IProductRepository
	{
		IEnumerable<Product> Products { get; }
	}
    ```

## Creating a Fake Repository

3. Create the fake repository called FakeProductRepository.cs to the Models folder

     ```C#
    public class FakeProductRepository : IProductRepository
	{
		public IEnumerable<Product> Products => new List<Product> {
			new Product { Name = "Football", Price = 25 },
			new Product { Name = "Surf board", Price = 179 },
			new Product { Name = "Running shoes", Price = 95 }
		};
	}
    ```     

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

5. Try to add a controler called `ProductController` to the Controllers folder

6. Add Scaffolding CLI tool to the project:
    
    ```
    <ItemGroup>
        <DotNetCliToolReference Include="Microsoft.VisualStudio.Web.CodeGeneration.Tools" Version="1.0.1" />
    </ItemGroup>
    ```

7. Add a controler called `ProductController` to the Controllers folder

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

    This is known as dependency injection, and its approach allows the ProductController to access the application’s repository through the IProductRepository interface without having any need to know which implementation class has been configured. Later, we’ll replace the fake repository with the real one, and dependency injection means that the controller will continue to work without changes.

8. Add the following action to the `ProductController`

    ```C#
    public ViewResult List()
	{
		return View(repository.Products);
	}
    ```

9. Add the corresponding view

    ```
    @model IEnumerable<MVCStore.Model.Product>

	@{
    ViewData["Title"] = "Product";
	}

	<h2>Product</h2>

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
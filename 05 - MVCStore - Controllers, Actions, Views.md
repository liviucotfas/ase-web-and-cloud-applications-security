# Controllers, Actions and Views
<!-- vscode-markdown-toc -->
* 1. [Objectives](#Objectives)
* 2. [Preparing the Controller](#PreparingtheController)
* 3. [Unit Testing the HomeController](#UnitTestingtheHomeController)
* 4. [Updating the View](#UpdatingtheView)
* 5. [Displaying Page Links](#DisplayingPageLinks)
* 6. [Bibliography](#Bibliography)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

##  1. <a name='Objectives'></a>Objectives
- unit testing the actions in the controllers;
- declaring tag helpers;
- adding a new route;

##  2. <a name='PreparingtheController'></a>Preparing the Controller

1. Modify the `HomeController` as highlighted below.

    ```C#
    public class HomeController : Controller
    {
        private IStoreRepository repository;
        public HomeController(IStoreRepository repo)
        {
            repository = repo;
        }
        public IActionResult Index()
        {
            return View(repository.Products);
        }
    }
    ```

    > When ASP.NET Core needs to create a new instance of the `HomeController` class to handle an HTTP request, it will inspect the constructor and see that it requires an object that implements the `IStoreRepository` interface. To determine what implementation class should be used, ASP.NET Core consults the configuration in the `Startup` class, which tells it that `EFStoreRepository` should be used and that a new instance should be created for every request. ASP.NET Core creates a new `EFStoreRepository` object and uses it to invoke the `HomeController` constructor to create the controller object that will process the HTTP request.
    > This is known as **dependency injection**, and its approach allows the `HomeController` object to access the application’s repository through the `IStoreRepository` interface without knowing which implementation class has been configured. I could reconfigure the service to use a different implementation class—one that doesn’t use Entity Framework Core, for example—and dependency injection means that the controller will continue to work without changes.

##  3. <a name='UnitTestingtheHomeController'></a>Unit Testing the HomeController

2. We can unit test that the controller is accessing the repository correctly by creating a mock repository, injecting it into the constructor of the `HomeController` class, and then calling the `Index` method to get the response that contains the list of products. We then compare the `Product` objects we get to what we would expect from the test data in the mock implementation.

    ```C#
    public class HomeControllerTests
    {
        [Fact]
        public void Can_Use_Repository()
        {
            // Arrange
            Mock<IStoreRepository> mock = new Mock<IStoreRepository>();
            mock.Setup(m => m.Products).Returns(
                (new Product[] {
                    new Product {ProductID = 1, Name = "P1"},
                    new Product {ProductID = 2, Name = "P2"}
                    }).AsQueryable<Product>()
                 );
            HomeController controller = new HomeController(mock.Object);
            
            // Act
            IEnumerable<Product>? result = (controller.Index() as ViewResult)?.ViewData.Model as IEnumerable<Product>;
            
            // Assert
            Product[] prodArray = result?.ToArray() ?? Array.Empty<Product>();
            Assert.True(prodArray.Length == 2);
            Assert.Equal("P1", prodArray[0].Name);
            Assert.Equal("P2", prodArray[1].Name);
        }
    }
    ```

##  4. <a name='UpdatingtheView'></a>Updating the View
3. The `Index `action method passes the collection of `Product` objects from the repository to the `View` method, which means these objects will be the view model that Razor uses when it generates HTML content from the view. Make the changes to the view shown below to generate content using the `Product` view model objects.

    ```CSHTML
    @model IQueryable<Product>

    @foreach (var p in Model ?? Enumerable.Empty<Product>()) {
        <div>
            <h3>@p.Name</h3>
            @p.Description
            <h4>@p.Price.ToString("c")</h4>
        </div>
    }
    ```

    >Note: You might need to add the following line to the `_ViewImports.cshtml` file.

    ```CSHTML
    @using MVCStore.Models
    ```

    >The `@model` expression at the top of the file specifies that the view expects to receive a sequence of `Product` objects from the action method as its model data. We use an `@foreach` expression to work through the sequence and generate a simple set of HTML elements for each `Product` object that is received.
    The view doesn’t know where the `Product` objects came from, how they were obtained, or whether they represent all the products known to the application. Instead, the view deals only with how details of each `Product` are displayed using HTML elements.

4. Add support for pagination so that the view displays a smaller number of products on a page, and the user can move from page to page to view the overall catalog. Update the `Index` method on the `HomeController` as follows.

    ```C#

    public int PageSize = 2;
    public IActionResult Index(int productPage = 1)
    {
        var products = repository.Products
            .OrderBy(p => p.ProductID)
            .Skip((productPage - 1) * PageSize)
            .Take(PageSize);

        return View(products);
    }
    ```
    > The `PageSize` field specifies the number of products per page. We have added an optional parameter to the `Index` method, which means that if the method is called without a parameter, the call is treated as though we had supplied the value specified in the parameter definition, with the effect that the action method displays the first page of products when it is invoked without an argument. Within the body of the action method, we get the `Product` objects, order them by the primary key, skip over the products that occur before the start of the current page, and take the number of products specified by the `PageSize` field.

5. Unit test the pagination feature in the `HomeControllerTests` class.

    ```C#
    [Fact]
    public void Can_Paginate()
    {
        // Arrange
        Mock<IStoreRepository> mock = new Mock<IStoreRepository>();

        mock.Setup(m => m.Products).Returns((new Product[] {
            new Product {ProductID = 1, Name = "P1"},
            new Product {ProductID = 2, Name = "P2"},
            new Product {ProductID = 3, Name = "P3"},
            new Product {ProductID = 4, Name = "P4"},
            new Product {ProductID = 5, Name = "P5"}
            }).AsQueryable<Product>());

        HomeController controller = new HomeController(mock.Object);
        controller.PageSize = 3;

        // Act
        IEnumerable<Product> result = (controller.Index(2) as ViewResult)?.ViewData.Model as IEnumerable<Product> ?? Enumerable.Empty<Product>();

        // Assert
        Product[] prodArray = result.ToArray();
        Assert.True(prodArray.Length == 2);
        Assert.Equal("P4", prodArray[0].Name);
        Assert.Equal("P5", prodArray[1].Name);
    }
    ```
##  5. <a name='DisplayingPageLinks'></a>Displaying Page Links

To support the tag helper that will display the page numbers, we need to pass information to the view about the number of pages available, the current page, and the total number of products in the repository. The easiest way to do this is to create a view model class, which is used specifically to pass data between a controller and a view.

6. Create a `ViewModels` folder in your project. Add a class called `PagingInfoViewModel`.

    ```C#
    public class PagingInfoViewModel
    {
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages
        {
            get 
            { 
                return (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage);
            }
        }
    }
    ```
7. Let's net create the tag helper that will display the page numbers. Create a folder named `Infrastructure` and add to it a class called `PageLinkTagHelper`

    ```C#
    [HtmlTargetElement("div", Attributes = "page-model")]
    public class PageLinkTagHelper : TagHelper
    {
        private IUrlHelperFactory urlHelperFactory;
        public PageLinkTagHelper(IUrlHelperFactory helperFactory)
        {
            urlHelperFactory = helperFactory;
        }
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext? ViewContext { get; set; }
        public PagingInfoViewModel? PageModel { get; set; }
        public string? PageAction { get; set; }
        public override void Process(TagHelperContext context,
        TagHelperOutput output)
        {
            if (ViewContext != null && PageModel != null)
            {
                IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(ViewContext);
                TagBuilder result = new TagBuilder("div");
                for (int i = 1; i <= PageModel.TotalPages; i++)
                {
                    TagBuilder tag = new TagBuilder("a");
                    tag.Attributes["href"] = urlHelper.Action(PageAction,
                    new { productPage = i });

                    tag.InnerHtml.Append(i.ToString());
                    result.InnerHtml.AppendHtml(tag);
                }
                output.Content.AppendHtml(result.InnerHtml);
            }
        }
    }
    ```
    
    >This tag helper populates a `div` element with a elements that correspond to pages of products. Tag helpers are one of the most useful ways that you can introduce C# logic into your views. The code for a tag helper can look tortured because C# and HTML don’t mix easily. But using tag helpers is preferable to including blocks of C# code in a view because a tag helper can be easily unit tested.

8. Register the new tag helper in the `_ViewImports.cshtml` file.

    ```CSHTML
    @addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
    // !!!! new/updated code {
    @addTagHelper *, MVCStore
    //}
    @using MVCStore.Models
    // !!!! new/updated code {
    @using MVCStore.ViewModels
    //}
    ```

9. Add a new class in the `MVCStore.Tests` project for unit testing the `PageLinkTagHelper` tag helper class.

    ```C#
    public class PageLinkTagHelperTests
    {
        [Fact]
        public void Can_Generate_Page_Links()
        {
            // Arrange
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.SetupSequence(x => x.Action(It.IsAny<UrlActionContext>()))
            .Returns("Test/Page1")
            .Returns("Test/Page2")
            .Returns("Test/Page3");

            var urlHelperFactory = new Mock<IUrlHelperFactory>();
            urlHelperFactory.Setup(f =>
            f.GetUrlHelper(It.IsAny<ActionContext>()))
            .Returns(urlHelper.Object);
            PageLinkTagHelper helper =
            new PageLinkTagHelper(urlHelperFactory.Object)
            {
                PageModel = new PagingInfo
                {
                    CurrentPage = 2,
                    TotalItems = 28,
                    ItemsPerPage = 10
                },
                PageAction = "Test"
            };
            TagHelperContext ctx = new TagHelperContext(
            new TagHelperAttributeList(),
            new Dictionary<object, object>(), "");
            var content = new Mock<TagHelperContent>();
            TagHelperOutput output = new TagHelperOutput("div",
            new TagHelperAttributeList(),
            (cache, encoder) => Task.FromResult(content.Object));
            // Act
            helper.Process(ctx, output);
            // Assert
            Assert.Equal(@"<a href=""Test/Page1"">1</a>"
            + @"<a href=""Test/Page2"">2</a>"
            + @"<a href=""Test/Page3"">3</a>",
            output.Content.GetContent());
        }
    }
    ```

    > The complexity in this test is in creating the objects that are required to create and use a tag helper. Tag helpers use `IUrlHelperFactory` objects to generate URLs that target different parts of the application, and I have used Moq to create an implementation of this interface and the related `IUrlHelper` interface that provides test data.

10. We need  to provide an instance of the PagingInfo view model class to the view. Add a new class called `ProductsListViewModel` to the `ViewModels` folder.

    ```C#
    public class ProductsListViewModel
    {
        public IEnumerable<Product> Products { get; set; } = Enumerable.Empty<Product>();
        public PagingInfoViewModel PagingInfo { get; set; } = new();
    }
    ```

11. Update the `Index` action in the `HomeController` class to use the `ProductsListViewModel` class in order to provide the view with details of the products to display on the page and with details of the pagination, as shown below.

    ```C#
    public ViewResult Index(int productPage = 1)
    {
        var viewModel = new ProductsListViewModel
        {
            Products = repository.Products
                .OrderBy(p => p.ProductID)
                .Skip((productPage - 1) * PageSize)
                .Take(PageSize),
            PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItems = repository.Products.Count()
            }
        };

        return View(viewModel);
    }
    ```

    >These changes pass a `ProductsListViewModel` object as the model data to the view.

12. Modify the earlier unit tests to reflect the new result from the Index action method.
    ```c#
    [Fact]
    public void Can_Use_Repository()
    {
        // Arrange
        Mock<IStoreRepository> mock = new Mock<IStoreRepository>();
        mock.Setup(m => m.Products).Returns(
            (new Product[] {
                new Product {ProductID = 1, Name = "P1"},
                new Product {ProductID = 2, Name = "P2"}
                }).AsQueryable<Product>()
                );
        HomeController controller = new HomeController(mock.Object);

        // Act
        // !!!! new/updated code {
        ProductsListViewModel result = controller.Index()?.ViewData.Model as ProductsListViewModel ?? new();
        // }

        // Assert
        // !!!! new/updated code {
        Product[] prodArray = result.Products.ToArray();
        // }
        Assert.True(prodArray.Length == 2);
        Assert.Equal("P1", prodArray[0].Name);
        Assert.Equal("P2", prodArray[1].Name);
    }
    ```

    ```C#
    [Fact]
    public void Can_Paginate()
    {
        // Arrange
        Mock<IStoreRepository> mock = new Mock<IStoreRepository>();

        mock.Setup(m => m.Products).Returns((new Product[] {
            new Product {ProductID = 1, Name = "P1"},
            new Product {ProductID = 2, Name = "P2"},
            new Product {ProductID = 3, Name = "P3"},
            new Product {ProductID = 4, Name = "P4"},
            new Product {ProductID = 5, Name = "P5"}
            }).AsQueryable<Product>());

        HomeController controller = new HomeController(mock.Object);
        controller.PageSize = 3;

        // Act
        // !!!! new/updated code {
        ProductsListViewModel result =  controller.Index(2)?.ViewData.Model as ProductsListViewModel ?? new();
        // }

        // Assert
        // !!!! new/updated code {
        Product[] prodArray = result.Products.ToArray();
        // }
        Assert.True(prodArray.Length == 2);
        Assert.Equal("P4", prodArray[0].Name);
        Assert.Equal("P5", prodArray[1].Name);
    }
    ```

13. Add a unit test in order to check if controller sends the correct pagination data to the view in the class `HomeControllerTests`.

    ```C#
    [Fact]
    public void Can_Send_Pagination_View_Model()
    {
        // Arrange
        Mock<IStoreRepository> mock = new Mock<IStoreRepository>();
        mock.Setup(m => m.Products).Returns((new Product[] {
            new Product {ProductID = 1, Name = "P1"},
            new Product {ProductID = 2, Name = "P2"},
            new Product {ProductID = 3, Name = "P3"},
            new Product {ProductID = 4, Name = "P4"},
            new Product {ProductID = 5, Name = "P5"}
            }).AsQueryable<Product>());
        // Arrange
        HomeController controller =
        new HomeController(mock.Object) { PageSize = 3 };
        // Act
        ProductsListViewModel result = controller.Index(2)?.ViewData.Model as ProductsListViewModel ?? new();
        // Assert
        PagingInfo pageInfo = result.PagingInfo;
        Assert.Equal(2, pageInfo.CurrentPage);
        Assert.Equal(3, pageInfo.ItemsPerPage);
        Assert.Equal(5, pageInfo.TotalItems);
        Assert.Equal(2, pageInfo.TotalPages);
    }
    ```
14. Update the `Index.cshtml` corresponding to the `HomeController` to use the new viewmodel type.

    ```CSHTML
    @model ProductsListViewModel
    @foreach (var p in Model?.Products ?? Enumerable.Empty<Product>())
    {
        <div>
            <h3>@p.Name</h3>
            @p.Description
            <h4>@p.Price.ToString("c")</h4>
        </div>
    }
    ```
    >We have changed the `@model` directive to tell Razor that we are now working with a different data type.

15. Dispay the links towards the pages in the `Index.cshtml` corresponding to the `HomeController`.

    ```CSHTML
    @model ProductsListViewModel

    @foreach (var p in Model.Products)
    {
        <div>
            <h3>@p.Name</h3>
            @p.Description
            <h4>@p.Price.ToString("c")</h4>
        </div>
    }

    <div page-model="@Model?.PagingInfo" page-action="Index"></div>
    ```

    > When Razor finds the `page-model` attribute on the `div` element, it asks the `PageLinkTagHelper` class to transform the element.

16. Let's improve the urls using the ASP.NET Core routing feature. Modify the `Main` method of the `Program` class as follows.

    ```C#
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllersWithViews();
        builder.Services.AddDbContext<ApplicationDbContext>(opts => {
            opts.UseSqlServer(
            builder.Configuration["ConnectionStrings:DefaultConnection"]);
        });

        builder.Services.AddScoped<IStoreRepository, EFStoreRepository>();

        var app = builder.Build();
        app.UseStaticFiles();
        // !!!! new/updated code {
        app.MapControllerRoute("pagination",
            "Products/Page{productPage}",
            new { Controller = "Home", action = "Index" });
        //}
        app.MapDefaultControllerRoute();

        SeedData.EnsurePopulated(app);

        app.Run();
    }
    ```
    > It is important that you add the new route before the call to the `MapDefaultControllerRoute` method. The routing system processes routes in the order they are listed, and we need the new route to take precedence over the existing one.

    > This is the only alteration required to change the URL scheme for product pagination. ASP.NET Core and the routing function are tightly integrated, so the application automatically reflects a change like this in the URLs used by the application, including those generated by tag helpers.


##  6. <a name='Bibliography'></a>Bibliography
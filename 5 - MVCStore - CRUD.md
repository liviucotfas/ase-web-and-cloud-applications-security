# CRUD ASP.NET MVC Core Application

## Creating a CRUD Controller

1. Add a new layout file called "_AdminLayout"

    ```
    <!DOCTYPE html>
    <html>
    <head>
        <meta name="viewport" content="width=device-width" />
        <link rel="stylesheet" asp-href-include="lib/bootstrap/dist/css/*.min.css" />
        <title>@ViewBag.Title</title>
    </head>
    <body class="panel panel-default">
        <div class="panel-heading"><h4>@ViewBag.Title</h4></div>
        <div class="panel-body">
            @RenderBody()
        </div>
    </body>
    </html>
    ```

1. Add a new controller to the Controllers folder called AdminController

    ```C#
    public class AdminController : Controller
	{
		private IProductRepository repository;
		public AdminController(IProductRepository repo)
		{
			repository = repo;
		}
		public ViewResult Index()
		{
			return View(repository.Products);
		}
	}
    ```

2. Add an UnitTest project
3. Remove the Microsoft.VisualStudio.Web.CodeGeneration.Design NuGet package
4. Add the Moq NuGet package    
5. Add the following unit test
    ```C#
    public class AdminControllerTests
    {
		[Fact]
		public void Index_Contains_All_Products()
		{
			// Arrange - create the mock repository
			Mock<IProductRepository> mock = new Mock<IProductRepository>();
			mock.Setup(m => m.Products).Returns(new Product[] {
new Product {ProductID = 1, Name = "P1"},
new Product {ProductID = 2, Name = "P2"},
new Product {ProductID = 3, Name = "P3"},
});
			// Arrange - create a controller
			AdminController target = new AdminController(mock.Object);
			// Action
			Product[] result
			= GetViewModel<IEnumerable<Product>>(target.Index())?.ToArray();
			// Assert
			Assert.Equal(3, result.Length);
			Assert.Equal("P1", result[0].Name);
			Assert.Equal("P2", result[1].Name);
			Assert.Equal("P3", result[2].Name);
		}
		private T GetViewModel<T>(IActionResult result) where T : class
		{
			return (result as ViewResult)?.ViewData.Model as T;
		}
	}
    ```
6. Run the unit test

## Implementing the List View

7. In the Views/Admin folder add a Razor file called Index.cshtml
    ```C#
    @model IEnumerable<Product>
    @{
        ViewBag.Title = "All Products";
        Layout = "_AdminLayout";
    }
    <table class="table table-striped table-bordered table-condensed">
        <tr>
            <th class="text-right">ID</th>
            <th>Name</th>
            <th class="text-right">Price</th>
            <th class="text-center">Actions</th>
        </tr>
        @foreach (var item in Model)
        {
            <tr>
                <td class="text-right">@item.ProductID</td>
                <td>@item.Name</td>
                <td class="text-right">@item.Price.ToString("c")</td>
                <td class="text-center">
                    <form asp-action="Delete" method="post">
                        <a asp-action="Edit" class="btn btn-sm btn-warning"
                        asp-route-productId="@item.ProductID">
                            Edit
                        </a>
                        <input type="hidden" name="ProductID" value="@item.ProductID" />
                        <button type="submit" class="btn btn-danger btn-sm">
                            Delete
                        </button>
                    </form>
                </td>
            </tr>
        }
    </table>
    <div class="text-center">
        <a asp-action="Create" class="btn btn-primary">Add Product</a>
    </div>
    ```

## Editing Products

    ```
    public ViewResult Edit(int productId)
    {
        return View(repository.Products.FirstOrDefault(p => p.ProductID == productId));
    }
    ```

    ```C#
    @model Product
    @{
        ViewBag.Title = "Edit Product";
        Layout = "_AdminLayout";
    }
    <form asp-action="Edit" method="post">
        <input type="hidden" asp-for="ProductID" />
        <div class="form-group">
            <label asp-for="Name"></label>
            <input asp-for="Name" class="form-control" />
        </div>
        <div class="form-group">
            <label asp-for="Description"></label>
    <textarea asp-for="Description" class="form-control"></textarea>
        </div>
        <div class="form-group">
            <label asp-for="Category"></label>
            <input asp-for="Category" class="form-control" />
        </div>
        <div class="form-group">
            <label asp-for="Price"></label>
            <input asp-for="Price" class="form-control" />
        </div>
        <div class="text-center">
            <button class="btn btn-primary" type="submit">Save</button>
            <a asp-action="Index" class="btn btn-default">Cancel</a>
        </div>
    </form>
    ```

    ```
    public interface IProductRepository
	{
		IEnumerable<Product> Products { get; }

		void SaveProduct(Product product);
	}
    ```

    ```
    public void SaveProduct(Product product)
    {
        if (product.ProductID == 0)
        {
            context.Products.Add(product);
        }
        else
        {
            Product dbEntry = context.Products
            .FirstOrDefault(p => p.ProductID == product.ProductID);
            if (dbEntry != null)
            {
                dbEntry.Name = product.Name;
                dbEntry.Description = product.Description;
                dbEntry.Price = product.Price;
                dbEntry.Category = product.Category;
            }
        }
        context.SaveChanges();
    }
    ```

    ```
    [HttpPost]
    public IActionResult Edit(Product product)
    {
        if (ModelState.IsValid)
        {
            repository.SaveProduct(product);
            TempData["message"] = $"{product.Name} has been saved";
            return RedirectToAction("Index");
        }
        else
        {
            // there is something wrong with the data values
            return View(product);
        }
    }
    ```

    ``` Add in panel-body
    @if (TempData["message"] != null) {
    <div class="alert alert-success">@TempData["message"]</div>
    }
    ```

## Adding model validation

    ```
    public class Product
	{
		public int ProductID { get; set; }
		[Required(ErrorMessage = "Please enter a product name")]
		public string Name { get; set; }
		[Required(ErrorMessage = "Please enter a description")]
		public string Description { get; set; }
		[Required]
		[Range(0.01, double.MaxValue,
		ErrorMessage = "Please enter a positive price")]
		public decimal Price { get; set; }
		[Required(ErrorMessage = "Please specify a category")]
		public string Category { get; set; }
	}
    ```

    Follow the steps in document "11 - SportsStore - Administration" starting from page 23
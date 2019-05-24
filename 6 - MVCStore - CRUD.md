# MVCStore - CRUD

## Creating a CRUD Controller

1. Add a new layout file called "_AdminLayout"

    ```HTML
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        <title>@ViewData["Title"] - IronBankWeb</title>

        <environment include="Development">
            <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.css" />
            <link rel="stylesheet" href="~/css/site.css" />
        </environment>
        <environment exclude="Development">
            <link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/3.4.1/css/bootstrap.min.css"
                asp-fallback-href="~/lib/bootstrap/dist/css/bootstrap.min.css"
                asp-fallback-test-class="sr-only" asp-fallback-test-property="position" asp-fallback-test-value="absolute" />
            <link rel="stylesheet" href="~/css/site.min.css" asp-append-version="true" />
        </environment>
    </head>
    <body>
        <nav class="navbar navbar-inverse navbar-fixed-top">
            <div class="container">
                <div class="navbar-header">
                    <button type="button" class="navbar-toggle" data-toggle="collapse" data-target=".navbar-collapse">
                        <span class="sr-only">Toggle navigation</span>
                        <span class="icon-bar"></span>
                        <span class="icon-bar"></span>
                        <span class="icon-bar"></span>
                    </button>
                    <a asp-area="" asp-controller="Home" asp-action="Index" class="navbar-brand">MVC Store</a>
                </div>
                <div class="navbar-collapse collapse">
                    <ul class="nav navbar-nav">
                        <li><a asp-area="" asp-controller="Home" asp-action="Index">Home</a></li>
                    </ul>
                     @*<partial name="_LoginPartial" />*@
                </div>
            </div>
        </nav>


        <div class="container body-content">
            @RenderBody()
            <hr />
            <footer>
                <p>&copy; 2019 - Web &amp; Cloud Security </p>
            </footer>
        </div>

        <environment include="Development">
            <script src="~/lib/jquery/dist/jquery.js"></script>
            <script src="~/lib/bootstrap/dist/js/bootstrap.js"></script>
            <script src="~/js/site.js" asp-append-version="true"></script>
        </environment>
        <environment exclude="Development">
            <script src="https://ajax.aspnetcdn.com/ajax/jquery/jquery-3.3.1.min.js"
                    asp-fallback-src="~/lib/jquery/dist/jquery.min.js"
                    asp-fallback-test="window.jQuery"
                    crossorigin="anonymous"
                    integrity="sha384-tsQFqpEReu7ZLhBV2VZlAu7zcOV+rXbYlF2cqB8txI/8aZajjp4Bqd+V6D5IgvKT">
            </script>
            <script src="https://stackpath.bootstrapcdn.com/bootstrap/3.4.1/js/bootstrap.min.js"
                    asp-fallback-src="~/lib/bootstrap/dist/js/bootstrap.min.js"
                    asp-fallback-test="window.jQuery && window.jQuery.fn && window.jQuery.fn.modal"
                    crossorigin="anonymous"
                    integrity="sha384-aJ21OjlMXNL5UyIl/XNwTMqvzeRMZH2w8c5cRVpzpU8Y5bApTppSuUkhZXN0VxHd">
            </script>
            <script src="~/js/site.min.js" asp-append-version="true"></script>
        </environment>

        @RenderSection("Scripts", required: false)
    </body>
    </html>

    ```

2. Add a new controller to the `Controllers` folder called `AdminController`

    ```C#
    public class AdminController : Controller
	{
		private IProductRepository repository;
		public AdminController(IProductRepository repo)
		{
			repository = repo;
		}
		public IActionResult Index()
		{
			return View(repository.Products);
		}
	}
    ```

5. Add the following unit test to the `MVCStore.Tests` project
    ```C#
    public class AdminControllerTests {
        [Fact]
        public void Index_Contains_All_Products() {
            // Arrange - create the mock repository
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] {
                new Product {ProductID = 1, Name = "P1"},
                new Product {ProductID = 2, Name = "P2"},
                new Product {ProductID = 3, Name = "P3"},
            }.AsQueryable<Product>());
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
        private T GetViewModel<T>(IActionResult result) where T : class {
            return (result as ViewResult)?.ViewData.Model as T;
        }
    }
    ```
6. Run the unit test

## Implementing the List View

7. In the Views/Admin folder add a Razor file called Index.cshtml
    ```CSHTML
    @model IEnumerable<Product>
    @{
        ViewBag.Title = "All Products";
        Layout = "_AdminLayout";
    }

    <h1>Products</h1>

    <table class="table table-striped table-bordered table-sm">
        <tr>
            <th class="text-right">ID</th>
            <th>Name</th>
            <th class="text-right">Price</th>
            <th class="text-center">Actions</th>
        </tr>
        @foreach (var item in Model) {
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

1. Add an `Edit` action on the `AdminController` 

    ```C#
    public IActionResult Edit(int productId)
    {
        return View(repository.Products.FirstOrDefault(p => p.ProductID == productId));
    }
    ```

2. Add the corresponding view.

    ```HTML
    @model Product
    @{
        ViewBag.Title = "Edit Product";
        Layout = "_AdminLayout";
    }

    <h1>Edit product</h1>

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

3. Add a `SaveProduct` method in the `IProductRepository` interface.
    ```C#
    public interface IProductRepository
	{
		IEnumerable<Product> Products { get; }

		void SaveProduct(Product product);
	}
    ```

4. Implement the `SaveProduct` method as follows.
    ```C#
    public void SaveProduct(Product product) { 
        if (product.ProductID == 0) { 
            context.Products.Add(product); 
        } else { 
            Product dbEntry = context.Products 
                .FirstOrDefault(p => p.ProductID == product.ProductID); 
            if (dbEntry != null) { 
                dbEntry.Name = product.Name; 
                dbEntry.Description = product.Description; 
                dbEntry.Price = product.Price; 
                dbEntry.Category = product.Category; 
            } 
        } 
        context.SaveChanges(); 
    } 
    ```

## Handling Edit POST Requests

1. Add the `Edit` action that will handle POST requests on the `AdminController`

    ```C#
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

    > Notice the `TempData` object

2. Update the `_AdminLayout.cshtml` layout file in order to display the confirmation message.

    ```CSHTML
     @if (TempData["message"] != null)
    {
        <div class="alert alert-success">@TempData["message"]</div>
    }
    ```

## Adding model validation

1. Update the `Product` class as follows.

    ```C#
    public class Product {
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

2. Update the `Edit` view as follows

    ```HTML
    @model Product
    @{
        ViewBag.Title = "Edit Product";
        Layout = "_AdminLayout";
    }
    <form asp-action="Edit" method="post">
        <input type="hidden" asp-for="ProductID" />
        <div class="form-group">
            <label asp-for="Name"></label>
            <div><span asp-validation-for="Name" class="text-danger"></span></div> 
            <input asp-for="Name" class="form-control" />
        </div>
        <div class="form-group">
            <label asp-for="Description"></label>
            <div><span asp-validation-for="Description" class="text-danger"></span></div> 
            <textarea asp-for="Description" class="form-control"></textarea>
        </div>
        <div class="form-group">
            <label asp-for="Category"></label>
            <div><span asp-validation-for="Category" class="text-danger"></span></div> 
            <input asp-for="Category" class="form-control" />
        </div>
        <div class="form-group">
            <label asp-for="Price"></label>
            <div><span asp-validation-for="Price" class="text-danger"></span></div> 
            <input asp-for="Price" class="form-control" />
        </div>
        <div class="text-center">
            <button class="btn btn-primary" type="submit">Save</button>
            <a asp-action="Index" class="btn btn-secondary">Cancel</a>
        </div>
    </form>
    ```

## Creating New Products

1. Add a `Create` action to the `AdminController` class.

    ```C#
    public IActionResult Create(){
        return View("Edit", new Product());
    }  
    ```

## Deleting Products

1. Add a `DeleteProduct` method to the `IProductRepository` interface.

    ```C#
    Product DeleteProduct(int productID);
    ```

2. Implement the method in the `EFProductRepository` class.

    ```C#
    public Product DeleteProduct(int productID) { 
        Product dbEntry = context.Products 
                .FirstOrDefault(p => p.ProductID == productID); 
    
        if (dbEntry != null) { 
            context.Products.Remove(dbEntry); 
            context.SaveChanges(); 
        } 
    
        return dbEntry; 
    } 
    ```
3. Add the corresponding action to the `AdminController`

    ```C#
    [HttpPost] 
    public IActionResult Delete(int productId) { 
        Product deletedProduct = repository.DeleteProduct(productId); 
        if (deletedProduct != null) { 
            TempData["message"] = $"{deletedProduct.Name} was deleted"; 
        } 
        return RedirectToAction("Index"); 
    } 
    ```

4. Add a test method

    ```C#
    [Fact]
    public void Can_Delete_Valid_Products() {
        // Arrange - create a Product
        Product prod = new Product { ProductID = 2, Name = "Test" };
        // Arrange - create the mock repository
        Mock<IProductRepository> mock = new Mock<IProductRepository>();
        mock.Setup(m => m.Products).Returns(new Product[] {
            new Product {ProductID = 1, Name = "P1"},
            prod,
            new Product {ProductID = 3, Name = "P3"},
        }.AsQueryable<Product>());
        // Arrange - create the controller
        AdminController target = new AdminController(mock.Object);
        // Act - delete the product
        target.Delete(prod.ProductID);
        // Assert - ensure that the repository delete method was
        // called with the correct Product
        mock.Verify(m => m.DeleteProduct(prod.ProductID));
    }
    ```
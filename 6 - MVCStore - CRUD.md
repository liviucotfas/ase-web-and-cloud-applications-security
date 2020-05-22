# MVCStore - CRUD

## Stryling the Content
1. Add a new folder called 'wwwroot` to the project that will host the static files.

2. Add the "Bootstrap" framework to your project by right clicking on the project and chosing "Add" > "Client Side Library". Use "unpkg" as a provider and search for "bootstrap". Choose only the following files:
- "dist/css/bootstrap.css",
- "dist/css/bootstrap.css.map",
- "dist/css/bootstrap.min.css",
- "dist/css/bootstrap.min.css.map",
- "dist/js/bootstrap.js",
- "dist/js/bootstrap.js.map",
- "dist/js/bootstrap.min.js",
- "dist/js/bootstrap.min.js.map"

3. Add a new folder called "css" to the "wwwroot" folder. Add a new CSS file to this folder called "site.css" 

4. Add a new folder called "js" to the "wwwroot" folder. Add a new JavaScript file to this folder called "site.js".

5. Add the "jQuery" library using the "cdnjs" provider.

6. Add a new folder called "Shared" to the "Views" folder. Add a new file of the type "Razor Layout" called "_Layout" to this folder.

    ```HTML
    <!doctype html>
    <html lang="en">
    <head>
        <meta charset="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">
        <title>@ViewData["Title"] - MVCStore</title>

        <environment include="Development">
            <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.css" />
            <link rel="stylesheet" href="~/css/site.css" />
        </environment>
        <environment exclude="Development">
            <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/4.5.0/css/bootstrap.min.css"
                asp-fallback-href="~/lib/bootstrap/dist/css/bootstrap.min.css"
                asp-fallback-test-class="sr-only" asp-fallback-test-property="position" asp-fallback-test-value="absolute" />
            <link rel="stylesheet" href="~/css/site.min.css" asp-append-version="true" />
        </environment>
    </head>
    <body>
        <header>
            <nav class="navbar navbar-expand-sm navbar-toggleable-sm navbar-light bg-white border-bottom box-shadow mb-3">
                <div class="container">
                    <a class="navbar-brand" asp-area="" asp-controller="Home" asp-action="Index">MVCStore</a>
                    <button class="navbar-toggler" type="button" data-toggle="collapse" data-target=".navbar-collapse" aria-controls="navbarSupportedContent"
                            aria-expanded="false" aria-label="Toggle navigation">
                        <span class="navbar-toggler-icon"></span>
                    </button>
                    <div class="navbar-collapse collapse d-sm-inline-flex flex-sm-row-reverse">
                        <ul class="navbar-nav flex-grow-1">
                            <li class="nav-item">
                                <a class="nav-link text-dark" asp-area="" asp-controller="Home" asp-action="Index">Home</a>
                            </li>
                        </ul>
                    </div>
                </div>
            </nav>
        </header>

        <div class="container">
            <main role="main" class="pb-3">
                @RenderBody()
            </main>
        </div>

        <footer class="border-top footer text-muted">
            <div class="container">
                <p>&copy; 2020 - Web &amp; Cloud Security </p>
            </div>
        </footer>

        <environment include="Development">
            <script src="~/lib/jquery/jquery.slim.js"></script>
            <script src="~/lib/bootstrap/dist/js/bootstrap.js"></script>
            <script src="~/js/site.js" asp-append-version="true"></script>
        </environment>
        <environment exclude="Development">
            <script src="https://ajax.aspnetcdn.com/ajax/jQuery/jquery-3.5.0.slim.min.js"
                    asp-fallback-src="~/lib/jquery/jquery.slim.min.js"
                    asp-fallback-test="window.jQuery">
            </script>
            <script src="https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/4.5.0/js/bootstrap.min.js"
                    asp-fallback-src="~/lib/bootstrap/dist/js/bootstrap.min.js"
                    asp-fallback-test="window.jQuery && window.jQuery.fn && window.jQuery.fn.modal">
            </script>
            <script src="~/js/site.min.js" asp-append-version="true"></script>
        </environment>

        @RenderSection("Scripts", required: false)
    </body>
    </html>
    ```

7. Add to the "Views" folder a new file of the type "Razor View Start", called "_ViewStart.cshtml".

8. Also update the `Index.cshtml` to match the following code:

    ```HTML
    @model IEnumerable<Product>
    @{
        ViewData["Title"] = "Index";
    }

    <h1>Products</h1>

    <div class="row">
        @foreach (var product in Model)
        {
            <div class="col-sm-4">
                <div class="card mx-2 mb-3">
                    <div class="card-body">
                        <h5 class="card-title">
                            @product.Name
                        </h5>
                        <p class="card-text">
                            @product.Description
                        </p>
                        <p class="card-text">
                            @(((decimal)product.Price).ToString("c"))
                        </p>
                    </div>
                </div>
            </div>
        }
    </div>
    ```

## Creating a CRUD Controller

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
    }

    <h1>Products</h1>

    <a asp-action="Create" class="btn btn-primary mb-3">Add Product</a>

    <table class="table table-striped table-bordered table-sm">
        <tr>
            <th class="text-right">ID</th>
            <th>Name</th>
            <th class="text-right">Price</th>
            <th class="text-center">Actions</th>
        </tr>
        @foreach (var item in Model)
        {
            <tr>
                <td class="text-right">@item.ProductId</td>
                <td>@item.Name</td>
                <td class="text-right">@item.Price</td>
                <td class="text-center">
                    <a asp-action="Edit" class="btn btn-sm btn-warning"
                        asp-route-productId="@item.ProductId">
                        Edit
                    </a>
                    <form asp-action="Delete" method="post">
                        <input type="hidden" name="ProductId" value="@item.ProductId" />
                        <button type="submit" class="btn btn-danger btn-sm">
                            Delete
                        </button>
                    </form>
                </td>
            </tr>
        }
    </table>
    ```

## Editing Products

1. Add an `Edit` action on the `AdminController` 

    ```C#
    public IActionResult Edit(int productId)
    {
        var product = repository.Products.FirstOrDefault(p => p.ProductId == productId);
        return View(product);
    }
    ```

2. Scaffold the corresponding view. Update the content of the view as follows:

    ```HTML
    @model MVCStore.Models.Product

    @{
        ViewData["Title"] = "Edit";
    }

    <h1>Edit product</h1>
    <hr />

    <form asp-action="Edit">
        <div asp-validation-summary="ModelOnly" class="text-danger"></div>
        <input type="hidden" asp-for="ProductId" />
        <div class="form-group">
            <label asp-for="Name" class="control-label"></label>
            <input asp-for="Name" class="form-control" />
            <span asp-validation-for="Name" class="text-danger"></span>
        </div>
        <div class="form-group">
            <label asp-for="Description" class="control-label"></label>
            <input asp-for="Description" class="form-control" />
            <span asp-validation-for="Description" class="text-danger"></span>
        </div>
        <div class="form-group">
            <label asp-for="Price" class="control-label"></label>
            <input asp-for="Price" class="form-control" />
            <span asp-validation-for="Price" class="text-danger"></span>
        </div>
        <div class="form-group text-right">
            <input type="submit" value="Save" class="btn btn-primary" />
            <a asp-action="Index" class="btn btn-secondary">Back to List</a>
        </div>
    </form>
    ```

3. Add a `SaveProduct` method in the `IProductRepository` interface.
    ```C#
    public interface IProductRepository
	{
		IEnumerable<Product> Products { get; }

		Task SaveProductAsync(Product product);
	}
    ```

4. Implement the `SaveProduct` method as follows.
    ```C#
    public async Task SaveProductAsync(Product product)
    {
        if (product.ProductId == 0)
        {
            context.Products.Add(product);
        }
        else
        {
            Product dbEntry = context.Products
                .FirstOrDefault(p => p.ProductId == product.ProductId);
            if (dbEntry != null)
            {
                dbEntry.Name = product.Name;
                dbEntry.Description = product.Description;
                dbEntry.Price = product.Price;
            }
        }
        await context.SaveChangesAsync();
    }
    ```

## Handling Edit POST Requests

1. Add the `Edit` action that will handle POST requests on the `AdminController`

    ```C#
    [HttpPost]
    public async Task<IActionResult> Edit(Product product)
    {
        if (ModelState.IsValid)
        {
            await repository.SaveProductAsync(product);
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

2. Update the `Layout.cshtml` layout file in order to display the confirmation message.

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
    }
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
    Task<Product> DeleteProductAsync(int productID);
    ```

2. Implement the method in the `EFProductRepository` class.

    ```C#
    public async Task<Product> DeleteProductAsync(int productID) { 
        Product dbEntry = context.Products 
                .FirstOrDefault(p => p.ProductID == productID); 
    
        if (dbEntry != null) { 
            context.Products.Remove(dbEntry); 
            await context.SaveChangesAsync(); 
        } 
    
        return dbEntry; 
    } 
    ```
3. Add the corresponding action to the `AdminController`

    ```C#
    [HttpPost] 
    public async Task<IActionResult> Delete(int productId) { 
        Product deletedProduct = repository.DeleteProductAsync(productId); 
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
        mock.Verify(m => m.DeleteProductAsync(prod.ProductID));
    }
    ```
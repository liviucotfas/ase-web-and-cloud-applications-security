# CRUD - Create, Read, Update and Delete

<!-- vscode-markdown-toc -->
* 1. [Objectives](#Objectives)
* 2. [Creating a CRUD Controller](#CreatingaCRUDController)
* 3. [Displaying the Products](#DisplayingtheProducts)
* 4. [Editing the Products](#EditingtheProducts)
* 5. [Creating New Products](#CreatingNewProducts)
* 6. [Deleting Products](#DeletingProducts)
* 7. [Bibliography](#Bibliography)

<!-- vscode-markdown-toc-config
    numbering=true
    autoSave=true
    /vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

##  1. <a name='Objectives'></a>Objectives
- retrieving items from the database; 
- adding new items to the database;
- updating existing items in the database;
- deleting existing item from the database;

##  2. <a name='CreatingaCRUDController'></a>Creating a CRUD Controller

1. Add a new controller to the `Controllers` folder called `AdminController`

    ```C#
    public class AdminController : Controller
    {
        private readonly IProductService _productService;
        public AdminController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index(CancellationToken ct = default)
        {
            var products = await _productService.GetAllProductsAsync(ct);
            return View(products);
        }
    }
    ```

    > The controller constructor declares a dependency on the `IProductService` interface, which will be resolved by dependency injection. The controller defines an `Index` action that calls the service to obtain products and passes them to the view. The service layer contains business logic and performs data access using `ApplicationDbContext`.

##  3. <a name='DisplayingtheProducts'></a>Displaying the Products

2. Add a new layout file, called `_AdminLayout`, to the `Views/Shared` folder with the following code.

    ```CSHTML
    <!DOCTYPE html>

    <html lang="en">
    <head>
        <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">

        <title>@ViewBag.Title</title>

        <link href="/lib/bootstrap/dist/css/bootstrap.min.css" rel="stylesheet" />
    </head>
    <body>
       <nav class="navbar bg-light navbar-expand-sm">
            <div class="container-fluid">
                <span class="navbar-brand mb-0">MVCStore</span>
            </div>
        </nav>

        <div class="container-fluid">
            @RenderBody()
        </div>

        @await RenderSectionAsync("Scripts", required: false)
    </body>
    </html>
    ```

    > For navbars that never collapse, add the `.navbar-expand` class on the navbar. Link: https://getbootstrap.com/docs/4.0/components/navbar/

3. Add a view corresponding to the `Index` action in the `Admin` controller.

    ```CSHTML
   @model IEnumerable<Product>

    @{
        ViewBag.Title = "Admin";
        Layout = "~/Views/Shared/_AdminLayout.cshtml";
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
                <td class="text-right">@item.ProductID</td>
                <td>@item.Name</td>
                <td class="text-right">@item.Price</td>
                <td class="text-center">
                    <a asp-action="Edit" class="btn btn-sm btn-warning"
                    asp-route-productId="@item.ProductID">
                        Edit
                    </a>
                    <form 
                        asp-action="Delete" 
                        method="post" style="display: inline">
                        <input type="hidden" name="ProductId" value="@item.ProductID" />
                        <button type="submit" class="btn btn-danger btn-sm">
                            Delete
                        </button>
                    </form>
                </td>
            </tr>
        }
    </table>
    ```

##  4. <a name='EditingtheProducts'></a>Editing the Products

4. Add an `Edit` action on the `AdminController` that uses the service to retrieve a single product.

    ```C#
    public async Task<IActionResult> Edit(int productId, CancellationToken ct = default)
    {
        var product = await _productService.GetProductByIdAsync(productId, ct);
        return View(product);
    }
    ```

5. Add the corresponding view. Update the content of the view as follows:

    ```HTML
    @model MVCStore.Models.Product

    @{
        ViewBag.Title = "Edit";
        Layout = "_AdminLayout";
    }

    <h1>Edit product</h1>
    <hr />

    <form asp-action="Edit" method="post">
        <div asp-validation-summary="ModelOnly" class="text-danger"></div>
        <input type="hidden" asp-for="ProductID" />
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
        <div>
            <input type="submit" value="Save" class="btn btn-primary" />
            <a asp-action="Index" class="btn btn-secondary">Back to List</a>
        </div>
    </form>
    ```

6. Implement save/update operations in the `IProductService` and `ProductService` rather than in a repository. Example service interface methods:
    ```C#
    Task CreateProductAsync(Product product, CancellationToken ct = default);
    Task UpdateProductAsync(Product product, CancellationToken ct = default);
    ```

7. Example implementation (in `ProductService`):
    ```C#
    public async Task CreateProductAsync(Product product, CancellationToken ct = default)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateProductAsync(Product product, CancellationToken ct = default)
    {
        var dbEntry = await _context.Products.FirstOrDefaultAsync(p => p.ProductID == product.ProductID, ct);
        if (dbEntry != null)
        {
            dbEntry.Name = product.Name;
            dbEntry.Description = product.Description;
            dbEntry.Price = product.Price;
            await _context.SaveChangesAsync(ct);
        }
    }
    ```

8. Add the `Edit` action that will handle POST requests on the `AdminController` and call the service.

    ```C#
    [HttpPost]
    public async Task<IActionResult> Edit(Product product, CancellationToken ct = default)
    {
        if (ModelState.IsValid)
        {
            if (product.ProductID == 0)
            {
                await _productService.CreateProductAsync(product, ct);
                TempData["message"] = $"{product.Name} has been created";
            }
            else
            {
                await _productService.UpdateProductAsync(product, ct);
                TempData["message"] = $"{product.Name} has been saved";
            }
            return RedirectToAction("Index");
        }
        else
        {
            // there is something wrong with the data values 
            return View(product);
        }
    }
    ```

    > Notice the `TempData` object used to show success messages.

9. Update the `_AdminLayout.cshtml` layout file in order to display the confirmation message.

    ```CSHTML
    @if (TempData["message"] != null)
    {
        <div class="alert alert-success">@TempData["message"]</div>
    }
    ```

10. Update the `Product` class as follows (validation attributes remain the same):

    ```C#
    public class Product {
        public int ProductID { get; set; }
        [Required(ErrorMessage = "Please enter a product name")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Please enter a description")]
        public string Description { get; set; }
        [Required]
        [Range(0.01, double.MaxValue,ErrorMessage = "Please enter a positive price")]
        [Column(TypeName = "decimal(8, 2)")]
        public decimal Price { get; set; }
        public string Category { get; set; }
    }
    ```

##  5. <a name='CreatingNewProducts'></a>Creating New Products

11. Add a `Create` action to the `AdminController` class.

    ```C#
    public IActionResult Create(){
        return View("Edit", new Product());
    }  
    ```

##  6. <a name='DeletingProducts'></a>Deleting Products

12. Define a delete method on the service interface instead of a repository method:

    ```C#
    Task<Product?> DeleteProductAsync(int productID, CancellationToken ct = default);
    ```

13. Example implementation in `ProductService`:

    ```C#
    public async Task<Product?> DeleteProductAsync(int productID, CancellationToken ct = default)
    {
        var dbEntry = await _context.Products.FirstOrDefaultAsync(p => p.ProductID == productID, ct);
        if (dbEntry != null)
        {
            _context.Products.Remove(dbEntry);
            await _context.SaveChangesAsync(ct);
        }
        return dbEntry;
    }
    ```
14. Add the corresponding action to the `AdminController` that calls the service:

    ```C#
    [HttpPost]
    public async Task<IActionResult> Delete(int productId, CancellationToken ct = default)
    {
        var deletedProduct = await _productService.DeleteProductAsync(productId, ct);
        if (deletedProduct != null)
        {
            TempData["message"] = $"{deletedProduct.Name} was deleted";
        }
        return RedirectToAction("Index");
    }
    ```

15. Unit testing: mock `IProductService` instead of `IStoreRepository`. Example test for delete behavior:

    ```C#
    [Fact]
    public async Task Can_Delete_Valid_Products()
    {
        // Arrange - create a Product
        var prod = new Product { ProductID = 2, Name = "Test" };
        var mock = new Mock<IProductService>();
        mock.Setup(m => m.DeleteProductAsync(prod.ProductID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(prod);

        var controller = new AdminController(mock.Object);

        // Act
        var result = await controller.Delete(prod.ProductID);

        // Assert - ensure the service delete method was called
        mock.Verify(m => m.DeleteProductAsync(prod.ProductID, It.IsAny<CancellationToken>()));
    }
    ```
##  7. <a name='Bibliography'></a>Bibliography

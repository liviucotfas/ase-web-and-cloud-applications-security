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
		private IStoreRepository repository;
		public AdminController(IStoreRepository repo)
		{
			repository = repo;
		}
		public IActionResult Index()
		{
			return View(repository.Products);
		}
	}
    ```

##  3. <a name='DisplayingtheProducts'></a>Displaying the Products

2. In the Views/Admin folder add a Razor file called Index.cshtml

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

##  4. <a name='EditingtheProducts'></a>Editing the Products

3. Add an `Edit` action on the `AdminController` 

    ```C#
    public IActionResult Edit(int productId)
    {
        var product = repository.Products.FirstOrDefault(p => p.ProductId == productId);
        return View(product);
    }
    ```

4. Scaffold the corresponding view. Update the content of the view as follows:

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

5. Add a `SaveProduct` method in the `IStoreRepository` interface.
    ```C#
    public interface IStoreRepository
	{
		IEnumerable<Product> Products { get; }

		Task SaveProductAsync(Product product);
	}
    ```

6. Implement the `SaveProduct` method as follows.
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

7. Add the `Edit` action that will handle POST requests on the `AdminController`

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

8. Update the `Layout.cshtml` layout file in order to display the confirmation message.

    ```CSHTML
    @if (TempData["message"] != null)
    {
        <div class="alert alert-success">@TempData["message"]</div>
    }
    ```

9. Update the `Product` class as follows.

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

##  5. <a name='CreatingNewProducts'></a>Creating New Products

10. Add a `Create` action to the `AdminController` class.

    ```C#
    public IActionResult Create(){
        return View("Edit", new Product());
    }  
    ```

##  6. <a name='DeletingProducts'></a>Deleting Products

11. Add a `DeleteProduct` method to the `IStoreRepository` interface.

    ```C#
    Task<Product> DeleteProductAsync(int productID);
    ```

12. Implement the method in the `EFProductRepository` class.

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
13. Add the corresponding action to the `AdminController`

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

14. Add a test method

    ```C#
    [Fact]
    public void Can_Delete_Valid_Products() {
        // Arrange - create a Product
        Product prod = new Product { ProductID = 2, Name = "Test" };
        // Arrange - create the mock repository
        Mock<IStoreRepository> mock = new Mock<IStoreRepository>();
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
##  7. <a name='Bibliography'></a>Bibliography
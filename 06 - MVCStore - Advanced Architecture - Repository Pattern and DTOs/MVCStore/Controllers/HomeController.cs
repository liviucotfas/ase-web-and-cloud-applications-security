using Microsoft.AspNetCore.Mvc;
using MVCStore.Services;

namespace MVCStore.Controllers
{
	public class HomeController : Controller
	{
		private readonly IProductService _productService;

		public HomeController(IProductService productService)
		{
			_productService = productService;
		}

		public async Task<IActionResult> Index(CancellationToken ct = default)
		{
			var products = await _productService.GetAllProductsAsync(ct);
			return View(products);  // Returns List<ProductListItemDto>
		}
	}
}

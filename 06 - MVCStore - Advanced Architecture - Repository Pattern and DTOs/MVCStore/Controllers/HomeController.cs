using Microsoft.AspNetCore.Mvc;
using MVCStore.Services;
using MVCStore.ViewModels;

namespace MVCStore.Controllers
{
	public class HomeController : Controller
	{
		private readonly IProductService _productService;
		public int PageSize = 4;  // Number of products per page

		public HomeController(IProductService productService)
		{
			_productService = productService;
		}

		public async Task<IActionResult> Index(int productPage = 1, CancellationToken ct = default)
		{
			var products = await _productService.GetProductsPageAsync(productPage, PageSize, ct);
			var totalProducts = await _productService.GetProductCountAsync(ct);

			ViewBag.PagingInfo = new PagingInfoViewModel
			{
				CurrentPage = productPage,
				ItemsPerPage = PageSize,
				TotalItems = totalProducts
			};

			return View(products);
		}
	}
}

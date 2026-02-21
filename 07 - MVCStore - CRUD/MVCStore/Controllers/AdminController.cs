using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVCStore.Models.DTOs;
using MVCStore.Repositories;
using MVCStore.Services;

namespace MVCStore.Controllers
{
	public class AdminController : Controller
	{
		private readonly IProductService _productService;
		private readonly ICategoryRepository _categoryRepository;

		public AdminController(IProductService productService, ICategoryRepository categoryRepository)
		{
			_productService = productService;
			_categoryRepository = categoryRepository;
		}

		public async Task<IActionResult> Index(CancellationToken ct = default)
		{
			var products = await _productService.GetAllProductsAsync(ct);
			return View(products);
		}

		public async Task<IActionResult> Edit(int productId, CancellationToken ct = default)
		{
			var product = await _productService.GetProductByIdAsync(productId, ct);
			if (product is null)
			{
				return NotFound();
			}

			var dto = new UpdateProductDto
			{
				ProductID = product.ProductID,
				Name = product.Name,
				Price = product.Price,
				CategoryID = product.Category.CategoryID
			};

			await PopulateCategoriesAsync(dto.CategoryID, ct);
			return View(dto);
		}

		// Private helper – loads categories into ViewBag.Categories
		private async Task PopulateCategoriesAsync(int selectedId = 0, CancellationToken ct = default)
		{
			var categories = await _categoryRepository.GetAllAsync(ct);
			ViewBag.Categories = new SelectList(categories, "CategoryID", "Name", selectedId);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(UpdateProductDto dto, CancellationToken ct = default)
		{
			if (!ModelState.IsValid)
			{
				await PopulateCategoriesAsync(dto.CategoryID, ct);
				return View(dto);
			}

			try
			{
				await _productService.UpdateProductAsync(dto, ct);
				TempData["message"] = $"{dto.Name} has been saved.";
				return RedirectToAction(nameof(Index));
			}
			catch (InvalidOperationException ex)
			{
				ModelState.AddModelError("", ex.Message);
				await PopulateCategoriesAsync(dto.CategoryID, ct);
				return View(dto);
			}
		}

		public async Task<IActionResult> Create(CancellationToken ct = default)
		{
			await PopulateCategoriesAsync(ct: ct);
			return View(new CreateProductDto());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(CreateProductDto dto, CancellationToken ct = default)
		{
			if (!ModelState.IsValid)
			{
				await PopulateCategoriesAsync(dto.CategoryID, ct);
				return View(dto);
			}

			try
			{
				var created = await _productService.CreateProductAsync(dto, ct);
				TempData["message"] = $"{created.Name} has been created.";
				return RedirectToAction(nameof(Index));
			}
			catch (InvalidOperationException ex)
			{
				ModelState.AddModelError("", ex.Message);
				await PopulateCategoriesAsync(dto.CategoryID, ct);
				return View(dto);
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int productId, CancellationToken ct = default)
		{
			var product = await _productService.GetProductByIdAsync(productId, ct);
			if (product is not null)
			{
				await _productService.DeleteProductAsync(productId, ct);
				TempData["message"] = $"{product.Name} was deleted.";
			}
			return RedirectToAction(nameof(Index));
		}
	}
}
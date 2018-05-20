using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCStore.Data;
using MVCStore.Models;

namespace MVCStore.Controllers
{
	[Authorize]
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

		public ViewResult Edit(int productId)
		{
			return View(repository.Products.FirstOrDefault(p => p.ProductID == productId));
		}

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

		[Authorize(Roles = "ProductManagement")]
		public ViewResult Create() => View("Edit", new Product());

		[HttpPost]
		[Authorize(Roles = "ProductManagement")]
		public IActionResult Delete(int productId)
		{
			Product deletedProduct = repository.DeleteProduct(productId);
			if (deletedProduct != null)
			{
				TempData["message"] = $"{deletedProduct.Name} was deleted";
			}
			return RedirectToAction("Index");
		}
	}
}
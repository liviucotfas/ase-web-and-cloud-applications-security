using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCStore.Data;
using MVCStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCStore.Controllers
{
    [Authorize]
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
		public IActionResult Edit(int productId)
		{
			var product = repository.Products.FirstOrDefault(p => p.ProductID == productId);
			return View(product);
		}
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
        public IActionResult Create()
        {
            return View("Edit", new Product());
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int productId)
        {
            Product deletedProduct = await repository.DeleteProductAsync(productId);
            if (deletedProduct != null)
            {
                TempData["message"] = $"{deletedProduct.Name} was deleted";
            }
            return RedirectToAction("Index");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MVCStore.Model;

namespace MVCStore.Controllers
{
	public class ProductController : Controller
	{
		private IProductRepository repository;
		public ProductController(IProductRepository repo)
		{
			repository = repo;
		}

		public ViewResult List()
		{
			return View(repository.Products);
		}
	}
}
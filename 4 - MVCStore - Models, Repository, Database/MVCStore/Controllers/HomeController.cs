using Microsoft.AspNetCore.Mvc;
using MVCStore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCStore.Controllers
{
    public class HomeController : Controller
    {
        private IStoreRepository repository;
        public int PageSize = 2;

        public HomeController(IStoreRepository repo)
        {
            repository = repo;
        }
        public IActionResult Index(int productPage = 1)
        {
            var products = repository.Products
                .OrderBy(p => p.ProductID)
                .Skip((productPage - 1) * PageSize)
                .Take(PageSize);

            return View(products);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using MVCStore.Data;
using MVCStore.ViewModels;

namespace MVCStore.Controllers
{
    public class HomeController : Controller
    {
        private IStoreRepository repository;
        public HomeController(IStoreRepository repo)
        {
            repository = repo;
        }
        public int PageSize = 2;
        public ViewResult Index(int productPage = 1)
        {
            var viewModel = new ProductsListViewModel
            {
                Products = repository.Products
                    .OrderBy(p => p.ProductID)
                    .Skip((productPage - 1) * PageSize)
                    .Take(PageSize),
                PagingInfo = new PagingInfo
                {
                    CurrentPage = productPage,
                    ItemsPerPage = PageSize,
                    TotalItems = repository.Products.Count()
                }
            };

            return View(viewModel);
        }
    }
}

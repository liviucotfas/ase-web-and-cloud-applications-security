using Microsoft.AspNetCore.Mvc;

namespace MVCStore.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

using MainApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainApplication.Controllers
{
    [Authorize]
    public class APITokensController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

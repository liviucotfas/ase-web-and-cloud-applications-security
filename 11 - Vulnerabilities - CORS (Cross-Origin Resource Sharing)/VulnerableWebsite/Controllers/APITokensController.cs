using IronBankWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IronBankWeb.Controllers
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

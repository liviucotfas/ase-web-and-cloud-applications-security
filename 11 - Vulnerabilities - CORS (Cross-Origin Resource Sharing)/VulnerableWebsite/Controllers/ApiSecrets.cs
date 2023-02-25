using IronBankWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IronBankWeb.Controllers
{
    [Authorize]
    public class ApiSecretsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

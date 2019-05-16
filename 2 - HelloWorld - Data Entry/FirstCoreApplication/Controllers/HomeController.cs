using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FirstCoreApplication.Models;
using FirstCoreApplication.Data;
using System.Linq;

namespace FirstCoreApplication.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            int hour = DateTime.Now.Hour;
            ViewBag.Greeting = hour < 12 ? "Good Morning" : "Good Afternoon";
            return View();
        }

        public IActionResult RsvpForm()
        {
            return View();
        }

        public IActionResult Thanks(GuestResponse guestResponse)
        {
            return View(TempData["GuestResponse"]);
        }

        [HttpPost]
        public IActionResult RsvpForm(GuestResponse guestResponse)
        {
            if (ModelState.IsValid)
            {
                Repository.AddResponse(guestResponse);

                TempData["GuestResponse"] = guestResponse;
                return RedirectToAction("Thanks");
            }
            else
            {
                // there is a validation error 
                return View();
            }
        }

        public ViewResult ListResponses()
        {
            return View(Repository.Responses.Where(r => r.WillAttend == true));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

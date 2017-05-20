using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FirstCoreApplication.Models;

namespace FirstCoreApplication.Controllers
{
	public class HomeController : Controller
	{
		public ViewResult Index()
		{
			return View();
		}

		[HttpGet]
		public ViewResult RsvpForm()
		{
			return View();
		}

		[HttpPost]
		public ViewResult RsvpForm(GuestResponse guestResponse)
		{
			if (ModelState.IsValid)
			{
				Repository.AddResponse(guestResponse);
				return View("Thanks", guestResponse);
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
	}
}

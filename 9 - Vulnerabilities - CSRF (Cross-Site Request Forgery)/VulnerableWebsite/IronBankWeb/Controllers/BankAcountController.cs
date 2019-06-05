using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IronBankWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IronBankWeb.Controllers
{
    [Authorize]
    public class BankAcountController : Controller
    {
        public IActionResult Transfer()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Transfer(TransferViewModel transfer)
        {
            // User.Identity

            if(ModelState.IsValid)
            {
                ViewBag.Message = $"You have transfered {transfer.Amount} euros to {transfer.DestinationAccount}.";
            }
            return View(transfer);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using blockchain.Models;
using blockchain.Data;
using blockchain.Models.BlockchainModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using blockchain.SignalR;
using blockchain.Extensions;

namespace blockchain.Controllers
{
    public class DonateController : BaseController
    {
        private readonly IHubContext<DonateHub, IHubProvider> _hub;
        public DonateController(ApplicationDbContext db, IHubContext<DonateHub, IHubProvider> hub) : base(db) 
        { 
            _hub = hub;
        }

        public IActionResult Index()
        {
            var foundations = _db.Foundations
                                 .Select(x => new SelectListItem
                                              {
                                                  Text = x.NameEn,
                                                  Value = x.NameEn
                                              })
                                 .ToList();


            ViewBag.Foundations = foundations;
            return View(new DonateViewModel { FromAddress = Program.Name });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Donate(DonateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var transaction = new Transaction
                                  {
                                      FromAddress = model.FromAddress,
                                      ToAddress = model.ToAddress,
                                      Amount = model.Amount,
                                      UnixTimeStamp = DateTime.Now.ToUnixTimeStamp()
                                  };

                await _hub.Clients.Group("Donate").SendTransaction(transaction);
                // _db.Transactions.Add(transaction);
                // await _db.SaveChangesAsync();
                return Json("done");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

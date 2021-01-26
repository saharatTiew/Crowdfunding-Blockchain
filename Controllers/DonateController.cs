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
using Newtonsoft.Json;
using blockchain.Providers.Interfaces;

namespace blockchain.Controllers
{
    public class DonateController : BaseController
    {
        private readonly IHubContext<DonateHub, IHubProvider> _hub;
        private readonly IHashProvider _hash;
        public DonateController(ApplicationDbContext db, 
                                IHubContext<DonateHub, IHubProvider> hub,
                                IHashProvider hash) : base(db) 
        { 
            _hub = hub;
            _hash = hash;
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
        
        [HttpGet("transactions/{userName}")]
        public IActionResult Transactions(string userName)
        {
            var hashedUserName = _hash.CalculateHash(userName);

            var transactions = _db.Transactions
                                  .Where(x => x.FromAddress == userName || x.FromAddress == hashedUserName)
                                  .ToList();
            
            var totalDonated = transactions.Where(x => x.IsDonated).Sum(x => x.Amount);
            var totalUnDonated = transactions.Where(x => !x.IsDonated).Sum(x => x.Amount);
            return Json(new { totalDonated, totalUnDonated, transactions });
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
                                      ToAddress = _db.Foundations.OrderBy(x => x.id).LastOrDefault()?.NameEn,
                                      IntendedFoundation = model.ToAddress,
                                      Amount = model.Amount,
                                      UnixTimeStamp = DateTime.Now.ToUnixTimeStamp()
                                  };

                await _hub.Clients.Group("Donate").SendTransaction(transaction);
                return Json("Donate Is Pending...");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

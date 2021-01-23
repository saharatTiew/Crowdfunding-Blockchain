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
using blockchain.Enums;
using Microsoft.AspNetCore.SignalR;
using blockchain.SignalR;

namespace blockchain.Controllers
{
    public class LoginController : BaseController
    {
        private readonly IHubContext<DonateHub, IHubProvider> _hub;
        public LoginController(ApplicationDbContext db, IHubContext<DonateHub, IHubProvider> hub) : base(db) 
        { 
            _hub = hub;
        }

        public IActionResult Index()
        {
            return View(new LoginViewModel());
        }

        // public async Task<IActionResult> AAA()
        // {
        //     var isReceived = false;
        //     var port = 0;
        //     await _hub.Clients.Group("Donate").SendTransaction("sss");
        //     // while (!isReceived)
        //     // {
        //     //     await Task.Delay(20);
        //     // }
        //     return Json("sss");
        // }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(LoginViewModel model)
        {
            var user = _db.Users.FirstOrDefault(x => x.Username == model.Username);
            var role = user?.Role;
            Program.Role = role;
            Program.Name = model.Username;

            if (role == RoleType.VALIDATOR.ToStringValue())
                return RedirectToAction(nameof(MineController.Index), "Mine");
            
            else if (role == RoleType.DONOR.ToStringValue())
                return RedirectToAction(nameof(DonateController.Index), "Donate");
            
            else if (role == RoleType.ORGANIZER.ToStringValue())
                return Json("");
            
            else
                return Json("Failed");
        }
    }

}

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

namespace blockchain.Controllers
{
    public class MineController : BaseController
    {
        public MineController(ApplicationDbContext db) : base(db) { }

        public IActionResult Index()
        {
            Console.Write("Sss");
            return Json(new {Program.Role, Program.Name});
            // return View();
        }
    }

}

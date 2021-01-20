// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Logging;
// using blockchain.Models;
// using blockchain.Data;
// using blockchain.Models.BlockchainModels;

// namespace blockchain.Controllers
// {
//     public class RaftController : BaseController
//     {
//         // public static Client Client = new Client();
//         public RaftController(ApplicationDbContext db) : base(db) { }

//         public IActionResult Voting()
//         {
//             Client.Broadcast("Voting Message");
//         }
//     }

// }

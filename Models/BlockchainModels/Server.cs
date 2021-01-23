using System;
using System.Collections.Generic;
using System.Linq;
using blockchain.Data;
using blockchain.Providers;
using blockchain.Providers.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace blockchain.Models.BlockchainModels
{
    public class Server : WebSocketBehavior
    {
        private bool IsChainSynced = false;
        private WebSocketServer wss = null;
        private static ApplicationDbContext _dbContext;

        public void Start()
        {
            int port = Program.Port;
            wss = new WebSocketServer($"ws://127.0.0.1:{port}");
            wss.AddWebSocketService<Server>("/Blockchain");
            wss.Start();
            Console.WriteLine($"Started server at ws://127.0.0.1:{port}");

            var services = new ServiceCollection();
            services.AddTransient<IHashProvider, HashProvider>();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer("Server=localhost; Database=BlockchainCharity; Trusted_Connection=True; MultipleActiveResultSets=True;"));

            var serviceProvider = services.BuildServiceProvider();
            _dbContext = serviceProvider.GetService<ApplicationDbContext>();
        }

        public void Test()
        {
            // CASE : NOT WORK
            // Send("GGGGGG");
            wss.WebSocketServices.Broadcast("GGGGGG");
            // Sessions.Broadcast("GGGGGG");
        }

        public void BroadcastLeader()
        {
            wss.WebSocketServices.Broadcast($"New Leader : {Program.Port}");
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            if (e.Data == "Hi Server")
            {
                Console.WriteLine(e.Data);
                Send("Hi Client");
            }
            else if (e.Data == "sss")
            {
                Console.WriteLine("Got ya");
            }
            else if (e.Data == "You Are Next Leader")
            {
                Console.WriteLine("GOT SELECTED TO BE A LEADER!");
                Console.WriteLine($"Cooldown : {Program.CoolDown}");
                if (Program.CoolDown > 0)
                {
                    Send("Not Applicable For Leader");
                }
                else
                {
                    Console.WriteLine("Y");
                    Program.CurrentLeader = Program.Port;
                    Send($"I'm New Leader : {Program.Port}");
                }
            }
            else if (e.Data.StartsWith("New Leader"))
            {
                int currentLeader = Convert.ToInt32(e.Data.Split(" : ").Last());
                if (currentLeader != Program.Port)
                {
                    Program.CurrentLeader = currentLeader;
                    Console.WriteLine("OK");
                }
                else
                {
                    Program.CoolDown = Program.Penalty();
                    Console.WriteLine($"Cool Down : {Program.CoolDown}");
                }
            }
            else if (e.Data.StartsWith("NumNode :"))
            {
                int numNode = Convert.ToInt32(e.Data.Split(" : ").Last());
                Program.NumNode = numNode; 
            }
            else if (e.Data == "Decrease Cooldown")
            {
                Program.CoolDown--;
            }
            else if (e.Data.StartsWith("Verify : "))
            {
                var serealizedTransaction = e.Data.Split(" : ").Last();
                var transaction = JsonConvert.DeserializeObject<Transaction>(serealizedTransaction);

                var fromUser = _dbContext.Users.FirstOrDefault(x => x.Username == transaction.FromAddress);
                var moneyLeft = fromUser.RemainingMoney;

                if (moneyLeft < transaction.Amount)
                    Send("Verify : Approve");
    
                else
                    Send("Verify : Reject");
            }
            else if (e.Data.StartsWith("New Block"))
            {
                var serealizedBlock = e.Data.Split(" : ").Last();
                var block = JsonConvert.DeserializeObject<BlockGeneric>(serealizedBlock);

                var isValid = false;

                if (isValid)
                    Send("Verify : Approve");
    
                else
                    Send("Verify : Reject");
            }
            else
            {
                Blockchain newChain = JsonConvert.DeserializeObject<Blockchain>(e.Data);

                if (newChain.IsValid() && newChain.Chain.Count > Program.Blockchain.Chain.Count)
                {
                    List<Transaction> newTransactions = new List<Transaction>();
                    newTransactions.AddRange(newChain.PendingTransactions);
                    newTransactions.AddRange(Program.Blockchain.PendingTransactions);

                    newChain.PendingTransactions = newTransactions;
                    Program.Blockchain = newChain;
                }

                if (!IsChainSynced)
                {
                    Send(JsonConvert.SerializeObject(Program.Blockchain));
                    IsChainSynced = true;
                }
            }
        }
    }
}
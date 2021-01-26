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
        private static IHashProvider _hashProvider = new HashProvider();
        private readonly IBlockchainProvider _blockchainProvider = new BlockchainProvider(_hashProvider);

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
                options.UseSqlServer("Server=localhost; Database=BlockchainCharity; Trusted_Connection=True; MultipleActiveResultSets=True;")
                ,ServiceLifetime.Transient);

            var serviceProvider = services.BuildServiceProvider();
            _dbContext = serviceProvider.GetService<ApplicationDbContext>();
        }

        public void BroadcastLeader()
        {
            wss.WebSocketServices.Broadcast($"New Leader : {Program.Port}");
        }

        public void BroadcastGGGGG()
        {
            wss.WebSocketServices.Broadcast($"GGGGG");
        }

        protected async override void OnMessage(MessageEventArgs e)
        {
            if (e.Data.StartsWith("Hi Server"))
            {
                var msgFrom = e.Data.Split(" : ").Last();
                Console.WriteLine($"Port {msgFrom} connected with you....");
                Send($"Hi Client : {Program.Port}");
            }
            else if (e.Data == "You Are Next Leader")
            {
                Console.WriteLine("Got randomly selected to be leader....");
                Console.WriteLine($"Cooldown : {Program.CoolDown}");
                if (Program.CoolDown > 0)
                {
                    // Console.WriteLine("Cooldown Remain.....");
                    Send("Not Applicable For Leader");
                }
                else
                {
                    Console.WriteLine("I am New Leader....");
                    Program.CurrentLeader = Program.Port;
                    await Program.JoinLeaderChannel();
                    Send($"I'm New Leader : {Program.Port}");
                }
            }
            else if (e.Data.StartsWith("New Leader"))
            {
                int currentLeader = Convert.ToInt32(e.Data.Split(" : ").Last());
                if (currentLeader != Program.Port)
                {
                    Program.CurrentLeader = currentLeader;
                    Console.WriteLine("New Leader is randomly selected....");
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
                Console.WriteLine(serealizedTransaction);
                var fromUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Username == transaction.FromAddress);
                Console.WriteLine(JsonConvert.SerializeObject(fromUser));

                if (transaction.FromAddress == (await _dbContext.Foundations.OrderBy(x => x.id).LastOrDefaultAsync()).NameEn)
                {
                    var amount = _dbContext.Transactions
                                           .Where(x => !x.IsDonated
                                                       && x.IntendedFoundation == transaction.IntendedFoundation)
                                           .Sum(x => x.Amount);
                    
                    if (amount == transaction.Amount)
                    {
                        Send("Verify : Approve");
                        Console.WriteLine("Approve This Transaction....");
                    }
                    else
                    {
                        Send("Verify : Reject");
                        Console.WriteLine("Reject This Transaction....");
                    }
                }
                else
                {
                    var moneyLeft = fromUser.RemainingMoney;

                    if (moneyLeft >= transaction.Amount)
                    {
                        Send("Verify : Approve");
                        Console.WriteLine("Approve This Transaction....");
                    }
                    else
                    {
                        Send("Verify : Reject");
                        Console.WriteLine("Reject This Transaction....");
                    }
                }
            }
            else if (e.Data.StartsWith("New Block"))
            {
                var serealizedBlock = e.Data.Split(" : ").Last();
                BlockGeneric newBlock;
            
                if (Program.Port == 2222)
                {
                    newBlock = JsonConvert.DeserializeObject<Block1>(serealizedBlock);
                }
                else if (Program.Port == 2223)
                {
                    newBlock = JsonConvert.DeserializeObject<Block2>(serealizedBlock);
                }
                else
                {
                    newBlock = JsonConvert.DeserializeObject<Block3>(serealizedBlock);
                }
                // Console.WriteLine(serealizedBlock);
                // Console.WriteLine(JsonConvert.SerializeObject(Program.chain.Last(), Formatting.Indented));
                
                var isValid = _blockchainProvider.IsBlockValid(Program.Chain.Last(), newBlock);

                if (isValid)
                {
                    Send("Transaction Verify : Approve");
                    Console.WriteLine("Approve This Block....");
                }
                else
                {
                    Send("Transaction Verify : Reject");
                    Console.WriteLine("Reject This Block....");
                }
            }
            else if (e.Data.StartsWith("Add Block to Chain"))
            {
                var serealizedBlock = e.Data.Split(" : ").Last();

                if (Program.Port == 2222)
                {
                    var newBlock = JsonConvert.DeserializeObject<Block1>(serealizedBlock);
                    newBlock.id = 0;
                    await _dbContext.Blockchains1.AddAsync(newBlock);
                }
                else if (Program.Port == 2223)
                {
                    var newBlock = JsonConvert.DeserializeObject<Block2>(serealizedBlock);
                    newBlock.id = 0;
                    await _dbContext.Blockchains2.AddAsync(newBlock);
                }
                else
                {
                    var newBlock = JsonConvert.DeserializeObject<Block3>(serealizedBlock);
                    newBlock.id = 0;
                    await _dbContext.Blockchains3.AddAsync(newBlock);
                }
                await _dbContext.SaveChangesAsync();
                Console.WriteLine("Add block to a chain...");
                await Program.LoadChain();
            }
            else if (e.Data.StartsWith("Donate"))
            {
                Console.WriteLine("Confirmation to Donate!");
                Send(e.Data);
            }
            else
            {
                var information = e.Data.Split(" : ");
                var chain = information[2];
                var port = information[1];

                List<BlockGeneric> newChain;

                if (Program.Port == 2222)
                {
                    var tempChain = JsonConvert.DeserializeObject<List<Block1>>(chain);
                    newChain = tempChain.Cast<BlockGeneric>().ToList();
                }
                else if (Program.Port == 2223)
                {
                    var tempChain = JsonConvert.DeserializeObject<List<Block2>>(chain);
                    newChain = tempChain.Cast<BlockGeneric>().ToList();
                }
                else
                {
                    var tempChain = JsonConvert.DeserializeObject<List<Block3>>(chain);
                    newChain = tempChain.Cast<BlockGeneric>().ToList();
                }

                // Console.WriteLine(JsonConvert.SerializeObject(newChain.TakeLast(5), Formatting.Indented));
                
                var isBlockchainValid = _blockchainProvider.IsBlockchainValid(newChain);
                // Console.WriteLine(isBlockchainValid);
                if (isBlockchainValid && newChain.Count > Program.Chain.Count)
                {
                    Console.WriteLine("Updating new chain.....");
                    Program.DeleteChains(newChain);
                }
                else if (!isBlockchainValid)
                {
                    // TODO : Disconnect the node that has the corrupted chain
                    Program.IsChainValid = false;
                    Console.WriteLine("The new chain is invalid...");
                }

                if (!IsChainSynced)
                {
                    // Console.WriteLine("Sending...");
                    // Console.WriteLine(JsonConvert.SerializeObject(Program.chain));
                    Send($"From Port : {Program.Port} : {JsonConvert.SerializeObject(Program.Chain)}");
                    // IsChainSynced = true;
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using blockchain.Models.BlockchainModels;
using Newtonsoft.Json;
using blockchain.Extensions;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using blockchain.Data;
using Microsoft.EntityFrameworkCore;
using blockchain.Providers.Interfaces;
using blockchain.Providers;

namespace blockchain
{
    public class Program
    {
        public static int Port = 0;
        public static Server Server = null;
        public static Client Client = new Client();
        public static Blockchain Blockchain = new Blockchain();
        public static HubConnection signalRConnection = null;

        public static int NumNode = 0;
        public static string Name = "Unknown";
        public static string Role = "Unknown";
        public static int CurrentLeader = 0;
        public static int CoolDown = 0;
        public static int ConfirmedNumber = 0;
        public static int RejectedNumber = 0;
        public static int Penalty() => ((int)Math.Floor((double) NumNode / 2)) + 1;
        public static int RequiredNumber() => ((int)Math.Ceiling((double) NumNode / 2));
        private static ApplicationDbContext _dbContext;
        private static IHashProvider _hash;

        public static async Task Main(string[] args)
        { 
            if (args.Length == 0)
            {
                Console.WriteLine("User Mode");
            }
            else
            {
                // Blockchain.InitializeChain();
                if (args.Length >= 1)
                    Port = int.Parse(args[0]);
                if (args.Length >= 2)
                    Name = args[1];
                // Console.WriteLine("HEY");
                if (Port > 0)
                {
                    Server = new Server();
                    Server.Start();

                    // inject DbContext and HashProvider to Program.cs
                    var services = new ServiceCollection();

                    services.AddTransient<IHashProvider, HashProvider>();
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer("Server=localhost; Database=BlockchainCharity; Trusted_Connection=True; MultipleActiveResultSets=True;"));

                    var serviceProvider = services.BuildServiceProvider();
                    _dbContext = serviceProvider.GetService<ApplicationDbContext>();
                    _hash = serviceProvider.GetService<IHashProvider>();

                    // construct signalr connection
                    signalRConnection = new HubConnectionBuilder()
                                            .WithUrl(new Uri("https://127.0.0.1:5001/donateHub"), options => 
                                            {
                                                //bypass SSL on SignalR Client
                                                var handler = new HttpClientHandler
                                                {
                                                    ClientCertificateOptions = ClientCertificateOption.Manual,
                                                    ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
                                                };
                                                options.HttpMessageHandlerFactory = _ => handler;
                                                options.WebSocketConfiguration = sockets =>
                                                {
                                                    sockets.RemoteCertificateValidationCallback = (sender, certificate, chain, policyErrors) => true;
                                                };
                                            })
                                            .Build();
                    
                    // subscribe on method SendTransaction
                    signalRConnection.On<Transaction>("SendTransaction", async transaction =>
                    {
                        var fromUser = _dbContext.Users.FirstOrDefault(x => x.Username == transaction.FromAddress);
                        var moneyLeft = fromUser.RemainingMoney;

                        if (moneyLeft < transaction.Amount)
                        {
                            Console.WriteLine("Money Not Enough!");
                        }
                        else
                        {
                            fromUser.RemainingMoney -= transaction.Amount;
                            transaction.HashedTransactionId = _hash.HashTransaction(transaction);
                            var serializedTransaction = JsonConvert.SerializeObject(transaction);
                            Client.Broadcast($"Verify : {serializedTransaction}");

                            while (ConfirmedNumber < RequiredNumber() && ConfirmedNumber + RejectedNumber < NumNode)
                            {
                                await Task.Delay(30);
                            }

                            if (ConfirmedNumber < RequiredNumber())
                            {
                                Console.WriteLine("Rejected Transaction.....");
                            }
                            else
                            {
                                _dbContext.Transactions.Add(transaction);
                                _dbContext.SaveChanges();
                                
                                ConfirmedNumber = 0;
                                RejectedNumber = 0;
                                Console.WriteLine("Added Transaction.....");
                                await Task.Delay(10000);
                                var block = new BlockGeneric();

                                var prevBlock = _dbContext.Blockchains
                                                          .OrderByDescending(x => x.UnixTimeStamp)
                                                          .LastOrDefault();

                                var transactions = _dbContext.Transactions
                                                             .Where(x => !x.IsCommitted)
                                                             .OrderByDescending(x => x.UnixTimeStamp)
                                                             .Take(block.BlockSize)
                                                             .ToList();
                                
                                block.Height = (prevBlock?.Height + 1) ?? 0;
                                block.PreviousHash = prevBlock?.Hash;
                                block.TransactionJsons = transactions != null ? JsonConvert.SerializeObject(transactions) : null;
                                block.UnixTimeStamp = DateTime.Now.ToUnixTimeStamp();
                                block.Hash = _hash.HashBlock(block);
                                block.HashedBy = _hash.CalculateHash(Program.Port.ToString());
                                Console.WriteLine(JsonConvert.SerializeObject(block));

                                var serializedBlock = JsonConvert.SerializeObject(block);
                                Client.Broadcast($"New Block : {serializedBlock}");

                                while (ConfirmedNumber < RequiredNumber() && ConfirmedNumber + RejectedNumber < NumNode)
                                {
                                    await Task.Delay(30);
                                }

                                if (ConfirmedNumber < RequiredNumber())
                                {
                                    Console.WriteLine("Rejected Block.....");
                                }
                                else
                                {
                                    Console.WriteLine("Added Block.....");
                                    transactions.ForEach(x => x.IsCommitted = true);
                                    _dbContext.Blockchains.Add(block);
                                    await _dbContext.SaveChangesAsync();
                                }
                            }
                        }
                    });

                    await signalRConnection.StartAsync();
                }

                Console.WriteLine("=========================");
                Console.WriteLine("1. Connect to a server");
                Console.WriteLine("2. Add a transaction");
                Console.WriteLine("3. Display Blockchain");
                Console.WriteLine("4. Exit");
                Console.WriteLine("7. Vote for New Leader");
                Console.WriteLine("8. Get All Server that this client connect to");
                Console.WriteLine("10. Get Current Leader");
                Console.WriteLine("11. Be A Leader");
                Console.WriteLine("12. Get Cool down");
                Console.WriteLine("13. Sync Num Node");
                Console.WriteLine("=========================");

                int selection = 0;
                while (selection != 4)
                {
                    switch (selection)
                    {
                        case 13:
                            NumNode = Client.GetServers().Count + 1;
                            Console.WriteLine(NumNode);
                            Client.Broadcast($"NumNode : {NumNode}");
                            break;
                        case 1:
                            Console.WriteLine("Please enter the server URL");
                            string serverURL = Console.ReadLine();
                            Client.Connect($"{serverURL}/Blockchain");
                            break;
                        case 2:
                            Console.WriteLine("Please enter the receiver name");
                            string receiverName = Console.ReadLine();
                            Console.WriteLine("Please enter the amount");
                            string amount = Console.ReadLine();
                            Blockchain.CreateTransaction(new Transaction(Name, receiverName, int.Parse(amount), DateTime.Now.ToUnixTimeStamp()));
                            Blockchain.ProcessPendingTransactions(Name);
                            Client.Broadcast(JsonConvert.SerializeObject(Blockchain));
                            break;
                        case 3:
                            Console.WriteLine("Blockchain");
                            Console.WriteLine(JsonConvert.SerializeObject(Blockchain, Formatting.Indented));
                            break;
                        case 6:
                            Client.Send("ws://127.0.0.1:2223/Blockchain", "sss");
                            break;
                        case 7:
                            Client.Broadcast($"Decrease Cooldown");
                            Client.InvokeVote();
                            break;
                        case 8:
                            Console.WriteLine(JsonConvert.SerializeObject(Client.GetServers(), Formatting.Indented));
                            break;
                        case 9:
                            Server.Test();
                            break;
                        case 10:
                            Console.WriteLine(CurrentLeader);
                            break;
                        case 11:
                            CurrentLeader = Program.Port;
                            CoolDown = Penalty();
                            Server.BroadcastLeader();
                            await signalRConnection.InvokeAsync("AddToGroup", "Donate");
                            break;
                        case 12:
                            Console.WriteLine($"CoolDown : {CoolDown}");
                            Console.WriteLine($"Penalty : {Penalty()}");
                            Console.WriteLine($"NumNode : {NumNode}");
                            break;
                        case 14:
                            await signalRConnection.InvokeAsync("RemoveFromGroup", "Donate");
                            break;
                    }

                    Console.WriteLine("Please select an action");
                    string action = Console.ReadLine();
                    selection = int.Parse(action);
                }

                Client.Close();
            }
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

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
using blockchain.Models;

namespace blockchain
{
    public class Program
    {
        public static int Port = 0;
        public static Server Server = null;
        public static Client Client = null;
        public static List<BlockGeneric> Chain = new List<BlockGeneric>();
        public static HubConnection SignalRConnection = null;
        public const string SocketUrl = "ws://127.0.0.1:";
        public static bool IsChainValid = true;
        public static bool DoneConnecting = false;
        public static bool IsDonatable = false;
        public static Transaction TempDonateTransaction = null;
        public static Foundation TempFoundation = null;
        public static int NumNode = 0;
        public static string Name = "Unknown";
        public static string Role = "Unknown";
        public static int CurrentLeader = 0;
        public static int CoolDown = 0;
        public static int ConfirmedNumber = 1;
        public static int RejectedNumber = 0;
        public static int Penalty() => ((int)Math.Floor((double) NumNode / 2)) + 1;
        public static int RequiredNumber() => ((int)Math.Ceiling((double) NumNode / 2));
        private static ApplicationDbContext _dbContext;
        private static IHashProvider _hash;
        private static IBlockchainProvider _blockchain;

        public static async Task Main(string[] args)
        { 
            if (args.Length == 0)
            {
                Console.WriteLine("User Mode");
                CreateHostBuilder(args).Build().Run();
            }
            else
            {
                if (args.Length >= 1)
                    Port = int.Parse(args[0]);
            
                if (Port > 0)
                {
                    // inject DbContext and HashProvider to Program.cs
                    var services = new ServiceCollection();

                    services.AddTransient<IHashProvider, HashProvider>();
                    services.AddTransient<IBlockchainProvider, BlockchainProvider>();
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer("Server=localhost; Database=BlockchainCharity; Trusted_Connection=True; MultipleActiveResultSets=True;")
                        ,ServiceLifetime.Transient);
                    
                    var serviceProvider = services.BuildServiceProvider();
                    _dbContext = serviceProvider.GetService<ApplicationDbContext>();
                    _hash = serviceProvider.GetService<IHashProvider>();
                    _blockchain = serviceProvider.GetService<IBlockchainProvider>();

                    Client = new Client(_blockchain, _hash);
                    Server = new Server();
                    Server.Start();

                    // construct signalr connection
                    SignalRConnection = new HubConnectionBuilder()
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
                    SignalRConnection.On<Transaction>("SendTransaction", async transaction =>
                    {
                        await Validate(transaction);
                        // await ChangeLeader();
                        Client.Broadcast($"Decrease Cooldown");
                        Client.InvokeVote();
                        await SignalRConnection.InvokeAsync("RemoveFromGroup", "Donate");
                    });

                    await SignalRConnection.StartAsync();
                }

                await LoadChain();

                Console.WriteLine("=========================");
                Console.WriteLine("1. Connect to a node");
                Console.WriteLine("2. Be A Leader");
                Console.WriteLine("3. Sync Number of Node");
                Console.WriteLine("4. Vote for New Leader");
                Console.WriteLine("5. Get All nodes that this node connects to");
                Console.WriteLine("6. Get Current Leader");
                Console.WriteLine("7. Get Cooldown");
                Console.WriteLine("8. Search Transaction");
                Console.WriteLine("9. Edit Transaction (For malicious node)");
                Console.WriteLine("10. Get Chains");
                Console.WriteLine("999. Exit");
                Console.WriteLine("=========================");

                int selection = 0;
                while (selection != 999)
                {
                    switch (selection)
                    {   
                        case 1:
                            Console.WriteLine("Please enter the port");
                            string port = Console.ReadLine();
                            Client.Connect($"{SocketUrl}{port}/Blockchain");
                            break;
                        case 2:
                            if (IsChainValid)
                                await Coup();
                            else
                                Console.WriteLine("The chain is invalid....");
                            break;
                        case 3:
                            NumNode = Client.GetServers().Count + 1;
                            Console.WriteLine("Current Nodes are : " + NumNode);
                            Client.Broadcast($"NumNode : {NumNode}");
                            DoneConnecting = true;
                            break;
                        case 4:
                            Client.Broadcast($"Decrease Cooldown");
                            Client.InvokeVote();
                            await SignalRConnection.InvokeAsync("RemoveFromGroup", "Donate");
                            break;
                        case 5:
                            Console.WriteLine(JsonConvert.SerializeObject(Client.GetServers(), Formatting.Indented));
                            break;
                        case 6:
                            Console.WriteLine($"CurrentLeader : {CurrentLeader}");
                            break;
                        case 7:
                            Console.WriteLine($"CoolDown : {CoolDown}");
                            Console.WriteLine($"Penalty : {Penalty()}");
                            Console.WriteLine($"NumNode : {NumNode}");
                            break;
                        case 8:
                            Console.WriteLine("Please enter the transactionId");
                            string transactionId = Console.ReadLine();
                            var searchedTransaction = _dbContext.Transactions
                                                                .FirstOrDefault(x => x.HashedTransactionId == transactionId);
                            
                            if (searchedTransaction == null)
                            {
                                Console.WriteLine("Transaction not found!");
                            }
                            else
                            {
                                 Console.WriteLine(JsonConvert.SerializeObject(searchedTransaction));
                            }                           
                            break;
                        case 9:
                            Console.WriteLine("Please enter the transactionId");
                            string editedTransactionId = Console.ReadLine();
                            var transaction = _dbContext.Transactions
                                                        .AsNoTracking()
                                                        .FirstOrDefault(x => x.HashedTransactionId == editedTransactionId);
                            
                            if (transaction == null)
                            {
                                Console.WriteLine("Transaction not found!");
                            }
                            else
                            {
                                var allTransactions = _dbContext.Transactions
                                                                .Where(x => x.BlockHeight == transaction.BlockHeight)
                                                                .AsNoTracking()
                                                                .ToList();
                                
                                transaction.Amount = 5000;
                                transaction.HashedTransactionId = _hash.HashTransaction(transaction);
                                
                                foreach (var item in allTransactions)
                                {
                                    if (item.id == transaction.id)
                                    {
                                        item.HashedTransactionId = transaction.HashedTransactionId;
                                        item.Amount = transaction.Amount;
                                    }
                                }

                                if (Program.Port == 2222)
                                {
                                    var block = _dbContext.Blockchains1.FirstOrDefault(x => x.Height == transaction.BlockHeight);
                                    var hashedTransactions = String.Join('-', allTransactions.Select(x => x.HashedTransactionId));
                                    block.HashedTransactionIds = hashedTransactions;
                                    block.Hash = _hash.HashBlock(block);
                                }
                                else if (Program.Port == 2223)
                                {
                                    var block = _dbContext.Blockchains1.FirstOrDefault(x => x.Height == transaction.BlockHeight);
                                    var hashedTransactions = String.Join('-', allTransactions.Select(x => x.HashedTransactionId));
                                    block.HashedTransactionIds = hashedTransactions;
                                    block.Hash = _hash.HashBlock(block);
                                }
                                else
                                {
                                    var block = _dbContext.Blockchains1.FirstOrDefault(x => x.Height == transaction.BlockHeight);
                                    var hashedTransactions = String.Join('-', allTransactions.Select(x => x.HashedTransactionId));
                                    block.HashedTransactionIds = hashedTransactions;
                                    block.Hash = _hash.HashBlock(block);
                                }

                                await _dbContext.SaveChangesAsync();
                                Console.WriteLine("Done Changing Transaction.....");
                            }
                            break;
                        case 10:
                            Console.WriteLine(JsonConvert.SerializeObject(Chain, Formatting.Indented));
                            break;
                    }
                    Console.WriteLine("Please select an action");
                    string action = Console.ReadLine();
                    await LoadChain();
                    selection = int.Parse(action);

                    // if (DoneConnecting)
                    // {
                        // await Task.Delay(5000);
                        if (IsDonatable)
                        {
                            await DonateToFoundationIfAble();
                        }
                    //     Console.WriteLine("FCK ASYNC");
                    // }
                }

                Client.Close();
                await SignalRConnection.InvokeAsync("RemoveFromGroup", "Donate");
            }
            // CreateHostBuilder(args).Build().Run();
        }

        public static List<BlockGeneric> DeleteChains(List<BlockGeneric> newChain)
        {
            if (Program.Port == 2222)
            {
                var oldChain = _dbContext.Blockchains1;
                _dbContext.Blockchains1.RemoveRange(oldChain);
            }
            else if (Program.Port == 2223)
            {
                var oldChain = _dbContext.Blockchains2;
                _dbContext.Blockchains2.RemoveRange(oldChain);
            }
            else
            {
                var oldChain = _dbContext.Blockchains3;
                _dbContext.Blockchains3.RemoveRange(oldChain);
            }
    
            List<BlockGeneric> addedChain = new List<BlockGeneric>();

            foreach (var item in newChain)
            {
                var oldblockJson = JsonConvert.SerializeObject(item);

                if (Program.Port == 2222)
                {
                    var addedBlock = JsonConvert.DeserializeObject<Block1>(oldblockJson);
                    addedBlock.id = 0;
                    _dbContext.Blockchains1.Add(addedBlock);
                }
                else if (Program.Port == 2223)
                {
                    var addedBlock = JsonConvert.DeserializeObject<Block2>(oldblockJson);
                    addedBlock.id = 0;
                    _dbContext.Blockchains2.Add(addedBlock);
                }
                else
                {
                    var addedBlock = JsonConvert.DeserializeObject<Block3>(oldblockJson);
                    addedBlock.id = 0;
                    _dbContext.Blockchains3.Add(addedBlock);
                }
                addedChain.Add(item);
            }
            _dbContext.SaveChanges();
            Chain = addedChain;
            return addedChain;
        }

        public static async Task Validate(Transaction transaction)
        {
            var transactionStrings = JsonConvert.SerializeObject(transaction, Formatting.Indented);
            Console.WriteLine($"New Transaction : {transactionStrings} \n");
            
            float moneyLeft = 999999999.00f;
            User fromUser = new User();

            if (!IsDonatable)
            {
                fromUser = _dbContext.Users.FirstOrDefault(x => x.Username == transaction.FromAddress);
                moneyLeft = fromUser.RemainingMoney;
            }

            if (moneyLeft < transaction.Amount)
            {
                Console.WriteLine("Money Not Enough!");
            }
            else
            {
                if (!IsDonatable)
                {
                    Console.WriteLine("User's Money : " + JsonConvert.SerializeObject(fromUser));
                    fromUser.RemainingMoney -= transaction.Amount;
                }

                transaction.HashedTransactionId = _hash.HashTransaction(transaction);
                var serializedTransaction = JsonConvert.SerializeObject(transaction);
                Client.Broadcast($"Verify : {serializedTransaction}");

                while (ConfirmedNumber < RequiredNumber() && ConfirmedNumber + RejectedNumber < NumNode)
                {
                    await Task.Delay(1000);
                }

                if (ConfirmedNumber < RequiredNumber())
                {
                    Console.WriteLine("Rejected Transaction.....");
                    ConfirmedNumber = 1;
                    RejectedNumber = 0;
                }
                else
                {
                    var blockCharName = _dbContext.Foundations.OrderBy(x => x.NameEn).LastOrDefault()?.NameEn;

                    if (transaction.ToAddress == blockCharName)
                    {
                        var foundations = _dbContext.Foundations.FirstOrDefault(x => x.NameEn == transaction.ToAddress);
                        var donatedTransactions = _dbContext.Transactions
                                                            .Where(x => x.IntendedFoundation == blockCharName)
                                                            .ToList();

                        foundations.TotalDonate += foundations.TotalUnDonate;
                        foundations.TotalUnDonate = 0;

                        donatedTransactions.ForEach(x => x.IsDonated = true);
                        transaction.IsDonated = true;
                    }
                    else
                    {
                        var foundations = _dbContext.Foundations.FirstOrDefault(x => x.NameEn == transaction.IntendedFoundation);
                        foundations.TotalUnDonate += transaction.Amount;
                        transaction.FromAddress = _hash.CalculateHash(transaction.FromAddress);
                    }
                    
                    // foundations.TotalDonate += transaction.Amount;
                    _dbContext.Transactions.Add(transaction);
                    _dbContext.SaveChanges();
                    
                    ConfirmedNumber = 1;
                    RejectedNumber = 0;
                    Console.WriteLine("Added Transaction.....");
                    await Task.Delay(5000);
                    
                    BlockGeneric block;
                    BlockGeneric prevBlock;
                    if (Program.Port == 2222)
                    {
                        block = new Block1();
                        prevBlock = _dbContext.Blockchains1
                                            .OrderBy(x => x.UnixTimeStamp)
                                            .LastOrDefault();
                    }
                    else if (Program.Port == 2223)
                    {
                        block = new Block2();
                        prevBlock = _dbContext.Blockchains2
                                            .OrderBy(x => x.UnixTimeStamp)
                                            .LastOrDefault();
                    }
                    else
                    {
                        block = new Block3();
                        prevBlock = _dbContext.Blockchains3
                                            .OrderBy(x => x.UnixTimeStamp)
                                            .LastOrDefault();
                    }
                    
                    var transactions = _dbContext.Transactions
                                                 .Where(x => !x.IsCommitted)
                                                 .OrderBy(x => x.UnixTimeStamp)
                                                 .Take(block.MaxBlockSize)
                                                 .ToList();
                    
                    var hashedTransactions = String.Join('-', transactions.Select(x => x.HashedTransactionId));
                
                    block.HashedTransactionIds = hashedTransactions;
                    block.Height = (prevBlock?.Height + 1) ?? 0;
                    block.PreviousHash = prevBlock?.Hash;
                    block.TransactionJsons = transactions != null ? JsonConvert.SerializeObject(transactions) : null;
                    block.UnixTimeStamp = DateTime.Now.ToUnixTimeStamp();
                    block.Hash = _hash.HashBlock(block);
                    block.VerifiedBy = _hash.CalculateHash(Program.Port.ToString());
                    block.BlockSize = transactions.Count;

                    transactions.ForEach(x => x.BlockHeight = block.Height);

                    var serializedBlock = JsonConvert.SerializeObject(block, Formatting.Indented);
                    Console.WriteLine($"Transaction inside \n {JsonConvert.SerializeObject(transactions, Formatting.Indented)}");
                    Console.WriteLine($"BlockSize : {block.BlockSize}");
                    Console.WriteLine($"Block : {serializedBlock}\n");
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

                        if (Program.Port == 2222)
                            _dbContext.Blockchains1.Add((Block1)block);
                        
                        else if (Program.Port == 2223)
                        _dbContext.Blockchains2.Add((Block2)block);
                        
                        else
                        _dbContext.Blockchains3.Add((Block3)block);
                        
                        await _dbContext.SaveChangesAsync();

                        Client.Broadcast($"Add Block to Chain : {JsonConvert.SerializeObject(block)}");
                        PreDonateToFoundationIfAble(transaction);
                    }
                    ConfirmedNumber = 1;
                    RejectedNumber = 0;
                }
            }
        }

        public static async Task LoadChain()
        {
            Chain =  new List<BlockGeneric>();
            if (Program.Port == 2222)
            {
                var chainDB = await _dbContext.Blockchains1
                                              .OrderBy(x => x.UnixTimeStamp)
                                              .ToListAsync();
            
                Chain.AddRange(chainDB);
            }
            else if (Program.Port == 2223)
            {
                var chainDB = await _dbContext.Blockchains2
                                              .OrderBy(x => x.UnixTimeStamp)
                                              .ToListAsync();
            
                Chain.AddRange(chainDB);
            }
            else
            {
                var chainDB = await _dbContext.Blockchains3
                                              .OrderBy(x => x.UnixTimeStamp)
                                              .ToListAsync();
            
                Chain.AddRange(chainDB);
            }
        }

        public static void PreDonateToFoundationIfAble(Transaction donateTransaction)
        {
            var foundation = _dbContext.Foundations.FirstOrDefault(x => x.NameEn == donateTransaction.IntendedFoundation);
            // Console.WriteLine(JsonConvert.SerializeObject(foundation));
            if (foundation != null && foundation.TotalUnDonate >= foundation.DonateGoal)
            {
                // Console.WriteLine("SSS");
                IsDonatable = true;
                TempFoundation = foundation;
                TempDonateTransaction = donateTransaction;
            }
            // Console.WriteLine("DDDDDD");
        }

        public static async Task DonateToFoundationIfAble()
        {
            var foundation = TempFoundation;
            Transaction donateTransaction = TempDonateTransaction;
            Console.WriteLine($"Transaction : {JsonConvert.SerializeObject(foundation)}");
            var blockCharFoundation = _dbContext.Foundations
                                                .OrderBy(x => x.id)
                                                .LastOrDefault();

            var transaction = new Transaction
                              {
                                  FromAddress = blockCharFoundation?.NameEn,
                                  ToAddress = foundation.NameEn,
                                  UnixTimeStamp = DateTime.Now.ToUnixTimeStamp(),
                                  IntendedFoundation = foundation.NameEn,
                                  Amount = _dbContext.Transactions
                                                     .Where(x => !x.IsDonated
                                                                && x.IntendedFoundation == donateTransaction.IntendedFoundation)
                                                     .Sum(x => x.Amount)
                             };

           
            
            await Validate(transaction);
            TempFoundation = null;
            TempDonateTransaction = null;
            IsDonatable = false;
            Console.WriteLine("Donated Money to project.....");
        }

        public static async Task Coup()
        {
            Console.WriteLine("Be a Leader....");
            CurrentLeader = Program.Port;
            CoolDown = Penalty();
            Server.BroadcastLeader();
            await SignalRConnection.InvokeAsync("AddToGroup", "Donate");
        }

        public static async Task JoinLeaderChannel()
        {
            await SignalRConnection.InvokeAsync("AddToGroup", "Donate");
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
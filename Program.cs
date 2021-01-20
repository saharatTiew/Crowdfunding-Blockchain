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

namespace blockchain
{
    public class Program
    {
        public static int Port = 0;
        public static Server Server = null;
        public static Client Client = new Client();
        public static Blockchain Blockchain = new Blockchain();
        // TODO : fix NumNode Length
        public static int NumNode = 0;
        public static string Name = "Unknown";
        public static string Role = "Unknown";
        public static int CurrentLeader = 0;
        public static int CoolDown = 0;
        public static int Penalty() => ((int)Math.Floor((double) NumNode / 2)) + 1;

        public static void Main(string[] args)
        {
            Blockchain.InitializeChain();
            
            if (args.Length >= 1)
                Port = int.Parse(args[0]);
            if (args.Length >= 2)
                Name = args[1];

            if (Port > 0)
            {
                Server = new Server();
                Server.Start();
            }
            if (Name != "Unknown")
            {
                Console.WriteLine($"Current user is {Name}");
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
                        break;
                    case 12:
                        Console.WriteLine($"CoolDown : {CoolDown}");
                        Console.WriteLine($"Penalty : {Penalty()}");
                        Console.WriteLine($"NumNode : {NumNode}");
                        break;
                }

                Console.WriteLine("Please select an action");
                string action = Console.ReadLine();
                selection = int.Parse(action);
            }

            Client.Close();

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

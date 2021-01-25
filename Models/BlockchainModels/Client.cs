using blockchain.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using WebSocketSharp;
using blockchain.Providers.Interfaces;

namespace blockchain.Models.BlockchainModels
{
    public class Client
    {
        // list of server that this specific client connect to
        private readonly IDictionary<string, WebSocket> wsDict = new Dictionary<string, WebSocket>();
        private StateType CurrentState = StateType.FOLLLOWER;
        private readonly IBlockchainProvider _blockchainProvider;
        private readonly IHashProvider _hashProvider;

        // public Client() { }

        public Client(IBlockchainProvider blockchainProvider, IHashProvider hashProvider)
        {
            _blockchainProvider = blockchainProvider;
            _hashProvider = hashProvider;
        }

        public void Connect(string url)
        {
            if (!wsDict.ContainsKey(url))
            {
                WebSocket ws = new WebSocket(url);
                ws.OnMessage += (sender, e) =>
                {
                    // Console.WriteLine(e.Data);
                    if (e.Data.StartsWith("Hi Client"))
                    {
                        var msgFrom = e.Data.Split(" : ").Last();
                        Console.WriteLine($"You connected with Port {msgFrom}....");
                    }
                    else if (e.Data == "GGGGG")
                    {
                        Console.WriteLine("GGGGG");
                    }
                    else if (e.Data == "Not Applicable For Leader")
                    {
                        CurrentState = StateType.FOLLLOWER;
                        InvokeVote();
                    }
                    else if (e.Data.StartsWith("I'm New Leader"))
                    {
                        Program.CurrentLeader = Convert.ToInt32(e.Data.Split(" : ").Last());
                        CurrentState = StateType.FOLLLOWER;
                        Broadcast($"New Leader : {Program.CurrentLeader}");
                    }
                    else if (e.Data.StartsWith("New Leader"))
                    {
                        int currentLeader = Convert.ToInt32(e.Data.Split(" : ").Last());
                        if (currentLeader != Program.Port)
                        {
                            Program.CurrentLeader = currentLeader;
                            Console.WriteLine($"New Leader is {Program.CurrentLeader}");
                        }
                    }
                    else if (e.Data.StartsWith("Verify :"))
                    {
                        string result = e.Data.Split(" : ").Last();
                        if (result == "Approve")
                        {
                            Program.ConfirmedNumber++;
                            Console.WriteLine("Some Node Approved Transaction.....");
                        }
                        else
                        {
                            Program.RejectedNumber++;
                            Console.WriteLine("Some Node Rejected Transaction......");
                        }
                    }
                    else if (e.Data.StartsWith("Transaction Verify :"))
                    {
                        string result = e.Data.Split(" : ").Last();
                        if (result == "Approve")
                        {
                            Program.ConfirmedNumber++;
                            Console.WriteLine("Some Node Approved Block.....");
                        }
                        else
                        {
                            Program.RejectedNumber++;
                            Console.WriteLine("Some Node Rejected Block......");
                        }
                    }
                    else if (e.Data.StartsWith("Donate :"))
                    {
                        var transactionJson = e.Data.Split(" : ").Last();
                        var transaction = JsonConvert.DeserializeObject<Transaction>(transactionJson);
                        // await Program.Validate(transaction, BackgroundJobClient);
                    }
                    else
                    {
                        List<BlockGeneric> newChain;
                        if (Program.Port == 2222)
                        {
                            var tempChain = JsonConvert.DeserializeObject<List<Block1>>(e.Data);
                            newChain = tempChain.Cast<BlockGeneric>().ToList();
                        }
                        else if (Program.Port == 2223)
                        {
                            var tempChain = JsonConvert.DeserializeObject<List<Block2>>(e.Data);
                            newChain = tempChain.Cast<BlockGeneric>().ToList();
                        }
                        else
                        {
                            var tempChain = JsonConvert.DeserializeObject<List<Block3>>(e.Data);
                            newChain = tempChain.Cast<BlockGeneric>().ToList();
                        }

                        // Console.WriteLine(JsonConvert.SerializeObject(Program.chain, Formatting.Indented));
                        // Console.WriteLine(_blockchainProvider.IsBlockchainValid(newChain));

                        if (_blockchainProvider.IsBlockchainValid(newChain) && newChain.Count > Program.chain.Count)
                        {
                            Console.WriteLine("Updating new chain.....");
                            Program.DeleteChains(newChain);
                        }
                    }
                };
                ws.Connect();
                ws.Send($"Hi Server : {Program.Port}");
                ws.Send(JsonConvert.SerializeObject(Program.chain));
                wsDict.Add(url, ws);
            }
        }

        public void Send(string url, string data)
        {
            foreach (var item in wsDict)
            {
                if (item.Key == url)
                {
                    item.Value.Send(data);
                }
            }
        }

        public void Broadcast(string data)
        {
            foreach (var item in wsDict)
            {
                item.Value.Send(data);
            }
        }

        public void InvokeVote()
        {
            Random rnd = new Random();
            var nextLeader = wsDict.ElementAt(rnd.Next(0, wsDict.Count)).Value;
            nextLeader.Send("You Are Next Leader");
        }

        public void InvokeDonate(Transaction transaction)
        {
            Random rnd = new Random();
            var nextLeader = wsDict.ElementAt(rnd.Next(0, wsDict.Count)).Value;
            var transactionJson = JsonConvert.SerializeObject(transaction);
            Console.WriteLine(transactionJson);
            nextLeader.Send($"Donate : {transactionJson}");
        }

        public IList<string> GetServers()
        {
            IList<string> servers = new List<string>();
            foreach (var item in wsDict)
            {
                servers.Add(item.Key);
            }
            return servers;
        }

        public void Close()
        {
            foreach (var item in wsDict)
            {
                item.Value.Close();
            }
        }
    }
}

using blockchain.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using WebSocketSharp;

namespace blockchain.Models.BlockchainModels
{
    public class Client
    {
        // list of server that this specific client connect to
        private readonly IDictionary<string, WebSocket> wsDict = new Dictionary<string, WebSocket>();
        // private int voteCount = 0;
        private StateType CurrentState = StateType.FOLLLOWER;
        // private int CurrentLeader = 0;

        public void Connect(string url)
        {
            if (!wsDict.ContainsKey(url))
            {
                WebSocket ws = new WebSocket(url);
                ws.OnMessage += (sender, e) => 
                {
                    Console.WriteLine(e.Data);
                    if (e.Data == "Hi Client")
                    {
                        Console.WriteLine(e.Data);
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
                        // Program.CoolDown--;
                        Broadcast($"New Leader : {Program.CurrentLeader}");
                    }
                    else if (e.Data == "GGGGGG")
                    {
                        Console.WriteLine("GGGGGG");
                    }
                    else if (e.Data.StartsWith("New Leader"))
                    {
                        Console.WriteLine("LNW ZA");
                        int currentLeader = Convert.ToInt32(e.Data.Split(" : ").Last());
                        if (currentLeader != Program.Port)
                        {
                            Program.CurrentLeader = currentLeader;
                            Console.WriteLine("OK");
                        }
                    }
                    // else if (e.Data == "Voted")
                    // {
                    //     Console.WriteLine(e.Data);
                    //     voteCount++;
                    // }
                    // else if (e.Data.StartsWith("Current"))
                    // {
                    //     CurrentLeader = Convert.ToInt32(e.Data.Split(" ").Last());
                    //     CurrentState = StateType.FOLLLOWER;
                    // }
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
                    }
                };
                ws.Connect();
                ws.Send("Hi Server");
                ws.Send(JsonConvert.SerializeObject(Program.Blockchain));
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

        // public async Task InvokeVote()
        // {
        //     Random rnd = new Random();
        //     await Task.Delay(rnd.Next(100, 250));
        //     voteCount++;

        //     CurrentState = StateType.CANDIDATE;
        //     Broadcast("Voting Message");

        //     while (voteCount < (int)Math.Ceiling((double)Program.NumNode / 2))
        //     {
        //         if (CurrentState != StateType.CANDIDATE)
        //         {
        //             break;
        //         }

        //         await Task.Delay(100);
        //     }

        //     voteCount = 0;
            
        //     if (CurrentState == StateType.CANDIDATE)
        //     {
        //         CurrentState = StateType.LEADER;     
        //         Broadcast($"Current {Program.Port}");
        //         while (voteCount < (int)Math.Ceiling((double)Program.NumNode / 2))
        //         {
        //             await Task.Delay(100);
        //         }
        //     }  
        // }

        public void InvokeVote()
        {
            Random rnd = new Random();
            var nextLeader = wsDict.ElementAt(rnd.Next(0, wsDict.Count)).Value;
            nextLeader.Send("You Are Next Leader");
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
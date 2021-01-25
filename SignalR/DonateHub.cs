using System;
using System.Threading.Tasks;
using blockchain.Models.BlockchainModels;
using Microsoft.AspNetCore.SignalR;

namespace blockchain.SignalR
{
    public class DonateHub : Hub<IHubProvider>
    {
        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task NotifyLeader(string leaderPort)
        {
            await Clients.Group("Donate").NotifyLeader(leaderPort);
        }

        public async Task DonateToFoundation(Transaction transaction)
        {
            Console.WriteLine("Starting Donate to foundation.....");
            await Clients.Group("Donate").SendTransaction(transaction);
        }
    }
}
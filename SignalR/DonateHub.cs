using System;
using System.Threading.Tasks;
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
    }
}
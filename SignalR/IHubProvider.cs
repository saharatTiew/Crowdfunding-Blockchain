using System.Threading.Tasks;
using blockchain.Models.BlockchainModels;
using Microsoft.AspNetCore.SignalR;

namespace blockchain.SignalR
{
    public interface IHubProvider
    {
        Task AddToGroup(string groupName);
        Task RemoveFromGroup(string groupName);
        Task NotifyLeader(string leaderPort);
        Task AskLeader();
        Task SendTransaction(Transaction transactionJson);
    }
}
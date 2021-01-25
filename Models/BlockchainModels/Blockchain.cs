using System;
using System.Collections.Generic;
using System.Text;
using blockchain.Extensions;

namespace blockchain.Models.BlockchainModels
{
    public class Blockchain
    {
        public IList<Transaction> PendingTransactions { get; set; } = new List<Transaction>();
        public IList<BlockGeneric> Chain { get; set; }
        public int Difficulty { get; set; } = 2;
        public int Reward { get; set; } = 1; //1 cryptocurrency

        // public void InitializeChain()
        // {
        //     Chain = new List<BlockGeneric>();
        //     AddGenesisBlock();
        // }

        // public BlockGeneric CreateGenesisBlock()
        // {
        //     BlockGeneric block = new BlockGeneric(DateTime.Now.ToUnixTimeStamp(), null, PendingTransactions);
        //     block.Mine(Difficulty);
        //     PendingTransactions = new List<Transaction>();
        //     return block;
        // }

        // public void AddGenesisBlock()
        // {
        //     Chain.Add(CreateGenesisBlock());
        // }

        public BlockGeneric GetLatestBlock()
        {
            return Chain[Chain.Count - 1];
        }

        // public void CreateTransaction(Transaction transaction)
        // {
        //     PendingTransactions.Add(transaction);
        // }
        
        // public void ProcessPendingTransactions(string minerAddress)
        // {
        //     BlockGeneric block = new BlockGeneric(DateTime.Now.ToUnixTimeStamp(), GetLatestBlock().Hash, PendingTransactions);
        //     AddBlock(block);

        //     PendingTransactions = new List<Transaction>();
        //     CreateTransaction(new Transaction(null, minerAddress, Reward, DateTime.Now.ToUnixTimeStamp()));
        // }

        // public void AddBlock(BlockGeneric block)
        // {
        //     BlockGeneric latestBlock = GetLatestBlock();
        //     block.Height = latestBlock.Height + 1;
        //     block.PreviousHash = latestBlock.Hash;
        //     block.Mine(this.Difficulty);
        //     Chain.Add(block);
        // }

        // public bool IsValid()
        // {
        //     for (int i = 1; i < Chain.Count; i++)
        //     {
        //         BlockGeneric currentBlock = Chain[i];
        //         BlockGeneric previousBlock = Chain[i - 1];

        //         if (currentBlock.Hash != currentBlock.CalculateHash())
        //         {
        //             return false;
        //         }

        //         if (currentBlock.PreviousHash != previousBlock.Hash)
        //         {
        //             return false;
        //         }
        //     }
        //     return true;
        // }

        // public int GetBalance(string address)
        // {
        //     int balance = 0;

        //     for (int i = 0; i < Chain.Count; i++)
        //     {
        //         for (int j = 0; j < Chain[i].Transactions.Count; j++)
        //         {
        //             var transaction = Chain[i].Transactions[j];

        //             if (transaction.FromAddress == address)
        //             {
        //                 balance -= transaction.Amount;
        //             }

        //             if (transaction.ToAddress == address)
        //             {
        //                 balance += transaction.Amount;
        //             }
        //         }
        //     }

        //     return balance;
        // }
    }
}

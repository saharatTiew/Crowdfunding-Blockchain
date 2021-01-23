using System;
using blockchain.Models.BlockchainModels;

namespace blockchain.Providers.Interfaces
{
    public interface IHashProvider
    {
       string CalculateHash(string message);
    //    void Mine(Block block, int difficulty);
       string HashTransaction(Transaction transaction);
       string HashBlock(BlockGeneric block);
    }
}
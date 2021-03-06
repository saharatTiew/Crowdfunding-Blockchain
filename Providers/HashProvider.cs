using System;
using System.Security.Cryptography;
using System.Text;
using blockchain.Models.BlockchainModels;
using blockchain.Providers.Interfaces;
using Newtonsoft.Json;

namespace blockchain.Providers
{
    public class HashProvider : IHashProvider
    {
        public string CalculateHash(string message)
        {
            SHA256 sha256 = SHA256.Create();

            byte[] inputBytes = Encoding.ASCII.GetBytes(message);
            byte[] outputBytes = sha256.ComputeHash(inputBytes);

            return Convert.ToBase64String(outputBytes);
        }

        public string HashTransaction(Transaction transaction)
        {
            return CalculateHash($"{transaction.FromAddress}-{transaction.ToAddress}-{transaction.Amount}-{transaction.UnixTimeStamp}");
        }

        public string HashBlock(BlockGeneric block)
        {
            return CalculateHash($"{block.UnixTimeStamp}-{block.PreviousHash ?? ""}-{block.HashedTransactionIds}");
        }

        // public void Mine(Block block, int difficulty)
        // {
        //     var leadingZeros = new string('0', difficulty);
        //     while (block.Hash == null || block.Hash.Substring(0, difficulty) != leadingZeros)
        //     {
        //         block.Nonce++;
        //         block.Hash = CalculateHash($"{block.UnixTimeStamp}-{block.PreviousHash ?? ""}-{JsonConvert.SerializeObject(block.Transactions)}-{block.Nonce}");
        //     }
        // }
    }
}
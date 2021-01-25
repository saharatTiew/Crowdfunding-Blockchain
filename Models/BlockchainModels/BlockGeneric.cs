using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;

namespace blockchain.Models.BlockchainModels
{
    public abstract class BlockGeneric
    {
        public long id { get; set; }
        public int Height { get; set; }
        public int UnixTimeStamp { get; set; }
        public int BlockSize { get; set; }
        [NotMapped]
        public int MaxBlockSize { get; set; } = 10;
        public string PreviousHash { get; set; }
        public string Hash { get; set; }
        public string VerifiedBy { get; set; }
        public string TransactionJsons { get; set; }

        [NotMapped, JsonIgnore]
        public IList<Transaction> Transactions { get; set; }

        public BlockGeneric()
        {

        }

        public BlockGeneric(int timeStamp, string previousHash, IList<Transaction> transactions)
        {
            Height = 0;
            UnixTimeStamp = timeStamp;
            PreviousHash = previousHash;
            Transactions = transactions;
        }

        public string CalculateHash()
        {
            SHA256 sha256 = SHA256.Create();
            var nonce = 0;
            byte[] inputBytes = Encoding.ASCII.GetBytes($"{UnixTimeStamp}-{PreviousHash ?? ""}-{JsonConvert.SerializeObject(Transactions)}-{nonce}");
            byte[] outputBytes = sha256.ComputeHash(inputBytes);

            return Convert.ToBase64String(outputBytes);
        }

        public void Mine(int difficulty)
        {
            var leadingZeros = new string('0', difficulty);
            var nonce = 0;
            while (this.Hash == null || this.Hash.Substring(0, difficulty) != leadingZeros)
            {
                nonce++;
                this.Hash = this.CalculateHash();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using blockchain.Providers.Interfaces;
using Newtonsoft.Json;

namespace blockchain.Models.BlockchainModels
{
    public class Transaction
    {   
        [JsonIgnore]
        public long id { get; set; }
        public string HashedTransactionId { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public int UnixTimeStamp { get; set; }
        public string IntendedFoundation { get; set; }
        public bool IsDonated { get; set; }

        [JsonIgnore]
        public bool IsCommitted { get; set; }
        public float Amount { get; set; }
        private readonly IHashProvider _hashProvider;

        public Transaction()
        {

        }

        public Transaction(IHashProvider hash)
        {
            _hashProvider = hash;
        }

        public Transaction(string fromAddress, string toAddress, int amount, int timestamp)
        {
            FromAddress = fromAddress;
            ToAddress = toAddress;
            Amount = amount;
            UnixTimeStamp = timestamp;
        }

        // public void HashTransaction()
        // {
        //     this.HashedTransactionId = _hashProvider.CalculateHash($"{this.FromAddress}-{this.ToAddress}-{this.Amount}-{this.UnixTimeStamp}");
        // }
    }
}

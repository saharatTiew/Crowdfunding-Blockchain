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
        public string HashedTransactionIds { get; set; }
    }
}

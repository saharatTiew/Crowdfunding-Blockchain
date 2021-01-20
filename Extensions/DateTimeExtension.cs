using System;
using System.Security.Cryptography;
using System.Text;
using blockchain.Models.BlockchainModels;
using blockchain.Providers.Interfaces;
using Newtonsoft.Json;

namespace blockchain.Extensions
{
    public static class DateTimeExtensions
    {
        public static int ToUnixTimeStamp(this DateTime dateTime)
        {
            return (int)(dateTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
    }
}

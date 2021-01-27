using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using blockchain.Data;
using blockchain.Models.BlockchainModels;
using blockchain.Providers.Interfaces;
using Newtonsoft.Json;

namespace blockchain.Providers
{
    public class BlockchainProvider : IBlockchainProvider
    {
        private readonly IHashProvider _hash;

        public BlockchainProvider(IHashProvider hash)
        {
            _hash = hash;
        }
        public bool IsBlockchainValid(IList<BlockGeneric> blockchain)
        {
            for (int i = 1; i < blockchain.Count; i++)
            {
                BlockGeneric currentBlock = blockchain[i];
                BlockGeneric previousBlock = blockchain[i - 1];

                if (currentBlock.Hash != _hash.HashBlock(currentBlock))
                {
                    Console.WriteLine($"The hash of the block is invalid at height {i}");
                    return false;
                }

                if (currentBlock.PreviousHash != previousBlock.Hash)
                {
                    Console.WriteLine($"The previous hash of the block is invalid at height {i-1}");
                    return false;
                }
            }
            return true;
        }

        public bool IsBlockValid(BlockGeneric prevBlock, BlockGeneric newBlock)
        {
            return newBlock.PreviousHash == prevBlock.Hash;
        }
    }
}


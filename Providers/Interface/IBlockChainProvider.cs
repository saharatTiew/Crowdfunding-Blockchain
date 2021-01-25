using System;
using System.Collections.Generic;
using blockchain.Models.BlockchainModels;

namespace blockchain.Providers.Interfaces
{
    public interface IBlockchainProvider
    {
        bool IsBlockchainValid(IList<BlockGeneric> block);
        bool IsBlockValid(BlockGeneric prevBlock, BlockGeneric newBlock);
    }
}
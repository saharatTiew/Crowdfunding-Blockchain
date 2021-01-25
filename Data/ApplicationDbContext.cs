using blockchain.Models;
using blockchain.Models.BlockchainModels;
using Microsoft.EntityFrameworkCore;

namespace blockchain.Data
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> option) : base(option) { }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Foundation> Foundations { get; set; }
        public DbSet<User> Users { get; set; }
        // public DbSet<BlockGeneric> Blockchains { get; set; }
        public DbSet<Block1> Blockchains1 { get; set; }
        public DbSet<Block2> Blockchains2 { get; set; }
        public DbSet<Block3> Blockchains3 { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Transaction>()
                   .ToTable("Transactions");

            builder.Entity<Foundation>()
                   .ToTable("Foundations");

            builder.Entity<User>()
                   .ToTable("Users");

            // builder.Entity<BlockGeneric>()
            //        .ToTable("BlockchainsValidator1");
            builder.Entity<Block1>()
                   .ToTable("BlockchainsValidator1");

            builder.Entity<Block2>()
                   .ToTable("BlockchainsValidator2");
            
            builder.Entity<Block3>()
                   .ToTable("BlockchainsValidator3");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }

    }
}
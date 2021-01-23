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
        public DbSet<BlockGeneric> Blockchains { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Transaction>()
                   .ToTable("Transactions");

            builder.Entity<Foundation>()
                   .ToTable("Foundations");

            builder.Entity<User>()
                   .ToTable("Users");

            builder.Entity<BlockGeneric>()
                   .ToTable("BlockchainsValidator1");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }

    }
}
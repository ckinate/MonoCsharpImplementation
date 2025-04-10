using Microsoft.EntityFrameworkCore;

namespace MonoIntegrationNew.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<MonoLinkingRequest> MonoLinkingRequests { get; set; }
        public DbSet<MonoAccount> MonoAccounts { get; set; }
        public DbSet<MonoTransaction> MonoTransactions { get; set; }

     

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Add any model configurations here
            modelBuilder.Entity<MonoLinkingRequest>()
                .HasIndex(r => r.Reference)
                .IsUnique();
            modelBuilder.Entity<MonoAccount>()
               .HasIndex(a => a.MonoAccountId)
               .IsUnique();
            //modelBuilder.Entity<MonoTransaction>()
            // .HasIndex(t => t.TransactionId)
            // .IsUnique();
            // Added New
            modelBuilder.Entity<MonoLinkingRequest>()
            .HasOne(m => m.Account)
            .WithOne(a => a.LinkingRequest)
            .HasForeignKey<MonoAccount>(a => a.LinkingRequestId);

            // MonoAccount ↔ MonoTransaction (One-to-Many)
            modelBuilder.Entity<MonoTransaction>()
                .HasOne(t => t.Account)
                .WithMany(a => a.Transactions) 
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}

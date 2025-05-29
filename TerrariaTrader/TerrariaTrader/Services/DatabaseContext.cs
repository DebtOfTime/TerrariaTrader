using System.Data.Entity;
using TerrariaMarketplace.Models;

namespace TerrariaMarketplace.Services
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext() : base("name=TerrariaMarketConnection") { }

        public DbSet<User> Users { get; set; }
        public DbSet<Merchant> Merchants { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<MerchantItem> MerchantItems { get; set; }
        public DbSet<UserMerchantRelation> UserMerchantRelations { get; set; }
        public DbSet<AccessLevel> AccessLevels { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MerchantItem>()
                .HasRequired(mi => mi.Merchant)
                .WithMany(m => m.MerchantItems)
                .HasForeignKey(mi => mi.MerchantID);

            modelBuilder.Entity<MerchantItem>()
                .HasRequired(mi => mi.Item)
                .WithMany(i => i.MerchantItems)
                .HasForeignKey(mi => mi.ItemID);

            modelBuilder.Entity<UserMerchantRelation>()
                .HasRequired(um => um.User)
                .WithMany(u => u.UserMerchantRelations)
                .HasForeignKey(um => um.UserID);

            modelBuilder.Entity<UserMerchantRelation>()
                .HasRequired(um => um.Merchant)
                .WithMany(m => m.UserMerchantRelations)
                .HasForeignKey(um => um.MerchantID);

            modelBuilder.Entity<Cart>()
                .HasRequired(c => c.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(c => c.UserID);

            modelBuilder.Entity<Order>()
                .HasRequired(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserID);
        }
    }
}

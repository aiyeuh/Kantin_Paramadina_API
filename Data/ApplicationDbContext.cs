using Microsoft.EntityFrameworkCore;

namespace Kantin_Paramadina.Model
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Outlet> Outlets { get; set; } = null!;
        public DbSet<MenuItem> MenuItems { get; set; } = null!;
        public DbSet<Stock> Stocks { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<TransactionItem> TransactionItems { get; set; } = null!;
        public DbSet<User> Users { get; set; }
        public DbSet<UserToken> UserToken { get; set; } = null;
        
    }
}

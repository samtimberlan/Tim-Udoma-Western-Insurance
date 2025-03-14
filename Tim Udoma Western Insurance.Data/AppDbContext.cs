using Microsoft.EntityFrameworkCore;
using Tim_Udoma_Western_Insurance.Data.Models;

namespace Tim_Udoma_Western_Insurance.Data
{
    public class AppDbContext :DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
                
        }

        public DbSet<Buyer> Buyers { get; set; }
        public DbSet<Product> Products { get; set; }
    }
}

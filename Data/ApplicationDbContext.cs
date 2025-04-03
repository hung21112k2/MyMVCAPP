using Microsoft.EntityFrameworkCore;
using MyMvcApp.Models;

namespace MyMvcApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().ToTable("Product");
        }
    }

    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                if (context.Products.Any())
                {
                    return;
                }

                context.Products.AddRange(
                    new Product { Name = "Laptop", Price = 15000000, Description = "Máy tính xách tay cao cấp" },
                    new Product { Name = "Điện thoại", Price = 8000000, Description = "Smartphone đời mới" },
                    new Product { Name = "Tai nghe", Price = 1200000, Description = "Tai nghe không dây" }
                );

                context.SaveChanges();
            }
        }
    }
}
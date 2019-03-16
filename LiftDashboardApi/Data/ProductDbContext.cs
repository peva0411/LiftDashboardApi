using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LiftDashboardApi.Data
{
  public class ProductDbContext : DbContext
  {
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {

    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Review> Reviews { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Product>().ToTable("Product");
      modelBuilder.Entity<Review>().ToTable("Review");
      modelBuilder.Entity<Review>().HasOne(r => r.Product).WithMany(p => p.Reviews).HasForeignKey(r => r.Asin).HasPrincipalKey(p => p.Asin);

      modelBuilder.Entity<ClientProduct>()
        .HasOne<Client>(c => c.Client)
        .WithMany(c => c.ClientProducts)
        .HasForeignKey(c => c.ClientId);

      modelBuilder.Entity<ClientProduct>()
        .HasOne(c => c.Product)
        .WithMany(c => c.ClientProducts)
        .HasForeignKey(c => c.Asin).HasPrincipalKey(p => p.Asin);
    }
  }

  public class Client
  {
    public int ClientId { get; set; }
    public string Name { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<ClientProduct> ClientProducts { get; set; }
  }

  public class ClientProduct
  {
    public int ClientProductId { get; set; }
    public int ClientId { get; set; }
    public string Asin { get; set; }
    public Client Client { get; set; }
    public Product Product { get; set; }
  }

  public class Product
  {
    public int ProductId { get; set; }
    public string Asin { get; set; }
    public string Title { get; set; }
    public decimal? Cost { get; set; }
    public decimal? Msrp  { get; set;}
    public decimal? PriceChange { get; set; }
    public decimal? LastPrice { get; set; }

    public ICollection<ClientProduct> ClientProducts { get; set; }
    public ICollection<Review> Reviews { get; set; }
  }

  public class Review
  {
    public int ReviewId { get; set; }
    public string Asin { get; set; }
    public string Title { get; set; }
    public string Text { get; set; }
    public decimal Rating { get; set; }
    public string Author { get; set; }
    public Product Product { get; set; }
    public DateTime Date { get; set; }
  }
}

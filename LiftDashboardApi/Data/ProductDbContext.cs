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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Product>().ToTable("Product");
    }
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
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiftDashboardApi.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LiftDashboardApi.Controllers
{
  public class Index
  {
    public class Query : IRequest<Result>
    {
    }


    public class Result
    {

      public List<Indicator> Indicators { get; set; }
      public List<ProductDetail> ProductDetails { get; set; }

      public class Indicator
      {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Count { get; set; }
      }

      public class ProductDetail
      {
        public string Client { get; set; }
        public string Name { get; set; }
        public string Asin { get; set; }
        public decimal Price  { get; set; }
        public decimal Msrp { get; set; }
      }
    }


    public class Handler : RequestHandler<Query, Result>
    {
      private readonly ProductDbContext _db;

      public Handler(ProductDbContext db)
      {
        _db = db;
      }

      protected override Result Handle(Query request)
      {
        var belowMsrpProducts = _db.Products
          .Include(p => p.ClientProducts)
          .ThenInclude(p => p.Client)
          .Where(p => p.LastPrice.HasValue && p.Msrp.HasValue && p.LastPrice < p.Msrp).ToList();


        return new Result()
        {
          Indicators = new List<Result.Indicator>
          {
            new Result.Indicator(){Title = "Below MSRP", Description = "Products with last reported price below MSRP", Count = belowMsrpProducts.Count}, 
            new Result.Indicator(){Title = "3P Sellers", Description = "Third Party Sellers", Count = 10},
            new Result.Indicator(){Title = "Bad Reviews", Description = "Some bad reviews", Count = 42}
          },
          ProductDetails = belowMsrpProducts.Select(m => new Result.ProductDetail
          {
            Asin = m.Asin,
            Client = m.ClientProducts.FirstOrDefault().Client.Name,
            Msrp = m.Msrp.Value,
            Price = m.LastPrice.Value,
            Name = m.Title
          }).ToList()
        };
       
      }
    }
  }

}

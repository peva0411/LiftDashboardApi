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
      public List<BadReview> BadReviews { get; set; }
      public ChangeEventSummary ChangeEvent { get; set; }

      public class ChangeEventSummary
      {
          public List<PriceChange> ProductPriceChangeEvents { get; set; }
          public List<BuyBoxOwnerChange> BuyBoxOwnerChangeEvents { get; set; }
      }

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

      public class BadReview
      {
        public string Asin { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public decimal Rating { get; set; }
        public string ProductName { get; set; }
        public DateTime Date { get; set; }
        public string FormattedDate { get; set; }
      }

      public class PriceChange
      {
          public string Asin { get; set; }
          public string ProductName { get; set; }

          public string Client { get; set; }
          public decimal OldPrice { get; set; }
          public decimal NewPrice { get; set; }
          public DateTime DateChanged { get; set; }
      }

      public class BuyBoxOwnerChange
      {
          public string Asin { get; set; }
          public string ProductName { get; set; }

          public string Client { get; set; }
          public string OldOwner { get; set; }
          public string NewOwner { get; set; }
          public DateTime DateChanged { get; set; }
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

        var monthAgo = DateTime.Now.AddDays(-30);
        var badReviews = _db.Reviews.Include(r => r.Product)
          .Where(r => r.Rating <= 2.0M && r.Date > monthAgo)
          .OrderByDescending(r => r.Date)
          .ToList();

        var buyBoxOwnerChangeEvents = _db.BuyBoxOwnerChangeEvents.Where(b => b.DateChanged >= monthAgo).ToList();
        var priceChangeEvents = _db.ProductPriceChangeEvents.Where(p => p.DateChanged >= monthAgo).ToList();
        var totalEventCount = buyBoxOwnerChangeEvents.Count + priceChangeEvents.Count;

        var productPriceChangeInfoes = _db.Products.Include(p => p.ClientProducts)
            .ThenInclude(p => p.Client)
            .Where(p => priceChangeEvents.Select(pp => pp.Asin).Contains(p.Asin) || buyBoxOwnerChangeEvents.Select(b => b.Asin).Contains(p.Asin));



        var priceChangeEventDtos = priceChangeEvents.Join(productPriceChangeInfoes, p => p.Asin, i => i.Asin, (@event, product) =>
            new Result.PriceChange
            {
                Asin = @event.Asin,
                ProductName = product.Title,
                Client = product.ClientProducts.FirstOrDefault().Client.Name,
                DateChanged = @event.DateChanged,
                OldPrice = @event.OldPrice,
                NewPrice = @event.NewPrice
            }).ToList();

        var buyBoxChangeEventDtos = buyBoxOwnerChangeEvents.Join(productPriceChangeInfoes, b => b.Asin, i => i.Asin,
            (@event, product) => new Result.BuyBoxOwnerChange
            {
                Asin = @event.Asin,
                ProductName = product.Title,
                Client = product.ClientProducts.FirstOrDefault().Client.Name,
                DateChanged = @event.DateChanged,
                OldOwner = @event.OldOwner,
                NewOwner = @event.NewOwner
            }).ToList();

        return new Result()
        {
          Indicators = new List<Result.Indicator>
          {
            new Result.Indicator(){Title = "Below MSRP", Description = "Products with last reported price below MSRP", Count = belowMsrpProducts.Count}, 
            new Result.Indicator(){Title = "Change Events", Description = "Price or Buy Box Owner changes", Count = totalEventCount},
            new Result.Indicator(){Title = "Bad Reviews", Description = "Reviews with rating below 3 stars within last 30 days", Count = badReviews.Count}
          },
          ChangeEvent = new Result.ChangeEventSummary
          {
            ProductPriceChangeEvents = priceChangeEventDtos,
            BuyBoxOwnerChangeEvents = buyBoxChangeEventDtos
          },
          ProductDetails = belowMsrpProducts.Select(m => new Result.ProductDetail
          {
            Asin = m.Asin,
            Client = m.ClientProducts.FirstOrDefault().Client.Name,
            Msrp = m.Msrp.Value,
            Price = m.LastPrice.Value,
            Name = m.Title
          }).ToList(),
          BadReviews = badReviews.Select(b => new Result.BadReview
          {
            Asin = b.Asin, 
            Title = b.Title,
            ProductName = b.Product.Title,
            Date = b.Date,
            FormattedDate = b.Date.ToShortDateString(),
            Rating = b.Rating,
            Author = b.Author,
            Text = b.Text
          }).ToList()
        };
       
      }
    }
  }

}

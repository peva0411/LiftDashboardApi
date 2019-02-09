using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiftDashboardApi.Data;
using MediatR;

namespace LiftDashboardApi.Controllers
{
  public class Index
  {
    public class Query : IRequest<Result>
    {
    }


    public class Result
    {

      public Summary DashboardSummary { get; set; }

      public class Summary
      {
        public int Count { get; set; }
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
        var count = _db.Products.Where(p => p.LastPrice.HasValue && p.Msrp.HasValue).Count(p => p.LastPrice < p.Msrp);
        return new Result { DashboardSummary = new Result.Summary() { Count = count } };
      }
    }
  }

}

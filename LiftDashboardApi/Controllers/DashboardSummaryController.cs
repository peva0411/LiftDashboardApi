using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LiftDashboardApi.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class DashboardSummaryController : ControllerBase
  {
    private IMediator _mediator;
    public DashboardSummaryController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
      var summary = await _mediator.Send(new Index.Query());

      return Ok(summary);
    }
  }
}

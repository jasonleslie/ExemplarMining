using Exemplar_Mining.Extensions;
using Exemplar_Mining.Models;
using Exemplar_Mining.Models.DTOs;
using Exemplar_Mining.Models.Responses;
using Exemplar_Mining.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Exemplar_Mining.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly IActionDescriptorCollectionProvider _actionCollection;
        private readonly ISearcher _searcher;

        public ReportsController(DBContext context, IActionDescriptorCollectionProvider actionCollection, ISearcher searcher)
        {
            _context = context;
            _actionCollection = actionCollection;
            _searcher = searcher;
        }


        /*
        *   Returns a list of available reports
        */
        [HttpGet]
        public IActionResult GetReports()
        {
            var baseurl = this.Request.Scheme + "://" + this.Request.Host;

            var routes = _actionCollection.ActionDescriptors.Items.Where(x => x.RouteValues["Controller"] == "Reports").Select(x => new
            {
                Action = x.RouteValues["Action"],
                Location = baseurl + "/" + x.AttributeRouteInfo.Template
            }).ToList();

            return Ok(routes);
        }

        /*
        *   Returns a list of Mine Overseers with the count of mines they oversee.
        */
        [HttpGet]
        [Route("overseers")]
        public async Task<ActionResult<IEnumerable<ReportOverseerDTO>>> GetMineOverseers()
        {
            return await _context.Employee.Where(x => x.Mines.Count > 0).Select(x => new ReportOverseerDTO
            {
                Name = x.FirstName + " " + x.LastName,
                Position = x.Position,
                MineCount = x.Mines.Count
            }).ToListAsync();
        }

        /*
        *   Returns a list of Mine types (type of resource e.g. gold) with the count of mines of said type.
        */
        [HttpGet]
        [Route("resources")]
        public async Task<ActionResult<IEnumerable<ReportResourceDTO>>> GetMineTypes()
        {
            return await _context.Resource.Where(x => x.Mines.Count > 0).Select(x => new ReportResourceDTO
            {
                Type = x.Type,
                Value = x.Value,
                MineCount = x.Mines.Count
            }).ToListAsync();
        }

        /*
        *   Returns the average amount of production a specific mine
        */
        [HttpGet("productionaverage/{mineID:int?}")]
        public async Task<ActionResult<decimal>> GetAverageProductionByID(int mineID)
        {

            var amounts = await _context.Production.WhereIf(mineID != 0, x => x.MineId == mineID).Select(x => x.Amount).ToListAsync();

            if (amounts.Count == 0)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = String.Format("The given id '{0}' did not match any known mine", mineID)
                });
            }

            //If very large this could be blocking for long time
            return Ok(new BaseAPIResponse()
            {
                message = Math.Round(amounts.Average(), 3).ToString()
            });
        }

        /*
        *   Returns the sum of the amounts of production a specific mine
        */
        [HttpGet("productionsum/{mineID:int?}")]
        public async Task<ActionResult<decimal>> GetSumProductionByID(int mineID)
        {
            var amounts = await _context.Production.WhereIf(mineID != 0, x => x.MineId == mineID).Select(x => x.Amount).ToListAsync();

            if (amounts.Count == 0)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = String.Format("The given id '{0}' did not match any known mine", mineID)
                });
            }

            //If very large this could be blocking for long time
            return Ok(new BaseAPIResponse()
            {
                message = amounts.Sum().ToString()
            });
        }

        /*
        *   Returns the average amount of production a specific mine
        */
        [HttpGet("productionaverage/{mineName}")]
        public async Task<ActionResult<decimal>> GetAverageProductionByName(string mineName)
        {

            var mine = _searcher.SearchMine(mineName).Result;

            if (mine == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = "Cannot find a mine with the given name of " + mineName + "."
                });
            }

            var amounts = await _context.Production.WhereIf(mineName != null, x => x.MineId == mine.MineId).Select(x => x.Amount).ToListAsync();

            //If very large this could be blocking for long time
            return Ok(new BaseAPIResponse()
            {
                message = Math.Round(amounts.Average(), 3).ToString()
            });
        }

        /*
        *   Returns the sum of the amounts of production a specific mine
        */
        [HttpGet("productionsum/{mineName}")]
        public async Task<ActionResult<decimal>> GetSumProductionByName(string mineName)
        {

            var mine = _searcher.SearchMine(mineName).Result;

            if (mine == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = "Cannot find a mine with the given name of " + mineName + "."
                });
            }

            var amounts = await _context.Production.WhereIf(mineName != null, x => x.MineId == mine.MineId).Select(x => x.Amount).ToListAsync();

            //If very large this could be blocking for long time
            return Ok(new BaseAPIResponse()
            {
                message = amounts.Sum().ToString()
            });
        }

        /*
        *   Returns a report on the revenues of all mines grouped by Overseer/Employee.
        */
        [HttpGet]
        [Route("highrevenue")]
        public async Task<ActionResult<IEnumerable<RevenueDTO>>> GetHighestRevenue()  //?? named correctly
        {
            var revenue = await
                         (from emp in _context.Employee
                          join mine in _context.Mine
                            on emp.EmpId equals mine.OverseerId
                          join res in _context.Resource
                            on mine.Type equals res.Type
                          join product in _context.Production
                            on mine.MineId equals product.MineId
                          select new
                          {
                              name = emp.FirstName + " " + emp.LastName,
                              position = emp.Position,
                              mineid = mine.MineId,
                              amount = product.Amount,
                              value = res.Value,
                              date = product.Datelogged
                          }).ToListAsync();


            return
                revenue.AsEnumerable().GroupBy(x => x.name).Select(x => new RevenueDTO
                {
                    EmployeeName = x.First().name,
                    Position = x.First().position,
                    MineAverage = Math.Round(x.Average(z => z.amount), 2),
                    MineCount = x.Select(x => x.mineid).Distinct().Count(),
                    TotalRevenue = x.Sum(z => z.amount)
                })
            .OrderByDescending(x => x.TotalRevenue).ToList();

        }



    }
}

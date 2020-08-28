using Exemplar_Mining.Extensions;
using Exemplar_Mining.Models;
using Exemplar_Mining.Models.DTOs;
using Exemplar_Mining.Models.Responses;
using Exemplar_Mining.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public class ProductionController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly ISearcher _searcher;
        public ProductionController(DBContext context, ISearcher searcher)
        {
            _context = context;
            _searcher = searcher;
        }

        /*
         * Returns a list of production data for a specific day ( defaults to current date if value not specified ) optional filter on mine ID.         
         */
        [HttpGet("{mineID:int?}")]
        public async Task<ActionResult<IEnumerable<ProductionDTO>>> GetProductionByID(long mineID, DateTime productionDate)
        {

            productionDate = (productionDate == null) ? DateTime.Now.Date : productionDate;

            var production = await _context.Production.WhereIf(mineID != 0, x => x.MineId == mineID).Where(x => x.Datelogged == productionDate).ToListAsync();

            if (production == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = String.Format("No data found for mine with ID '{0}' for the date of '{1}'.", mineID, productionDate)
                });
            }

            return Ok(production.Select(x => new ProductionDTO()
            {
                MineId = x.MineId,
                Amount = x.Amount,
                Datelogged = x.Datelogged
            }));
        }

        /*
        * Returns a list of production data for a specific day ( defaults to current date if value not specified ) optional filter on mine name.         
        */
        [HttpGet("name/{mineName}")]
        public async Task<ActionResult<IEnumerable<ProductionDTO>>> GetProductionByName(string mineName, DateTime productionDate)
        {

            productionDate = (productionDate == null) ? DateTime.Now.Date : productionDate;

            var mine = _searcher.SearchMine(mineName).Result;

            if (mine == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = "Cannot find a mine with the given name of " + mineName + "."
                });
            }

            var production = await _context.Production.Where(x => x.MineId == mine.MineId && x.Datelogged == productionDate).ToListAsync();

            if (production == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = String.Format("No data found for mine with name '{0}' for the date of '{1}'.", mineName, productionDate)
                });
            }

            return Ok(production.Select(x => new ProductionDTO()
            {
                MineId = x.MineId,
                Amount = x.Amount,
                Datelogged = x.Datelogged
            }));
        }


        /*
        * Returns a list of the latest production data for each mine.
        */
        [HttpGet("last")]
        public async Task<ActionResult<IEnumerable<ProductionDTO>>> GetLatestProduction()
        {

            var production = await
                             (from prod in _context.Production
                              join latest in (
                                                 _context.Production.GroupBy(x => x.MineId).Select(x => new
                                                 {
                                                     MineId = x.Key,
                                                     Datelogged = x.Max(y => y.Datelogged)
                                                 })
                                             )
                              on new { prod.MineId, prod.Datelogged } equals new { latest.MineId, latest.Datelogged }
                              select new
                              {
                                  mineID = prod.MineId,
                                  amount = prod.Amount,
                                  dateLogged = prod.Datelogged
                              })
                             .ToListAsync();

            return base.Ok((object)Enumerable.Select(production, x => (ProductionDTO)new ProductionDTO()
            {
                MineId = x.mineID,
                Amount = x.amount,
                Datelogged = x.dateLogged
            }));
        }


        [HttpPost]
        public async Task<ActionResult> PostProduction(Production production)
        {
            _context.Production.Add(production);
            await _context.SaveChangesAsync();

            return Ok(new BaseAPIResponse() { message = "Production data has been created." });
        }

        private bool ProductionExists(long id)
        {
            return _context.Production.Any(e => e.Id == id);
        }
    }
}

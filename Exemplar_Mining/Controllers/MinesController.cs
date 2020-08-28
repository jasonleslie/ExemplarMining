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
    public class MinesController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly ISearcher _searcher;

        public MinesController(DBContext context, ISearcher searcher)
        {
            _context = context;
            _searcher = searcher;
        }

        /*
        *   Returns a list of mines with optional filtering on resource type e.g. gold.
        */
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Mine>>> GetMines(string resourceType)
        {
            var mines = await _context.Mine.WhereIf(!String.IsNullOrEmpty(resourceType), x => x.Type == resourceType).ToListAsync();

            if (mines == null)
            {

                    return NotFound(new APIResponse()
                    {
                        code = 404,
                        message = "There are no mines with for the resouce of " + resourceType + "."
                    });
   
            }

            return Ok(await Task.FromResult
                (from mine in mines
                 join emp in _context.Employee
                 on mine.OverseerId equals emp.EmpId
                 select new MineDTO()
                 {
                     MineId = mine.MineId,
                     Name = mine.Name,
                     Lattitude = mine.Lattitude,
                     Longitude = mine.Longitude,
                     Type = mine.Type,
                     OverseerName = emp.FirstName + " " + emp.LastName
                 }
                ));
        }


        [HttpGet("{mineID:int}")]
        public async Task<ActionResult<Mine>> GetMineByID(short mineID)
        {

            var mine = await _context.Mine.FindAsync(mineID);

            if (mine == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = "Cannot find a mine with ID of " + mineID + "."
                });
            }
            var employee = await _context.Employee.FindAsync(mine.OverseerId);

            return Ok(new MineDTO()
            {
                MineId = mine.MineId,
                Name = mine.Name,
                Lattitude = mine.Lattitude,
                Longitude = mine.Longitude,
                Type = mine.Type,
                OverseerName = employee.FirstName + " " + employee.LastName
            });

        }


        [HttpGet("name/{mineName}")]
        public async Task<ActionResult<Mine>> GetMineByName(string mineName)
        {

            var mine = await _searcher.SearchMine(mineName);

            if (mine == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = "Cannot find a mine with the given name of " + mineName + "."
                });
            }

            var employee = await _context.Employee.FindAsync(mine.OverseerId);

            return Ok(new MineDTO()
            {
                MineId = mine.MineId,
                Name = mine.Name,
                Lattitude = mine.Lattitude,
                Longitude = mine.Longitude,
                Type = mine.Type,
                OverseerName = employee.FirstName + " " + employee.LastName
            });

        }

        /*
        *   Changes the Overseer of a specific mine to the provided Employee.
        */
        [HttpPatch]
        [Route("~/api/{controller}/overseer")]
        public async Task<ActionResult> SetMineOverseer(string mineName, string employeeName)
        {

            if ((mineName.Length == 0) || (employeeName.Length == 0))
            {
                return new BadRequestObjectResult(new { code = 400, errors = new string[] { "Please provide a valid Mine Name and the Employee Name of the new overseer." } });
            }

            var mine = await _searcher.SearchMine(mineName);

            var employee = await _searcher.SearchEmployee(employeeName);

            if ((mine == null) || (employee == null))
            {
                String responseMessage = "";

                responseMessage += (mine == null) ? String.Format("Mine with name '{0}' was not found. ", mineName) : "";
                responseMessage += (employee == null) ? String.Format("Employee with name '{0}' was not found.", employeeName) : "";

                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = responseMessage
                });
            }

            mine.OverseerId = employee.EmpId;

            await _context.SaveChangesAsync();

            return Ok(new BaseAPIResponse() { message = String.Format("Mine Overseer successfully updated.") });
        }


        [HttpPost]
        public async Task<ActionResult<Mine>> PostMine(Mine mine)
        {
            _context.Mine.Add(mine);
            await _context.SaveChangesAsync();

            return Ok(new BaseAPIResponse() { message = String.Format("Mine '{0}' has been created.", mine.Name) });
        }


        [HttpDelete("{mineID:int}")]
        public async Task<ActionResult<Mine>> DeleteMineByID(short mineID)
        {
            var mine = await _context.Mine.FindAsync(mineID);

            if (mine == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = "Cannot find a mine with ID of " + mineID + "."
                });
            }

            _context.Mine.Remove(mine);
            await _context.SaveChangesAsync();

            return Ok(new BaseAPIResponse() { message = String.Format("Mine '{0}' has been deleted.", mine.Name) });
        }


        [HttpDelete("name/{mineName}")]
        public async Task<ActionResult<Mine>> DeleteMineByName(string mineName)
        {
            var mine = await _searcher.SearchMine(mineName);

            if (mine == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = "Cannot find a mine with name of " + mineName + "."
                });
            }

            _context.Mine.Remove(mine);
            await _context.SaveChangesAsync();

            return Ok(new BaseAPIResponse() { message = String.Format("Mine '{0}' has been deleted.", mine.Name) });
        }

        private bool MineExists(short id)
        {
            return _context.Mine.Any(e => e.MineId == id);
        }
    }
}

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
    public class PerformanceController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly ISearcher _searcher;

        public PerformanceController(DBContext context, ISearcher searcher)
        {
            _context = context;
            _searcher = searcher;
        }

        /*
        *   Returns a list of employees with their varios performance metrics.
        */
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PerformanceDTO>>> GetPerformance()
        {

            return Ok(
                await (
                from pef in _context.Performance
                join emp in _context.Employee
                on pef.EmpId equals emp.EmpId
                select new PerformanceDTO
                {
                    EmployeeName = emp.FirstName + " " + emp.LastName,
                    PerformanceType = pef.PerformanceType,
                    Rating = pef.Rating
                })
                .ToListAsync());

        }

        /*
        *   Returns a list of performance metrics for a specific employee.
        */
        [HttpGet("name/{employeeName}")]
        public async Task<ActionResult<PerformanceDTO>> GetPerformance(string employeeName)
        {

            var employee = await _searcher.SearchEmployee(employeeName);

            if (employee == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = "There is no employee with a name matching " + employeeName + "."
                });
            }

            var performance = await _context.Performance.Where(x => x.EmpId == employee.EmpId).ToListAsync();

            return Ok(performance.Select(x => new PerformanceDTO()
            {
                EmployeeName = employee.FirstName + " " + employee.LastName,
                PerformanceType = x.PerformanceType,
                Rating = x.Rating
            }));
        }

        /*
        *   Creates or updates a performance metric for a specific employee.
        *   When updating, a weighted average is taken between the old and new rating.
        */
        [HttpPut]
        public async Task<ActionResult> PostPerformance(PerformanceDTO performance)
        {

            var employee = await _searcher.SearchEmployee(performance.EmployeeName);

            if (employee == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = "There is no employee with a name matching " + performance.EmployeeName + "."
                });
            }

            var perfrom = await _searcher.SearchPerformance(employee.EmpId, performance.PerformanceType);

            if (perfrom == null)
            {
                _context.Performance.Add(new Performance()
                {
                    EmpId = employee.EmpId,
                    PerformanceType = performance.PerformanceType.ToUpper(),
                    Rating = performance.Rating
                });
            }
            else
            {
                //Check to see if rating was given previously for this type 
                var rating = (perfrom.Rating == null) ? performance.Rating.Value : perfrom.Rating.Value;

                //Create a new rating using a weighted average between the previous and current rating
                rating = (short)Math.Round(((rating / 4) + performance.Rating.Value) / 1.25d);

                perfrom.Rating = rating;

                _context.Entry(perfrom).State = EntityState.Modified;
            }


            await _context.SaveChangesAsync();

            return Ok(new BaseAPIResponse() { message = String.Format("Performance of type {0} successfully modified for employee named '{1}'.", performance.PerformanceType, performance.EmployeeName) });

        }

        /*
        *   Deletes a specifc performance type from a specific employee.
        */
        [HttpDelete]
        public async Task<ActionResult> DeletePerformance(string employeeName, string performanceType)
        {

            var employee = await _searcher.SearchEmployee(employeeName);

            if (employee == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = "There is no employee with a name matching " + employeeName + "."
                });
            }

            var performance = await _searcher.SearchPerformance(employee.EmpId, performanceType);

            if (performance == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = String.Format("There is no Performance of type {0} to delete from employee {1}.", performanceType, employeeName)
                });
            }

            _context.Performance.Remove(performance);

            await _context.SaveChangesAsync();

            return Ok(new BaseAPIResponse() { message = String.Format("Performance of type {0} successfully deleted from employee {1}.", performanceType, employeeName) });

        }

        private bool PerformanceExists(short id)
        {
            return _context.Performance.Any(e => e.EmpId == id);
        }
    }
}

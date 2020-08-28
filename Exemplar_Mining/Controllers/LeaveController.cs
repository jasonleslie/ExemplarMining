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
    public class LeaveController : ControllerBase
    {
        private readonly DBContext _context;

        private readonly ISearcher _searcher;

        public LeaveController(DBContext context, ISearcher searcher)
        {
            _context = context;
            _searcher = searcher;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<LeaveDTO>>> GetLeave()
        {

            return Ok(
                await (
                from lev in _context.Leave
                join emp in _context.Employee
                on lev.EmpId equals emp.EmpId
                select new LeaveDTO
                {
                    EmployeeName = emp.FirstName + " " + emp.LastName,
                    LeaveType = lev.LeaveType,
                    Amount = lev.Amount
                })
                .ToListAsync());

        }


        [HttpGet("name/{employeeName}")]
        public async Task<ActionResult<LeaveDTO>> GetLeave(string employeeName)
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

            var leave = await _context.Leave.Where(x => x.EmpId == employee.EmpId).ToListAsync();

            return Ok(leave.Select(x => new LeaveDTO()
            {
                EmployeeName = employee.FirstName + " " + employee.LastName,
                LeaveType = x.LeaveType,
                Amount = x.Amount
            }));
        }

        /*
        *   Method allows leave of a certain type to be given to or taken from a specific employee.
        */
        [HttpPatch]
        public async Task<IActionResult> UseLeave(LeaveDTO leave)
        {

            var employee = await _searcher.SearchEmployee(leave.EmployeeName);

            if (employee == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = "There is no employee with a name matching " + leave.EmployeeName + "."
                });
            }

            var lev = await _searcher.SearchLeave(employee.EmpId, leave.LeaveType);

            if (lev == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = String.Format("There is no leave of type {0} to modify for employee {1}.", leave.LeaveType, leave.EmployeeName)
                });
            }



            if (leave.Amount == 0)
            {
                return new BadRequestObjectResult(new { code = 400, errors = new string[] { "The amount of leave days cannot be 0 (zero)." } });
            }

            var remainingLeave = (short)(lev.Amount + leave.Amount);

            if (remainingLeave < 0)
            {
                return new BadRequestObjectResult(new
                {
                    code = 400,
                    errors = new string[]
                {
                    String.Format("Cannot take '{0}' days of leave type '{1}' for employee '{2}' as this would result in a negative leave balance."
                                   ,leave.Amount*-1, leave.LeaveType, leave.EmployeeName)
                }
                });
            }

            lev.Amount = remainingLeave;

            _context.Entry(lev).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Ok(new BaseAPIResponse() { message = String.Format("Leave of type {0} successfully modified for employee named '{1}'.", leave.LeaveType, leave.EmployeeName) });
        }


        [HttpPost]
        public async Task<ActionResult> PostLeave(LeaveDTO leave)
        {

            var employee = await _searcher.SearchEmployee(leave.EmployeeName);

            if (employee == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = "There is no employee with a name matching " + leave.EmployeeName + "."
                });
            }

            var lev = await _searcher.SearchLeave(employee.EmpId, leave.LeaveType);

            if (lev != null)
            {
                return Conflict(new APIResponse()
                {
                    code = 409,
                    message = String.Format("There is already leave of type {0} for employee {1}.", leave.LeaveType, leave.EmployeeName)
                });
            }

            //Leave type is upcase in DB
            leave.LeaveType = leave.LeaveType.ToUpper();

            _context.Leave.Add(new Leave()
            {
                EmpId = employee.EmpId,
                LeaveType = leave.LeaveType,
                Amount = leave.Amount
            });


            await _context.SaveChangesAsync();


            return Ok(new BaseAPIResponse() { message = String.Format("Leave of type {0} successfully added for employee name '{1}'.", leave.LeaveType, leave.EmployeeName) });
        }

        /*
        *   Removes leave of a certain type from a specific employee.
        */
        [HttpDelete]
        public async Task<ActionResult<Leave>> DeleteLeave(string employeeName, string leaveType)
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

            var leave = await _searcher.SearchLeave(employee.EmpId, leaveType);

            if (leave == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = String.Format("There is no leave of type {0} to delete from employee {1}.", leaveType, employeeName)
                });
            }

            _context.Leave.Remove(leave);

            await _context.SaveChangesAsync();

            return Ok(new BaseAPIResponse() { message = String.Format("Leave of type {0} successfully deleted from employee {1}.", leaveType, employeeName) });
        }

        private bool LeaveExists(short id)
        {
            return _context.Leave.Any(e => e.EmpId == id);
        }
    }
}

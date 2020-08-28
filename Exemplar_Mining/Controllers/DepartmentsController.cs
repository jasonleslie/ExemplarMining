using Exemplar_Mining.Models;
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
    public class DepartmentsController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly ISearcher _searcher;

        public DepartmentsController(DBContext context, ISearcher searcher)
        {
            _context = context;
            _searcher = searcher;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartmentDTO>>> GetDepartment()
        {
            return await _context.Department.Select(x => new DepartmentDTO
            {
                DepartmentName = x.DepartmentName,
                DateEstablished = x.DateEstablished
            }).ToListAsync();
        }


        [HttpGet("{departmentID:int}")]
        public async Task<ActionResult<DepartmentDTO>> GetDepartmentById(short departmentID)
        {
            var department = await _context.Department.FindAsync(departmentID);

            if (department == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = "There is no department with an ID matching " + departmentID + "."
                });
            }

            return new DepartmentDTO
            {
                DepartmentName = department.DepartmentName,
                DateEstablished = department.DateEstablished
            };
        }


        [HttpGet("name/{departmentName}")]
        public async Task<ActionResult<DepartmentDTO>> GetDepartmentByName(string departmentName)
        {
            var department = await _searcher.SearchDepartment(departmentName);

            if (department == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = "There is no department with a name matching " + departmentName + "."
                });
            }

            return new DepartmentDTO
            {
                DepartmentName = department.DepartmentName,
                DateEstablished = department.DateEstablished
            };
        }


        [HttpPost]
        public async Task<ActionResult> PostDepartment(DepartmentDTO department)
        {

            _context.Department.Add(new Department
            {
                DepartmentName = department.DepartmentName,
                DateEstablished = department.DateEstablished
            });

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (DepartmentNameExists(department.DepartmentName))
                {
                    return Conflict(new APIResponse()
                    {
                        code = 409,
                        message = String.Format("Department '{0}' already exists.", department.DepartmentName)
                    });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new BaseAPIResponse() { message = String.Format("Department '{0}' has been created.", department.DepartmentName) });

        }

        /*
         * Removes a department with option to remove employees as well.         
         */
        [HttpDelete("{departmentID:int}")]
        public async Task<ActionResult> DeleteDepartmentByID(short departmentID, bool retrenchEmployees)
        {
            var department = await _context.Department.FindAsync(departmentID);

            if (department == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = String.Format("The given id '{0}' does not match any known department", departmentID)
                });
            }


            if ((!retrenchEmployees) && (_context.Employee.Where(x => x.DepId == department.DepId).Any()))
            {
                return Conflict(new APIResponse()
                {
                    code = 409,
                    message = String.Format("Cannot delete department '{0}' as there are employees belonging to it.", department.DepartmentName)
                });
            }


            _context.Department.Remove(department);
            await _context.SaveChangesAsync();

            return Ok(new BaseAPIResponse() { message = String.Format("Department '{0}' has been deleted.", department.DepartmentName) });
        }

        /*
         * Removes a department with option to remove employees as well.         
         */
        [HttpDelete("name/{departmentName}")]
        public async Task<ActionResult> DeleteDepartmentByName(string departmentName, bool retrenchEmployees)
        {
            var department = await _searcher.SearchDepartment(departmentName);

            if (department == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = String.Format("The given name '{0}' does not match any known department", departmentName)
                });
            }


            if ((!retrenchEmployees) && (_context.Employee.Where(x => x.DepId == department.DepId).Any()))
            {
                return Conflict(new APIResponse()
                {
                    code = 409,
                    message = String.Format("Cannot delete department '{0}' as there are employees belonging to it.", department.DepartmentName)
                });
            }


            _context.Department.Remove(department);
            await _context.SaveChangesAsync();

            return Ok(new BaseAPIResponse() { message = String.Format("Department '{0}' has been deleted.", department.DepartmentName) });
        }

        private bool DepartmentNameExists(string name)
        {
            return _context.Department.Any(x => x.DepartmentName == name);
        }
    }
}

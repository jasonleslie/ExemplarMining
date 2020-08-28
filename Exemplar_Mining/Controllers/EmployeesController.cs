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
    public class EmployeesController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly ISearcher _searcher;

        public EmployeesController(DBContext context, ISearcher searcher)
        {
            _context = context;
            _searcher = searcher;
        }

        /*
        *   Returns a list of employees with optional filtering on department and/or title.
        */
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDTO>>> GetEmployee(string departmentName, string titleName)
        {

            var employees = await _context.Employee.ToListAsync();

            if ((departmentName != null) && (departmentName != ""))
            {
                var department = await _searcher.SearchDepartment(departmentName);

                if (department == null)
                {
                    return NotFound(new APIResponse()
                    {
                        code = 404,
                        message = departmentName + " is not a known department."
                    });
                }

                employees = employees.AsEnumerable().Where(x => x.DepId == department.DepId).ToList();
            }

            if ((titleName != null) && (titleName != ""))
            {
                employees = employees.AsEnumerable().Where(x => x.Position.Contains(titleName, StringComparison.OrdinalIgnoreCase)).ToList();

                if (!employees.Any())
                {
                    return NotFound(new APIResponse()
                    {
                        code = 404,
                        message = "There are no employees with a title matching " + titleName + "."
                    });
                }
            }

            return await Task.FromResult(employees.Select(x => new EmployeeDTO
            {
                Name = x.FirstName + " " + x.LastName,
                Position = x.Position
            }).ToList());

        }


        [HttpGet("{employeeID:int}")]
        public async Task<ActionResult<EmployeeDetailDTO>> GetEmployeeById(short employeeID)
        {
            var employee = await _context.Employee.FindAsync(employeeID);

            if (employee == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = "There are no employees with an ID matching " + employeeID + "."
                });
            }

            var department = await _context.Department.FindAsync(employee.DepId);

            var manager = await _context.Employee.FindAsync(employee.ManagerId);


            return Ok(new EmployeeDetailDTO
            {
                Name = employee.FirstName + " " + employee.LastName,
                Position = employee.Position,
                Department = department.DepartmentName,
                Manager = (manager == null) ? "None" : manager.FirstName + " " + manager.LastName,
                EnrollmentDate = employee.EnrollmentDate
            });

        }


        [HttpGet("name/{employeeName}")]
        public async Task<ActionResult<EmployeeDetailDTO>> GetEmployeeByName(string employeeName)
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

            var department = await _context.Department.FindAsync(employee.DepId);

            var manager = await _context.Employee.FindAsync(employee.ManagerId);

            return Ok(new EmployeeDetailDTO
            {
                Name = employee.FirstName + " " + employee.LastName,
                Position = employee.Position,
                Department = department.DepartmentName,
                Manager = (manager == null) ? "None" : manager.FirstName + " " + manager.LastName,
                EnrollmentDate = employee.EnrollmentDate
            });

        }


        [HttpPost]
        public async Task<ActionResult> PostEmployee(EmployeeCreateDTO employee)
        {

            var department = await _searcher.SearchDepartment(employee.Department);

            var manager = await _searcher.SearchEmployee(employee.Manager);

            if (department == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = employee.Department + " is not a known department."
                });
            }

            _context.Employee.Add(new Employee
            {
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Position = employee.Position,
                DepId = department.DepId,
                EnrollmentDate = employee.EnrollmentDate,
                Salary = employee.Salary,
                ManagerId = manager?.EmpId
            });

            await _context.SaveChangesAsync();

            return Ok(new BaseAPIResponse() { message = String.Format("Employee '{0} {1}' has been created.", employee.FirstName, employee.LastName) });
        }

        /*
        *   This method is used to update various employeee details such as their department, salary and/or title.
        */
        [HttpPut]
        [Route("update")]
        public async Task<ActionResult> UpdateEmployee(EmployeeCreateDTO employee)          // ?? whats happening with FK relations
        {

            var emp = await _searcher.SearchEmployee(employee.FirstName, employee.LastName);

            if (emp == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = String.Format("Cannot complete operation, employee '{0} {1}' is not a known employee.", employee.FirstName, employee.LastName)
                });
            }

            var department = await _searcher.SearchDepartment(employee.Department);

            var manager = await _searcher.SearchEmployee(employee.Manager);

            if (department == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = employee.Department + " is not a known department."
                });
            }


            emp.DepId = department.DepId;
            // and issue :> emp.Dep =  ??
            emp.Manager = manager; // and issue :> emp.Mines =  ?? and all other sets with FK relationships
            emp.ManagerId = manager?.EmpId;
            emp.Position = employee.Position;
            emp.Salary = employee.Salary;


            await _context.SaveChangesAsync();

            return Ok(new BaseAPIResponse() { message = String.Format("Operation for employee '{0} {1}' was successful.", employee.FirstName, employee.LastName) });

        }


        [HttpDelete("{employeeID:int}")]
        public async Task<ActionResult> DeleteEmployeeById(short employeeID)
        {
            var employee = await _context.Employee.FindAsync(employeeID);

            if (employee == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = String.Format("The given id {0} did not match any known employee", employeeID)
                });
            }

            var department = await _context.Department.FindAsync(employee.DepId);

            var manager = await _context.Employee.FindAsync(employee.ManagerId);

            _context.Employee.Remove(employee);

            await _context.SaveChangesAsync();

            return Ok(String.Format("Employee '{0} {1}' with id '{2}' has been deleted.", employee.FirstName, employee.LastName, employee.EmpId));
        }


        [HttpDelete("name/{employeeName}")]
        public async Task<ActionResult> DeleteEmployeeByName(string employeeName)
        {

            var employee = await _searcher.SearchEmployee(employeeName);

            if (employee == null)
            {
                return NotFound(new APIResponse()
                {
                    code = 404,
                    message = String.Format("The given name {0} did not match any known employee", employeeName)
                });
            }

            var department = await _context.Department.FindAsync(employee.DepId);

            var manager = await _context.Employee.FindAsync(employee.ManagerId);

            _context.Employee.Remove(employee);

            await _context.SaveChangesAsync();

            return Ok(new BaseAPIResponse() { message = String.Format("Employee '{0} {1}' with id '{2}' has been deleted.", employee.FirstName, employee.LastName, employee.EmpId) });
        }
    }
}

using Exemplar_Mining.Models;
using Exemplar_Mining.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Exemplar_Mining.Services
{
    public class Searcher : ISearcher
    {
        private readonly DBContext _context;

        public Searcher(DBContext context)
        {
            _context = context;
        }

        public async Task<Department> SearchDepartment(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var department = await _context.Department.Where(x => x.DepartmentName == name).FirstOrDefaultAsync();

            return department;
        }

        public async Task<Employee> SearchEmployee(string name)
        {

            if (string.IsNullOrEmpty(name))
                return null;

            var fullname = name.Split(' ');

            if (fullname.Length != 2)
            {
                return null;
            }

            var employee = await _context.Employee.Where(x => x.FirstName == fullname[0] && x.LastName == fullname[1]).FirstOrDefaultAsync();


            return employee;

        }

        public async Task<Employee> SearchEmployee(string firstName, string lastName)
        {

            if ((string.IsNullOrEmpty(firstName)) || (string.IsNullOrEmpty(lastName)))
                return null;

            var employee = await _context.Employee.Where(x => x.FirstName == firstName && x.LastName == lastName).FirstOrDefaultAsync();

            return employee;
        }

        public async Task<Leave> SearchLeave(int empId, string type)
        {

            if (string.IsNullOrEmpty(type))
            {
                return null;
            }

            type = type.ToUpper();

            var leave = await _context.Leave.Where(x => x.EmpId == empId && x.LeaveType == type).FirstOrDefaultAsync();

            return leave;
        }

        public async Task<Mine> SearchMine(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            var mine = await _context.Mine.Where(x => x.Name == name).FirstOrDefaultAsync();

            return mine;
        }

        public async Task<Performance> SearchPerformance(int empId, string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                return null;
            }

            type = type.ToUpper();

            var performance = await _context.Performance.Where(x => x.EmpId == empId && x.PerformanceType == type).FirstOrDefaultAsync();

            return performance;
        }
    }
}

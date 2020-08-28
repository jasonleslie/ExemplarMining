using Exemplar_Mining.Models;
using System.Threading.Tasks;

namespace Exemplar_Mining.Services.Interfaces
{
    /*
     * Interface is used to abstract searching of the various repositories.
     */
    public interface ISearcher
    {
        public Task<Employee> SearchEmployee(string name);

        public Task<Leave> SearchLeave(int empId, string type);
        public Task<Performance> SearchPerformance(int empId, string type);
        public Task<Department> SearchDepartment(string name);
        public Task<Mine> SearchMine(string name);
        public Task<Employee> SearchEmployee(string firstName, string LastName);

    }
}

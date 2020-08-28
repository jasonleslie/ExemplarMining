using System;
using System.Collections.Generic;

namespace Exemplar_Mining.Models
{
    public partial class Employee
    {
        public Employee()
        {
            InverseManager = new HashSet<Employee>();
            Leave = new HashSet<Leave>();
            Performance = new HashSet<Performance>();
        }

        public short EmpId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Position { get; set; }
        public short DepId { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public decimal Salary { get; set; }
        public short? ManagerId { get; set; }

        public virtual Department Dep { get; set; }
        public virtual Employee Manager { get; set; }
        public virtual ICollection<Employee> InverseManager { get; set; }
        public virtual ICollection<Mine> Mines { get; set; }
        public virtual ICollection<Leave> Leave { get; set; }
        public virtual ICollection<Performance> Performance { get; set; }
    }
}

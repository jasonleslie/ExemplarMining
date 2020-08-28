
using System;
using System.Collections.Generic;

namespace Exemplar_Mining.Models
{
    public partial class Department
    {
        public Department()
        {
            Employee = new HashSet<Employee>();
        }

        public short DepId { get; set; }
        public string DepartmentName { get; set; }
        public DateTime DateEstablished { get; set; }

        public virtual ICollection<Employee> Employee { get; set; }
    }
}

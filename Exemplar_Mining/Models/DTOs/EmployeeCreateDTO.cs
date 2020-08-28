using System;

namespace Exemplar_Mining.Models.DTOs
{
    public class EmployeeCreateDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public decimal Salary { get; set; }
        public string Manager { get; set; }
    }
}

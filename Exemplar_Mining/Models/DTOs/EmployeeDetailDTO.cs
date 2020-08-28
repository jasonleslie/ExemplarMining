using System;

namespace Exemplar_Mining.Models.DTOs
{
    public class EmployeeDetailDTO
    {
        public string Name { get; set; }
        public string Position { get; set; }

        public string Department { get; set; }
        public string Manager { get; set; }

        public DateTime EnrollmentDate { get; set; }
    }
}

namespace Exemplar_Mining.Models
{
    public partial class Leave
    {
        public short EmpId { get; set; }
        public string LeaveType { get; set; }
        public short Amount { get; set; }

        public virtual Employee Emp { get; set; }
    }
}

namespace Exemplar_Mining.Models
{
    public partial class Performance
    {
        public short EmpId { get; set; }
        public string PerformanceType { get; set; }
        public short? Rating { get; set; }

        public virtual Employee Emp { get; set; }
    }
}

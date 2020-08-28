namespace Exemplar_Mining.Models.DTOs
{
    public class RevenueDTO
    {
        public string EmployeeName { get; set; }
        public string Position { get; set; }
        public int MineCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MineAverage { get; set; }
    }
}
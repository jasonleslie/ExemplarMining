namespace Exemplar_Mining.Models.DTOs
{
    public class MineDTO
    {
        public short MineId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public decimal Lattitude { get; set; }
        public decimal Longitude { get; set; }
        public string OverseerName { get; set; }
    }
}

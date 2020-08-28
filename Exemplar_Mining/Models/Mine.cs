using System.Collections.Generic;

namespace Exemplar_Mining.Models
{
    public partial class Mine
    {
        public Mine()
        {
            Production = new HashSet<Production>();
        }

        public short MineId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public decimal Lattitude { get; set; }
        public decimal Longitude { get; set; }
        public short OverseerId { get; set; }

        public virtual Employee Emp { get; set; }

        public virtual Resource Res { get; set; }
        public virtual ICollection<Production> Production { get; set; }
    }
}

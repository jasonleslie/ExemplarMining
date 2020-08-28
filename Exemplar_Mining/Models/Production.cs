using System;

namespace Exemplar_Mining.Models
{
    public partial class Production
    {
        public long Id { get; set; }
        public short MineId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Datelogged { get; set; }

        public virtual Mine Mine { get; set; }
    }
}

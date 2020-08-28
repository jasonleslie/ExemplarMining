using System.Collections.Generic;

namespace Exemplar_Mining.Models
{
    public partial class Resource
    {
        public string Type { get; set; }
        public decimal Value { get; set; }
        public string Metric { get; set; }
        public virtual ICollection<Mine> Mines { get; set; }
    }
}

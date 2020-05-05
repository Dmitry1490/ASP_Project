using System;
using System.Collections.Generic;

namespace Catalog_REST_API.Models
{
    public partial class Values
    {
        public Values()
        {
            ProductValue = new HashSet<ProductValue>();
        }

        public int IdValues { get; set; }
        public int IdAttribute { get; set; }
        public string Values1 { get; set; }
        public string Units { get; set; }

        public virtual Attributes IdAttributeNavigation { get; set; }
        public virtual ICollection<ProductValue> ProductValue { get; set; }
    }
}

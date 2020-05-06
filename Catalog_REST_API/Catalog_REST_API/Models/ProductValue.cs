using System;
using System.Collections.Generic;

namespace Catalog_REST_API.Models
{
    public partial class ProductValue
    {
        public int IdProduct { get; set; }
        public int IdValues { get; set; }

        public virtual Product IdProductNavigation { get; set; }
        public virtual Values IdValuesNavigation { get; set; }
    }
}

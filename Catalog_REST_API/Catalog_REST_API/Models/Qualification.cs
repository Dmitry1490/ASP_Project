using System;
using System.Collections.Generic;

namespace Catalog_REST_API.Models
{
    public partial class Qualification
    {
        public Qualification()
        {
            Product = new HashSet<Product>();
        }

        public int IdQualification { get; set; }
        public string NameQualification { get; set; }

        public virtual ICollection<Product> Product { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace Catalog_REST_API.Models
{
    public partial class Country
    {
        public Country()
        {
            Product = new HashSet<Product>();
        }

        public int IdCountry { get; set; }
        public string NameCountry { get; set; }

        public virtual ICollection<Product> Product { get; set; }
    }
}

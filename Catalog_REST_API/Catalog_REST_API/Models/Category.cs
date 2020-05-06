using System;
using System.Collections.Generic;

namespace Catalog_REST_API.Models
{
    public partial class Category
    {
        public Category()
        {
            CategoryAttributes = new HashSet<CategoryAttributes>();
            Product = new HashSet<Product>();
        }

        public int IdCategory { get; set; }
        public string NameCategory { get; set; }

        public virtual ICollection<CategoryAttributes> CategoryAttributes { get; set; }
        public virtual ICollection<Product> Product { get; set; }
    }
}

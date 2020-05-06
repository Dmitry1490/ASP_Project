﻿using System;
using System.Collections.Generic;

namespace Catalog_REST_API.Models
{
    public partial class Manufacturer
    {
        public Manufacturer()
        {
            Product = new HashSet<Product>();
        }

        public int IdManufacturer { get; set; }
        public string NameManufacturer { get; set; }

        public virtual ICollection<Product> Product { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace Catalog_REST_API.Models
{
    public partial class Product
    {
        public Product()
        {
            ProductValue = new HashSet<ProductValue>();
        }

        public int IdProduct { get; set; }
        public string NameProduct { get; set; }
        public int IdCategory { get; set; }
        public int? IdCountry { get; set; }
        public int? IdQualification { get; set; }
        public int? IdManufacturer { get; set; }
        public string TySmd { get; set; }
        public string Description { get; set; }

        public virtual Category IdCategoryNavigation { get; set; }
        public virtual Country IdCountryNavigation { get; set; }
        public virtual Manufacturer IdManufacturerNavigation { get; set; }
        public virtual Qualification IdQualificationNavigation { get; set; }
        public virtual ICollection<ProductValue> ProductValue { get; set; }
    }
}

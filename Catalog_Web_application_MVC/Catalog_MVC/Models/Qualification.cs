using System;
using System.Collections.Generic;

namespace Catalog_Веб_приложение_MVC.Models
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

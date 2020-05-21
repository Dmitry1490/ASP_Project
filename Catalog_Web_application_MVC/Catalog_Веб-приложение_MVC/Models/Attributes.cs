using System;
using System.Collections.Generic;

namespace Catalog_Веб_приложение_MVC.Models
{
    public partial class Attributes
    {
        public Attributes()
        {
            CategoryAttributes = new HashSet<CategoryAttributes>();
            Values = new HashSet<Values>();
        }

        public int IdAttributes { get; set; }
        public string NameAttribute { get; set; }

        public virtual ICollection<CategoryAttributes> CategoryAttributes { get; set; }
        public virtual ICollection<Values> Values { get; set; }
    }
}

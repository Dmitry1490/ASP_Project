using System;
using System.Collections.Generic;

namespace Catalog_Веб_приложение_MVC.Models
{
    public partial class ProductValue
    {
        public int IdProduct { get; set; }
        public int IdValues { get; set; }

        public virtual Product IdProductNavigation { get; set; }
        public virtual Values IdValuesNavigation { get; set; }
    }
}

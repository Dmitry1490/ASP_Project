using System;
using System.Collections.Generic;

namespace Catalog_REST_API.Models
{
    public partial class ProjectProduct
    {
        public int IdProject { get; set; }
        public int IdProduct { get; set; }

        public virtual Product IdProductNavigation { get; set; }
        public virtual Projects IdProjectNavigation { get; set; }
    }
}

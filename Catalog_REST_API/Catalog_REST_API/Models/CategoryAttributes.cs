using System;
using System.Collections.Generic;

namespace Catalog_REST_API.Models
{
    public partial class CategoryAttributes
    {
        public int IdCategory { get; set; }
        public int IdAttribute { get; set; }

        public virtual Attributes IdAttributeNavigation { get; set; }
        public virtual Category IdCategoryNavigation { get; set; }
    }
}

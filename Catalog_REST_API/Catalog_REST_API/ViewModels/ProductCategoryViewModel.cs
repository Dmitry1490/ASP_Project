using System;
using System.Collections.Generic;
using Catalog_REST_API.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Catalog_REST_API.ViewModels
{
    public class ProductCategoryViewModel
    {

        public List<Product> Product { get; set; }
        public SelectList Category { get; set; }
        public string ProductCategory { get; set; }
        public string SearchString { get; set; }

    }
}

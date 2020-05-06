using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog_REST_API.Models;
using Catalog_REST_API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace Catalog_REST_API.Controllers
{

    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class SearchController : Controller
    {
        private readonly CatalogContext _context;
        private readonly ILogger _logger;

        public SearchController(CatalogContext context, ILogger<SearchController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/GetAllProduct
        [HttpGet("/GetAllProduct")]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProduct()
        {
            _logger.LogInformation("Вызван метод GetAllProduct");
            return await _context.Product.ToListAsync();
        }

        
        // GET: api/Search
        [HttpGet("api/Search")]
        public async Task<IActionResult> Search(string productCategory, string searchString)
        {

            IQueryable<string> categoryQuery = from p in _context.Product
                                               orderby p.IdCategoryNavigation.NameCategory
                                               select p.IdCategoryNavigation.NameCategory;

            var products = from p in _context.Product
                           select p;

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(s => s.NameProduct.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(productCategory))
            {
                products = products.Where(x => x.IdCategoryNavigation.NameCategory.Equals(productCategory));
            }

            var productCategoryVM = new ProductCategoryViewModel
            {
                Category = new SelectList(await categoryQuery.Distinct().ToListAsync()),
                Product = await products.ToListAsync()
            };

            return View(productCategoryVM);
        }

    }
}

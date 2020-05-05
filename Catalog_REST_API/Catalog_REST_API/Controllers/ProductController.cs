using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Catalog_REST_API.Models;
using Microsoft.Extensions.Logging;

namespace Catalog_REST_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly CatalogContext _context;
        private readonly ILogger _logger;

        public ProductController(CatalogContext context, ILogger<ProductController> logger)
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

        // GET: api/GetProduct
        [HttpGet("/GetProduct/{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            _logger.LogInformation("Вызван метод GetProduct");
            var product = await _context.Product.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // GET: api/GetProductByCategory
        [HttpGet("/GetProductByCategory/{idCategory}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductByCategory(int idCategory)
        {
            _logger.LogInformation("Вызван метод GetProductByCategory");
            var products = from p in _context.Product
                           select p;

            if (idCategory != 0)
            {
                products = products.Where(s => s.IdCategory == idCategory);
            }

            return await products.ToListAsync();
        }


    }
}

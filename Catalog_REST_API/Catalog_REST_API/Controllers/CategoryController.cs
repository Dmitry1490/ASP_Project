using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Catalog_REST_API.Models;

namespace Catalog_REST_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly CatalogContext _context;

        public CategoryController(CatalogContext context)
        {
            _context = context;
        }

        // GET: api/GetAllCategory
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetAllCategory()
        {
            return await _context.Category.ToListAsync();
        }

        // GET: api/GetCategory
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Category.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        // POST: api/GetAtributeCategory
        [HttpPost("/GetAtributeCategory/{nameCategory}")]
        public async Task<ActionResult<IEnumerable<Attributes>>> GetAtributeCategory(String nameCategory)
        {
            var query = from atribute in _context.Attributes
                        where atribute.CategoryAttributes.Any(c => c.IdCategoryNavigation.NameCategory.Equals(nameCategory))
                        select atribute;
            


            if (query == null)
            {

                return NotFound();
            }

            return await query.ToListAsync();
        }
    }
}

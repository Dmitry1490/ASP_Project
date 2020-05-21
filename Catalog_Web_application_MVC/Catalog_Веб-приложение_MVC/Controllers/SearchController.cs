using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Linq.Expressions;
using Catalog_Веб_приложение_MVC.Interface;
using Catalog_Веб_приложение_MVC.Models;
using Catalog_Веб_приложение_MVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Newtonsoft.Json.JsonConvert;

namespace Catalog_REST_API.Controllers
{

    
    public class SearchController : Controller
    {
        private readonly CatalogContext _context;
        private readonly ILogger _logger;

        public SearchController(CatalogContext context, ILogger<SearchController> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        public async Task<ActionResult<IEnumerable<Product>>> Search()
        {
            _logger.LogInformation("Вызван метод GetAllProduct");

            var productCategoryVM = new ProductCategoryViewModel
            {
                //Category = new SelectList(await categoryQuery.Distinct().ToListAsync())
                Product = await _context.Product.ToListAsync(),
                Categories = await _context.Category.ToListAsync(),
                Countries = await _context.Country.ToListAsync(),
                Qualifications = await _context.Qualification.ToListAsync(),
                Manufacturers = await _context.Manufacturer.ToListAsync()

            };

            if (User.Identity.IsAuthenticated)
            {
                IQueryable<Projects> projects = _context.Projects.Where(p => p.IdUserNavigation.EMail == HttpContext.User.Claims.First().Value);
                productCategoryVM.Projects = await projects.ToListAsync();
            }

            return View(productCategoryVM);
        }

        
        // Post: api/Search
        [HttpPost]
        public async Task<IActionResult> Search(string productCategory,
                                                string searchString,
                                                string productCountry)
        {

            var predicate = PredicateBuilder.True<Product>();

            if (!string.IsNullOrEmpty(productCategory) && !productCategory.Equals("0"))
            {
                predicate = predicate.And(product => product.IdCategory == Int32.Parse(productCategory));
            }


            if (!string.IsNullOrEmpty(searchString))
            {
                predicate = predicate.And(product => product.NameProduct.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(productCountry) && !productCountry.Equals("0"))
            {
                predicate = predicate.And(product => product.IdCountry == Int32.Parse(productCountry));
            }


            List<Product> p = _context.Product.Where(predicate).ToList();

            IQueryable<Product> products = _context.Product.Where(predicate);

            var productCategoryVM = new ProductCategoryViewModel
            {
                //Category = new SelectList(await categoryQuery.Distinct().ToListAsync()),
                Product = await products.ToListAsync(),
                Categories = await _context.Category.ToListAsync(),
                Countries = await _context.Country.ToListAsync(),
                Qualifications = await _context.Qualification.ToListAsync(),
                Manufacturers = await _context.Manufacturer.ToListAsync()
            };

            if (User.Identity.IsAuthenticated)
            {
                IQueryable<Projects> projects = _context.Projects.Where(p => p.IdUserNavigation.EMail == HttpContext.User.Claims.First().Value);
                productCategoryVM.Projects = await projects.ToListAsync();
            }

            return View(productCategoryVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddProductToProject([FromBody]AddProductModel model) {
            if (ModelState.IsValid)
            {
                _context.ProjectProduct.Add(new ProjectProduct { IdProduct = model.ProductId, IdProject = Int32.Parse(model.IdProjects) });
                await _context.SaveChangesAsync();
                return Ok(model);
            }
            return BadRequest();
            
        }
    }
}

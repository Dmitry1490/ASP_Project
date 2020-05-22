using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Catalog_Веб_приложение_MVC.Models;
using Catalog_Веб_приложение_MVC.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Catalog_Веб_приложение_MVC.Controllers
{
    public class PortfolioController : Controller
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CatalogContext _context;

        public PortfolioController(IHttpContextAccessor httpContextAccessor, CatalogContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }
        

        //GET: /<controller>/
        public async Task<IActionResult> Portfolio()
        {
            var EMail = HttpContext.User.Claims.FirstOrDefault()?.Value;
            var productCategoryVM = new ProductCategoryViewModel
            {
                Projects = null
            };
            var claim = HttpContext.User.Claims.FirstOrDefault();
            if (claim!=null)
            {
                IQueryable<Projects> projects = _context.Projects.Where(p => p.IdUserNavigation.EMail == claim.Value);
                if (projects.ToList().Count > 0)
                {
                    productCategoryVM = new ProductCategoryViewModel
                    {
                        Projects = await projects.ToListAsync(),
                        ProjectProducts = await _context.ProjectProduct.ToListAsync(),
                        Users = await _context.User.ToListAsync(),
                        Product = await _context.Product.ToListAsync()

                    };
                    return View(productCategoryVM);
                }
            }
            
            return View(productCategoryVM);
        }

        [HttpPost]
        public async Task<IActionResult> Portfolio(PortfolioModel model)
        {
            var outputModel = new ProductCategoryViewModel();
            if (model.Projectname.Length != 0 && model.Email.Length != 0)
            {

                var userEmail = HttpContext.User.Claims.FirstOrDefault()?.Value; //Пользуйтесь FirstOrDefault т.к. если юзера нет, будет нулреф
                if (userEmail != null && userEmail.Equals(model.Email))
                {
                    User user = _context.User.Where(u => u.EMail == model.Email).FirstOrDefault();
                    _context.Projects.Add(new Projects { NameProject = model.Projectname, IdUser = user.IdUser});
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Added Successfully!";
                    TempData["Warning"] = null;
                    ViewBag.Message = String.Format("You created a new project: {0}. ", model.Projectname);
                    return View(outputModel); //Без модели у Вас на этом месте должно было крашиться
                }
                else
                    TempData["Warning"] = "Wrang Email!";
                    TempData["Success"] = null;
                    ViewBag.Message = String.Format("Warning! Wrang email: {0}. Enter your account email", model.Email);
                    return View(outputModel);//Без модели у Вас на этом месте должно было крашиться. Модель должна быть именно из вью
                                             //Portfolio.cshtml т.е. ProductCategoryViewModel. Так и планировалось?  У Вас здесь View(), когда вот так было
                                             //все нормально приходило?


            }
            TempData["Warning"] = "Wrang Fill out!";
            TempData["Success"] = null;
            ViewBag.Message = String.Format("Warning! Fill out all field!.");
            return View(outputModel);//Без модели у Вас на этом месте должно было крашиться
        }

        [HttpPost]
        public ProductCategoryViewModel TableProjectsProduct([FromBody]ProjectNameModel model)
        {
            if (ModelState.IsValid)
            {
                List<Product> Prod = new List<Product>();
                List<ProjectProduct> prjProd = _context.ProjectProduct.Where(p => p.IdProjectNavigation.NameProject.Equals(model.ProjectName.Trim())).ToList();
                foreach (ProjectProduct pd in prjProd)
                {
                    Prod.Add(_context.Product.Find(pd.IdProduct));
                }


                var productCategoryVM = new ProductCategoryViewModel
                {
                    //Category = new SelectList(await categoryQuery.Distinct().ToListAsync()),
                    Product = Prod,
                    Categories = _context.Category.ToList(),
                    Countries = _context.Country.ToList(),
                    Qualifications = _context.Qualification.ToList(),
                    Manufacturers = _context.Manufacturer.ToList()
                };
                
                return productCategoryVM;
                
            }
            return null;

        }

    }
}

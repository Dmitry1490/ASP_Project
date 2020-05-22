using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Catalog_Веб_приложение_MVC.Models;
using Catalog_Веб_приложение_MVC.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Catalog_Веб_приложение_MVC.Controllers
{
   
    public class AuthController : Controller
    {
        private readonly IConfiguration _config;
        private readonly CatalogContext _context;


        public AuthController(IConfiguration config, CatalogContext context)
        {
            _config = config;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            
            bool isUservalid = false;

            User user = await _context.User.FirstOrDefaultAsync(u => u.EMail == model.Email && u.Password == model.Password);

            if (user != null)
            {
                isUservalid = true;
            }

            if (ModelState.IsValid && isUservalid)
            {
                if (user != null)
                {
                    await Authenticate(model.Email); // аутентификация

                    // Remember Me
                    var props = new AuthenticationProperties();
                    props.IsPersistent = model.RememberMe;

                    //return RedirectToAction("Index", "Home");
                    return RedirectToAction("Search", "Search");

                }
                ModelState.AddModelError("", "Некорректные логин и(или) пароль");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _context.User.FirstOrDefaultAsync(u => u.EMail == model.Email);
                if (user == null)
                {
                    // добавляем пользователя в бд
                    _context.User.Add(new User {NameUser = model.Username, EMail = model.Email, Password = model.Password });
                    await _context.SaveChangesAsync();

                    await Authenticate(model.Email); // аутентификация

                    return RedirectToAction("Login", "Auth");
                }
                else
                    ModelState.AddModelError("", "Некорректные логин и(или) пароль");
            }
            return View(model);
        }

        private async Task Authenticate(string userName)
        {
            // создаем один claim
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };

            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            // установка аутентификационных куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Search", "Search");
        }

    }
}

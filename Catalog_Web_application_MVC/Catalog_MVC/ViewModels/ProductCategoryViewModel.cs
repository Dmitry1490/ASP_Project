using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Catalog_Веб_приложение_MVC.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Catalog_Веб_приложение_MVC.ViewModels
{
    public class ProductCategoryViewModel
    {

        public List<Product> Product { get; set; }
        //        public SelectList Category { get; set; }
        public List<Category> Categories { get; set; }
        public List<Country> Countries { get; set; }
        public List<Qualification> Qualifications { get; set; }
        public List<Manufacturer> Manufacturers { get; set; }

        [JsonPropertyName("ProjectName")]
        public string ProjectName { get; set; }

        public List<Projects> Projects { get; set; }
        public List<ProjectProduct> ProjectProducts { get; set; }
        public List<User> Users { get; set; }


        public int ProductId { get; set; }
        public string IdProjects { get; set; }
        
        public string ProductCategory { get; set; }
        public string ProductCountry { get; set; }
        public string SearchString { get; set; }
        public string Projectname { get; set; }


        [Required(ErrorMessage = "Не указан Login")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Не указан Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароль введен неверно")]
        public string ConfirmPassword { get; set; }

        [Required]
        public bool RememberMe { get; set; }

    }
}

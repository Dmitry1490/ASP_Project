using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Catalog_Веб_приложение_MVC.Models;

namespace Catalog_Веб_приложение_MVC.ViewModels
{
    public class PortfolioModel
    {
        [Required(ErrorMessage = "Не указан Projectname")]
        public string Projectname { get; set; }

        [Required(ErrorMessage = "Не указан Email")]
        public string Email { get; set; }

        public List<Projects> Projects { get; set; }
        public List<ProjectProduct> ProjectProducts { get; set; }
        public List<User> Users { get; set; }

    }
}

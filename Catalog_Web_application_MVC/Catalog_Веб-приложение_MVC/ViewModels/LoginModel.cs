using System;
using System.ComponentModel.DataAnnotations;

namespace Catalog_Веб_приложение_MVC.ViewModels
{
    public class LoginModel
    {
        
        [Required(ErrorMessage = "Не указан Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public bool RememberMe { get; set; }


    }
}

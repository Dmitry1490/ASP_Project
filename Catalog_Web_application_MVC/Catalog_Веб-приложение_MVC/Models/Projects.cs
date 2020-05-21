using System;
using System.Collections.Generic;

namespace Catalog_Веб_приложение_MVC.Models
{
    public partial class Projects
    {
        public Projects()
        {
            ProjectProduct = new HashSet<ProjectProduct>();
        }

        public int IdProjects { get; set; }
        public string NameProject { get; set; }
        public int IdUser { get; set; }

        public virtual User IdUserNavigation { get; set; }
        public virtual ICollection<ProjectProduct> ProjectProduct { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace Catalog_REST_API.Models
{
    public partial class User
    {
        public User()
        {
            Projects = new HashSet<Projects>();
        }

        public int IdUser { get; set; }
        public string NameUser { get; set; }
        public string EMail { get; set; }
        public string Password { get; set; }

        public virtual ICollection<Projects> Projects { get; set; }
    }
}

namespace MukMafiaTool.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class User
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string[] Roles { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MukMafiaTool.Models
{
    public class HomePagePlayer
    {
        public string Name { get; set; }
        public int PostCount { get; set; }
        public bool IsInGame { get; set; }
        public string ExtraInfo { get; set; }
    }
}
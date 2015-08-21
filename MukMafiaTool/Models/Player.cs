using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MukMafiaTool.Models
{
    public class Player
    {
        public string Name { get; set; }
        public int PostCount { get; set; }
        public bool Excluded { get; set; }
        public bool Dead { get; set; }
        public string ExclusionNotes { get; set; }
    }
}
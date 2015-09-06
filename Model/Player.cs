using System.Collections.Generic;

namespace MukMafiaTool.Model
{
    public class Player
    {
        public string Name { get; set; }
        public IEnumerable<Recruitment> Recruitments { get; set; }
        public bool Participating { get; set; }
        public string Character { get; set; }
        public string Fatality { get; set; }
        public string Notes { get; set; }
        public IEnumerable<string> Aliases { get; set; }
    }
}

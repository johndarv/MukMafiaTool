namespace MukMafiaTool.Model
{
    using System.Collections.Generic;

    public class Player
    {
        public string Name { get; set; }

        public IList<Recruitment> Recruitments { get; } = new List<Recruitment>();

        public bool Participating { get; set; }

        public string Character { get; set; }

        public string Role { get; set; }

        public string Fatality { get; set; }

        public string Notes { get; set; }

        public IList<string> Aliases { get; } = new List<string>();

        public void AddRecruitments(IEnumerable<Recruitment> recruitments)
        {
            foreach (var recruitment in recruitments)
            {
                this.Recruitments.Add(recruitment);
            }
        }

        public void AddAliases(IEnumerable<string> aliases)
        {
            foreach (var alias in aliases)
            {
                this.Aliases.Add(alias);
            }
        }
    }
}
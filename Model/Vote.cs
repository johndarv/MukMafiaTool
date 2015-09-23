using System;

namespace MukMafiaTool.Model
{
    public class Vote
    {
        public bool IsUnvote { get; set; }
        public string Voter { get; set; }
        public string Recipient { get; set; }
        public DateTime DateTime { get; set; }
        public string ForumPostNumber { get; set; }
        public int PostContentIndex { get; set; }
        public bool ManuallyEdited { get; set; }
    }
}
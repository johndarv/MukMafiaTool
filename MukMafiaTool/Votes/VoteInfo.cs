namespace MukMafiaTool.Votes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using MukMafiaTool.Model;

    public class VoteInfo
    {
        public string VoterFactionName { get; set; }

        public string TargetFactionName { get; set; }

        public Allegiance TargetAllegiance { get; set; }

        public Allegiance VoterFactionAllegiance { get; set; }
    }
}
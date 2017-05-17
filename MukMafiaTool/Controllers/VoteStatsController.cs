namespace MukMafiaTool.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using MukMafiaTool.Common;
    using MukMafiaTool.Database;
    using MukMafiaTool.Model;
    using MukMafiaTool.Models.ViewModels;
    using MukMafiaTool.Votes;

    public class VoteStatsController : Controller
    {
        private IRepository repo;

        public VoteStatsController(IRepository repo)
        {
            this.repo = repo;
        }

        [HttpGet]
        public ActionResult Index()
        {
            VoteStatsViewModel viewModel = new VoteStatsViewModel();

            var allVotes = this.repo.FindAllVotes();
            var allPlayers = this.repo.FindAllPlayers();

            var meaningfulVotes = allVotes.Where(v => !v.IsUnvote);
            var meaningfulVoteGroups = meaningfulVotes.GroupBy(v => new { v.Voter, v.Recipient, v.Day });
            var votes = meaningfulVoteGroups.Select(g => g.First());

            CalculateTotalStats(viewModel, votes, allPlayers);

            viewModel.IndividualVoteStats = CalculateIndividualStats(votes, allPlayers);

            viewModel.FactionVoteStats = this.CalculateFactionStats(votes, allPlayers);

            return this.View(viewModel);
        }

        private static void CalculateTotalStats(VoteStatsViewModel viewModel, IEnumerable<Vote> votes, IEnumerable<Player> allPlayers)
        {
            viewModel.NumberOfVotes = votes.Count();
            viewModel.PercentageOfVotesOntoMafia =
                VoteAnalyser.CalculatePercentage(votes, (v) => v.TargetAllegiance == Allegiance.Mafia, allPlayers);

            viewModel.PercentageOfVotesOntoTown =
                VoteAnalyser.CalculatePercentage(votes, (v) => v.TargetAllegiance == Allegiance.Town, allPlayers);

            viewModel.PercentageOfVotesOntoNonTown =
                VoteAnalyser.CalculatePercentage(votes, (v) => v.TargetAllegiance != Allegiance.Town, allPlayers);
        }

        private static IEnumerable<IndividualVoteStatsViewModel> CalculateIndividualStats(IEnumerable<Vote> votes, IEnumerable<Player> allPlayers)
        {
            var individualStats = new List<IndividualVoteStatsViewModel>();

            foreach (var player in allPlayers)
            {
                var stats = new IndividualVoteStatsViewModel();
                stats.Name = player.Name;
                stats.FactionName = player.Recruitments.Last().FactionName;
                stats.Character = player.Character;

                var individualVotes = votes.Where(v => string.Equals(v.Voter, player.Name));
                stats.VotesCast = individualVotes.Count();

                stats.PercentageOfVotesOntoMafia =
                    VoteAnalyser.CalculatePercentage(individualVotes, (v) => v.TargetAllegiance == Allegiance.Mafia, allPlayers);

                stats.PercentageOfVotesOntoNonTown =
                    VoteAnalyser.CalculatePercentage(individualVotes, (v) => v.TargetAllegiance != Allegiance.Town, allPlayers);

                stats.PercentageOfVotesOntoTown =
                    VoteAnalyser.CalculatePercentage(individualVotes, (v) => v.TargetAllegiance == Allegiance.Town, allPlayers);

                stats.PercentageOfVotesOntoOwnAllegiance =
                    VoteAnalyser.CalculatePercentage(individualVotes, (v) => v.TargetAllegiance == v.VoterFactionAllegiance, allPlayers);

                stats.PercentageOfVotesOntoOwnFaction =
                    VoteAnalyser.CalculatePercentage(individualVotes, (v) => v.TargetFactionName == v.VoterFactionName, allPlayers);

                stats.TimesVotedFor = votes.Where(v => string.Equals(v.Recipient, player.Name)).Count();

                individualStats.Add(stats);
            }

            return individualStats;
        }

        private IEnumerable<FactionVoteStatsViewModel> CalculateFactionStats(IEnumerable<Vote> votes, IEnumerable<Player> allPlayers)
        {
            var factionVoteStatsCollection = new List<FactionVoteStatsViewModel>();

            var allRecruitmens = allPlayers.SelectMany(p => p.Recruitments);
            var factionNames = allRecruitmens.GroupBy(r => r.FactionName).Select(g => g.Key);

            foreach (var factionName in factionNames)
            {
                var factionVoteStats = new FactionVoteStatsViewModel();
                factionVoteStats.Name = factionName;
                factionVoteStats.Allegiance = Allegiance.Unknown;

                var votesByFaction = votes.Where(v => string.Equals(v.GetVoteInfo(allPlayers).VoterFactionName, factionName));

                factionVoteStats.VotesCast = votesByFaction.Count();

                factionVoteStats.PercentageOfVotesOntoOwnFaction =
                    VoteAnalyser.CalculatePercentage(votesByFaction, (v) => v.TargetFactionName == factionName, allPlayers);

                var firstFactionVote = votesByFaction.FirstOrDefault();

                if (firstFactionVote != null)
                {
                    factionVoteStats.Allegiance = firstFactionVote.GetVoteInfo(allPlayers).VoterFactionAllegiance;
                }

                factionVoteStatsCollection.Add(factionVoteStats);
            }

            return factionVoteStatsCollection;
        }
    }
}
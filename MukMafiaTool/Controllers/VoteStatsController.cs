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

namespace MukMafiaTool.Controllers
{
    public class VoteStatsController : Controller
    {
        private IRepository _repo;

        public VoteStatsController(IRepository repo)
        {
            _repo = repo;
        }

        // GET: VoteStats
        [Authorize()]
        public ActionResult Index()
        {
            VoteStatsViewModel viewModel = new VoteStatsViewModel();

            var allVotes = _repo.FindAllVotes();
            var allPlayers = _repo.FindAllPlayers();

            var votes = allVotes.Where(v => !v.IsUnvote);

            CalculateTotalStats(viewModel, votes, allPlayers);

            viewModel.IndividualVoteStats = CalculateIndividualStats(votes, allPlayers);

            viewModel.FactionVoteStats = CalculateFactionStats(votes, allPlayers);

            return View(viewModel);
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

                var individualVotes = votes.Where(v => string.Equals(v.Voter, player.Name));
                stats.VotesCast = individualVotes.Count();

                stats.PercentageOfVotesOntoMafia =
                    VoteAnalyser.CalculatePercentage(individualVotes, (v) => v.TargetAllegiance == Allegiance.Mafia, allPlayers);

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
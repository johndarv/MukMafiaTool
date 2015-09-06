using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MukMafiaTool.Model;
using MukMafiaTool.Models;

namespace MukMafiaTool.Votes
{
    public static class VoteAnalyser
    {
        public static VoteSituation DetermineCurrentVoteSituation(IEnumerable<Player> players, IEnumerable<Vote> votes, IEnumerable<Day> days)
        {
            var currentSituation = new VoteSituation();
            currentSituation.CurrentVotes = new List<Vote>();
            currentSituation.NotVoted = new List<Player>();

            var votesByPlayer = votes.GroupBy(v => v.Voter);
            var activePlayers = players.Where(p => p.Participating && string.IsNullOrEmpty(p.Fatality));

            foreach (var player in activePlayers)
            {
                // If they haven't cast a vote or an unvote - put in not voted list
                
                // If their latest vote was an unvote - put in not voted list

                // else put their latest vote into the current votes list
            }

            return currentSituation;
        }

        public static VoteInfo GetVoteInfo(this Vote vote, IEnumerable<Player> players)
        {
            VoteInfo result = new VoteInfo
            {
                VoterFactionName = string.Empty,
                TargetAllegiance = Allegiance.Unknown,
                TargetFactionName = string.Empty,
            };

            var voter = players.FirstOrDefault(p => p.Name == vote.Voter);
            if (voter == null)
            {
                return result;
            }

            var voterRecruitment = DetermineRecruitment(voter, vote.ForumPostNumber);
            result.VoterFactionName = voterRecruitment.FactionName;
            result.VoterFactionAllegiance = voterRecruitment.Allegiance;            

            var recipient = players.FirstOrDefault(p => p.Name == vote.Recipient);
            if (recipient == null)
            {
                return result;
            }

            var recruitment = DetermineRecruitment(recipient, vote.ForumPostNumber);

            result.TargetAllegiance = recruitment.Allegiance;
            result.TargetFactionName = recruitment.FactionName;

            return result;
        }

        private static Recruitment DetermineRecruitment(Player player, string forumPostNumber)
        {
            foreach (var recruitment in player.Recruitments)
            {
                if (string.Compare(forumPostNumber, recruitment.ForumPostNumber) >= 0)
                {
                    return recruitment;
                }
            }

            string msg = string.Format(
                "Something went wrong with recruitments and stuff. Player name: {0}. Forum Post Number: {1}.",
                player.Name,
                forumPostNumber);

            throw new InvalidOperationException(msg);
        }

        public static double CalculatePercentage(
            IEnumerable<Vote> votes,
            Func<VoteInfo, bool> filter,
            IEnumerable<Player> allPlayers)
        {
            IList<Vote> matchingVotes = new List<Vote>();

            foreach (var vote in votes)
            {
                var voteInfo = vote.GetVoteInfo(allPlayers);

                if (filter(voteInfo))
                {
                    matchingVotes.Add(vote);
                }
            }

            return ((double)matchingVotes.Count() / votes.Count());
        }
    }
}
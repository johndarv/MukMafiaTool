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
        public static VoteSituation DetermineCurrentVoteSituation(IEnumerable<Vote> allVotes, IEnumerable<Player> players, IEnumerable<Day> days)
        {
            var currentDay = days.OrderBy(d => d.Number).LastOrDefault();

            if (currentDay == null)
            {
                return new VoteSituation
                {
                    CurrentVotes = new List<Vote>(),
                    NotVoted = new List<Player>(),
                };
            }

            var todaysVotes = allVotes.Where(v => string.Compare(v.ForumPostNumber, currentDay.StartForumPostNumber) >= 0);
            var todaysVotesOrdered = todaysVotes.OrderBy(v => v.ForumPostNumber).ThenBy(v => v.PostContentIndex);
            var activePlayers = players.Where(p => p.Participating && string.IsNullOrEmpty(p.Fatality));

            IList<Vote> todaysCurrentVotes = new List<Vote>();
            IList<Player> notVotedYetToday = new List<Player>();

            foreach (var player in activePlayers)
            {
                // If they haven't cast a vote or an unvote - put in not voted list
                if (!todaysVotesOrdered.Any(v => string.Equals(v.Voter, player.Name)))
                {
                    notVotedYetToday.Add(player);
                }
                else
                {
                    var lastVote = todaysVotesOrdered.Last(v => string.Equals(v.Voter, player.Name));

                    // If their latest vote was an unvote - put in not voted list
                    if (lastVote.IsUnvote)
                    {
                        notVotedYetToday.Add(player);
                    }
                    else
                    {
                        todaysCurrentVotes.Add(lastVote);
                    }
                }
            }

            return new VoteSituation
            {
                CurrentVotes = todaysCurrentVotes,
                NotVoted = notVotedYetToday,
            };
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
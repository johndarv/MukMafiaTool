using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MukMafiaTool.Database;
using MukMafiaTool.Models;
using MukMafiaTool.Model;

namespace MukMafiaTool.ForumScanner.Extensions
{
    public static class ForumPostExtensions
    {
        public static IList<Vote> GetVotes(this ForumPost post, IList<string> playerNames)
        {
            var votes = new List<Vote>();

            GatherVotes(post, playerNames, votes);
            GatherUnvotes(post, votes);

            return votes;
        }

        private static void GatherVotes(ForumPost post, IList<string> playerNames, List<Vote> votes)
        {
            var content = post.Content.ToString();
            var indexes = content.ToLower().EndIndexOfNotInQuoteblock("vote:");
            indexes = indexes.Concat(content.ToLower().EndIndexOfNotInQuoteblock("<strong>vote ")).ToList();
            indexes = indexes.Concat(content.ToLower().EndIndexOfNotInQuoteblock("<br>vote ")).ToList();

            foreach (var index in indexes.Distinct())
            {
                var voteSubString = content.Substring(index, 6).Trim();

                var newVote = new Vote
                {
                    Voter = post.Poster,
                    DateTime = post.DateTime,
                    IsUnvote = false,
                    Recipient = DetermineRecipient(voteSubString, playerNames),
                    ForumPostNumber = post.ForumPostNumber,
                    PostContentIndex = index,
                };

                votes.Add(newVote);
            }
        }

        private static void GatherUnvotes(ForumPost post, List<Vote> votes)
        {
            var content = post.Content.ToString();
            var indexes = content.ToLower().EndIndexOfNotInQuoteblock("<strong>unvote");
            indexes = indexes.Concat(content.ToLower().EndIndexOfNotInQuoteblock("<br>unvote<br>")).ToList();

            foreach (var index in indexes.Distinct())
            {
                var newVote = new Vote
                {
                    Voter = post.Poster,
                    DateTime = post.DateTime,
                    IsUnvote = true,
                    Recipient = string.Empty,
                    ForumPostNumber = post.ForumPostNumber,
                    PostContentIndex = index,
                };

                votes.Add(newVote);
            }
        }

        private static string DetermineRecipient(string voteSubString, IList<string> playerNames)
        {
            foreach (var name in playerNames)
            {
                var matchLength = Math.Min(voteSubString.Length, name.Length);

                var str1 = voteSubString.Substring(0, matchLength);
                var str2 = name.Substring(0, matchLength);

                if (string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase))
                {
                    return name;
                }
            }

            return string.Empty;
        }
    }
}
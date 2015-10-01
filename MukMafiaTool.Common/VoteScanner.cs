using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MukMafiaTool.Model;

namespace MukMafiaTool.Common
{
    public static class VoteScanner
    {
        // I apolgise to anyone ever attempting to read this method
        public static IList<Vote> ScanForVotes(ForumPost post, IEnumerable<IEnumerable<string>> playerNamesGroups)
        {
            var votes = new List<Vote>();

            var content = post
                .Content
                .ToString()
                .FilterOutContentInQuoteBlocks()
                .FilterOutSpanTags()
                .ReplaceNonBreakingSpacesWithSpaces()
                .RemoveNewLineAndTabChars()
                .ToLower();

            var indexes = content.AllIndexesOf("vote");

            foreach (var index in indexes)
            {
                if (index == 0 || content[index - 1] == 'n')
                {
                    // if it says unvote, or drunkenvote! then:
                    if (content.IsInBold(index))
                    {
                        // And it is in bold, then:
                        AddUnvote(post, votes, index);
                    }
                    else if (index >= 6 && string.Equals(content.Substring(index - 6, 6), "<br>un"))
                    {
                        // or if the unvote is clearly on a new line, then:
                        AddUnvote(post, votes, index);
                    }
                }
                else if (((index + 4) <= content.Length && content[index + 4] == ':') ||
                         ((index + 5) <= content.Length && string.Equals(content.Substring(index + 4, 2), " :")) ||
                         ((index + 4) <= content.Length && content[index + 4] == ';') ||
                         ((index + 5) <= content.Length && string.Equals(content.Substring(index + 4, 2), " ;")))
                {
                    // else if there's a semi colon then we determine this is a vote
                    var recipientSubString = content.Substring(index + 4, Math.Min(10, content.Length - (index + 4)));

                    var successfulSplit = recipientSubString.Split(':').Count() == 2;

                    if (successfulSplit)
                    {
                        recipientSubString = recipientSubString.Split(':')[1];
                    }
                    else
                    {
                        recipientSubString = recipientSubString.Split(';')[1];
                    }

                    recipientSubString = recipientSubString.Trim();
                    recipientSubString = recipientSubString.Substring(0, Math.Min(6, recipientSubString.Length));

                    // If somebody writes vote: name, don't count it
                    if (!string.Equals(recipientSubString.Substring(0, Math.Min(4, recipientSubString.Length)), "name".Substring(0, Math.Min(4, recipientSubString.Length)), StringComparison.OrdinalIgnoreCase))
                    {
                        var newVote = new Vote
                        {
                            Voter = post.Poster,
                            DateTime = post.DateTime,
                            IsUnvote = false,
                            Recipient = DetermineRecipient(recipientSubString.Trim(), playerNamesGroups),
                            ForumPostNumber = post.ForumPostNumber,
                            PostContentIndex = index,
                            ManuallyEdited = false,
                            Day = post.Day,
                        };

                        votes.Add(newVote);
                    }
                }
                else if ((index + 4) <= content.Length && content[index + 4] == ' ')
                {
                    // else if it has a space right after the word vote...
                    if (content.IsInBold(index))
                    {
                        // and it's in bold, then we assume it's a vote and what follows directly after is the recipient
                        var recipientSubString = content.Substring(index + 5, Math.Min(6, content.Length - (index + 5)));

                        var newVote = new Vote
                        {
                            Voter = post.Poster,
                            DateTime = post.DateTime,
                            IsUnvote = false,
                            Recipient = DetermineRecipient(recipientSubString.Trim(), playerNamesGroups),
                            ForumPostNumber = post.ForumPostNumber,
                            PostContentIndex = index,
                            ManuallyEdited = false,
                            Day = post.Day,
                        };

                        votes.Add(newVote);
                    }
                }
            }

            return votes;
        }

        private static void AddUnvote(ForumPost post, List<Vote> votes, int index)
        {
            var newUnvote = new Vote
            {
                Voter = post.Poster,
                DateTime = post.DateTime,
                IsUnvote = true,
                Recipient = string.Empty,
                ForumPostNumber = post.ForumPostNumber,
                PostContentIndex = index,
                ManuallyEdited = false,
                Day = post.Day,
            };

            votes.Add(newUnvote);
        }

        private static string DetermineRecipient(string voteSubString, IEnumerable<IEnumerable<string>> playerNameGroups)
        {
            foreach (var playerNames in playerNameGroups)
            {
                foreach (var playerName in playerNames)
                {
                    var matchLength = Math.Min(voteSubString.Length, playerName.Length);

                    var str1 = voteSubString.Substring(0, matchLength);
                    var str2 = playerName.Substring(0, matchLength);

                    if (string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase))
                    {
                        return playerNames.First();
                    }
                }
            }

            return string.Empty;
        }
    }
}

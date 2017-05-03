using System;
using System.Collections.Generic;
using System.Linq;
using MukMafiaTool.Model;

namespace MukMafiaTool.Common
{
    public class VoteScanner
    {
        private const int MaxLengthOfRecipientSubString = 6;

        private IRepository _repo;
        private IEnumerable<Player> _players;
        private IEnumerable<IEnumerable<string>> _playerNameGroups;

        public VoteScanner(IRepository repo)
        {
            _repo = repo;
            _players = _repo.FindAllPlayers();
            _playerNameGroups = _players.Select(p => (new string[] { p.Name }).Concat(p.Aliases));
        }

        // I apolgise to anyone ever attempting to read this method
        public IEnumerable<Vote> ScanForVotes(ForumPost post)
        {
            var votes = new List<Vote>();

            var content = post
                .Content
                .ToString()
                .FilterOutContentInQuoteBlocks()
                .FilterOutSpanTags()
                .ReplaceNonBreakingSpacesWithSpaces()
                .RemoveNewLineAndTabChars()
                .RemoveUnecessaryClosedOpenHtmlTags()
                .ReplaceAtMentionsWithPlainNameText()
                .FilterOutStrongTagsAfterTheWordVote()
                .ReplaceNonBreakingSpaceCharactersWithRegularWhitespaceCharacters()
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
                        votes.Add(CreateUnvote(post, index));
                    }
                    else if (index >= 6 && string.Equals(content.Substring(index - 6, 6), "<br>un"))
                    {
                        // or if the unvote is clearly on a new line, then:
                        votes.Add(CreateUnvote(post, index));
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

                    recipientSubString = recipientSubString.Substring(0, Math.Min(MaxLengthOfRecipientSubString, recipientSubString.Length));

                    // If somebody writes vote: name, don't count it
                    if (!string.Equals(recipientSubString.Substring(0, Math.Min(4, recipientSubString.Length)), "name".Substring(0, Math.Min(4, recipientSubString.Length)), StringComparison.OrdinalIgnoreCase))
                    {
                        Vote newVote = CreateVote(post, index, recipientSubString);

                        if (IsValid(newVote, recipientSubString))
                        {
                            votes.Add(newVote);
                        }
                    }
                }
                else if ((index + 4) <= content.Length && content[index + 4] == ' ') // else if it has a space right after the word vote...
                {
                    if (content.IsInBold(index))
                    {
                        // and it's in bold, then we assume it's a vote and what follows directly after is the recipient
                        var recipientSubString = content.Substring(index + 5, Math.Min(MaxLengthOfRecipientSubString, content.Length - (index + 5)));

                        Vote newVote = CreateVote(post, index, recipientSubString);

                        if (IsValid(newVote, recipientSubString))
                        {
                            votes.Add(newVote);
                        }
                    }
                }
            }

            return votes;
        }

        private bool IsValid(Vote vote, string recipientSubString)
        {
            if (!_players.Select(p => p.Name).Contains(vote.Recipient) && vote.IsUnvote == false)
            {
                var msg = string.Format(
                    "Not adding vote because recipient is not in the player list. Voter: {0}. Recipient: {1}. Forum Post Number: {2}. Recipient substring: {3}.",
                    vote.Voter,
                    vote.Recipient,
                    vote.ForumPostNumber,
                    recipientSubString);

                _repo.LogMessage(msg);

                return false;
            }
            else if (_players.Single(p => string.Equals(p.Name, vote.Voter)).Participating == false)
            {
                var msg = string.Format(
                    "Not adding vote because voter is not participating. Voter: {0}. Recipient: {1}. Forum Post Number: {2}. Recipient substring: {3}.",
                    vote.Voter,
                    vote.Recipient,
                    vote.ForumPostNumber,
                    recipientSubString);

                _repo.LogMessage(msg);

                return false;
            }

            return true;
        }

        private Vote CreateVote(ForumPost post, int index, string recipientSubString)
        {
            return new Vote
            {
                Voter = post.Poster,
                DateTime = post.DateTime,
                IsUnvote = false,
                Recipient = DetermineRecipient(recipientSubString.Trim()),
                ForumPostNumber = post.ForumPostNumber,
                PostContentIndex = index,
                ManuallyEdited = false,
                Day = post.Day,
            };
        }

        private static Vote CreateUnvote(ForumPost post, int index)
        {
            return new Vote
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
        }

        private string DetermineRecipient(string voteSubString)
        {
            foreach (var playerNames in _playerNameGroups)
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

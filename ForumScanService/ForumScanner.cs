using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using MukMafiaTool.Database;
using MukMafiaTool.Model;
using MukMafiaTool.Common;

namespace MukMafiaTool.ForumScanService
{
    public class ForumScanner : IForumScanner
    {
        IRepository _repo;
        ForumAccessor _forumAccessor;
        TimeSpan _pollInterval;

        public ForumScanner(IRepository repository)
        {
            _repo = repository;
            _forumAccessor = new ForumAccessor();
            _pollInterval = GetInterval();
        }

        public void DoWholeUpdate()
        {
            UpdatePosts();
            CalculateVotes();

            _repo.UpdateLastUpdated();
        }

        private void UpdatePosts()
        {
            int currentPageNumber = 1;
            var latestPost = _repo.FindLatestPost();            

            if (latestPost != null)
            {
                currentPageNumber = latestPost.PageNumber - 10;
            }

            string pageContent = _forumAccessor.RetrievePageHtml(currentPageNumber);

            while (!string.IsNullOrEmpty(pageContent))
            {
                PageScanner pageScanner = new PageScanner(_repo.FindAllDays());

                IList<ForumPost> posts = pageScanner.RetrieveAllPosts(pageContent, currentPageNumber);

                _repo.UpsertPosts(posts);

                currentPageNumber++;
                pageContent = _forumAccessor.RetrievePageHtml(currentPageNumber);
            }
        }

        private void CalculateVotes()
        {
            // Get first post of last day
            string startOfDayForumPostNumber = _repo.FindCurrentDay().StartForumPostNumber;

            // Delete votes collection
            _repo.WipeVotes();

            var allPosts = _repo.FindAllPosts().ToList();
            var startOfDayPost = allPosts.Single(p => string.Equals(p.ForumPostNumber, startOfDayForumPostNumber));
            allPosts = allPosts.OrderBy(p => p.ForumPostNumber).ToList();
            var postNumbers = allPosts.Select(p => p.ForumPostNumber);
            var index = allPosts.IndexOf(startOfDayPost);
            var posts = allPosts.GetRange(index, allPosts.Count - index);

            var players = allPosts.Select(p => p.Poster).Distinct().ToList();

            IList<Vote> votes = new List<Vote>();

            foreach (var post in posts)
            {
                votes = votes.Concat<Vote>(GetVotes(post, players)).ToList();
            }

            votes = votes.OrderBy(v => v.ForumPostNumber).ThenBy(v => v.PostContentIndex).ToList();

            foreach (var vote in votes)
            {
                _repo.ProcessVote(vote);
            }
        }

        // For anyone ever reading this. I feel embarrassed enough about this method to let you know that I know it's horrible and I'm sorry. Fuck string processing.
        public static IList<Vote> GetVotes(ForumPost post, IList<string> playerNames)
        {
            var votes = new List<Vote>();

            var content = post.Content.ToString().FilterOutContentInQuoteBlocks().RemoveNewLineAndTabChars().ToLower();

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

                    var newVote = new Vote
                    {
                        Voter = post.Poster,
                        DateTime = post.DateTime,
                        IsUnvote = false,
                        Recipient = DetermineRecipient(recipientSubString.Trim(), playerNames),
                        ForumPostNumber = post.ForumPostNumber,
                        PostContentIndex = index,
                    };

                    votes.Add(newVote);
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
                            Recipient = DetermineRecipient(recipientSubString.Trim(), playerNames),
                            ForumPostNumber = post.ForumPostNumber,
                            PostContentIndex = index,
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
            };

            votes.Add(newUnvote);
        }

        private static string DetermineRecipient(string voteSubString, IList<string> playerNames)
        {
            foreach (var playerName in playerNames)
            {
                var matchLength = Math.Min(voteSubString.Length, playerName.Length);

                var str1 = voteSubString.Substring(0, matchLength);
                var str2 = playerName.Substring(0, matchLength);

                if (string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase))
                {
                    return playerName;
                }
            }

            return string.Empty;
        }

        private TimeSpan GetInterval()
        {
            try
            {
                string intervalString = ConfigurationManager.AppSettings["PollForumIntervalMinutes"];
                int intervalInt = int.Parse(intervalString);
                return TimeSpan.FromMinutes(intervalInt);
            }
            catch
            {
                return TimeSpan.FromMinutes(5);
            }
        }
    }
}
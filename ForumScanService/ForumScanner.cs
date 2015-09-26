using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using ForumScanService;
using MukMafiaTool.Common;
using MukMafiaTool.Database;
using MukMafiaTool.Model;

namespace MukMafiaTool.ForumScanService
{
    public class ForumScanner : IForumScanner
    {
        IRepository _repo;
        ForumAccessor _forumAccessor;
        TimeSpan _pollInterval;
        DayScanner _dayScanner;

        public ForumScanner(IRepository repository)
        {
            _repo = repository;
            _forumAccessor = new ForumAccessor();
            _pollInterval = GetInterval();
            _dayScanner = new DayScanner(_repo);
        }

        public void DoWholeUpdate()
        {
            UpdateRepoFromThread();

            _repo.UpdateLastUpdated();
        }

        public void DoEndOfGameScan()
        {
            UpdateRepoFromThread();
            UpdateVotesForWholeGame("10515623");
        }

        private void UpdateRepoFromThread()
        {
            int currentPageNumber = 1;
            var latestPost = _repo.FindLatestPost();

            if (latestPost != null)
            {
                currentPageNumber = Math.Max(latestPost.PageNumber - 1, 1);
            }

            string pageContent = _forumAccessor.RetrievePageHtml(currentPageNumber);

            while (!string.IsNullOrEmpty(pageContent))
            {
                PageScanner pageScanner = new PageScanner(_repo.FindAllDays());

                var scannedPosts = pageScanner.RetrieveAllPosts(pageContent, currentPageNumber);

                // Ensure any new players that have posted new posts are in the repository
                var newPosts = latestPost == null ? scannedPosts : FindAllPostsAfter(scannedPosts, latestPost.ForumPostNumber);
                _repo.EnsurePlayersInRepo(newPosts);

                _dayScanner.UpdateDays(scannedPosts);

                UpdateVotes(scannedPosts);

                currentPageNumber++;
                pageContent = _forumAccessor.RetrievePageHtml(currentPageNumber);
            }
        }

        private void HandleValidPosts(IList<ForumPost> scannedPosts)
        {
            foreach (var post in scannedPosts)
            {
                var repoPost = _repo.FindSpecificPost(post.ForumPostNumber);

                if (repoPost == null || repoPost.LastScanned - repoPost.DateTime < TimeSpan.FromMinutes(5))
                {
                    _repo.UpsertPost(post);
                }
            }
        }

        private void UpdateVotesForWholeGame(string startOfGameForumPostNumber)
        {
            var allPosts = _repo.FindAllPosts().OrderBy(p => p.ForumPostNumber).ToList();
            var relevantPosts = FindAllPostsAfter(allPosts, startOfGameForumPostNumber);

            UpsertVotes(relevantPosts);
        }

        private void UpdateVotes(IList<ForumPost> scannedPosts)
        {
            foreach (var post in scannedPosts)
            {
                var repoPost = _repo.FindSpecificPost(post.ForumPostNumber);

                if (repoPost == null || repoPost.LastScanned - repoPost.DateTime < TimeSpan.FromMinutes(5))
                {
                    _repo.UpsertPost(post);
                    _repo.DeleteVotes(post.ForumPostNumber);
                    UpsertVotes(post);
                }
            }
        }

        private void UpsertVotes(ForumPost relevantPost)
        {
            UpsertVotes(new ForumPost[] { relevantPost });
        }

        private void UpsertVotes(IList<ForumPost> relevantPosts)
        {
            var players = _repo.FindAllPlayers();
            var playerNameGroups = players.Select(p => (new string[] { p.Name }).Concat(p.Aliases));

            foreach (var post in relevantPosts)
            {
                foreach (var vote in VoteScanner.ScanForVotes(post, playerNameGroups))
                {
                    _repo.UpsertVote(vote);
                }
            }
        }

        private IList<ForumPost> FindAllPostsAfter(IList<ForumPost> posts, string forumPostNumber)
        {
            posts = posts.OrderBy(p => p.ForumPostNumber).ToList();
            return posts.Where(p => string.Compare(p.ForumPostNumber, forumPostNumber) > 0).ToList();
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
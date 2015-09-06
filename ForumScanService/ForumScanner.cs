using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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

        public ForumScanner(IRepository repository)
        {
            _repo = repository;
            _forumAccessor = new ForumAccessor();
            _pollInterval = GetInterval();
        }

        public void DoWholeUpdate()
        {
            UpdateRepoFromThread();
            CalculateVotes();

            _repo.UpdateLastUpdated();
        }

        public void DoEndOfGameScan()
        {
            UpdateRepoFromThread();
            AmalgamateVotes("10515623");
        }

        private void UpdateRepoFromThread()
        {
            int currentPageNumber = 1;
            var latestPost = _repo.FindLatestPost();

            if (latestPost != null)
            {
                currentPageNumber = latestPost.PageNumber - 1;
            }

            string pageContent = _forumAccessor.RetrievePageHtml(currentPageNumber);

            while (!string.IsNullOrEmpty(pageContent))
            {
                PageScanner pageScanner = new PageScanner(_repo.FindAllDays());

                var scannedPosts = pageScanner.RetrieveAllPosts(pageContent, currentPageNumber);
                var validPosts = scannedPosts.Where(p => p.Day > 1).ToList();

                // Ensure any new players that have posted new posts are in the repository
                var newPosts = latestPost == null ? validPosts : FindAllPostsAfter(validPosts, latestPost.ForumPostNumber);
                _repo.EnsurePlayersInRepo(newPosts);

                HandleValidPosts(validPosts);

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

        private void CalculateVotes()
        {
            // Get first post of last day
            string startOfDayForumPostNumber = _repo.FindCurrentDay().StartForumPostNumber;

            // Delete votes collection
            _repo.WipeVotes();

            var allPosts = _repo.FindAllPosts().OrderBy(p => p.ForumPostNumber).ToList();
            var posts = FindAllPostsAfter(allPosts, startOfDayForumPostNumber);
            var players = allPosts.Select(p => p.Poster).Distinct().ToList();

            IList<Vote> votes = new List<Vote>();

            foreach (var post in posts)
            {
                votes = votes.Concat<Vote>(VoteScanner.ScanForVotes(post, players)).ToList();
            }

            votes = votes.OrderBy(v => v.ForumPostNumber).ThenBy(v => v.PostContentIndex).ToList();

            foreach (var vote in votes)
            {
                _repo.ProcessVote(vote);
            }
        }

        private void AmalgamateVotes(string startOfGameForumPostNumber)
        {
            var allPosts = _repo.FindAllPosts().OrderBy(p => p.ForumPostNumber).ToList();
            var posts = FindAllPostsAfter(allPosts, startOfGameForumPostNumber);
            var playerNames = allPosts.Select(p => p.Poster).Distinct().ToList();

            foreach (var post in posts)
            {
                foreach (var vote in VoteScanner.ScanForVotes(post, playerNames))
                {
                    _repo.UpsertVote(vote);
                }
            }
        }

        private IList<ForumPost> FindAllPostsAfter(IList<ForumPost> validScannedPosts, string forumPostNumber)
        {
            validScannedPosts = validScannedPosts.OrderBy(p => p.ForumPostNumber).ToList();
            return validScannedPosts.Where(p => string.Compare(forumPostNumber, forumPostNumber) > 0).ToList();
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
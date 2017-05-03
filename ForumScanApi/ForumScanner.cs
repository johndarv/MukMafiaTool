using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using MukMafiaTool.Common;
using MukMafiaTool.Model;

namespace ForumScanApi
{
    public class ForumScanner : IForumScanner
    {
        private IRepository _repo;
        private ForumAccessor _forumAccessor;
        private TimeSpan _pollInterval;
        private DayScanner _dayScanner;
        private VoteScanner _voteScanner;
        private string _firstForumPostNumber;

        public ForumScanner(IRepository repository)
        {
            _repo = repository;
            _forumAccessor = new ForumAccessor();
            _pollInterval = GetInterval();
            _dayScanner = new DayScanner(_repo);
            _voteScanner = new VoteScanner(_repo);
            _firstForumPostNumber = ConfigurationManager.AppSettings["FirstForumPostNumber"] ?? "1";
        }

        public void DoWholeUpdate()
        {
            UpdateRepoFromThread();

            _repo.UpdateLastUpdatedTime();
        }

        public void DoEndOfGameScan()
        {
            UpdateRepoFromThread();
            UpdateVotesForWholeGame("11368576");
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
                var postsAfterDay0 = scannedPosts.Where(p => p.Day > 0);

                // Ensure any new players that have posted new posts are in the repository
                var newPostsAfterDay0 = latestPost == null ? postsAfterDay0 : FindAllPostsAfter(postsAfterDay0, latestPost.ForumPostNumber);
                _repo.EnsurePlayersInRepo(newPostsAfterDay0.Select(p => p.Poster), _firstForumPostNumber);

                _dayScanner.UpdateDays(postsAfterDay0);

                UpdateVotes(postsAfterDay0);

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

        private void UpdateVotes(IEnumerable<ForumPost> scannedPosts)
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
            foreach (var post in relevantPosts)
            {
                foreach (var vote in _voteScanner.ScanForVotes(post))
                {
                    _repo.UpsertVote(vote);
                }
            }
        }

        private IList<ForumPost> FindAllPostsAfter(IEnumerable<ForumPost> posts, string forumPostNumber)
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
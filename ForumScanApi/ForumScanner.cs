namespace ForumScanApi
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using MukMafiaTool.Common;
    using MukMafiaTool.Model;

    public class ForumScanner : IForumScanner
    {
        private IRepository repo;
        private ForumAccessor forumAccessor;
        private TimeSpan pollInterval;
        private DayScanner dayScanner;
        private VoteScanner voteScanner;
        private string firstForumPostNumber;

        public ForumScanner(IRepository repository)
        {
            this.repo = repository;
            this.forumAccessor = new ForumAccessor();
            this.pollInterval = this.GetInterval();
            this.dayScanner = new DayScanner(this.repo);
            this.voteScanner = new VoteScanner(this.repo);
            this.firstForumPostNumber = ConfigurationManager.AppSettings["FirstForumPostNumber"] ?? "1";
        }

        public void DoWholeUpdate()
        {
            this.UpdateRepoFromThread();

            this.repo.UpdateLastUpdatedTime();
        }

        public void DoEndOfGameScan()
        {
            this.UpdateRepoFromThread();
            this.UpdateVotesForWholeGame("11368576");
        }

        private void UpdateRepoFromThread()
        {
            int currentPageNumber = 1;
            var latestPost = this.repo.FindLatestPost();

            if (latestPost != null)
            {
                currentPageNumber = Math.Max(latestPost.PageNumber - 1, 1);
            }

            string pageContent = this.forumAccessor.RetrievePageHtml(currentPageNumber);

            while (!string.IsNullOrEmpty(pageContent))
            {
                PageScanner pageScanner = new PageScanner(this.repo.FindAllDays());

                var scannedPosts = pageScanner.RetrieveAllPosts(pageContent, currentPageNumber);
                var postsAfterDay0 = scannedPosts.Where(p => p.Day > 0);

                // Ensure any new players that have posted new posts are in the repository
                var newPostsAfterDay0 = latestPost == null ? postsAfterDay0 : this.FindAllPostsAfter(postsAfterDay0, latestPost.ForumPostNumber);
                this.repo.EnsurePlayersInRepo(newPostsAfterDay0.Select(p => p.Poster), this.firstForumPostNumber);

                this.dayScanner.UpdateDays(postsAfterDay0);

                this.UpdateVotes(postsAfterDay0);

                currentPageNumber++;
                pageContent = this.forumAccessor.RetrievePageHtml(currentPageNumber);
            }
        }

        private void HandleValidPosts(IList<ForumPost> scannedPosts)
        {
            foreach (var post in scannedPosts)
            {
                var repoPost = this.repo.FindSpecificPost(post.ForumPostNumber);

                if (repoPost == null || repoPost.LastScanned - repoPost.DateTime < TimeSpan.FromMinutes(5))
                {
                    this.repo.UpsertPost(post);
                }
            }
        }

        private void UpdateVotesForWholeGame(string startOfGameForumPostNumber)
        {
            var allPosts = this.repo.FindAllPosts().OrderBy(p => p.ForumPostNumber).ToList();
            var relevantPosts = this.FindAllPostsAfter(allPosts, startOfGameForumPostNumber);

            this.UpsertVotes(relevantPosts);
        }

        private void UpdateVotes(IEnumerable<ForumPost> scannedPosts)
        {
            foreach (var post in scannedPosts)
            {
                var repoPost = this.repo.FindSpecificPost(post.ForumPostNumber);

                if (repoPost == null || repoPost.LastScanned - repoPost.DateTime < TimeSpan.FromMinutes(5))
                {
                    this.repo.UpsertPost(post);
                    this.repo.DeleteVotes(post.ForumPostNumber);
                    this.UpsertVotes(post);
                }
            }
        }

        private void UpsertVotes(ForumPost relevantPost)
        {
            this.UpsertVotes(new ForumPost[] { relevantPost });
        }

        private void UpsertVotes(IList<ForumPost> relevantPosts)
        {
            foreach (var post in relevantPosts)
            {
                foreach (var vote in this.voteScanner.ScanForVotes(post))
                {
                    this.repo.UpsertVote(vote);
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
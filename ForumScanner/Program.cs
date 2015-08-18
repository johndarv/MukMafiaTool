using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using MukMafiaTool.Database;
using MukMafiaTool.ForumScanner.Extensions;
using MukMafiaTool.ForumScanner.Pages;
using MukMafiaTool.Model;
using OpenQA.Selenium.Chrome;

namespace MukMafiaTool.ForumScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            MongoRepository repo = new MongoRepository();
            GatherNewPosts(repo);
            CalculateVotes(repo, "10522028");

            repo.UpdateLastUpdated();
        }

        private static void CalculateVotes(MongoRepository repo, string startOfDayForumPostNumber)
        {
            // Delete votes collection
            repo.WipeVotes();

            var allPosts = repo.FindAllPosts().ToList();
            var startOfDayPost = allPosts.Single(p => string.Equals(p.ForumPostNumber, startOfDayForumPostNumber));
            allPosts = allPosts.OrderBy(p => p.ForumPostNumber).ToList();
            var postNumbers = allPosts.Select(p => p.ForumPostNumber);
            var index = allPosts.IndexOf(startOfDayPost);
            var posts = allPosts.GetRange(index, allPosts.Count - index);

            var players = allPosts.Select(p => p.Poster).Distinct().ToList();

            IList<Vote> votes = new List<Vote>();

            foreach (var post in posts)
            {
                votes = votes.Concat<Vote>(post.GetVotes(players)).ToList();
            }

            votes = votes.OrderBy(v => v.ForumPostNumber).ThenBy(v => v.PostContentIndex).ToList();

            var jolly = votes.Where(v => v.Voter == "Jolly");

            foreach (var vote in votes)
            {
                repo.ProcessVote(vote);
            }
        }

        private static void GatherNewPosts(MongoRepository repo)
        {
            ChromeDriver driver = new ChromeDriver();

            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(10));

            driver.Navigate().GoToUrl("http://www.rllmukforum.com/");

            ForumHomePage homePage = new ForumHomePage(driver);

            homePage.SignIn();

            var threadUrl = ConfigurationManager.AppSettings["ThreadUrl"];

            driver.Navigate().GoToUrl(threadUrl);

            MafiascumThreadPage threadPage = new MafiascumThreadPage(driver);

            MafiascumThreadPage currentPage = threadPage.GoToLastPage();

            bool pageHasAllNewPosts = true;

            while (pageHasAllNewPosts && currentPage != null)
            {
                IList<ForumPost> posts = currentPage.GetAllPosts(repo.FindAllDays());

                pageHasAllNewPosts = repo.InsertNewPosts(posts);

                currentPage = currentPage.GoBackOnePage();
            }

            driver.Close();
            driver.Dispose();
        }
    }
}

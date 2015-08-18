using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MukMafiaTool.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using MukMafiaTool.ForumScanner.Extensions;
using System.Threading;
using System.Web;

namespace MukMafiaTool.ForumScanner.Pages
{
    public class MafiascumThreadPage : PageBase
    {
        [FindsBy(How = How.PartialLinkText, Using = "»")]
        private IWebElement _lastPageLink = null;

        [FindsBy(How = How.PartialLinkText, Using = "PREV")]
        private IWebElement _previousPageLink = null;

        [FindsBy(How = How.Id, Using = "submit_post")]
        private IWebElement _submitPostButton = null;

        public MafiascumThreadPage(IWebDriver driver)
            :base(driver)
        {
        }

        public IList<ForumPost> GetAllPosts(IList<Day> days)
        {
            IList<ForumPost> forumPosts = new List<ForumPost>();

            var postElements = Driver.FindElements(By.ClassName("post_wrap"));

            foreach(var postElement in postElements)
            {
                var newPost = ConvertElementToForumPost(days, postElement);

                forumPosts.Add(newPost);
            }

            return forumPosts;
        }

        public MafiascumThreadPage GoBackOnePage()
        {
            try
            {
                _previousPageLink.Click();
            }
            catch
            {
                return null;
            }

            Thread.Sleep(TimeSpan.FromSeconds(2));

            Driver.WaitUntil(d => d.FindElement(By.Id("submit_post")).Displayed);

            return new MafiascumThreadPage(Driver);
        }

        public MafiascumThreadPage GoToLastPage()
        {
            Driver.WaitUntil(d => _lastPageLink.Displayed);

            _lastPageLink.Click();

            Thread.Sleep(TimeSpan.FromSeconds(2));

            Driver.WaitUntil(d => d.FindElement(By.Id("submit_post")).Displayed);

            return new MafiascumThreadPage(Driver);
        }

        private ForumPost ConvertElementToForumPost(IList<Day> days, IWebElement postElement)
        {
            var newPost = new ForumPost();

            // Find Page Number
            var currentUrl = Driver.Url;
            var index = currentUrl.IndexOf("page", StringComparison.OrdinalIgnoreCase);
            newPost.PageNumber = int.Parse(currentUrl.Substring(index + 5, currentUrl.Length - (index + 5)));

            // Find poster
            newPost.Poster = postElement.FindElement(By.XPath(".//span[@itemprop='creator name']")).Text;

            // Find post number
            var postNumberElement = postElement.FindElement(By.XPath(".//a[@itemprop='replyToUrl']"));

            // Get forum post id
            newPost.ForumPostNumber = postNumberElement.GetAttribute("data-entry-pid");

            var postNumberString = postNumberElement.Text.Trim();
            postNumberString = postNumberString.Replace("#", string.Empty);
            newPost.ThreadPostNumber = int.Parse(postNumberString);

            // Find Day
            newPost.Day = DetermineDay(days, newPost.ForumPostNumber);

            // Find DateTime
            var dateTimeString = postElement.FindElement(By.XPath(".//abbr[@itemprop='commentTime']")).Text;

            newPost.DateTime = ConvertDate(dateTimeString);

            // Find Content
            var content = postElement.FindElement(By.XPath(".//div[@itemprop='commentText']")).GetAttribute("innerHTML");

            newPost.Content = new HtmlString(content);

            return newPost;
        }

        private int DetermineDay(IList<Day> days, string forumPostNumber)
        {
            foreach (var day in days)
            {
                if (string.Compare(forumPostNumber, day.StartForumPostNumber) >= 0)
                {
                    if (string.IsNullOrEmpty(day.EndForumPostNumber) || string.Compare(forumPostNumber, day.EndForumPostNumber) < 0)
                    {
                        return day.Number;
                    }
                }
            }

            return 0;
        }

        private DateTime ConvertDate(string dateTimeString)
        {
            if (dateTimeString.StartsWith("Today"))
            {
                var time = dateTimeString.Split(',')[1].Trim();
                dateTimeString = DateTime.Now.Date.ToShortDateString() + " " + time;
            }
            else if (dateTimeString.StartsWith("Yesterday"))
            {
                var time = dateTimeString.Split(',')[1].Trim();
                dateTimeString = DateTime.Now.AddDays(-1).Date.ToShortDateString() + " " + time;
            }
            else
            {
                // format example: 26 August 2014 - 04:00 PM
                return DateTime.SpecifyKind(DateTime.ParseExact(dateTimeString, "dd MMMM yyyy - hh:mm tt", null), DateTimeKind.Local);
            }

            return DateTime.SpecifyKind(DateTime.Parse(dateTimeString), DateTimeKind.Local);
        }
    }
}

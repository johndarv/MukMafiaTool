using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HtmlAgilityPack;
using MukMafiaTool.Model;

namespace MukMafiaTool.ForumScanService
{
    public class PageScanner
    {
        IList<Day> _days;

        public PageScanner(IList<Day> days)
        {
            _days = days;
        }

        public IList<Model.ForumPost> RetrieveAllPosts(string pageContent, int currentPageNumber)
        {
            IList<ForumPost> forumPosts = new List<ForumPost>();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(pageContent);

            var postDivs = doc.DocumentNode.Descendants("div")
                .Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value == "post_wrap");

            foreach (var postDiv in postDivs)
            {
                var newPost = ConvertElementToForumPost(_days, postDiv);
                newPost.PageNumber = currentPageNumber;

                forumPosts.Add(newPost);
            }

            return forumPosts;
        }

        private ForumPost ConvertElementToForumPost(IList<Day> days, HtmlNode postDiv)
        {
            var newPost = new ForumPost()
            {
                ManuallyEdited = false,
            };
            
            // Find poster
            newPost.Poster = postDiv.SelectSingleNode(".//span[@itemprop='creator name']").InnerText;

            // Find forum post number
            var postNumberElement = postDiv.SelectSingleNode(".//a[@itemprop='replyToUrl']");
            newPost.ForumPostNumber = postNumberElement.Attributes["data-entry-pid"].Value;

            // Find ThreadPostNumber
            var postNumberString = postNumberElement.InnerText.Trim();
            postNumberString = postNumberString.Replace("#", string.Empty);
            newPost.ThreadPostNumber = int.Parse(postNumberString);

            // Find Day
            newPost.Day = DetermineDay(newPost.ForumPostNumber);

            // Find DateTime
            var dateTimeString = postDiv.SelectSingleNode(".//abbr[@itemprop='commentTime']").InnerText;

            newPost.DateTime = ConvertDate(dateTimeString);

            // Find Content
            newPost.Content = RetrieveContent(postDiv);

            // Set LastScanned time
            newPost.LastScanned = DateTime.UtcNow;

            return newPost;
        }

        private static HtmlString RetrieveContent(HtmlNode postDiv)
        {
            HtmlDocument doc = new HtmlDocument();
            var commentDiv = postDiv.SelectSingleNode(".//div[@itemprop='commentText']");
            doc.LoadHtml(commentDiv.OuterHtml);

            var blockquotes = doc.DocumentNode.SelectNodes(".//blockquote");

            HtmlNode parentNode = null;

            if (blockquotes == null)
            {
                parentNode = postDiv.SelectSingleNode(".//div[@itemprop='commentText']");
            }
            else
            {
                foreach (var blockquote in blockquotes)
                {
                    var author = blockquote.GetAttributeValue("data-author", string.Empty);
                    var forumPostNumber = blockquote.GetAttributeValue("data-cid", string.Empty);

                    string html = "<p class=\"citation\">Quote<span></span></p>";
                    if (!string.IsNullOrEmpty(author) && !string.IsNullOrEmpty(forumPostNumber))
                    {
                        html = string.Format(
                        "<p class=\"citation\">{0} said:<a class=\"snapback right\" rel=\"citation\" href=\"{1}{2}\"><img src=\"http://www.rllmukforum.com/public/style_images/master/snapback.png\"></a></p>",
                        author,
                        "http://www.rllmukforum.com/index.php?app=forums&amp;module=forums&amp;section=findpost&amp;pid=",
                        forumPostNumber);
                    }
                    
                    var citationNode = HtmlNode.CreateNode(html);

                    doc.DocumentNode.Descendants().Single(n => n == blockquote).ParentNode.InsertBefore(citationNode, blockquote);
                }

                parentNode = doc.DocumentNode.SelectSingleNode(".//div[@itemprop='commentText']");
            }

            return new HtmlString(parentNode.InnerHtml);
        }

        private int DetermineDay(string forumPostNumber)
        {
            foreach (var day in _days)
            {
                if (string.Compare(forumPostNumber, day.StartForumPostNumber) >= 0)
                {
                    if (string.IsNullOrEmpty(day.EndForumPostNumber) || string.Compare(forumPostNumber, day.EndForumPostNumber) <= 0)
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
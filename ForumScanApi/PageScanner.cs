namespace ForumScanApi
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web;
    using HtmlAgilityPack;
    using MukMafiaTool.Model;

    public class PageScanner
    {
        private IList<Day> days;

        public PageScanner(IList<Day> days)
        {
            this.days = days;
        }

        public IList<ForumPost> RetrieveAllPosts(string pageContent, int currentPageNumber)
        {
            IList<ForumPost> forumPosts = new List<ForumPost>();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(pageContent);

            var postElements = doc.DocumentNode.Descendants("article")
                .Where(d => d.Attributes.Contains("id") && d.Attributes["id"].Value.StartsWith("elComment_", false, CultureInfo.InvariantCulture));

            foreach (var postElement in postElements)
            {
                var newPost = this.ConvertElementToForumPost(this.days, postElement);
                newPost.PageNumber = currentPageNumber;

                forumPosts.Add(newPost);
            }

            return forumPosts;
        }

        private static HtmlString RetrieveContent(HtmlNode commentNode)
        {
            HtmlDocument doc = new HtmlDocument();
            var commentDiv = commentNode.SelectSingleNode(".//div[@data-role='commentContent']");
            doc.LoadHtml(commentDiv.OuterHtml);

            var blockquotes = doc.DocumentNode.SelectNodes(".//blockquote");

            HtmlNode parentNode = null;

            parentNode = commentDiv;

            if (blockquotes.Any())
            {
                foreach (var blockquote in blockquotes)
                {
                    // Generate a new quote header node
                    var author = blockquote.GetAttributeValue("data-ipsquote-username", string.Empty);
                    var forumPostNumber = blockquote.GetAttributeValue("data-ipsquote-contentcommentid", string.Empty);

                    string htmlForBlockquoteHeader = "<div class=\"ipsQuote_contents\"><p>Quote</p></div>";
                    if (!string.IsNullOrEmpty(author) && !string.IsNullOrEmpty(forumPostNumber))
                    {
                        htmlForBlockquoteHeader = string.Format(
                            "<div class=\"ipsQuote_citation  ipsQuote_open\"><a class=\"ipsPos_right\" href=\"{1}{2}\"><img src=\"http://www.rllmukforum.com/public/style_images/master/snapback.png\"></a>{0} said:</div>",
                            author,
                            "http://www.rllmukforum.com/index.php?app=forums&amp;module=forums&amp;section=findpost&amp;pid=",
                            forumPostNumber);
                    }

                    // Replace the one in the extracted html with the generated replacement
                    var replacementQuoteHeaderNode = HtmlNode.CreateNode(htmlForBlockquoteHeader);

                    var currentQuoteHeaderNode = blockquote.SelectSingleNode(".//div[@class='ipsQuote_citation ipsQuote_open'");
                    var currentQuoteContents = blockquote.SelectSingleNode(".//div[@class='ipsQuote_contents']");

                    currentQuoteHeaderNode.Remove();

                    blockquote.InsertBefore(replacementQuoteHeaderNode, currentQuoteContents);
                }
            }

            return new HtmlString(parentNode.InnerHtml);
        }

        private ForumPost ConvertElementToForumPost(IList<Day> days, HtmlNode postNode)
        {
            var newPost = new ForumPost()
            {
                ManuallyEdited = false,
            };

            // Find poster
            newPost.Poster = postNode.SelectSingleNode(".//h3[@itemprop='creator']//a[@class='ipsType_break']").InnerText;

            // Find forum post number
            var commentDiv = postNode.SelectSingleNode(".//div[contains(@class, 'ipsComment_content')]");
            newPost.ForumPostNumber = commentDiv.Attributes["data-commentid"].Value;

            // Find Day
            newPost.Day = this.DetermineDay(newPost.ForumPostNumber);

            // Find DateTime
            var dateTimeNode = commentDiv.SelectSingleNode(".//time");
            var dateTimeString = dateTimeNode.Attributes["datetime"].Value;

            newPost.DateTime = this.ConvertDate(dateTimeString);

            // Find Content
            var contentHtml = postNode.SelectSingleNode(".//div[@data-role='commentContent']").InnerHtml;
            newPost.Content = new HtmlString(contentHtml);

            // Set LastScanned time
            newPost.LastScanned = DateTime.UtcNow;

            return newPost;
        }

        private int DetermineDay(string forumPostNumber)
        {
            foreach (var day in this.days)
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
            else if (dateTimeString[10] == 'T' && dateTimeString[19] == 'Z')
            {
                return DateTime.Parse(dateTimeString);
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
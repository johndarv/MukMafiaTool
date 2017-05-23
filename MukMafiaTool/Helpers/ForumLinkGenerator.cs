namespace MukMafiaTool.Helpers
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using static System.FormattableString;

    public class ForumLinkGenerator
    {
        public static MvcHtmlString LinkToForumPost(string forumPostNumber, string linkText)
        {
            var linkUrl = GenerateLinkUrl(forumPostNumber);

            var htmlString = new MvcHtmlString(Invariant($"<a href=\"{linkUrl}\">{linkText}</a>"));

            return htmlString;
        }

        public static MvcHtmlString LinkToForumPost(string forumPostNumber, string linkText, string linkClasses)
        {
            var linkUrl = GenerateLinkUrl(forumPostNumber);

            var htmlString = new MvcHtmlString(Invariant($"<a href=\"{linkUrl}\" class=\"{linkClasses}\">{linkText}</a>"));

            return htmlString;
        }

        private static string GenerateLinkUrl(string forumPostNumber)
        {
            string forumBaseAddress = "https://www.officeoffline.co.uk";
            var forumBaseAddressCookie = HttpContext.Current.Request.Cookies["rllmukBaseAddress"];

            if (forumBaseAddressCookie != null)
            {
                forumBaseAddress = forumBaseAddressCookie.Value;
            }

            return Invariant($"{forumBaseAddress}/index.php?app=forums&module=forums&section=findpost&pid={forumPostNumber}");
        }
    }
}
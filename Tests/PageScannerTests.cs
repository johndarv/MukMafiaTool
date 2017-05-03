using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using ForumScanApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MukMafiaTool.Common;
using MukMafiaTool.Model;

namespace Tests
{
    [TestClass]
    public class PageScannerTests
    {
        [TestMethod]
        public void FullPageTest()
        {
            var html = File.ReadAllText("Html\\exampleForumPage.txt");

            var pageScanner = new PageScanner(new List<Day>());

            pageScanner.RetrieveAllPosts(html, 1);
        }

        [TestMethod]
        public void SinglePostWithBlockquoteTest()
        {
            var html = File.ReadAllText("Html\\exampleForumPageSinglePost.txt");

            var pageScanner = new PageScanner(new List<Day>());

            var comments = pageScanner.RetrieveAllPosts(html, 1);

            comments.Count.ShouldBeEquivalentTo(1, because: "there is only one post on the page.");

            var comment = comments.Single();

            comment.Content.ToString().RemoveWhiteSpace().RemoveNewLineAndTabChars().ShouldAllBeEquivalentTo(
                @"<blockquote class=""ipsQuote"" data-ipsquote="""" data-ipsquote-contentapp=""forums"" data-ipsquote-contentclass=""forums_Topic"" data-ipsquote-contentcommentid=""11354295"" data-ipsquote-contentid=""286745"" data-ipsquote-contenttype=""forums"" data-ipsquote-timestamp=""1492701431"" data-ipsquote-userid=""27615"" data-ipsquote-username=""The Donald"">

<div class=""ipsQuote_citation ipsQuote_open""> <div class=""ipsQuote_citation  ipsQuote_open""><a class=""ipsPos_right"" href=""http://www.rllmukforum.com/?app=core&amp;module=system&amp;controller=content&amp;do=find&amp;content_class=forums_Topic&amp;content_id=286745&amp;content_commentid=11354295""><img src=""http://www.rllmukforum.com/public/style_images/master/snapback.png"">The Donald said: </div>

<div class=""ipsQuote_contents ipsClearfix"">
    <p>
        Guys, this is going to be the most beautiful game of Mafiascum that has ever been played.
    </p>
</div>
</blockquote>

<p>
	lol
</p>".RemoveWhiteSpace().RemoveNewLineAndTabChars(),
                because: "this is what it should be exactly like.");

            //var indexOfBlockQuote = comment.Content.ToString().IndexOf("<blockquote class=\"ipsQuote\" data-ipsquote=\"\" data-ipsquote-contentapp=\"forums\" data-ipsquote-contentclass=\"forums_Topic\" data-ipsquote-contentcommentid=\"11354295\" data-ipsquote-contentid=\"286745\" data-ipsquote-contenttype=\"forums\" data-ipsquote-timestamp=\"1492701431\" data-ipsquote-userid=\"27615\" data-ipsquote-username=\"The Donald\">");
            //var indexOfQuoteHeader = comment.Content.ToString().IndexOf("<div class=\"ipsQuote_citation  ipsQuote_open\"><a class=\"ipsPos_right\" href=\"http://www.rllmukforum.com/?app=core&amp;module=system&amp;controller=content&amp;do=find&amp;content_class=forums_Topic&amp;content_id=286745&amp;content_commentid=11354295\"><img src=\"http://www.rllmukforum.com/public/style_images/master/snapback.png\"></a>");
            //var indexOfNameWithinQuoteHeader = comment.Content.ToString().IndexOf("The Donald said:</div>");

            //indexOfBlockQuote.Should().BeGreaterThan(-1, because: "the blockquote should be present and just as it was in the source.");
            //indexOfQuoteHeader.Should().BeGreaterThan(indexOfBlockQuote, because: "the header to the quotation block should appear after (ie. within) the blockquote.");
            //indexOfNameWithinQuoteHeader.Should().BeGreaterThan(indexOfQuoteHeader, because: "the name within the to the quotation header should appear after (ie. within) the header itself.");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MukMafiaTool.Database;
using MukMafiaTool.ForumScanService;
using ForumScanService;
using MukMafiaTool.Model;

namespace MukMafiaToolTests
{
    [TestClass]
    public class InternalForumScannerTests
    {
        [Ignore]
        [TestMethod]
        public void DoWholeUpdateTest()
        {
            ForumScanner scanner = new ForumScanner(new MongoRepository());

            scanner.DoWholeUpdate();
        }

        [TestMethod]
        public void GetVotesTest()
        {
            ForumPost post = new ForumPost
            {
                DateTime = new DateTime(2015, 8, 18, 9, 0, 0),
                Day = 4,
                ForumPostNumber = "1111111",
                PageNumber = 50,
                Poster = "John0",
                ThreadPostNumber = 100,
                Content = new HtmlString(@"<div class=""post_body"">
                    
					<p class=""citation"">John0, on 17 Aug 2015 - 2:33 PM, said:<a class=""snapback right"" rel=""citation"" href=""http://www.rllmukforum.com/index.php?app=forums&amp;module=forums&amp;section=findpost&amp;pid=10526689""><img src=""http://www.rllmukforum.com/public/style_images/master/snapback.png""></a></p><blockquote class=""ipsBlockquote built"" data-author=""John0"" data-cid=""10526689"" data-time=""1439818425""><p>Surely we have to lynch snowbind now.</p></blockquote><br><p>I missed those nuances. As much as the lack of movement on sith for his play style frustrates me, I was prepared to vote snowbind for another daft joke (particularly after dino appeared to learn his lesson over the I'M WIGGUM thing), but it's not, is it? He's inadvertently let slip. If he swings today and turns out to be one of Tony's lot, we have a good lead on day 2.<br>&nbsp;</p><p><strong class=""bbc"">unvote: sith</strong></p><p><strong class=""bbc"">vote: snowbind</strong><br>&nbsp;</p><p class=""citation"">Jolly, on 17 Aug 2015 - 1:04 PM, said:<a class=""snapback right"" rel=""citation"" href=""http://www.rllmukforum.com/index.php?app=forums&amp;module=forums&amp;section=findpost&amp;pid=10526541""><img src=""http://www.rllmukforum.com/public/style_images/master/snapback.png""></a></p><blockquote class=""ipsBlockquote built"" data-author=""Jolly"" data-cid=""10526541"" data-time=""1439813084""><p>Complete turnaround on what he thinks about Gos when the general town feeling is that his roleclaim is genuine.&nbsp; Annoyingly help out by TehStu at this point.</p></blockquote><p>I want to clarify this, though. We didn't have the best track record in the last game for mafia lynches, so cherry picking a post of snowbind's where he's talking about gos prior to the reveal isn't nearly as helpful as post-reveal. It wasn't the smoking ORLY gun it was purported to be. Everything else you've pointed out, though? On the money.</p>
					
					<br>
					
				
                </div>"),
            };

            var playerNames = new List<string>() { "John0", "Xevious", "snowbind" };

            var votes = ForumScanner.GetVotes(post, playerNames);

            Assert.IsTrue(votes.Count == 2);
        }

        [TestMethod]
        public void GetVotesTest2()
        {
            ForumPost post = new ForumPost
            {
                DateTime = new DateTime(2015, 8, 18, 9, 0, 0),
                Day = 4,
                ForumPostNumber = "1111111",
                PageNumber = 50,
                Poster = "Alask",
                ThreadPostNumber = 100,
                Content = new HtmlString(@"<div class=""post_body"">
                    
					<blockquote class=""ipsBlockquote"" data-author=""footle"" data-cid=""10527247"" data-time=""1439839427""><br>
<div><br>
<p>Please keep voting. There's always the possibility that Bennette is the Reverend, though you'd assume the reverend would vote earlier.</p>
</div>
</blockquote>
<p>&#160;</p>
<p>Not entirely clear if that'd mean the vote counts reached and therefore day ends though or not though..</p>
<p>&#160;</p>
<p>anyway</p>
<p>&#160;</p>
<p><strong class='bbc'>unvote<p>
<p>vote : snowbind</strong></p>
<p>&#160;</p>
<p>should cover it shouldn't it?</p>
					
					<br>
					
				
                </div>"),
            };

            var playerNames = new List<string>() { "John0", "Xevious", "snowbind" };

            var votes = ForumScanner.GetVotes(post, playerNames);

            Assert.IsTrue(votes.Count == 2);
        }

        [TestMethod]
        public void GetVotesTest3()
        {
            ForumPost post = new ForumPost
            {
                DateTime = new DateTime(2015, 8, 18, 9, 0, 0),
                Day = 4,
                ForumPostNumber = "1111111",
                PageNumber = 50,
                Poster = "snowbind",
                ThreadPostNumber = 100,
                Content = new HtmlString(@"<div class=""post_body"">
                    
					<p><strong>unvote</strong></p>
<p>&#160;</p>
<p><b>vote gmass</b></p>

					
					<br>
					
				
                </div>"),
            };

            var playerNames = new List<string>() { "John0", "Xevious", "snowbind", "GMass" };

            var votes = ForumScanner.GetVotes(post, playerNames);

            Assert.IsTrue(votes.Count == 2);
        }

        [TestMethod]
        public void GetVotesTest4()
        {
            ForumPost post = new ForumPost
            {
                DateTime = new DateTime(2015, 8, 18, 9, 0, 0),
                Day = 4,
                ForumPostNumber = "1111111",
                PageNumber = 50,
                Poster = "Timmo",
                ThreadPostNumber = 100,
                Content = new HtmlString(@"<div class=""post_body"">
                    
					<p class=""citation"">Dinobot, on 12 Aug 2015 - 7:42 PM, said:<a class=""snapback right"" rel=""citation"" href=""http://www.rllmukforum.com/index.php?app=forums&amp;module=forums&amp;section=findpost&amp;pid=10520532""><img src=""http://www.rllmukforum.com/public/style_images/master/snapback.png""></a></p><blockquote class=""ipsBlockquote built"" data-author=""Dinobot"" data-cid=""10520532"" data-time=""1439404955""><p>Well that's it done.<br>
<br>
<em><strong class=""bbc"">I'M WIGGUM!</strong></em><strong class=""bbc""><strong class=""bbc""><br>
<br>
Good lord what are the odds of this happening? Got to be about 48-1.<br>
<br>
Gods sake spork.</strong></strong></p></blockquote><strong class=""bbc"">[b]unvote</strong><br>
<br>
What?!
					
					<br>
					
				
                </div>"),
            };

            var playerNames = new List<string>() { "John0", "Xevious", "snowbind", "GMass" };

            var votes = ForumScanner.GetVotes(post, playerNames);

            Assert.IsTrue(votes.Count == 1);
        }

        [TestMethod]
        public void GetVotesTest5()
        {
            ForumPost post = new ForumPost
            {
                DateTime = new DateTime(2015, 8, 18, 9, 0, 0),
                Day = 4,
                ForumPostNumber = "1111111",
                PageNumber = 50,
                Poster = "GMass",
                ThreadPostNumber = 100,
                Content = new HtmlString(@"<div class=""post_body"">
                    
					I voted for Timmo because I felt he was trying to direct the vote. I unvoted later. I reckon we have perhaps 1 Mafia kill and 1 serial killer kill here.    Dr Nick being too unsure to use his power and 1 of the Mafia teams not bothering in order to muddy the waters. <br>Merge<br><br>
Hmm scratch that 1 Mafia and 1 failed recruitment perhaps.
					
					<br>
					
				
                </div>"),
            };

            var playerNames = new List<string>() { "John0", "Xevious", "snowbind", "GMass" };

            var votes = ForumScanner.GetVotes(post, playerNames);

            Assert.IsTrue(votes.Count == 0);
        }

        [TestMethod]
        public void GetVotesTest6()
        {
            ForumPost post = new ForumPost
            {
                DateTime = new DateTime(2015, 8, 18, 9, 0, 0),
                Day = 4,
                ForumPostNumber = "1111111",
                PageNumber = 50,
                Poster = "PaulM",
                ThreadPostNumber = 100,
                Content = new HtmlString(@"<div class=""post_body"">
                    
					<p>You know what, I don't know if it'll make a difference or not, but&nbsp;<strong>unvote,&nbsp;</strong><strong>vote Liamness</strong></p>

					
					<br>
					
				
                </div>"),
            };

            var playerNames = new List<string>() { "John0", "Xevious", "snowbind", "GMass", "Liamness" };

            var votes = ForumScanner.GetVotes(post, playerNames);

            Assert.IsTrue(votes.Count == 2);
        }

        [TestMethod]
        public void GetVotesTest7()
        {
            ForumPost post = new ForumPost
            {
                DateTime = new DateTime(2015, 8, 18, 9, 0, 0),
                Day = 4,
                ForumPostNumber = "1111111",
                PageNumber = 50,
                Poster = "Danster",
                ThreadPostNumber = 100,
                Content = new HtmlString(@"<div class=""post_body"">
                    
					<p>I think i'm going to <strong class=""bbc"">unvote</strong><br><br>Gos is a wasted vote. If he isn't lynched he will be whacked tonight or recruited (and so if he isn't killed overnight he'll have to be tomorrow) unless he's protected, or watched... ffs<br><br>I think snowbind is iffy, and Liamness has now done just enough to not be modkilled, spork still looks panicky AND if dino is right is our best chance of mafia.<br><br>I say {b]vote: spork[/b] if he's town, well then we can re-group and figure who was calling for his head, if he's mafia or a n other then jobs a good un.</p>
					
					<br>
					
				
                </div>"),
            };

            var playerNames = new List<string>() { "John0", "Xevious", "snowbind", "GMass", "Liamness", "spork" };

            var votes = ForumScanner.GetVotes(post, playerNames);

            Assert.IsTrue(votes.Count == 2);
        }

        [TestMethod]
        public void GetVotesTest8()
        {
            ForumPost post = new ForumPost
            {
                DateTime = new DateTime(2015, 8, 18, 9, 0, 0),
                Day = 4,
                ForumPostNumber = "1111111",
                PageNumber = 50,
                Poster = "gospvg",
                ThreadPostNumber = 100,
                Content = new HtmlString(@"<div class=""post_body"">
                    
					<p>After the post 234 to post 264 the following vote actions occurred against Dinobot</p>
<p>&nbsp;</p>
<p>TehStu voted (237)</p>
<p>TehStu unvoted (247)</p>
<p>Benny voted (253)</p>
<p>Danster changed vote (256)</p>
<p>Footle unvote (258)</p>
<p>&nbsp;</p>
<p>Awfully convenient of Danster &amp; Footle to unvote before 264, maybe they read the clue in post 234?</p>
					
					<br>
					
				
                </div>"),
            };

            var playerNames = new List<string>() { "John0", "Xevious", "snowbind", "GMass", "Liamness", "spork" };

            var votes = ForumScanner.GetVotes(post, playerNames);

            Assert.IsTrue(votes.Count == 0);
        }

        [TestMethod]
        public void GetVotesTestSemiColon()
        {
            ForumPost post = new ForumPost
            {
                DateTime = new DateTime(2015, 8, 18, 9, 0, 0),
                Day = 4,
                ForumPostNumber = "1111111",
                PageNumber = 50,
                Poster = "bennette98",
                ThreadPostNumber = 100,
                Content = new HtmlString(@"<div class=""post_body"">
                    
					<p class=""citation"">snowbind, on 16 Aug 2015 - 9:03 PM, said:<a class=""snapback right"" rel=""citation"" href=""http://www.rllmukforum.com/index.php?app=forums&amp;module=forums&amp;section=findpost&amp;pid=10525781""><img src=""http://www.rllmukforum.com/public/style_images/master/snapback.png""></a></p><blockquote class=""ipsBlockquote built"" data-author=""snowbind"" data-cid=""10525781"" data-time=""1439755414"">
<div>
<p>Due to the situatuon I am going to remove my gooner vote.<br><br><strong>unvote</strong></p>
</div>
</blockquote>
<p>this doesn't make sense. You thought the character gooner was playing was scum, just because it is plums instead of gooner now doesn't mean he isn't potentially scum.</p>
<p>&nbsp;</p>
<p><strong>VOTE; Snowbind</strong></p>
					
					<br>
					
				
                </div>"),
            };

            var playerNames = new List<string>() { "John0", "Xevious", "snowbind", "GMass", "Liamness", "spork" };

            var votes = ForumScanner.GetVotes(post, playerNames);

            Assert.IsTrue(votes.Count == 1);
        }
    }
}

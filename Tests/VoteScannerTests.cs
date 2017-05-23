namespace Tests
{
    using System;
    using System.Linq;
    using System.Web;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using MukMafiaTool.Common;
    using MukMafiaTool.Model;
    using Tests.Helpers;

    [TestClass]
    public class VoteScannerTests
    {
        private VoteScanner voteScanner;

        public VoteScannerTests()
        {
            var players = new Player[]
            {
                PlayerGenerator.GeneratePlayer(name: "John0", aliases: new string[] { "John" }),
                PlayerGenerator.GeneratePlayer(name: "Xevious", aliases: new string[0]),
                PlayerGenerator.GeneratePlayer(name: "snowbind", aliases: new string[0]),
                PlayerGenerator.GeneratePlayer(name: "GMass", aliases: new string[0]),
                PlayerGenerator.GeneratePlayer(name: "Liamness", aliases: new string[0]),
                PlayerGenerator.GeneratePlayer(name: "spork", aliases: new string[0]),
                PlayerGenerator.GeneratePlayer(name: "Alask", aliases: new string[0]),
                PlayerGenerator.GeneratePlayer(name: "PaulM", aliases: new string[0]),
                PlayerGenerator.GeneratePlayer(name: "Danster", aliases: new string[0]),
                PlayerGenerator.GeneratePlayer(name: "gospvg", aliases: new string[0]),
                PlayerGenerator.GeneratePlayer(name: "Strategos", aliases: new string[0]),
                PlayerGenerator.GeneratePlayer(name: "bennette98", aliases: new string[0]),
                PlayerGenerator.GeneratePlayer(name: "Don Wiskerando", aliases: new string[0]),
                PlayerGenerator.GeneratePlayer(name: "The Grand Pursuivant", aliases: new string[0]),
                PlayerGenerator.GeneratePlayer(name: "Mr. Blonde", aliases: new string[0]),
                PlayerGenerator.GeneratePlayer(name: "Mr. Violet", aliases: new string[] { "Mr Violet", "Violet", "Mr.Violet", "MrViolet" }),
                PlayerGenerator.GeneratePlayer(name: "Mr. Viridian", aliases: new string[] { "Mr.Viridian", "Mr Viridian", "Viridian", "MrViridian" }),
                PlayerGenerator.GeneratePlayer(name: "Moderator", aliases: new string[0], participating: false),
                PlayerGenerator.GeneratePlayer(name: "Player With Aliases", aliases: new string[] { "PlayerWithAliases", "PWA" }),
            };

            var mockRepo = new Mock<IRepository>();
            mockRepo.Setup(m => m.FindAllPlayers())
                .Returns(players);

            this.voteScanner = new VoteScanner(mockRepo.Object, 8);
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
                Content = new HtmlString(@"<div class=""post_body"">
                    
					<p class=""citation"">John0, on 17 Aug 2015 - 2:33 PM, said:<a class=""snapback right"" rel=""citation"" href=""http://www.rllmukforum.com/index.php?app=forums&amp;module=forums&amp;section=findpost&amp;pid=10526689""><img src=""http://www.rllmukforum.com/public/style_images/master/snapback.png""></a></p><blockquote class=""ipsBlockquote built"" data-author=""John0"" data-cid=""10526689"" data-time=""1439818425""><p>Surely we have to lynch snowbind now.</p></blockquote><br><p>I missed those nuances. As much as the lack of movement on sith for his play style frustrates me, I was prepared to vote snowbind for another daft joke (particularly after dino appeared to learn his lesson over the I'M WIGGUM thing), but it's not, is it? He's inadvertently let slip. If he swings today and turns out to be one of Tony's lot, we have a good lead on day 2.<br>&nbsp;</p><p><strong class=""bbc"">unvote: sith</strong></p><p><strong class=""bbc"">vote: snowbind</strong><br>&nbsp;</p><p class=""citation"">Jolly, on 17 Aug 2015 - 1:04 PM, said:<a class=""snapback right"" rel=""citation"" href=""http://www.rllmukforum.com/index.php?app=forums&amp;module=forums&amp;section=findpost&amp;pid=10526541""><img src=""http://www.rllmukforum.com/public/style_images/master/snapback.png""></a></p><blockquote class=""ipsBlockquote built"" data-author=""Jolly"" data-cid=""10526541"" data-time=""1439813084""><p>Complete turnaround on what he thinks about Gos when the general town feeling is that his roleclaim is genuine.&nbsp; Annoyingly help out by TehStu at this point.</p></blockquote><p>I want to clarify this, though. We didn't have the best track record in the last game for mafia lynches, so cherry picking a post of snowbind's where he's talking about gos prior to the reveal isn't nearly as helpful as post-reveal. It wasn't the smoking ORLY gun it was purported to be. Everything else you've pointed out, though? On the money.</p>
					
					<br>
					
				
                </div>
"),
            };

            var votes = this.voteScanner.ScanForVotes(post);

            Assert.IsTrue(votes.Count() == 2);
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
					
				
                </div>
"),
            };

            var votes = this.voteScanner.ScanForVotes(post);

            Assert.IsTrue(votes.Count() == 2);
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
                Content = new HtmlString(@"<div class=""post_body"">
                    
					<p><strong>unvote</strong></p>
<p>&#160;</p>
<p><b>vote gmass</b></p>

					
					<br>
					
				
                </div>
"),
            };

            var votes = this.voteScanner.ScanForVotes(post);

            Assert.IsTrue(votes.Count() == 2);
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
					
				
                </div>
"),
            };

            var votes = this.voteScanner.ScanForVotes(post);

            Assert.IsTrue(votes.Count() == 1);
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
                Content = new HtmlString(@"<div class=""post_body"">
                    
					I voted for Timmo because I felt he was trying to direct the vote. I unvoted later. I reckon we have perhaps 1 Mafia kill and 1 serial killer kill here.    Dr Nick being too unsure to use his power and 1 of the Mafia teams not bothering in order to muddy the waters. <br>Merge<br><br>
Hmm scratch that 1 Mafia and 1 failed recruitment perhaps.
					
					<br>
					
				
                </div>
"),
            };

            var votes = this.voteScanner.ScanForVotes(post);

            Assert.IsTrue(votes.Count() == 0);
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
                Content = new HtmlString(@"<div class=""post_body"">
                    
					<p>You know what, I don't know if it'll make a difference or not, but&nbsp;<strong>unvote,&nbsp;</strong><strong>vote Liamness</strong></p>

					
					<br>
					
				
                </div>
"),
            };

            var votes = this.voteScanner.ScanForVotes(post);

            Assert.IsTrue(votes.Count() == 2);
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
                Content = new HtmlString(@"<div class=""post_body"">
                    
					<p>I think i'm going to <strong class=""bbc"">unvote</strong><br><br>Gos is a wasted vote. If he isn't lynched he will be whacked tonight or recruited (and so if he isn't killed overnight he'll have to be tomorrow) unless he's protected, or watched... ffs<br><br>I think snowbind is iffy, and Liamness has now done just enough to not be modkilled, spork still looks panicky AND if dino is right is our best chance of mafia.<br><br>I say {b]vote: spork[/b] if he's town, well then we can re-group and figure who was calling for his head, if he's mafia or a n other then jobs a good un.</p>
					
					<br>
					
				
                </div>
"),
            };

            var votes = this.voteScanner.ScanForVotes(post);

            Assert.IsTrue(votes.Count() == 2);
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
					
				
                </div>
"),
            };

            var votes = this.voteScanner.ScanForVotes(post);

            Assert.IsTrue(votes.Count() == 0);
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
					
				
                </div>
"),
            };

            var votes = this.voteScanner.ScanForVotes(post);

            Assert.IsTrue(votes.Count() == 1);
        }

        [TestMethod]
        public void GetVotesTestUnvoteNewLine()
        {
            ForumPost post = new ForumPost
            {
                DateTime = new DateTime(2015, 8, 18, 9, 0, 0),
                Day = 4,
                ForumPostNumber = "1111111",
                PageNumber = 50,
                Poster = "TehStu",
                Content = new HtmlString(@"<div class=""post_body"">
                    
					May merge. Heh, posting on a phone you very no notification of other posts while you write yours.<br><br>
Unvote<br><br>
Who did you hide behind on night 2, plums? Or did you mean you hid behind bennette last night?
					
					<br>
					
				
                </div>
"),
            };

            var votes = this.voteScanner.ScanForVotes(post);

            Assert.IsTrue(votes.Count() == 1);
            Assert.IsTrue(votes.First().IsUnvote == true);
        }

        [TestMethod]
        public void GetVotesTest9()
        {
            ForumPost post = new ForumPost
            {
                DateTime = new DateTime(2015, 8, 18, 9, 0, 0),
                Day = 4,
                ForumPostNumber = "1111111",
                PageNumber = 50,
                Poster = "Strategos",
                Content = new HtmlString(@"<div itemprop=""commentText"" class='post entry-content '>
					<blockquote  class=""ipsBlockquote"">
<p>&#160;</p>
<p><span  style=""color:rgb(40,40,40);font-family:helvetica, arial, sans-serif"">I have explained my reasoning above. My vote for GMass was a weak one, I wasn't convinced but felt swayed by the crowd. When he started talking like he did I felt that he wasn't the SK. So I switched my vote to the next highest lynchee, a target I'd had my eye on for a while. It felt more comfortable to me and more correct.</span></p>
<p  style=""color:rgb(40,40,40);font-family:helvetica, arial, sans-serif"">&#160;</p>
<p  style=""color:rgb(40,40,40);font-family:helvetica, arial, sans-serif"">HAVING SAID THAT, wtf GMass are you doing voting for yourself? You've gone very anti-town in your play and I'm guessing you've gotten a bit bored of the game. If you are the SK then you're playing with fire, if you're not then you sound like you want out of the game, which is not pro-town.&#160;&#160; <img src='http://www.rllmukforum.com/public/style_emoticons/default/quote.gif' class='bbc_emoticon' alt=':quote:' /></p>
<p  style=""color:rgb(40,40,40);font-family:helvetica, arial, sans-serif"">&#160;</p>
<p  style=""color:rgb(40,40,40);font-family:helvetica, arial, sans-serif"">I'd like to know where you lot were Weds night?</p>
<p  style=""color:rgb(40,40,40);font-family:helvetica, arial, sans-serif"">&#160;</p>
<div  style=""margin:0px;color:rgb(40,40,40);font-family:helvetica, arial, sans-serif"">Uncle Mike</div>
<div  style=""margin:0px;color:rgb(40,40,40);font-family:helvetica, arial, sans-serif"">Benny</div>
<div  style=""margin:0px;color:rgb(40,40,40);font-family:helvetica, arial, sans-serif"">Don Wisk</div>
<div  style=""margin:0px;color:rgb(40,40,40);font-family:helvetica, arial, sans-serif"">Strategos</div>
<div  style=""margin:0px;color:rgb(40,40,40);font-family:helvetica, arial, sans-serif"">Plums</div>
<div  style=""margin:0px;color:rgb(40,40,40);font-family:helvetica, arial, sans-serif"">&#160;</div>
<div  style=""margin:0px;color:rgb(40,40,40);font-family:helvetica, arial, sans-serif"">Are you playing the game? Or were you happy to sit back and let the town lynch somebody you knew wasn't on your team? At least one of you is scum, I'm certain of it.</div>
</blockquote>
<p>&#160;</p>
<p>&#160;</p>
<p>Danster what are you saying ? &#160;Where was I ? I had already voted for Sith, I was sticking to my guns. While you were being swayed by the crowd and making weak votes apparently.</p>
<p>&#160;</p>
<p>GMass is a really easy target, I'm beginning to wonder if he really is just floundering around ? &#160;I don't know why he is voting for himself though. Is this clever or stupid ? I have no idea.</p>
<p>&#160;</p>
<p>I'm not sure about Gerry trying to sway gospvgs investigation. It strikes me as scummy. I'm not simply retaliating because he was accusing me but have a feeling it's the sort of thing the mafia would do.</p>
<p>.&#160;</p>
<p>&#160;</p>
<blockquote  class=""ipsBlockquote"">
<p>&#160;</p>
<p>&#160;</p>
<span  style=""color:rgb(40,40,40);font-family:helvetica, arial, sans-serif"">Popped iin to vote at the last minute isn't playing the game - is it?</span></blockquote>
<p>&#160;</p>
<p>&#160;GMass is gunning for Dino for the same reason - (why is not voting for him ?) . We do seem to have a few players that are just skirting the mod kill rule - this has been going on for a couple of days as well - it's not cricket. At the moment I'm happy to vote for Don W.</p>
<p>&#160;</p>
<p><strong>Vote:<span  style=""font-family:helvetica"">Don Wiskerando</span></strong></p>
					
					<br />
					
				</div>"),
            };

            var votes = this.voteScanner.ScanForVotes(post);

            Assert.IsTrue(votes.Count() == 1);
            Assert.IsTrue(votes.First().IsUnvote == false);
            Assert.IsTrue(votes.First().Recipient == "Don Wiskerando");
        }

        [TestMethod]
        public void GetVotesAliasTest()
        {
            ForumPost post = new ForumPost
            {
                DateTime = new DateTime(2015, 8, 18, 9, 0, 0),
                Day = 4,
                ForumPostNumber = "1111111",
                PageNumber = 50,
                Poster = "Alask",
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
<p>vote : john</strong></p>
<p>&#160;</p>
<p>should cover it shouldn't it?</p>
					
					<br>
					
				
                </div>
"),
            };

            var votes = this.voteScanner.ScanForVotes(post);

            Assert.IsTrue(votes.Count() == 2);
            Assert.IsTrue(votes.Last().Recipient == "John0");
        }

        [TestMethod]
        public void NonBreakingSpaceTest()
        {
            ForumPost post = new ForumPost
            {
                DateTime = new DateTime(2015, 8, 18, 9, 0, 0),
                Day = 4,
                ForumPostNumber = "1111111",
                PageNumber = 50,
                Poster = "Danster",
                Content = new HtmlString(@"<div class=""post_body"">
                    
					<p>I finding TGP to be peculiarly quiet. His hammer hit still rankles with me, there were people in the thread discussing stuff, someone had mentioned that we might come on at half time of the footie, which I did only to find the day had ended.</p>
<p>&#160;</p>
<p>I want answers!</p>
<p>&#160;</p>
<p><strong>vote:&#160;<span style=""color:rgb(51,51,51);font-family:'Helvetica Neue', Helvetica, Arial, sans-serif;"">The Grand Pursuivant</span></strong></p>

					
					<br>
					
				
                </div>
"),
            };

            var votes = this.voteScanner.ScanForVotes(post);

            Assert.IsTrue(votes.Count() == 1);
            Assert.IsTrue(votes.Single().Recipient == "The Grand Pursuivant");
        }

        [TestMethod]
        public void NamesClashTest()
        {
            ForumPost post = new ForumPost
            {
                DateTime = new DateTime(2015, 8, 18, 9, 0, 0),
                Day = 4,
                ForumPostNumber = "1111111",
                PageNumber = 50,
                Poster = "Mr. Blonde",
                Content = new HtmlString(@"<div class=""post_body"">
                    
					<p class=""citation"">Mr. Cobalt said:<a class=""snapback right"" rel=""citation"" href=""http://www.rllmukforum.com/index.php?app=forums&amp;module=forums&amp;section=findpost&amp;pid=10594607""><img src=""http://www.rllmukforum.com/public/style_images/master/snapback.png""></a></p><blockquote class=""ipsBlockquote"" data-author=""Mr. Cobalt"" data-cid=""10594607"" data-time=""1443993646""><br>
<div><br>
<p>Bit of a mixed night obviously but having a lead on mafia is useful. Based on looking through his posts here are some interesting ones:</p>
</div>
</blockquote>
<p>&#160;</p>
<p>That's four players, I think (Charcoal, Blue, Maroon and Teal) you mention there as associated with Ochre. With four mafia and 16 players remaining, that's spreading your targeting quite wide.</p>
<p>I find it interesting that&#160;Mr. Red reads the same one-sided interchange between Ochre and Maroon and comes to the exact opposite conclusion!</p>
<p>&#160;</p>
<p>Then there's Viridian and his Teal fetish. Viridian has targeted the same player two days running on very flimsy reasoning.</p>
<p>&#160;</p>
<p>Given that if he was the FBI agent and Teal the serial killer, he'd just immediately reveal, and we had two corpses overnight (so the roleblocker can't have any information), I don't understand the logic.</p>
<p>&#160;</p>
<p><strong class='bbc'>vote: Mr. Viridian</strong></p>
<p>&#160;</p>
<p>(I seem to have some internet explorer based cut and paste problem which is stopping me quoting easily.)</p>
					
					<br>
					
				
                </div>
"),
            };

            var votes = this.voteScanner.ScanForVotes(post);

            Assert.IsTrue(votes.Count() == 1);
            Assert.IsTrue(votes.Single().Recipient == "Mr. Viridian");
        }

        [TestMethod]
        public void NotParticipatingTest()
        {
            ForumPost post = new ForumPost
            {
                DateTime = new DateTime(2015, 8, 18, 9, 0, 0),
                Day = 4,
                ForumPostNumber = "1111111",
                PageNumber = 50,
                Poster = "Moderator",
                Content = new HtmlString(@"<div class=""post_body"">
                    
					<p class=""citation"">John0, on 17 Aug 2015 - 2:33 PM, said:<a class=""snapback right"" rel=""citation"" href=""http://www.rllmukforum.com/index.php?app=forums&amp;module=forums&amp;section=findpost&amp;pid=10526689""><img src=""http://www.rllmukforum.com/public/style_images/master/snapback.png""></a></p><blockquote class=""ipsBlockquote built"" data-author=""John0"" data-cid=""10526689"" data-time=""1439818425""><p>Surely we have to lynch snowbind now.</p></blockquote><br><p>I missed those nuances. As much as the lack of movement on sith for his play style frustrates me, I was prepared to vote snowbind for another daft joke (particularly after dino appeared to learn his lesson over the I'M WIGGUM thing), but it's not, is it? He's inadvertently let slip. If he swings today and turns out to be one of Tony's lot, we have a good lead on day 2.<br>&nbsp;</p><p><strong class=""bbc"">unvote: sith</strong></p><p><strong class=""bbc"">vote: snowbind</strong><br>&nbsp;</p><p class=""citation"">Jolly, on 17 Aug 2015 - 1:04 PM, said:<a class=""snapback right"" rel=""citation"" href=""http://www.rllmukforum.com/index.php?app=forums&amp;module=forums&amp;section=findpost&amp;pid=10526541""><img src=""http://www.rllmukforum.com/public/style_images/master/snapback.png""></a></p><blockquote class=""ipsBlockquote built"" data-author=""Jolly"" data-cid=""10526541"" data-time=""1439813084""><p>Complete turnaround on what he thinks about Gos when the general town feeling is that his roleclaim is genuine.&nbsp; Annoyingly help out by TehStu at this point.</p></blockquote><p>I want to clarify this, though. We didn't have the best track record in the last game for mafia lynches, so cherry picking a post of snowbind's where he's talking about gos prior to the reveal isn't nearly as helpful as post-reveal. It wasn't the smoking ORLY gun it was purported to be. Everything else you've pointed out, though? On the money.</p>
					
					<br>
					
				
                </div>
"),
            };

            var votes = this.voteScanner.ScanForVotes(post);

            Assert.AreEqual(1, votes.Count());
            Assert.AreEqual(true, votes.Single().IsUnvote);
        }

        [TestMethod]
        public void StrongTest()
        {
            ForumPost post = new ForumPost
            {
                DateTime = new DateTime(2015, 8, 18, 9, 0, 0),
                Day = 4,
                ForumPostNumber = "1111111",
                PageNumber = 50,
                Poster = "Liamness",
                Content = new HtmlString(@"<p>
	<strong>vote: snowbind</strong>
</p>"),
            };

            var votes = this.voteScanner.ScanForVotes(post);

            Assert.AreEqual(1, votes.Count());
            Assert.AreEqual(false, votes.Single().IsUnvote);
            Assert.IsTrue(votes.First().Recipient == "snowbind");
        }

        [TestMethod]
        public void DoubleStrongTest()
        {
            ForumPost post = new ForumPost
            {
                DateTime = new DateTime(2015, 8, 18, 9, 0, 0),
                Day = 4,
                ForumPostNumber = "1111111",
                PageNumber = 50,
                Poster = "Liamness",
                Content = new HtmlString(@"<p>
	<strong>Vote:</strong> <strong>snowbind</strong>
</p>"),
            };

            var votes = this.voteScanner.ScanForVotes(post);

            Assert.AreEqual(1, votes.Count());
            Assert.AreEqual(false, votes.Single().IsUnvote);
            Assert.IsTrue(votes.First().Recipient == "snowbind");
        }

        [TestMethod]
        public void AtMentionTest()
        {
            ForumPost post = new ForumPost
            {
                DateTime = new DateTime(2015, 8, 18, 9, 0, 0),
                Day = 4,
                ForumPostNumber = "1111111",
                PageNumber = 50,
                Poster = "Liamness",
                Content = new HtmlString(@"<p>
	<strong>Vote: <a contenteditable=""false"" data-ipshover="""" data-ipshover-target=""https://www.rllmukforum.com/index.php?/profile/27618-mr-beaver/&amp;do=hovercard"" data-mentionid=""27618"" href=""https://www.rllmukforum.com/index.php?/profile/27618-mr-beaver/"" rel="""">@snowbind</a></strong>
</p>

<p>"),
            };

            var votes = this.voteScanner.ScanForVotes(post);

            Assert.AreEqual(1, votes.Count());
            Assert.AreEqual(false, votes.Single().IsUnvote);
            Assert.IsTrue(votes.First().Recipient == "snowbind");
        }

        [TestMethod]
        public void BoldMidLineTest()
        {
            ForumPost post = new ForumPost
            {
                DateTime = new DateTime(2015, 8, 18, 9, 0, 0),
                Day = 4,
                ForumPostNumber = "1111111",
                PageNumber = 50,
                Poster = "Liamness",
                Content = new HtmlString(@"<p>
	I would vote Mr Dog but I assume that getting 15 others to switch their vote to you now is very unlikely, so I will <strong>vote: snowbind </strong>on the basis that I feel Dog is trying to divert attention away from him.
</p>"),
            };

            var votes = this.voteScanner.ScanForVotes(post);

            Assert.AreEqual(1, votes.Count());
            Assert.AreEqual(false, votes.Single().IsUnvote);
            Assert.IsTrue(votes.First().Recipient == "snowbind");
        }
    }
}
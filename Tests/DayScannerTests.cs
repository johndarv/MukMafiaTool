﻿namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using MukMafiaTool.Common;
    using MukMafiaTool.Model;

    [TestClass]
    public class DayScannerTests
    {
        private IRepository repo;
        private DayScanner target;

        public DayScannerTests()
        {
            var mockRepo = new Mock<IRepository>();

            mockRepo.Setup(r => r.FindDay(10))
                .Returns(new Day { Number = 10, StartForumPostNumber = "100", EndForumPostNumber = string.Empty });

            this.repo = mockRepo.Object;

            this.target = new DayScanner(this.repo);
        }

        [TestMethod]
        public void DayScannerStartDaySimpleTest()
        {
            ForumPost post = new ForumPost
            {
                DateTime = new DateTime(2015, 8, 18, 9, 0, 0),
                Day = 4,
                ForumPostNumber = "1111111",
                PageNumber = 50,
                Poster = "John0",
                Content = new HtmlString(@"[start of day 3]"),
            };

            this.target.UpdateDays(new ForumPost[] { post });

            var mock = Mock.Get<IRepository>(this.repo);

            mock.Verify(m => m.UpsertDay(It.Is<Day>(d => d.Number == 3 && d.StartForumPostNumber == "1111111")));
        }

        [TestMethod]
        public void DayScannerStartDaySimpleTest2()
        {
            ForumPost post = new ForumPost
            {
                DateTime = new DateTime(2015, 8, 18, 9, 0, 0),
                Day = 4,
                ForumPostNumber = "1111111",
                PageNumber = 50,
                Poster = "Danster",
                Content = new HtmlString(@"[start of day 1]"),
            };

            this.target.UpdateDays(new ForumPost[] { post });

            var mock = Mock.Get<IRepository>(this.repo);

            mock.Verify(m => m.UpsertDay(It.Is<Day>(d => d.Number == 1 && d.StartForumPostNumber == "1111111")));
        }

        [TestMethod]
        public void DayScannerIgnoreCase()
        {
            ForumPost post = new ForumPost
            {
                DateTime = new DateTime(2015, 8, 18, 9, 0, 0),
                Day = 4,
                ForumPostNumber = "1111111",
                PageNumber = 50,
                Poster = "Danster",
                Content = new HtmlString(@"[start of Day 1]"),
            };

            this.target.UpdateDays(new ForumPost[] { post });

            var mock = Mock.Get<IRepository>(this.repo);

            mock.Verify(m => m.UpsertDay(It.Is<Day>(d => d.Number == 1 && d.StartForumPostNumber == "1111111")));
        }

        [TestMethod]
        public void DayScannerStartDayTest()
        {
            ForumPost post = new ForumPost
            {
                DateTime = new DateTime(2015, 8, 18, 9, 0, 0),
                Day = 4,
                ForumPostNumber = "1111111",
                PageNumber = 50,
                Poster = "John0",
                Content = new HtmlString(@"<div itemprop=""commentText"" class='post entry-content '>
					<p>[start of day 3]</p>
<p>The town lynch TehStu he is mafia!</p>
<p>&#160;</p>
<p>Well done town! After a day 1 cop lynch you managed to come back and succeed. Good work!</p>

					
					<br />
					
				</div>"),
            };

            this.target.UpdateDays(new ForumPost[] { post });

            var mock = Mock.Get<IRepository>(this.repo);

            mock.Verify(m => m.UpsertDay(It.Is<Day>(d => d.Number == 3 && d.StartForumPostNumber == "1111111")));
        }

        [TestMethod]
        public void DayScannerEndDayTest()
        {
            ForumPost post = new ForumPost
            {
                DateTime = new DateTime(2015, 8, 18, 9, 0, 0),
                Day = 4,
                ForumPostNumber = "1111111",
                PageNumber = 50,
                Poster = "John0",
                Content = new HtmlString(@"<div itemprop=""commentText"" class='post entry-content '>
					<p>[end of day 10]</p>
<p>The town lynch TehStu he is mafia!</p>
<p>&#160;</p>
<p>Well done town! After a day 1 cop lynch you managed to come back and succeed. Good work!</p>

					
					<br />
					
				</div>"),
            };

            this.target.UpdateDays(new ForumPost[] { post });

            var mock = Mock.Get<IRepository>(this.repo);

            mock.Verify(m => m.UpsertDay(It.Is<Day>(d => d.Number == 10 && d.StartForumPostNumber == "100" && d.EndForumPostNumber == "1111111")));
        }
    }
}

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

namespace Tests
{
    [TestClass]
    public class DayScannerTests
    {
        IRepository _repo;
        DayScanner _target;

        public DayScannerTests()
        {
            var mockRepo = new Mock<IRepository>();

            mockRepo.Setup(r => r.FindDay(10))
                .Returns(new Day { Number = 10, StartForumPostNumber = "100", EndForumPostNumber = string.Empty });

            _repo = mockRepo.Object;

            _target = new DayScanner(_repo);
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
                ThreadPostNumber = 100,
                Content = new HtmlString(@"[start of day 3]"),
            };

            _target.UpdateDays(new ForumPost[] { post });

            var mock = Mock.Get<IRepository>(_repo);

            mock.Verify(m => m.UpsertDay(It.Is<Day>(d => d.Number == 3 && d.StartForumPostNumber == "1111111")));
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
                ThreadPostNumber = 100,
                Content = new HtmlString(@"<div itemprop=""commentText"" class='post entry-content '>
					<p>[start of day 3]</p>
<p>The town lynch TehStu he is mafia!</p>
<p>&#160;</p>
<p>Well done town! After a day 1 cop lynch you managed to come back and succeed. Good work!</p>

					
					<br />
					
				</div>"),
            };

            _target.UpdateDays(new ForumPost[] { post });

            var mock = Mock.Get<IRepository>(_repo);

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
                ThreadPostNumber = 100,
                Content = new HtmlString(@"<div itemprop=""commentText"" class='post entry-content '>
					<p>[end of day 10]</p>
<p>The town lynch TehStu he is mafia!</p>
<p>&#160;</p>
<p>Well done town! After a day 1 cop lynch you managed to come back and succeed. Good work!</p>

					
					<br />
					
				</div>"),
            };

            _target.UpdateDays(new ForumPost[] { post });

            var mock = Mock.Get<IRepository>(_repo);

            mock.Verify(m => m.UpsertDay(It.Is<Day>(d => d.Number == 10 && d.StartForumPostNumber == "100" && d.EndForumPostNumber == "1111111")));
        }
    }
}

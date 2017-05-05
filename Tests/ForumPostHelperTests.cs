namespace Tests
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MukMafiaTool.Common;
    using MukMafiaTool.Model;

    [TestClass]
    public class ForumPostHelperTests
    {
        private IEnumerable<Day> days;

        public ForumPostHelperTests()
        {
            this.days = new Day[]
            {
                new Day { Number = 1, StartForumPostNumber = "1056789", EndForumPostNumber = "1056790" }
            };
        }

        [TestMethod]
        public void DetermineDayTest()
        {
            var result = ForumPostHelper.DetermineDay("1056789", this.days);

            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void DetermineDayTest2()
        {
            var result = ForumPostHelper.DetermineDay("1056790", this.days);

            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void DetermineDayTest3()
        {
            var result = ForumPostHelper.DetermineDay("1056900", this.days);

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void DetermineDayTest4()
        {
            var result = ForumPostHelper.DetermineDay("1046900", this.days);

            Assert.AreEqual(0, result);
        }
    }
}

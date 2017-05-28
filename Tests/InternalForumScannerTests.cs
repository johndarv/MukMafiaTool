namespace Tests
{
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MukMafiaTool.Database;
    using MukMafiaTool.ForumScanning;

    [TestClass]
    public class InternalForumScannerTests
    {
        //[Ignore]
        [TestMethod]
        public void DoWholeUpdateTest()
        {
            ForumScanner scanner = new ForumScanner(new MongoRepository());

            scanner.DoWholeUpdate();
        }

        [Ignore]
        [TestMethod]
        public void DoEndOfGameScan()
        {
            ForumScanner scanner = new ForumScanner(new MongoRepository());

            scanner.DoEndOfGameScan();
        }

        [Ignore]
        [TestMethod]
        public void SignInTest()
        {
            var forumAccessor = new ForumAccessor();

            var pageHtml = forumAccessor.RetrievePageHtml(1);

            pageHtml.Should().NotBeNull(because: "otherwise the reuqest has definitely failed!");
            pageHtml.Contains("permission").Should().BeFalse(because: "any message about permission will probably mean that the sign in has failed.");
        }
    }
}
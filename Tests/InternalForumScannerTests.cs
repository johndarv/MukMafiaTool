namespace Tests
{
    using ForumScanApi;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MukMafiaTool.Database;

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

        [Ignore]
        [TestMethod]
        public void DoEndOfGameScan()
        {
            ForumScanner scanner = new ForumScanner(new MongoRepository());

            scanner.DoEndOfGameScan();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MukMafiaTool.Database;
using MukMafiaTool.ForumScanService;
using MukMafiaTool.Model;

namespace MukMafiaToolTests
{
    [TestClass]
    public class InternalForumScannerTests
    {
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
        public void FindAllPlayersNotInRepo()
        {
            var repo = new MongoRepository();

            var posters = repo.FindAllPosts().Select(p => p.Poster).Distinct();
            var playerNames = repo.FindAllPlayers().Select(p => p.Name);

            var postersNotInRepo = posters.Where(p => !playerNames.Contains(p));

            repo.EnsurePlayersInRepo(postersNotInRepo);
        }
    }
}

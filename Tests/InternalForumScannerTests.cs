using System.Collections.Generic;
using System.Linq;
using ForumScanApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MukMafiaTool.Database;
using MukMafiaTool.Model;

namespace Tests
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

            repo.EnsurePlayersInRepo(postersNotInRepo, "1");
        }

        [Ignore]
        [TestMethod]
        public void SetDay()
        {
            var repo = new MongoRepository();

            var day = new Day
            {
                Number = 1,
                StartForumPostNumber = "11368576",
                EndForumPostNumber = "11373503",
            };

            repo.UpsertDay(day);
        }

        [Ignore]
        [TestMethod]
        public void SignInTest()
        {
            var forumAccessor = new ForumAccessor();

            forumAccessor.RetrievePageHtml(1);
        }

        [Ignore]
        [TestMethod]
        public void InsertUser()
        {
            using (var repo = new MongoRepository())
            {
                var user = new User
                {
                    UserName = "jd1",
                    Password = "B1gly",
                    Roles = new string[] { "Admin" },
                };

                repo.UpsertUser(user);
            }
        }
        
        [TestMethod]
        public void UpdatePlayer()
        {
            using (var repo = new MongoRepository())
            {
                var player = repo.FindPlayer("The Grand Pursuivant");

                player.Recruitments = new List<Recruitment>();

                repo.UpsertPlayer(player);
            }
        }
        
        [TestMethod]
        public void KillPlayer()
        {
            using (var repo = new MongoRepository())
            {
                var player = repo.FindPlayer("Mr Antelope");

                player.Character = "Sean Spicer (Loudener)";
                player.Notes = string.Empty;
                player.Recruitments = new List<Recruitment> { new Recruitment { Allegiance = Allegiance.Town, FactionName = "Team USA", ForumPostNumber = "0" } };
                player.Fatality = "Lynched on Day 1";

                repo.UpsertPlayer(player);
            }
        }

        [Ignore]
        [TestMethod]
        public void AddPlayerAliases()
        {
            using (var repo = new MongoRepository())
            {
                var players = repo.FindAllPlayers();

                var participatingPlayers = players.Where(p => p.Participating);

                foreach (var player in participatingPlayers)
                {
                    if (player.Name.StartsWith("Mr ", System.StringComparison.OrdinalIgnoreCase))
                    {
                        var baseName = player.Name.Substring(3);

                        var aliases = new List<string>
                        {
                            $"Mr. {baseName}",
                            $"Mr.{baseName}",
                            $"Mr{baseName}",
                            baseName,
                        };

                        player.Aliases = aliases;

                        repo.UpsertPlayer(player);
                    }
                }
            }
        }
    }
}

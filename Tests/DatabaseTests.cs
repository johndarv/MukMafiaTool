namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MukMafiaTool.Database;
    using MukMafiaTool.Model;

    [TestClass]
    public class DatabaseTests
    {
        [Ignore]
        [TestMethod]
        public void UpsertLastUpdatedTime()
        {
            var repo = new MongoRepository();

            repo.UpdateLastUpdatedTime(DateTime.UtcNow.AddMonths(-1));
        }

        [Ignore]
        [TestMethod]
        public void InsertUser()
        {
            using (var repo = new MongoRepository())
            {
                var user = new User
                {
                    UserName = "jd",
                    Password = string.Empty,
                    Roles = new string[] { "Admin" },
                };

                repo.UpsertUser(user);
            }
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
        public void UpdatePlayer()
        {
            using (var repository = new MongoRepository())
            {
                var player = repository.FindPlayer("The Grand Pursuivant");

                player.Participating = false;
                player.Notes = "Moderator";
                player.Recruitments = new List<Recruitment>();

                repository.UpsertPlayer(player);
            }
        }

        [Ignore]
        [TestMethod]
        public void KillPlayer()
        {
            using (var repo = new MongoRepository())
            {
                var player = repo.FindPlayer("Mr Lion");

                player.Character = "Mike Pence (Vengeful Priest)";
                player.Notes = string.Empty;
                player.Recruitments = new List<Recruitment> { new Recruitment { Allegiance = Allegiance.Town, FactionName = "Team USA", ForumPostNumber = "0" } };
                player.Fatality = "Modkilled on Day 3";

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

        [TestMethod]
        public void ResetDayForPost()
        {
            using (var repository = new MongoRepository())
            {
                var posts = repository.FindAllPosts();
                var day3 = repository.FindDay(3);

                var postsWithIncorrectDay0Value = posts.Where(p => p.Day != 3 && string.Compare(p.ForumPostNumber, day3.StartForumPostNumber) >= 0);
            }
        }
    }
}
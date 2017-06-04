namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MukMafiaTool.Database;
    using MukMafiaTool.Model;

    [TestClass]
    public sealed class DatabaseTests : IDisposable
    {
        private readonly MongoRepository repository;

        public DatabaseTests()
        {
            this.repository = new MongoRepository();
        }

        public void Dispose()
        {
            if (this.repository != null)
            {
                this.repository.Dispose();
            }
        }

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
            var player = this.repository.FindPlayer("The Donald");
            player.Character = "Donald Trump";

            var recruitments = new List<Recruitment>
            {
                new Recruitment { Allegiance = Allegiance.Town, FactionName = "Team USA", ForumPostNumber = "0" },
                new Recruitment { Allegiance = Allegiance.Mafia, FactionName = "Allies, Definitely Allies", ForumPostNumber = "0" },
            };

            player.Recruitments.Clear();
            player.AddRecruitments(recruitments);

            this.repository.UpsertPlayer(player);
        }

        [Ignore]
        [TestMethod]
        public void KillPlayers()
        {
            this.KillPlayer("Mr Bison", "Kim Jong-Un", "Mafia Framer", "Lynched on Day 8", Allegiance.Mafia, "Bad Dudes", "0");
            this.KillPlayer("Mr Gorilla", "Donald Trump Jr.", "Off Roleblocker", "Killed on Night 8", Allegiance.Town, "Team USA", "0");
            this.KillPlayer("Mr Walrus", "Turned Jeff Sessions", "Former Weak Cop", "Killed on Night 7", Allegiance.Mafia, "Allies, Definitely Allies", "0");
            this.KillPlayer("Mr Horse", "Vladimir Putin", "Unblockable Godfather", "Killed on Night 6", Allegiance.Mafia, "Bad Dudes", "0");
        }

        [Ignore]
        [TestMethod]
        public void AddPlayerAliases()
        {
            var players = repository.FindAllPlayers();

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

                    player.AddAliases(aliases);

                    this.repository.UpsertPlayer(player);
                }
            }
        }

        [Ignore]
        [TestMethod]
        public void UpsertVoteTest()
        {
            var vote = new Vote
            {
                DateTime = new DateTime(2017, 5, 2, 20, 27, 35),
                Voter = "Mr Coyote",
                Recipient = "Mr Antelope",
                IsUnvote = false,
                ForumPostNumber = "11373467",
                PostContentIndex = 33,
                ManuallyEdited = false,
                Day = 1,
                Invalid = false,
            };

            this.repository.UpsertVote(vote);
        }

        private void KillPlayer(string playerName, string playerCharacter, string playerRole, string fatality, Allegiance allegiance, string factionName, string recruitmentPostNumber)
        {
            var player = this.repository.FindPlayer(playerName);

            player.Character = playerCharacter;
            player.Role = playerRole;
            player.Notes = string.Empty;
            player.Fatality = fatality;
            player.Recruitments.Clear();
            player.AddRecruitments(new List<Recruitment> { new Recruitment { Allegiance = allegiance, FactionName = factionName, ForumPostNumber = recruitmentPostNumber } });

            this.repository.UpsertPlayer(player);
        }
    }
}
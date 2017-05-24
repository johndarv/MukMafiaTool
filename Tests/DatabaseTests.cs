﻿namespace Tests
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
            var player = this.repository.FindPlayer("Mr Elephant");

            player.Role = "One Shot Governor, Watcher Enabler";

            this.repository.UpsertPlayer(player);
        }

        [Ignore]
        [TestMethod]
        public void KillPlayers()
        {
            this.KillPlayer("Mr Elephant", "Reince Preibus", "One Shot Governor - Watcher Enabler", "Lynched on Day 4", Allegiance.Town, "Team USA", "0");
            this.KillPlayer("Mr Tarantula", "Rick Perry", "Tracker", "Lynched on Day 4", Allegiance.Town, "Team USA", "0");
            this.KillPlayer("Mr Xerus", "Angela Merkel", "Odd/Even Jailor", "Modkilled on Day 4", Allegiance.Mafia, "Allies, Definitely Allies", "0");
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
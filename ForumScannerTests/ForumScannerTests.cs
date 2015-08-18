using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MukMafiaTool.Database;
using MukMafiaTool.Model;
using MukMafiaTool.ForumScanner.Extensions;
using System.Collections.Generic;

namespace ForumScannerTests
{
    [TestClass]
    public class ForumScannerTests
    {
        private MongoRepository _repo;

        public ForumScannerTests()
        {
            _repo = new MongoRepository();
        }

        [TestMethod]
        public void GetVotesTest()
        {
            ForumPost post = _repo.FindSpecificPost("10515980");

            var results = post.GetVotes(new List<string>() { "SMD" });

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Timmo", results.First().Voter);
            Assert.AreEqual("SMD", results.First().Recipient);
            Assert.AreEqual(post.DateTime, results.First().DateTime);
            Assert.AreEqual(false, results.First().IsUnvote);
        }

        [TestMethod]
        public void GetUnvotesAndVotesTest()
        {
            ForumPost post = _repo.FindSpecificPost("10516973");

            var results = post.GetVotes(new List<string>() { "stefcha" });

            Assert.AreEqual(2, results.Count);

            results = results.OrderBy(v => v.PostContentIndex).ToList();

            Assert.AreEqual("Danster", results.First().Voter);
            Assert.AreEqual(string.Empty, results.First().Recipient);
            Assert.AreEqual(post.DateTime, results.First().DateTime);
            Assert.AreEqual(true, results.First().IsUnvote);
        }

        [TestMethod]
        public void GetUnvotesAndVotesTestWithQuoteBlocks()
        {
            ForumPost post = _repo.FindSpecificPost("10517006");

            var results = post.GetVotes(new List<string>() { "stefcha", "danster", "dinobot" });

            Assert.AreEqual(1, results.Count);

            Assert.AreEqual("redbloodcel", results.First().Voter);
            Assert.AreEqual("danster", results.First().Recipient);
            Assert.AreEqual(post.DateTime, results.First().DateTime);
            Assert.AreEqual(false, results.First().IsUnvote);
        }

        [TestMethod]
        public void GetUnvoteAndVoteCapsTest()
        {
            ForumPost post = _repo.FindSpecificPost("10523451");

            var results = post.GetVotes(new List<string>() { "stefcha", "danster", "dinobot", "snowbind" });

            Assert.AreEqual(2, results.Count);

            results = results.OrderBy(v => v.PostContentIndex).ToList();

            Assert.AreEqual("Jolly", results.Last().Voter);
            Assert.AreEqual("snowbind", results.Last().Recipient);
            Assert.AreEqual(post.DateTime, results.Last().DateTime);
            Assert.AreEqual(false, results.Last().IsUnvote);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MukMafiaTool.Model;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MukMafiaTool.Database
{
    public class MongoRepository : IRepository
    {
        private IMongoCollection<BsonDocument> _posts;
        private IMongoCollection<BsonDocument> _votes;
        private IMongoCollection<BsonDocument> _players;
        private IMongoCollection<BsonDocument> _metadata;
        private IMongoCollection<BsonDocument> _exclusions;
        private IMongoCollection<BsonDocument> _days;
        private IMongoCollection<BsonDocument> _logs;

        public MongoRepository()
        {
            var connectionString = ConfigurationManager.AppSettings["MongoConnectionString"];

            MongoClient client = new MongoClient(connectionString);

            var database = client.GetDatabase("Simpsonscum");

            _posts = database.GetCollection<BsonDocument>("Posts");
            _votes = database.GetCollection<BsonDocument>("Votes");
            _players = database.GetCollection<BsonDocument>("Players");
            _metadata = database.GetCollection<BsonDocument>("Metadata");
            _exclusions = database.GetCollection<BsonDocument>("Exclusions");
            _days = database.GetCollection<BsonDocument>("Days");
            _logs = database.GetCollection<BsonDocument>("Logs");
        }

        public IEnumerable<ForumPost> FindAllPosts()
        {
            var documents = _posts.Find(new BsonDocument()).ToListAsync().Result;

            return documents.Select(d => d.ToForumPost());
        }

        public IEnumerable<Player> FindAllPlayers()
        {
            var documents = _players.Find(new BsonDocument()).ToListAsync().Result;

            return documents.Select(d => d.ToPlayer());
        }

        public IEnumerable<Vote> FindAllVotes()
        {
            var documents = _votes.Find(new BsonDocument()).ToListAsync().Result;

            return documents.Select(d => d.ToVote());
        }

        public IList<string> FindAllPlayerNamesFromPosts()
        {
            return FindAllPosts().Select(p => p.Poster).Distinct().ToList();
        }

        public IList<ForumPost> FindAllPosts(string playerName)
        {
            var allPosts = FindAllPosts();

            return allPosts.Where(p => string.Equals(p.Poster, playerName, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public IList<ForumPost> FindAllPostsContaining(string search)
        {
            var matchingPosts = new List<ForumPost>();

            var allPosts = FindAllPosts();

            CultureInfo culture = CultureInfo.InvariantCulture;

            foreach(var post in allPosts)
            {
                if (culture.CompareInfo.IndexOf(post.Content.ToString(), search, CompareOptions.IgnoreCase) >= 0)
                {
                    matchingPosts.Add(post);
                }
            }

            return matchingPosts;
        }

        public ForumPost FindSpecificPost(string forumPostNumber)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("ForumPostNumber", forumPostNumber);
            var result = _posts.Find(filter).FirstOrDefaultAsync().Result;

            if (result == null)
            {
                return null;
            }

            return result.ToForumPost();
        }

        public void UpsertPost(ForumPost post)
        {
            var newDoc = new BsonDocument
                {
                    { "ForumPostNumber", post.ForumPostNumber },
                    { "ThreadPostNumber", post.ThreadPostNumber },
                    { "Poster", post.Poster },
                    { "DateTime", post.DateTime.ToUniversalTime() },
                    { "Content", post.Content.ToString() },
                    { "Day", post.Day },
                    { "PageNumber", post.PageNumber },
                    { "LastScanned", post.LastScanned.ToUniversalTime() },
                };

            var filter = Builders<BsonDocument>.Filter.Eq("ForumPostNumber", post.ForumPostNumber);

            Upsert(newDoc, filter);
        }

        public void InsertPlayers(IList<ForumPost> posts)
        {
            foreach (var post in posts)
            {
                var filter = Builders<BsonDocument>.Filter.Eq("Name", post.Poster);

                var docs = _posts.Find(new BsonDocument()).ToListAsync().Result;

                if (docs.Count == 0)
                {
                    var newDoc = new BsonDocument
                    {
                        { "Name", post.ForumPostNumber }
                    };
                }
            }
        }

        public void WipeVotes()
        {
            _votes.DeleteManyAsync(new BsonDocument()).Wait();
        }

        public void ProcessVote(Vote vote)
        {
            if (vote.IsUnvote)
            {
                ProcessUnvote(vote);
            }
            else
            {
                // Delete all voter's vote up until this point
                var filter = Builders<BsonDocument>.Filter.Eq("Voter", vote.Voter);
                _votes.DeleteManyAsync(filter).Wait();

                var doc = new BsonDocument
                {
                    { "DateTime", vote.DateTime },
                    { "Voter", vote.Voter },
                    { "Recipient", vote.Recipient },
                    { "ForumPostNumber", vote.ForumPostNumber },
                    { "PostContentIndex", vote.PostContentIndex },
                };

                _votes.InsertOneAsync(doc).Wait();
            }
        }

        public void UpdateLastUpdated()
        {
            var doc = new BsonDocument
            {
                { "_id", 1 },
                { "LastUpdatedDateTime", DateTime.UtcNow },
            };

            var filter = Builders<BsonDocument>.Filter.Eq("_id", 1);

            Upsert(doc, filter);
        }

        public DateTime FindLastUpdatedDateTime()
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", 1);

            var doc = _metadata.Find(filter).FirstOrDefaultAsync().Result;

            return doc["LastUpdatedDateTime"].ToLocalTime();
        }

        public IList<string> FindAllExclusions()
        {
            var documents = _exclusions.Find(new BsonDocument()).ToListAsync().Result;

            IList<string> result = new List<string>();

            foreach (var doc in documents)
            {
                result.Add(doc["Name"].ToString());
            }

            return result;
        }

        public IList<Day> FindAllDays()
        {
            var documents = _days.Find(new BsonDocument()).ToListAsync().Result;

            var days = new List<Day>();

            foreach (var doc in documents)
            {
                days.Add(doc.ToDay());
            }

            return days;
        }

        public Day FindCurrentDay()
        {
            var days = FindAllDays();
            days = days.OrderBy(d => d.Number).ToList();
            return days.Last();
        }

        // Note: This finds the *current* latest page accessed, not the one when the Repository object was instantiated.</remarks>
        public ForumPost FindLatestPost()
        {
            var sortDefinition = Builders<BsonDocument>.Sort.Descending(d => d["ForumPostNumber"]);

            var doc = _posts.Find(new BsonDocument()).Sort(sortDefinition).FirstOrDefaultAsync().Result;
            
            if (doc == null)
            {
                return null;
            }

            return doc.ToForumPost();
        }

        public void UpsertVote(Vote vote)
        {
            var newDoc = new BsonDocument
            {
                { "IsUnvote", vote.IsUnvote },
                { "DateTime", vote.DateTime },
                { "Recipient", vote.Recipient },
                { "Voter", vote.Voter },
                { "ForumPostNumber", vote.ForumPostNumber },
                { "PostContentIndex", vote.PostContentIndex },
            };

            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Eq("ForumPostNumber", vote.ForumPostNumber) & builder.Eq("PostContentIndex", vote.PostContentIndex);

            Upsert(newDoc, filter);
        }

        public void EnsurePlayersInRepo(IList<ForumPost> posts)
        {
            foreach (var post in posts)
            {
                if (FindPlayer(post.Poster) == null)
                {
                    var recruitmentDoc = new BsonDocument
                    {
                        { "FactionName", "Town" },
                        { "Allegiance", Allegiance.Town.ToString() },
                        { "ForumPostNumber", "10515623" },
                    };

                    var playerDoc = new BsonDocument
                    {
                        { "Name", post.Poster },
                        { "Recruitments", new BsonArray { recruitmentDoc } },
                        { "Participating", true },
                        { "Fatality", string.Empty },
                        { "Character", string.Empty },
                        { "Notes", string.Empty },
                        { "Aliases", new BsonArray() }
                    };

                    _players.InsertOneAsync(playerDoc).Wait();
                }
            }
        }

        public void EnsurePlayersInRepo(IEnumerable<string> playerNames)
        {
            foreach (var playerName in playerNames)
            {
                var recruitmentDoc = new BsonDocument
                    {
                        { "FactionName", "Town" },
                        { "Allegiance", Allegiance.Town.ToString() },
                        { "ForumPostNumber", "10515623" },
                    };

                var playerDoc = new BsonDocument
                    {
                        { "Name", playerName },
                        { "Recruitments", new BsonArray { recruitmentDoc } },
                        { "Participating", true },
                        { "Fatality", string.Empty },
                        { "Character", string.Empty },
                        { "Notes", string.Empty },
                        { "Aliases", new BsonArray() }
                    };

                _players.InsertOneAsync(playerDoc).Wait();
            }
        }

        public void UpsertPlayer(Player player)
        {
            BsonArray recruitments = new BsonArray();

            foreach (var recruitment in player.Recruitments)
            {
                recruitments.Add(new BsonDocument
                    {
                        { "FactionName", recruitment.FactionName },
                        { "Allegiance", recruitment.Allegiance.ToString() },
                        { "ForumPostNumber", recruitment.ForumPostNumber },
                    });
            }

            BsonArray aliases = new BsonArray();

            foreach (var alias in aliases)
            {
                aliases.Add(alias.ToString());
            }

            var newDoc = new BsonDocument
            {
                { "Name", player.Name },
                { "Recruitments", recruitments },
                { "Participating", player.Participating },
                { "Fatality", player.Fatality },
                { "Character", player.Character },
                { "Notes", player.Notes },
                { "Aliases", aliases }
            };

            var filter = Builders<BsonDocument>.Filter.Eq("Name", player.Name);

            Upsert(newDoc, filter);
        }

        public Player FindPlayer(string name)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("Name", name);

            var player = _players.Find(filter).FirstOrDefaultAsync().Result;

            if (player == null)
            {
                return null;
            }

            return player.ToPlayer();
        }

        public void LogMessage(string message)
        {
            BsonDocument doc = new BsonDocument();
            doc["Message"] = message;

            _logs.InsertOneAsync(doc).Wait();
        }

        private void Upsert(BsonDocument newDoc, FilterDefinition<BsonDocument> filter)
        {
            var options = new UpdateOptions();
            options.IsUpsert = true;

            _posts.ReplaceOneAsync(filter, newDoc, options).Wait();
        }

        private void ProcessUnvote(Vote vote)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("Voter", vote.Voter);
            _votes.DeleteManyAsync(filter).Wait();
        }
    }
}
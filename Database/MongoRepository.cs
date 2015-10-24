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
using MukMafiaTool.Common;

namespace MukMafiaTool.Database
{
    public class MongoRepository : IRepository, IDisposable
    {
        private IMongoCollection<BsonDocument> _posts;
        private IMongoCollection<BsonDocument> _votes;
        private IMongoCollection<BsonDocument> _players;
        private IMongoCollection<BsonDocument> _metadata;
        private IMongoCollection<BsonDocument> _days;
        private IMongoCollection<BsonDocument> _users;
        private IMongoCollection<BsonDocument> _logs;

        public MongoRepository()
        {
            var connectionString = ConfigurationManager.AppSettings["MongoConnectionString"];
            var databaseName = ConfigurationManager.AppSettings["MongoDatabaseName"];

            MongoClient client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);

            _posts = database.GetCollection<BsonDocument>("Posts");
            _votes = database.GetCollection<BsonDocument>("Votes");
            _players = database.GetCollection<BsonDocument>("Players");
            _metadata = database.GetCollection<BsonDocument>("Metadata");
            _days = database.GetCollection<BsonDocument>("Days");
            _users = database.GetCollection<BsonDocument>("Users");
            _logs = database.GetCollection<BsonDocument>("Logs");
        }

        public void Dispose()
        {
        }

        public IEnumerable<ForumPost> FindAllPosts(bool includeDayZeros = false)
        {
            var documents = _posts.Find(new BsonDocument()).ToListAsync().Result;

            var posts = documents.Select(d => d.ToForumPost());

            if (!includeDayZeros)
            {
                posts = posts.Where(p => p.Day > 0);
            }

            return posts;
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
                    { "ManuallyEdited", post.ManuallyEdited },
                };

            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Eq("ForumPostNumber", post.ForumPostNumber);

            var existingPost = FindSpecificPost(post.ForumPostNumber);

            // In the case where the post already exists in the database, and the post has just been rescanned
            if (existingPost != null && post.ManuallyEdited == false)
            {
                // Then only upsert if it hasn't been previously edited
                filter = filter & builder.Eq("ManuallyEdited", false);
            }
            
            Upsert(_posts, newDoc, filter);
        }

        public void DeleteAllVotes()
        {
            var filter = Builders<BsonDocument>.Filter.Eq("ManuallyEdited", false);

            _votes.DeleteManyAsync(filter).Wait();
        }

        public void DeleteVotes(string forumPostNumber)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("ForumPostNumber", forumPostNumber);

            _votes.DeleteManyAsync(filter).Wait();
        }

        public void UpdateLastUpdated()
        {
            var doc = new BsonDocument
            {
                { "_id", 1 },
                { "LastUpdatedDateTime", DateTime.UtcNow },
            };

            var filter = Builders<BsonDocument>.Filter.Eq("_id", 1);

            Upsert(_metadata, doc, filter);
        }

        public DateTime FindLastUpdatedDateTime()
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", 1);

            var doc = _metadata.Find(filter).FirstOrDefaultAsync().Result;

            return doc["LastUpdatedDateTime"].ToLocalTime();
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

        public void UpsertDay(Day day)
        {
            var newDoc = new BsonDocument
                {
                    { "_id", day.Number },
                    { "StartForumPostNumber", day.StartForumPostNumber },
                    { "EndForumPostNumber", day.EndForumPostNumber },
                };

            var filter = Builders<BsonDocument>.Filter.Eq("_id", day.Number);

            Upsert(_days, newDoc, filter);
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
                { "ManuallyEdited", vote.ManuallyEdited },
                { "Day", vote.Day },            };

            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Eq("ForumPostNumber", vote.ForumPostNumber)
                & builder.Eq("PostContentIndex", vote.PostContentIndex)
                & builder.Eq("ManuallyEdited", false);

            Upsert(_votes, newDoc, filter);
        }

        public void EnsurePlayersInRepo(IEnumerable<string> playerNames, string firstForumPostNumber)
        {
            foreach (var playerName in playerNames)
            {
                if (FindPlayer(playerName) == null)
                {
                    var recruitmentDoc = new BsonDocument
                    {
                        { "FactionName", "Town" },
                        { "Allegiance", Allegiance.Town.ToString() },
                        { "ForumPostNumber", firstForumPostNumber },
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

            Upsert(_players, newDoc, filter);
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

        public Day FindDay(int dayNumber)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", dayNumber);

            var doc = _days.Find(filter).FirstOrDefaultAsync().Result;

            if (doc == null)
            {
                return null;
            }

            return doc.ToDay();
        }

        public void UpsertUser(User user)
        {
            BsonArray roles = new BsonArray();
            
            foreach (var role in user.Roles)
            {
                roles.Add(new BsonDocument { { "Role", role } });
            }

            var newDoc = new BsonDocument
            {
                { "UserName", user.UserName.ToString() },
                { "Password", user.Password.ToString() },
                { "Roles", roles },
            };

            var filter = Builders<BsonDocument>.Filter.Eq("UserName", user.UserName);

            Upsert(_users, newDoc, filter);
        }

        public User FindUser(string userName)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("UserName", userName);

            var doc = _users.Find(filter).FirstOrDefaultAsync().Result;

            if (doc == null)
            {
                return null;
            }

            return doc.ToUser();
        }

        public void LogMessage(string message)
        {
            BsonDocument doc = new BsonDocument();
            doc["Message"] = message;

            _logs.InsertOneAsync(doc).Wait();
        }

        private static void Upsert(IMongoCollection<BsonDocument> collection, BsonDocument newDoc, FilterDefinition<BsonDocument> filter)
        {
            var options = new UpdateOptions();
            options.IsUpsert = true;

            collection.ReplaceOneAsync(filter, newDoc, options).Wait();
        }
    }
}
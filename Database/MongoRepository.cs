namespace MukMafiaTool.Database
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Linq;
    using System.Security.Authentication;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using MukMafiaTool.Common;
    using MukMafiaTool.Model;

    public sealed class MongoRepository : IRepository, IDisposable
    {
        private readonly IMongoCollection<BsonDocument> postsCollection;
        private readonly IMongoCollection<BsonDocument> votesCollection;
        private readonly IMongoCollection<BsonDocument> playersCollection;
        private readonly IMongoCollection<BsonDocument> metadataCollection;
        private readonly IMongoCollection<BsonDocument> daysCollection;
        private readonly IMongoCollection<BsonDocument> usersCollection;
        private readonly IMongoCollection<BsonDocument> logsCollection;

        public MongoRepository()
        {
            var connectionString = ConfigurationManager.AppSettings["MongoConnectionString"];
            var databaseName = ConfigurationManager.AppSettings["MongoDatabaseName"];

            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));
            settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls };
            var mongoClient = new MongoClient(connectionString);
            var database = mongoClient.GetDatabase(databaseName);

            this.postsCollection = database.GetCollection<BsonDocument>("Posts");
            this.votesCollection = database.GetCollection<BsonDocument>("Votes");
            this.playersCollection = database.GetCollection<BsonDocument>("Players");
            this.metadataCollection = database.GetCollection<BsonDocument>("Metadata");
            this.daysCollection = database.GetCollection<BsonDocument>("Days");
            this.usersCollection = database.GetCollection<BsonDocument>("Users");
            this.logsCollection = database.GetCollection<BsonDocument>("Logs");
        }

        public void Dispose()
        {
        }

        public IEnumerable<ForumPost> FindAllPosts(bool includeDayZeros = false)
        {
            var documents = this.postsCollection.Find(new BsonDocument()).ToListAsync().Result;

            var posts = documents.Select(d => d.ToForumPost());

            if (!includeDayZeros)
            {
                posts = posts.Where(p => p.Day > 0);
            }

            return posts;
        }

        public IEnumerable<Player> FindAllPlayers()
        {
            var documents = this.playersCollection.Find(new BsonDocument()).ToListAsync().Result;

            return documents.Select(d => d.ToPlayer());
        }

        public IEnumerable<Vote> FindAllVotes()
        {
            var documents = this.votesCollection.Find(new BsonDocument()).ToListAsync().Result;

            return documents.Select(d => d.ToVote());
        }

        public IEnumerable<Vote> FindAllNonUnvotes()
        {
            var filter = Builders<BsonDocument>.Filter.Eq("IsUnovte", false);

            var documents = this.votesCollection.Find(filter).ToListAsync().Result;

            return documents.Select(d => d.ToVote());
        }

        public IEnumerable<Vote> FindAllValidNonUnotes()
        {
            var builder = Builders<BsonDocument>.Filter;

            var filter = builder.Eq("IsUnovte", false)
                & builder.Eq("Invalid", false);

            var documents = this.votesCollection.Find(filter).ToListAsync().Result;

            return documents.Select(d => d.ToVote());
        }

        public Vote FindVote(string forumPostNumber, int postContentIndex)
        {
            var builder = Builders<BsonDocument>.Filter;

            var filter = builder.Eq("ForumPostNumber", forumPostNumber)
                & builder.Eq("PostContentIndex", postContentIndex);

            var document = this.votesCollection.Find(filter).FirstOrDefaultAsync().Result;

            return document.ToVote();
        }

        public IList<ForumPost> FindAllPosts(string playerName)
        {
            var allPosts = this.FindAllPosts();

            return allPosts.Where(p => string.Equals(p.Poster, playerName, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public IList<ForumPost> FindAllPostsContaining(string search)
        {
            var matchingPosts = new List<ForumPost>();

            var allPosts = this.FindAllPosts();

            CultureInfo culture = CultureInfo.InvariantCulture;

            foreach (var post in allPosts)
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
            var result = this.postsCollection.Find(filter).FirstOrDefaultAsync().Result;

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

            var existingPost = this.FindSpecificPost(post.ForumPostNumber);

            // In the case where the post already exists in the database, and the post has just been rescanned
            if (existingPost != null && post.ManuallyEdited == false)
            {
                // Then only upsert if it hasn't been previously edited
                filter = filter & builder.Eq("ManuallyEdited", false);
            }

            Upsert(this.postsCollection, newDoc, filter);
        }

        public void DeleteAllVotes()
        {
            var filter = Builders<BsonDocument>.Filter.Eq("ManuallyEdited", false);

            this.votesCollection.DeleteManyAsync(filter).Wait();
        }

        public void DeleteVotes(string forumPostNumber)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("ForumPostNumber", forumPostNumber);

            this.votesCollection.DeleteManyAsync(filter).Wait();
        }

        public void UpdateLastUpdatedTime()
        {
            this.UpdateLastUpdatedTime(DateTime.UtcNow);
        }

        public void UpdateLastUpdatedTime(DateTime dateTime)
        {
            var doc = new BsonDocument
            {
                { "_id", 1 },
                { "LastUpdatedDateTime", dateTime },
            };

            var filter = Builders<BsonDocument>.Filter.Eq("_id", 1);

            Upsert(this.metadataCollection, doc, filter);
        }

        public DateTime FindLastUpdatedDateTime()
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", 1);

            var doc = this.metadataCollection.Find(filter).FirstOrDefaultAsync().Result;

            return doc["LastUpdatedDateTime"].ToLocalTime();
        }

        public IList<Day> FindAllDays()
        {
            var documents = this.daysCollection.Find(new BsonDocument()).ToListAsync().Result;

            var days = new List<Day>();

            foreach (var doc in documents)
            {
                days.Add(doc.ToDay());
            }

            return days;
        }

        public Day FindCurrentDay()
        {
            var days = this.FindAllDays();
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

            Upsert(this.daysCollection, newDoc, filter);
        }

        // Note: This finds the *current* latest page accessed, not the one when the Repository object was instantiated.</remarks>
        public ForumPost FindLatestPost()
        {
            var sortDefinition = Builders<BsonDocument>.Sort.Descending(d => d["ForumPostNumber"]);

            var doc = this.postsCollection.Find(new BsonDocument()).Sort(sortDefinition).FirstOrDefaultAsync().Result;

            if (doc == null)
            {
                return null;
            }

            return doc.ToForumPost();
        }

        public void UpsertVote(Vote vote)
        {
            this.UpsertVote(vote, false);
        }

        public void UpsertVote(Vote vote, bool overrideManuallyEditedVotes)
        {
            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Eq("ForumPostNumber", vote.ForumPostNumber)
                    & builder.Eq("PostContentIndex", vote.PostContentIndex);

            if (overrideManuallyEditedVotes == false)
            {
                var document = this.votesCollection.Find(filter).FirstOrDefaultAsync().Result;

                if (document != null && document.ToVote().ManuallyEdited)
                {
                    return;
                }
            }

            var newDoc = new BsonDocument
            {
                { "IsUnvote", vote.IsUnvote },
                { "DateTime", vote.DateTime },
                { "Recipient", vote.Recipient ?? string.Empty },
                { "Voter", vote.Voter },
                { "ForumPostNumber", vote.ForumPostNumber },
                { "PostContentIndex", vote.PostContentIndex },
                { "ManuallyEdited", vote.ManuallyEdited },
                { "Day", vote.Day },
                { "Invalid", vote.Invalid },
            };

            Upsert(this.votesCollection, newDoc, filter);
        }

        public void EnsurePlayersInRepo(IEnumerable<string> playerNames, string firstForumPostNumber)
        {
            foreach (var playerName in playerNames)
            {
                if (this.FindPlayer(playerName) == null)
                {
                    var recruitmentDoc = new BsonDocument
                    {
                        { "FactionName", "Town" },
                        { "Allegiance", Allegiance.Town.ToString() },
                        { "ForumPostNumber", firstForumPostNumber },
                    };

                    var determinedAliases = DetermineAliases(playerName);

                    var aliases = new BsonArray();

                    foreach (var alias in determinedAliases)
                    {
                        aliases.Add(BsonValue.Create(alias));
                    }

                    var playerDoc = new BsonDocument
                    {
                        { "Name", playerName },
                        { "Recruitments", new BsonArray { recruitmentDoc } },
                        { "Participating", true },
                        { "Fatality", string.Empty },
                        { "Character", string.Empty },
                        { "Role", string.Empty },
                        { "Notes", string.Empty },
                        { "Aliases", aliases },
                    };

                    this.playersCollection.InsertOneAsync(playerDoc).Wait();
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

            foreach (var alias in player.Aliases)
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
                { "Role", player.Role },
                { "Notes", player.Notes },
                { "Aliases", aliases }
            };

            var filter = Builders<BsonDocument>.Filter.Eq("Name", player.Name);

            Upsert(this.playersCollection, newDoc, filter);
        }

        public Player FindPlayer(string name)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("Name", name);

            var player = this.playersCollection.Find(filter).FirstOrDefaultAsync().Result;

            if (player == null)
            {
                return null;
            }

            return player.ToPlayer();
        }

        public Day FindDay(int dayNumber)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", dayNumber);

            var doc = this.daysCollection.Find(filter).FirstOrDefaultAsync().Result;

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
                roles.Add(role);
            }

            var newDoc = new BsonDocument
            {
                { "UserName", user.UserName.ToString() },
                { "Password", user.Password.ToString() },
                { "Roles", roles },
            };

            var filter = Builders<BsonDocument>.Filter.Eq("UserName", user.UserName);

            Upsert(this.usersCollection, newDoc, filter);
        }

        public User FindUser(string userName)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("UserName", userName);

            var doc = this.usersCollection.Find(filter).FirstOrDefaultAsync().Result;

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

            this.logsCollection.InsertOneAsync(doc).Wait();
        }

        public IEnumerable<string> FindAllLogMessages()
        {
            var docs = this.logsCollection.Find(new BsonDocument()).ToListAsync().Result;

            return docs.Select(d => d["Message"].ToString());
        }

        public void DeleteAllLogMessages()
        {
            this.logsCollection.DeleteManyAsync(Builders<BsonDocument>.Filter.Empty).Wait();
        }

        private static void Upsert(IMongoCollection<BsonDocument> collection, BsonDocument newDoc, FilterDefinition<BsonDocument> filter)
        {
            var options = new UpdateOptions();
            options.IsUpsert = true;

            collection.ReplaceOneAsync(filter, newDoc, options).Wait();
        }

        private static IEnumerable<string> DetermineAliases(string playerName)
        {
            IList<string> aliases = new List<string>();

            if (playerName.StartsWith("MS "))
            {
                aliases.Add(playerName.Substring(3));
                aliases.Add(playerName.Remove(2, 1));
            }

            return aliases;
        }
    }
}
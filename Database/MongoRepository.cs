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
        private IMongoCollection<BsonDocument> _metadata;
        private IMongoCollection<BsonDocument> _exclusions;
        private IMongoCollection<BsonDocument> _days;
        private IMongoCollection<BsonDocument> _logs;

        private ForumPost _latestPostAtTimeOfCreation;

        public MongoRepository()
        {
            var connectionString = ConfigurationManager.AppSettings["MongoConnectionString"];

            MongoClient client = new MongoClient(connectionString);

            var database = client.GetDatabase("MongoLab-o");

            _posts = database.GetCollection<BsonDocument>("posts");
            _votes = database.GetCollection<BsonDocument>("votes");
            _metadata = database.GetCollection<BsonDocument>("metadata");
            _exclusions = database.GetCollection<BsonDocument>("exclusions");
            _days = database.GetCollection<BsonDocument>("days");
            _logs = database.GetCollection<BsonDocument>("logs");

            _latestPostAtTimeOfCreation = FindAllPosts().OrderBy(p => p.ThreadPostNumber).LastOrDefault();
        }

        public IList<ForumPost> FindAllPosts()
        {
            var task = _posts.Find(new BsonDocument()).ToListAsync();
            task.Wait();
            var documents = task.Result;

            var posts = new List<ForumPost>();

            foreach (var document in documents)
            {
                posts.Add(ConvertBsonDocToForumPost(document));
            }

            return posts;
        }

        public IList<string> FindAllPlayerNames()
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
            var task = _posts.Find(filter).FirstOrDefaultAsync();
            task.Wait();
            var result = task.Result;
            var post = ConvertBsonDocToForumPost(result);
            return post;
        }

        public void UpsertPosts(IList<ForumPost> posts)
        {
            var options = new UpdateOptions();
            options.IsUpsert = true;

            foreach (var post in posts)
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
                    };

                var filter = Builders<BsonDocument>.Filter.Eq("ForumPostNumber", post.ForumPostNumber);

                var task = _posts.ReplaceOneAsync(filter, newDoc, options);
                task.Wait();
            }
        }

        public bool InsertNewPosts(IList<ForumPost> posts)
        {
            bool pageHasAllNewPosts = true;

            foreach (var post in posts)
            {
                if (WhetherToInsertPost(post))
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
                    };

                    var task = _posts.InsertOneAsync(newDoc);

                    task.Wait();
                }
                else
                {
                    pageHasAllNewPosts = false;
                }
            }

            return pageHasAllNewPosts;
        }

        public void WipeVotes()
        {
            var task = _votes.DeleteManyAsync(new BsonDocument());
            task.Wait();
        }

        public IList<Vote> FindAllVotes()
        {
            IList<Vote> allVotes = new List<Vote>();

            var task = _votes.Find(new BsonDocument()).ToListAsync();
            task.Wait();

            var documents = task.Result;
            
            foreach(var doc in documents)
            {
                allVotes.Add(ConvertBsonDocToVote(doc));
            }

            return allVotes;
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
                var deleteTask = _votes.DeleteManyAsync(filter);
                deleteTask.Wait();

                var doc = new BsonDocument
                {
                    { "DateTime", vote.DateTime },
                    { "Voter", vote.Voter },
                    { "Recipient", vote.Recipient },
                    { "ForumPostNumber", vote.ForumPostNumber },
                    { "PostContentIndex", vote.PostContentIndex },
                };

                var insertTask = _votes.InsertOneAsync(doc);
                insertTask.Wait();
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

            var options = new UpdateOptions();
            options.IsUpsert = true;

            var task = _metadata.ReplaceOneAsync(filter, doc, options);
            task.Wait();
        }

        public DateTime FindLastUpdatedDateTime()
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", 1);

            var task = _metadata.Find(filter).FirstOrDefaultAsync();
            task.Wait();

            var document = task.Result;

            return document["LastUpdatedDateTime"].ToLocalTime();
        }

        public IList<string> FindAllExclusions()
        {
            var task = _exclusions.Find(new BsonDocument()).ToListAsync();
            task.Wait();
            var documents = task.Result;

            IList<string> result = new List<string>();

            foreach (var doc in documents)
            {
                result.Add(doc["Name"].ToString());
            }

            return result;
        }

        public IList<Day> FindAllDays()
        {
            var task = _days.Find(new BsonDocument()).ToListAsync();
            task.Wait();
            var documents = task.Result;

            var days = new List<Day>();

            foreach (var document in documents)
            {
                days.Add(ConvertBsonDocDay(document));
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

            var task = _posts.Find(new BsonDocument()).Sort(sortDefinition).FirstOrDefaultAsync();
            task.Wait();
            var doc = task.Result;
            return ConvertBsonDocToForumPost(doc);
        }

        public void LogMessage(string message)
        {
            BsonDocument doc = new BsonDocument();
            doc["Message"] = message;

            var task = _logs.InsertOneAsync(doc);
            task.Wait();
        }

        private void ProcessUnvote(Vote vote)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("Voter", vote.Voter);
            var task = _votes.DeleteManyAsync(filter);
            task.Wait();
        }

        private bool WhetherToInsertPost(ForumPost post)
        {
            bool insertPost = false;

            // If we have no posts in the database
            if (_latestPostAtTimeOfCreation == null)
            {
                // always insert a post
                insertPost = true;
            }
            else
            {
                // If the post forum number is after the latest post forum number in the db
                if (string.Compare(post.ForumPostNumber, _latestPostAtTimeOfCreation.ForumPostNumber) > 0)
                {
                    insertPost = true;
                }
            }

            // If the post doesn't have a "Day" (e.g. it was Day 0)
            if (post.Day < 1)
            {
                insertPost = false;
            }

            // If the post was made < 5 minutes ago, we don't want to add it, in case there's a merge or edit to it
            if (DateTime.Now - post.DateTime.ToLocalTime() < TimeSpan.FromMinutes(5))
            {
                insertPost = false;
            }

            return insertPost;
        }

        private ForumPost ConvertBsonDocToForumPost(BsonDocument doc)
        {
            return new ForumPost
            {
                ThreadPostNumber = doc["ThreadPostNumber"].ToInt32(),
                ForumPostNumber = doc["ForumPostNumber"].ToString(),
                Poster = doc["Poster"].ToString(),
                DateTime = doc["DateTime"].ToUniversalTime(),
                Content = new HtmlString(doc["Content"].ToString()),
                Day = doc["Day"].ToInt32(),
                PageNumber = doc["PageNumber"].ToInt32(),
            };
        }

        private Vote ConvertBsonDocToVote(BsonDocument doc)
        {
            return new Vote
            {
                DateTime = doc["DateTime"].ToLocalTime(),
                Recipient = doc["Recipient"].ToString(),
                Voter = doc["Voter"].ToString(),
                ForumPostNumber = doc["ForumPostNumber"].ToString(),
                PostContentIndex = doc["PostContentIndex"].ToInt32(),
            };
        }

        private Day ConvertBsonDocDay(BsonDocument doc)
        {
            return new Day
            {
                Number = doc["_id"].ToInt32(),
                StartForumPostNumber = doc["StartForumPostNumber"].ToString(),
                EndForumPostNumber = doc["EndForumPostNumber"].ToString(),
            };
        }
    }
}
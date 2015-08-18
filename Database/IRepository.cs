using System;
using System.Collections.Generic;
using MukMafiaTool.Model;

namespace MukMafiaTool.Database
{
    public interface IRepository
    {
        IList<Day> FindAllDays();
        IList<string> FindAllExclusions();
        IList<string> FindAllPlayerNames();
        IList<ForumPost> FindAllPosts();
        IList<ForumPost> FindAllPosts(string playerName);
        IList<ForumPost> FindAllPostsContaining(string search);
        IList<Vote> FindAllVotes();
        Day FindCurrentDay();
        DateTime FindLastUpdatedDateTime();
        ForumPost FindLatestPost();
        ForumPost FindSpecificPost(string forumPostNumber);
        bool InsertNewPosts(IList<ForumPost> posts);
        void LogMessage(string message);
        void ProcessVote(Vote vote);
        void UpdateLastUpdated();
        void UpsertPosts(IList<ForumPost> posts);
        void WipeVotes();
    }
}

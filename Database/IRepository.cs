using System;
using System.Collections.Generic;
using MukMafiaTool.Model;

namespace MukMafiaTool.Database
{
    public interface IRepository
    {
        IList<Day> FindAllDays();
        IEnumerable<ForumPost> FindAllPosts();
        IEnumerable<Player> FindAllPlayers();
        IEnumerable<Vote> FindAllVotes();
        IList<ForumPost> FindAllPosts(string playerName);
        IList<ForumPost> FindAllPostsContaining(string search);
        Day FindCurrentDay();
        DateTime FindLastUpdatedDateTime();
        ForumPost FindLatestPost();
        ForumPost FindSpecificPost(string forumPostNumber);
        void LogMessage(string message);
        void UpdateLastUpdated();
        void UpsertPost(ForumPost post);
        void UpsertVote(Vote vote);
        void DeleteVotes(string forumPostNumber);
        Player FindPlayer(string name);
        void UpsertPlayer(Player player);
        void EnsurePlayersInRepo(IList<ForumPost> posts);
        void EnsurePlayersInRepo(IEnumerable<string> playerNames);
    }
}

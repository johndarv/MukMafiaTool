﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MukMafiaTool.Model;

namespace MukMafiaTool.Common
{
    public interface IRepository
    {
        IList<Day> FindAllDays();
        Day FindCurrentDay();
        void UpsertDay(Day day);
        IEnumerable<ForumPost> FindAllPosts(bool includeDayZeros = false);
        IList<ForumPost> FindAllPosts(string playerName);
        IList<ForumPost> FindAllPostsContaining(string search);
        ForumPost FindLatestPost();
        IEnumerable<Player> FindAllPlayers();
        IEnumerable<Vote> FindAllVotes();
        DateTime FindLastUpdatedDateTime();
        ForumPost FindSpecificPost(string forumPostNumber);
        void LogMessage(string message);
        void UpdateLastUpdated();
        void UpsertPost(ForumPost post);
        void UpsertVote(Vote vote);
        void DeleteAllVotes();
        void DeleteVotes(string forumPostNumber);
        Player FindPlayer(string name);
        void UpsertPlayer(Player player);
        void EnsurePlayersInRepo(IEnumerable<string> playerNames, string defaultForumPostNumber);
        Day FindDay(int dayNumber);
        void UpsertUser(User user);
        User FindUser(string userName);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MukMafiaTool.Model;

namespace MukMafiaTool.Database
{
    internal static class BsonValueConverters
    {
        public static ForumPost ToForumPost(this BsonValue doc)
        {
            return new ForumPost
            {
                ForumPostNumber = doc["ForumPostNumber"].ToString(),
                Poster = doc["Poster"].ToString(),
                DateTime = doc["DateTime"].ToUniversalTime(),
                Content = new HtmlString(doc["Content"].ToString()),
                Day = doc["Day"].ToInt32(),
                PageNumber = doc["PageNumber"].ToInt32(),
                LastScanned = doc["LastScanned"].ToUniversalTime(),
                ManuallyEdited = doc["ManuallyEdited"].ToBoolean(),
            };
        }

        public static Vote ToVote(this BsonValue doc)
        {
            return new Vote
            {
                IsUnvote = doc["IsUnvote"].ToBoolean(),
                DateTime = doc["DateTime"].ToLocalTime(),
                Recipient = doc["Recipient"].ToString(),
                Voter = doc["Voter"].ToString(),
                ForumPostNumber = doc["ForumPostNumber"].ToString(),
                PostContentIndex = doc["PostContentIndex"].ToInt32(),
                ManuallyEdited = doc["ManuallyEdited"] != null ? doc["ManuallyEdited"].ToBoolean() : false,
                Day = doc["Day"].ToInt32(),
            };
        }

        public static Day ToDay(this BsonValue doc)
        {
            return new Day
            {
                Number = doc["_id"].ToInt32(),
                StartForumPostNumber = doc["StartForumPostNumber"].ToString(),
                EndForumPostNumber = doc["EndForumPostNumber"].ToString(),
            };
        }

        public static Player ToPlayer(this BsonValue doc)
        {
            IList<Recruitment> recruitments = new List<Recruitment>();

            foreach (var recruitmentDoc in doc["Recruitments"].AsBsonArray)
            {
                recruitments.Add(recruitmentDoc.ToRecruitment());
            }

            IList<string> aliases = new List<string>();

            foreach (var alias in doc["Aliases"].AsBsonArray)
            {
                aliases.Add(alias.ToString());
            }

            return new Player
            {
                Name = doc["Name"].ToString(),
                Recruitments = recruitments,
                Participating = doc["Participating"].ToBoolean(),
                Fatality = doc["Fatality"].ToString(),
                Character = doc["Character"].ToString(),
                Notes = doc["Notes"].ToString(),
                Aliases = aliases.ToArray(),
            };
        }

        public static Recruitment ToRecruitment(this BsonValue doc)
        {
            Allegiance allegiance;

            if (!Enum.TryParse<Allegiance>(doc["Allegiance"].ToString(), out allegiance))
            {
                allegiance = Allegiance.Town;
            }

            return new Recruitment
            {
                FactionName = doc["FactionName"].ToString(),
                Allegiance = allegiance,
                ForumPostNumber = doc["ForumPostNumber"].ToString(),
            };
        }

        public static User ToUser(this BsonValue doc)
        {
            IList<string> roles = new List<string>();

            foreach (var role in doc["Roles"].AsBsonArray)
            {
                roles.Add(role.AsString);
            }

            return new User
            {
                UserName = doc["UserName"].ToString(),
                Password = doc["Password"].ToString(),
                Roles = roles.ToArray(),
            };
        }
    }
}

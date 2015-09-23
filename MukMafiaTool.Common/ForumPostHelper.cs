﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MukMafiaTool.Model;

namespace MukMafiaTool.Common
{
    public static class ForumPostHelper
    {
        public static int DetermineDay(this ForumPost post, IEnumerable<Day> days)
        {
            return DetermineDay(post.ForumPostNumber, days);
        }

        public static int DetermineDay(string forumPostNumber, IEnumerable<Day> days)
        {
            foreach (var day in days)
            {
                if (string.Compare(forumPostNumber, day.StartForumPostNumber) >= 0)
                {
                    if (string.IsNullOrEmpty(day.EndForumPostNumber) || string.Compare(forumPostNumber, day.EndForumPostNumber) <= 0)
                    {
                        return day.Number;
                    }
                }
            }

            return 0;
        }
    }
}

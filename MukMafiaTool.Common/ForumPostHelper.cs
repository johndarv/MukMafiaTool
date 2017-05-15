namespace MukMafiaTool.Common
{
    using System.Collections.Generic;
    using MukMafiaTool.Model;

    public static class ForumPostHelper
    {
        public static int DetermineDay(this ForumPost post, IRepository repo)
        {
            return DetermineDay(post, repo.FindAllDays());
        }

        public static int DetermineDay(this ForumPost post, IEnumerable<Day> days)
        {
            return post.ForumPostNumber.DetermineDay(days);
        }
    }
}

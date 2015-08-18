using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MukMafiaTool.ForumScanner.Extensions
{
    public static class StringExtensions
    {
        public static IList<int> EndIndexOfNotInQuoteblock(this string content, string search)
        {
            var indexes = new List<int>();

            int i = 0;
            int quoteLevel = 0;

            while (i < content.Length)
            {
                if (content.Length - i >= 11 && string.Equals(content.Substring(i, 11), "<blockquote"))
                {
                    quoteLevel++;
                }
                else if (content.Length - i >= 13 && string.Equals(content.Substring(i, 13), "</blockquote>"))
                {
                    if (quoteLevel > 0)
                    {
                        quoteLevel--;
                    }
                }
                else if (content.Length - i >= search.Length && string.Equals(content.Substring(i, search.Length), search))
                {
                    if (quoteLevel == 0)
                    {
                        indexes.Add(i + search.Length);
                    }
                }

                i++;
            }

            return indexes;
        }
    }
}
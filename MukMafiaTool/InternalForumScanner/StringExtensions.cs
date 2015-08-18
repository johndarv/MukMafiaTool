using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MukMafiaTool.InternalForumScanner
{
    public static class StringExtensions
    {
        public static string FilterOutContentInQuoteBlocks(this string content)
        {
            StringBuilder builder = new StringBuilder();

            int i = 0;
            int quoteLevel = 0;

            while (i < content.Length)
            {
                if (content.Length - i >= 11 && string.Equals(content.Substring(i, 11), "<blockquote"))
                {
                    quoteLevel++;
                    i = i + 11;
                }
                else if (content.Length - i >= 12 && string.Equals(content.Substring(i, 12), "</blockquote"))
                {
                    if (quoteLevel > 0)
                    {
                        quoteLevel--;
                    }

                    i = i + 12;
                }
                else
                {
                    if (quoteLevel == 0)
                    {
                        builder.Append(content[i]);
                    }

                    i++;
                }
            }

            return builder.ToString();
        }

        public static string RemoveWhiteSpace(this string str)
        {
            return str.Replace(" ", string.Empty);
        }

        public static string RemoveNewLineAndTabChars(this string str)
        {
            str = str.Replace("\n", string.Empty);
            str = str.Replace("\r", string.Empty);
            str = str.Replace("\t", string.Empty);

            return str;
        }

        public static IEnumerable<int> AllIndexesOf(this string str, string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("the string to find may not be empty", "value");
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    break;
                yield return index;
            }
        }

        public static bool IsInBold(this string content, int index)
        {
            int i = 0;
            int boldLevel = 0;

            while (i < content.Length)
            {
                if ((content.Length - i >= 8 && string.Equals(content.Substring(i, 8), "<strong>")) ||
                    (content.Length - i >= 20 && string.Equals(content.Substring(i, 20), "<strong class='bbc'>")) ||
                    (content.Length - i >= 20 && string.Equals(content.Substring(i, 20), "<strong class=\"bbc\">")) ||
                    (content.Length - i >= 3 && string.Equals(content.Substring(i, 3), "<b>")))
                {
                    boldLevel++;
                }
                else if ((content.Length - i >= 8 && string.Equals(content.Substring(i, 8), "</strong")) ||
                         (content.Length - i >= 4 && string.Equals(content.Substring(i, 4), "</b>")))
                {
                    if (boldLevel > 0)
                    {
                        boldLevel--;
                    }
                }
                
                if (i == index)
                {
                    if (boldLevel > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                i++;
            }

            return false;
        }
    }
}
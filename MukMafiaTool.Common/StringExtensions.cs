using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace MukMafiaTool.Common
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

        public static string FilterOutSpanTags(this string content)
        {
            StringBuilder builder = new StringBuilder();

            int i = 0;
            int chevronLevel = 0;

            while (i < content.Length)
            {
                if (content.Length - i >= 5 && (string.Equals(content.Substring(i, 5), "<span")))
                {
                    chevronLevel++;
                    i = i + 5;
                }
                else if (content.Length - i >= 6 && (string.Equals(content.Substring(i, 6), "</span")))
                {
                    chevronLevel++;
                    i = i + 6;
                }
                else if (char.Equals(content[i], '>'))
                {
                    if (chevronLevel > 0)
                    {
                        chevronLevel--;
                    }
                    else
                    {
                        builder.Append(content[i]);
                    }

                    i++;
                }
                else
                {
                    if (chevronLevel == 0)
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

        public static string ReplaceNonBreakingSpacesWithSpaces(this string str)
        {
            return str.Replace("&#160;", " ");
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

        public static string StripHtml(this string htmlText, bool decode = true)
        {
            Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
            var stripped = reg.Replace(htmlText, "");
            return decode ? HttpUtility.HtmlDecode(stripped) : stripped;
        }

        public static IList<string> SplitOnSeparatorExceptInSpeechMarks(this string str, char separator)
        {
            IList<string> result = new List<string>();

            int i = 0;
            bool inSpeechMarks = false;
            string currentElement = string.Empty;

            while (i < str.Length)
            {
                char character = str[i];
                i++;

                if (char.Equals(character, separator))
                {
                    if (inSpeechMarks)
                    {
                        currentElement = currentElement + character;
                    }
                    else
                    {
                        result.Add(currentElement);
                        currentElement = string.Empty;
                    }
                }
                else if (char.Equals(character, '"'))
                {
                    if (inSpeechMarks)
                    {
                        currentElement = currentElement + character;
                        inSpeechMarks = false;
                    }
                    else
                    {
                        currentElement = currentElement + character;
                        inSpeechMarks = true;
                    }
                }
                else
                {
                    currentElement = currentElement + character;
                }
            }

            result.Add(currentElement);

            return result;
        }
    }
}
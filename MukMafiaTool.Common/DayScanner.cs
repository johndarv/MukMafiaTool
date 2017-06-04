namespace MukMafiaTool.Common
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using MukMafiaTool.Model;

    public class DayScanner
    {
        private IRepository repo;

        public DayScanner(IRepository repository)
        {
            this.repo = repository;
        }

        public void UpdateDays(IEnumerable<ForumPost> posts)
        {
            foreach (var post in posts)
            {
                Regex startOfDayRegex = new Regex(@"\[start of day [0-9]+\]", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                Regex endOfDayRegex = new Regex(@"\[end of day [0-9]+\]", RegexOptions.IgnoreCase | RegexOptions.Multiline);

                var relevantPostContent = post.Content
                    .ToString()
                    .FilterOutContentInQuoteBlocks()
                    .FilterOutSpanTags()
                    .ReplaceNonBreakingSpacesWithSpaces()
                    .RemoveNewLineAndTabChars()
                    .RemoveUnnecessaryClosedOpenHtmlTags()
                    .ReplaceAtMentionsWithPlainNameText()
                    .FilterOutStrongTagsAfterTheWordVote()
                    .ReplaceNonBreakingSpaceCharactersWithRegularWhiteSpaceCharacters();

                var startMatched = startOfDayRegex.Match(relevantPostContent);
                var endMatched = endOfDayRegex.Match(relevantPostContent);

                Regex numbersRegex = new Regex("[0-9]+");

                if (startMatched.Success)
                {
                    var numbers = numbersRegex.Match(startMatched.Value);

                    int dayNumber;
                    if (int.TryParse(numbers.Value, out dayNumber))
                    {
                        var day = new Day
                        {
                            Number = dayNumber,
                            StartForumPostNumber = post.ForumPostNumber,
                            EndForumPostNumber = string.Empty,
                        };

                        this.repo.UpsertDay(day);
                    }
                }
                else if (endMatched.Success)
                {
                    var numbers = numbersRegex.Match(endMatched.Value);

                    int dayNumber;
                    if (int.TryParse(numbers.Value, out dayNumber))
                    {
                        Day day = this.repo.FindDay(dayNumber);

                        if (day != null)
                        {
                            day.EndForumPostNumber = post.ForumPostNumber;

                            this.repo.UpsertDay(day);
                        }
                    }
                }
            }
        }
    }
}

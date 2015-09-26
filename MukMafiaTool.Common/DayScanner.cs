using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MukMafiaTool.Model;

namespace MukMafiaTool.Common
{
    public class DayScanner
    {
        private IRepository _repo;

        public DayScanner(IRepository repository)
        {
            _repo = repository;
        }

        public void UpdateDays(IEnumerable<ForumPost> posts)
        {
            foreach (var post in posts)
            {
                Regex startOfDayRegex = new Regex("\\[start of day [0-9]+\\]", RegexOptions.IgnoreCase & RegexOptions.Multiline);
                Regex endOfDayRegex = new Regex("\\[end of day [0-9]+\\]", RegexOptions.IgnoreCase & RegexOptions.Multiline);

                var startMatched = startOfDayRegex.Match(post.Content.ToString());
                var endMatched = endOfDayRegex.Match(post.Content.ToString());

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

                        _repo.UpsertDay(day);
                    }
                }
                else if (endMatched.Success)
                {
                    var numbers = numbersRegex.Match(endMatched.Value);

                    int dayNumber;
                    if (int.TryParse(numbers.Value, out dayNumber))
                    {
                        Day day = _repo.FindDay(dayNumber);

                        if (day != null)
                        {
                            day.EndForumPostNumber = post.ForumPostNumber;

                            _repo.UpsertDay(day);
                        }
                    }
                }
            }
        }
    }
}

namespace Tests
{
    using System.Collections.Generic;
    using FluentAssertions;
    using MukMafiaTool.Common;
    using MukMafiaTool.Model;
    using Xunit;

    public class StringExtensionsTests
    {
        [Fact]
        public void FilterOutQuoteBlockTests()
        {
            string str = "hhhhhhhhhhhhhhhhhhhhhhhhh <blockquote>not uahsdhsyd</blockquote> aushdaysgdygs    l";
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual("hhhhhhhhhhhhhhhhhhhhhhhhh > aushdaysgdygs    l", str.FilterOutContentInQuoteBlocks());
        }

        [Fact]
        public void RemoveWhiteSpaceTest()
        {
            string str = "    hsuhds  llll     ";
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual("hsuhdsllll", str.RemoveWhiteSpace());
        }

        [Fact]
        public void IsInBoldTest()
        {
            string str = "h <strong class='bbc'>only this ohhh yeahhhhh</strong> aushdaysgdygs    l";
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(str.IsInBold(30));
        }

        [Fact]
        public void IsInBoldTest2()
        {
            string str = "h <strong class='bbc'>only this ohhh yeahhhhh</strong> aushdaysgdygs    l";
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(str.IsInBold(1));
        }

        [Fact]
        public void IsInBoldTest3()
        {
            string str = "h <strong>only this ohhh yeahhhhh</strong> aushdaysgdygs    l";
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(str.IsInBold(17));
        }

        [Fact]
        public void IsInBoldTest4()
        {
            string str = "h <b>only this ohhh yeahhhhh</b> aushdaysgdygs    l";
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(str.IsInBold(12));
        }

        [Fact]
        public void ReplaceAtMentionsWithPlainNameText()
        {
            string value = "<a contenteditable=\"false\" data-ipshover=\"\" data-ipshover-target=\"https://www.rllmukforum.com/index.php?/profile/27618-mr-beaver/&amp;do=hovercard\" data-mentionid=\"27618\" href=\"https://www.rllmukforum.com/index.php?/profile/27618-mr-beaver/\" rel=\"\">@Mr Beaver</a>";
            var result = value.ReplaceAtMentionsWithPlainNameText();

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(result, "Mr Beaver");
        }

        [Fact]
        public void FilterOutStrongTagsAfterTheWordVote()
        {
            var result = "vote: <strong>snowbind".FilterOutStrongTagsAfterTheWordVote();

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(result, "vote: snowbind");
        }

        [Theory]
        [InlineData("1", 0)]
        [InlineData("3", 1)]
        [InlineData("4", 1)]
        [InlineData("5", 1)]
        [InlineData("9", 2)]
        [InlineData("12", 2)]
        [InlineData("15", 3)]
        [InlineData("21", 4)]
        [InlineData("25", 4)]
        [InlineData("10000", 4)]
        internal static void DetermineDayTest(string forumPostNumber, int expectedDay)
        {
            var days = new List<Day>
            {
                new Day { Number = 1, StartForumPostNumber = "3", EndForumPostNumber = "5" },
                new Day { Number = 2, StartForumPostNumber = "6", EndForumPostNumber = "10" },
                new Day { Number = 3, StartForumPostNumber = "15", EndForumPostNumber = "20" },
                new Day { Number = 4, StartForumPostNumber = "21", EndForumPostNumber = string.Empty },
            };

            forumPostNumber.DetermineDay(days).ShouldBeEquivalentTo(expectedDay, because: "the day should have been determined correctly.");
        }

        [Theory]
        [InlineData("11375129", 2)]
        internal static void DetermineDayTest_WithRealisticForumPostNumbers(string forumPostNumber, int expectedDay)
        {
            var days = new List<Day>
            {
                new Day { Number = 1, StartForumPostNumber = "11368576", EndForumPostNumber = "11373503" },
                new Day { Number = 2, StartForumPostNumber = "11375085", EndForumPostNumber = "11384381" },
            };

            forumPostNumber.DetermineDay(days).ShouldBeEquivalentTo(expectedDay, because: "the day should have been determined correctly.");
        }
    }
}

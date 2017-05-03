using Microsoft.VisualStudio.TestTools.UnitTesting;
using MukMafiaTool.Common;

namespace Tests
{
    [TestClass]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void FilterOutQuoteBlockTests()
        {
            string str = "hhhhhhhhhhhhhhhhhhhhhhhhh <blockquote>not uahsdhsyd</blockquote> aushdaysgdygs    l";
            Assert.AreEqual("hhhhhhhhhhhhhhhhhhhhhhhhh > aushdaysgdygs    l", str.FilterOutContentInQuoteBlocks());
        }

        [TestMethod]
        public void RemoveWhiteSpaceTest()
        {
            string str = "    hsuhds  llll     ";
            Assert.AreEqual("hsuhdsllll", str.RemoveWhiteSpace());
        }

        [TestMethod]
        public void IsInBoldTest()
        {
            string str = "h <strong class='bbc'>only this ohhh yeahhhhh</strong> aushdaysgdygs    l";
            Assert.IsTrue(str.IsInBold(30));
        }

        [TestMethod]
        public void IsInBoldTest2()
        {
            string str = "h <strong class='bbc'>only this ohhh yeahhhhh</strong> aushdaysgdygs    l";
            Assert.IsFalse(str.IsInBold(1));
        }

        [TestMethod]
        public void IsInBoldTest3()
        {
            string str = "h <strong>only this ohhh yeahhhhh</strong> aushdaysgdygs    l";
            Assert.IsTrue(str.IsInBold(17));
        }

        [TestMethod]
        public void IsInBoldTest4()
        {
            string str = "h <b>only this ohhh yeahhhhh</b> aushdaysgdygs    l";
            Assert.IsTrue(str.IsInBold(12));
        }

        [TestMethod]
        public void ReplaceAtMentionsWithPlainNameText()
        {
            string value = "<a contenteditable=\"false\" data-ipshover=\"\" data-ipshover-target=\"https://www.rllmukforum.com/index.php?/profile/27618-mr-beaver/&amp;do=hovercard\" data-mentionid=\"27618\" href=\"https://www.rllmukforum.com/index.php?/profile/27618-mr-beaver/\" rel=\"\">@Mr Beaver</a>";
            var result = value.ReplaceAtMentionsWithPlainNameText();

            Assert.AreEqual(result, "Mr Beaver");
        }

        [TestMethod]
        public void FilterOutStrongTagsAfterTheWordVote()
        {
            var result = "vote: <strong>snowbind".FilterOutStrongTagsAfterTheWordVote();

            Assert.AreEqual(result, "vote: snowbind");
        }
    }
}

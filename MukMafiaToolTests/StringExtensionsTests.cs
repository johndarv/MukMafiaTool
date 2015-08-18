using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MukMafiaTool.InternalForumScanner;

namespace MukMafiaToolTests
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
    }
}

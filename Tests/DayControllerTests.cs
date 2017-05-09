namespace Tests
{
    using System.Net;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MukMafiaTool.Controllers;
    using MukMafiaTool.Database;

    [TestClass]
    public class DayControllerTests
    {
        [Ignore]
        [TestMethod]
        public void TestDayController()
        {
            var dayController = new DayController(new MongoRepository());

            var result = dayController.RedetermineDays();

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }
    }
}

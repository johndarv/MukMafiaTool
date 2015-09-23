﻿using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MukMafiaTool.Controllers;
using MukMafiaTool.Database;

namespace MukMafiaToolTests
{
    [TestClass]
    public class DayControllerTests
    {
        [Ignore]
        [TestMethod]
        public void TestDayController()
        {
            var dayController = new DayController(new MongoRepository());

            var result = dayController.RecalculateDays();

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }
    }
}

﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MukMafiaTool.Database;

namespace Tests
{
    [TestClass]
    public class DatabaseTests
    {
        [Ignore]
        [TestMethod]
        public void UpsertLastUpdatedTime()
        {
            var repo = new MongoRepository();

            repo.UpdateLastUpdatedTime(DateTime.UtcNow);
        }
    }
}
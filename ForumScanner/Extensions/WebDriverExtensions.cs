using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace MukMafiaTool.ForumScanner.Extensions
{
    public static class WebDriverExtensions
    {
        public static void WaitUntil(this IWebDriver driver, Func<IWebDriver, bool> condition)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

            wait.Until(condition);
        }
    }
}

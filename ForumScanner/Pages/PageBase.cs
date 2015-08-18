using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace MukMafiaTool.ForumScanner.Pages
{
    public abstract class PageBase
    {
        public PageBase(IWebDriver driver)
        {
            Driver = driver;
            PageFactory.InitElements(Driver, this);
        }

        protected IWebDriver Driver { get; private set; }
    }
}

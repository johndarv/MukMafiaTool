using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;
using MukMafiaTool.ForumScanner.Extensions;
using System.Configuration;

namespace MukMafiaTool.ForumScanner.Pages
{
    public class ForumHomePage : PageBase
    {
        [FindsBy(How = How.Id, Using = "sign_in")]
        private IWebElement _signInLink = null;

        [FindsBy(How = How.Id, Using = "ips_username")]
        private IWebElement _usernameTextBox = null;

        [FindsBy(How = How.Id, Using = "ips_password")]
        private IWebElement _passwordTextBox = null;

        [FindsBy(How = How.Id, Using = "user_link")]
        private IWebElement _userLink = null;

        private string _forumUsername;
        private string _forumPassword;

        public ForumHomePage(IWebDriver driver)
            : base(driver)
        {
            _forumUsername = ConfigurationManager.AppSettings["ForumUsername"];
            _forumPassword = ConfigurationManager.AppSettings["ForumPassword"];
        }

        public void SignIn()
        {
            Driver.WaitUntil(d => _signInLink.Displayed);

            _signInLink.Click();

            Driver.WaitUntil(d => _usernameTextBox.Displayed);

            _usernameTextBox.SendKeys(_forumUsername);
            _passwordTextBox.SendKeys(_forumPassword);

            _passwordTextBox.SendKeys(Keys.Enter);

            Driver.WaitUntil(d => _userLink.Displayed);
        }
    }
}

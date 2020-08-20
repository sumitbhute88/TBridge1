using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Rebex.Net;
using System;
using System.IO;

namespace JabaTalkSelTest
{
    public class Data
    {
        public static string webUrl = "http://jt-dev.azurewebsites.net/#/SignUp";
        public static string btnFlights = "Flights";

        public static string fromTextId = "fromCity";
        public static string toTextId = "toCity";
        public static string roundTrip = "////*[@id=\"root\"]/div/div[2]/div/div/div[1]/ul/li[2]/text()";
        public static string btnSearchId = "Search";
        public static string[] langList = new string[] { "English", "Dutch" };
        public static string emailLogin = "seltest68";
        public static string emailPswd = "Discovery@123";
    }

    [TestClass]
    public class Tester
    {
        protected static ChromeDriver chromeDriver;
        protected static StreamWriter streamWriter;
        protected static WebDriverWait aWait;
        Imap client = new Imap();

        [TestInitialize]
        public void Init()
        {
            //initialize browser and launch url
            streamWriter = new StreamWriter(@"SeleTest.log");
            chromeDriver = new ChromeDriver();
            aWait = new WebDriverWait(chromeDriver, new TimeSpan(0, 1, 0));

            streamWriter.WriteLine("1. Launch browser and navigated to url");
            chromeDriver.Navigate().GoToUrl(Data.webUrl);
            chromeDriver.Manage().Window.Maximize();
        }

        [TestCleanup]
        public void Cleanup()
        {
            chromeDriver.Close();
            streamWriter.Close();
            client.Disconnect();
        }


        [TestMethod]
        public void VerifyFlightSearch()
        {
            streamWriter.WriteLine("2. Clik on language selector");
            IWebElement chooseLang = chromeDriver.FindElementByCssSelector("#language > div.ui-select-match.ng-scope > span");
            chooseLang.Click();

            var wait = new WebDriverWait(chromeDriver, new TimeSpan(0, 0, 30));

            IWebElement li = chromeDriver.FindElementByXPath("//ul//li[@id='ui-select-choices-1']");

            streamWriter.WriteLine("3. Validate that the dropdown has English and Dutch");
            foreach (string lan in Data.langList)
            {
                Assert.AreEqual(true, li.Text.Contains(lan));
                streamWriter.WriteLine("Contains language " + lan);
            }

            IWebElement langEng = chromeDriver.FindElementByCssSelector("#ui-select-choices-row-1-0 > a > div");
            langEng.Click();

            streamWriter.WriteLine("4. Fill signup details");
            IWebElement nameText = chromeDriver.FindElementById("name");
            nameText.SendKeys("Tester");

            IWebElement orgText = chromeDriver.FindElementById("orgName");
            orgText.SendKeys("Tester");

            IWebElement emailText = chromeDriver.FindElementById("singUpEmail");
            emailText.SendKeys("seltest68@gmail.com");

            IWebElement agreeCheckbox = chromeDriver.FindElementByCssSelector("#content > div > div.main-body > div > section > div.form-container > form > fieldset > div:nth-child(4) > label > span");
            agreeCheckbox.Click();

            streamWriter.WriteLine("5. Click on 'I agree to the Terms And Conditions' and signup");
            IWebElement submitBtn = chromeDriver.FindElementByCssSelector("#content > div > div.main-body > div > section > div.form-container > form > fieldset > div:nth-child(5) > button");
            submitBtn.Click();

            chromeDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(60);

            streamWriter.WriteLine("6.Validate that you received an email");
            bool foundMail = CheckEmail();

            Assert.AreEqual(true, foundMail);
            streamWriter.WriteLine("received an email");



        }

        private bool CheckEmail()
        {
            bool foundMail = false;

            Rebex.Licensing.Key = "==AVa53dcdHrfok0qsRgOz82wX3sK1AIJ5gMx2oQA7AQYc==";


            client.AbortTimeout = 300000;

            client.Connect("imap.gmail.com", 993, SslMode.Implicit);
            var auth = client.GetSupportedAuthenticationMethods();
            client.Login(Data.emailLogin, Data.emailPswd, ImapAuthentication.Plain);

            ImapFolderCollection allFolders = client.GetFolderList("", ImapFolderListMode.All, true);
            client.SelectFolder("Inbox", true);

            var list = client.GetMessageList();

            ImapMessageCollection noReply = client.Search
            (
                    ImapSearchParameter.Body("Thanks for signing up for JabaTalks"),
                    ImapSearchParameter.Arrived(DateTime.Now.AddDays(-1), DateTime.Now.Date)
            );

            if (noReply.Count >= 1)
                foundMail = true;
            return foundMail;
        }
    }
}

﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using automatic_web_testing.Helper.Predefined;
using automatic_web_testing.Helper.Setup;
using automatic_web_testing.Helper.Tests;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using screen_recorder;

namespace automatic_web_testing.AutomaticTests.ZakladniFunkce
{
    public class ZalozeniRole
    {
        private ChromeDriver driver { get; set; }
        private ScreenRecorder Recorder { get; set; }
        private WebDriverWait wait { get; set; }

        [SetUp]
        public void Setup()
        {
            WebDriverWait _wait;
            driver = ChromeDriverSetup.Setup("https://dv1.aspehub.cz/Account", out _wait);
            Recorder = new ScreenRecorder();

            wait = _wait;
        }

        [TestCase(TestName = "Kontrola role + smazání",
            Description =
                "Kontroluje jestli role kterou chceme založit existuje pokud existuje tak ji smaže, potom ji založí a pokud neexistuje založí ji")]
        public void KontrolaRole()
        {
            Recorder.StartRecording(TestContext.CurrentContext.Test.Name, "Vytvoření role", "", "AspeHub");
            driver = LoginHelper.Login(driver, wait);
            Assert.NotNull(driver);

            var roleStatus = RoleHelper.VytvoreniRole(driver, wait, out var indexOfRole, RoleFunkce.Vytvoreni);
            Assert.True(roleStatus);
        }

        [TestCase(TestName = "Úprava role",
            Description = "Upraví roli = přidá do popisku další text. Také pokud role není založí ji")]
        public void UpravaRole()
        {
            Recorder.StartRecording(TestContext.CurrentContext.Test.Name, "Úprava role", "", "AspeHub");
            driver = LoginHelper.Login(driver, wait);
            Assert.NotNull(driver);

            var roleStatus = RoleHelper.VytvoreniRole(driver, wait, out var indexOfRole, RoleFunkce.Editace);
            Assert.True(roleStatus);
        }

        [TestCase(TestName = "Aktivita na roli",
            Description = "Aktivita na roli = Podívá se na aktivitu na roli a porovná ji jestli je vše v pořádku.")]
        public void AktivitaNaRoli()
        {
            Recorder.StartRecording(TestContext.CurrentContext.Test.Name, "Aktivita v roli", "", "AspeHub");
            driver = LoginHelper.Login(driver, wait);
            Assert.NotNull(driver);

            var roleStatus = RoleHelper.VytvoreniRole(driver, wait, out var indexOfRole, RoleFunkce.Vytvoreni);
            Assert.True(roleStatus);
            //Tady je potřeba sleep. Tlačítko je klikatelný ale selenium na něj spadne i tak. pokud tam chvíli počká tak to jede v pohodě. 
            Thread.Sleep(2000);

            var isCreated = false;
            var kontrolaRole = SearchHelper.WaitForElementsByClassName("RoleInfo_roleName__3xBnw", driver, 5, 250)
                .FirstOrDefault(e => e.Text == "Autonom");

            var role = SearchHelper.WaitForElementsByClassName("RoleInfo_roleName__3xBnw", driver, 5, 250);

            for (var i = 0; i < role.Count; i++)
                if (role[i].Text == "Autonom")
                    isCreated = true;

            //TODO: Předělat funkci
            RoleHelper.Uprava(driver, indexOfRole, isCreated, kontrolaRole);

            TestContext.WriteLine(isCreated);

            //TODO: If (kontrolaRole == null) Assert.Fail("");
            if (isCreated)
            {
                driver.Navigate().Refresh();

                Assert.NotNull(kontrolaRole);
                kontrolaRole?.Click();

                var kontrolaAktivita = SearchHelper.WaitForElementByClassName("anticon-history", driver, 10, 250);
                Assert.NotNull(kontrolaAktivita);


                var aktivita = driver.FindElements(By.ClassName("anticon-history")).ElementAt(3);
                Assert.NotNull(aktivita);
                // Tady je potřeba taky.
                Thread.Sleep(200);
                aktivita.Click();

                var itemCollection = SearchHelper.WaitForElementsByClassName("ant-collapse", driver, 5, 250);
                var zmenaKlik = itemCollection.ElementAt(0).FindElements(By.TagName("div")).ElementAt(0);

                Assert.NotNull(zmenaKlik);
                zmenaKlik.Click();

                IList<IWebElement> historieAktivit = driver.FindElements(By.TagName("tbody"));
                Assert.NotNull(historieAktivit);

                var popisZmeny = historieAktivit[0].FindElements(By.TagName("td"));
                var autorZmeny = historieAktivit[1].FindElements(By.TagName("td"));
                //To stejné.
                Thread.Sleep(500);

                var task = Task.Run(() =>
                {
                    Assert.AreEqual("Popis", popisZmeny[0].Text, "Špatný parametr | Parametr se nenašel");
                    Assert.AreEqual("Jsem automatický test který zapisuje do popisku!", popisZmeny[1].Text,
                        "Špatný parametr | Parametr se nenašel");
                    Assert.AreEqual("Jsem automatický test který zapisuje do popisku! Hele dopsal jsem ti to :)",
                        popisZmeny[2].Text, "Špatný parametr | Parametr se nenašel");
                });
                Assert.AreEqual("Email", autorZmeny[0].Text, "Špatný parametr | Parametr se nenašel");
                Assert.AreEqual(UserHelper.CorrectUser.Email, autorZmeny[1].Text,
                    "Špatný parametr | Parametr se nenašel");

                task.Wait();
            }

            TestContext.WriteLine("Kontrola hodnot proběhla úspěšně");
        }


        [TearDown]
        public void TearDown()
        {
            Recorder.StopRecording();
            ChromeDriverSetup.Dispose(driver);
        }
    }
}
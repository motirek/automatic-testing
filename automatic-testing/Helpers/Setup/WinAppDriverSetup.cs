﻿using automatic_testing.Helpers.Tests;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Appium;
using System;
using System.Diagnostics;
using System.Linq;

namespace automatic_testing.Helpers.Setup
{
    public class WinAppDriverSetup
    {
        private string DriverUrl { get; set; }
        private AppiumOptions AppiumOptionsRoot { get; set; }
        private AppiumOptions AppiumOptionsEsticon { get; set; }
        private Process WinAppDriver { get; set; }


        public WinAppDriverSetup(string driverUrl = "http://127.0.0.1:4723/")
        {
            DriverUrl = driverUrl;
            WinAppDriver = new Process();
        }

        /// <summary>
        /// Create session for already running app
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="windowName"></param>
        /// <returns>Session for already running app</returns>
        /// <example>CreateSessionForAlreadyRunningApp("calc");</example>
        //! Prozatím je zbytečný | Někdy se třeba bude používat
        public WindowsDriver<WindowsElement> ConnectToRunningProcess(WindowsDriver<WindowsElement> driver, string windowName)
        {
            var window = (WindowsElement)SearchHelper.WaitForElementByName(windowName, driver, 20, 100);
            // Attaching to existing Application Window
            var topLevelWindowHandle = window.GetAttribute("NativeWindowHandle");
            topLevelWindowHandle = int.Parse(topLevelWindowHandle).ToString("X");

            AppiumOptionsEsticon = new AppiumOptions();
            AppiumOptionsEsticon.AddAdditionalCapability("deviceName", "WindowsPC");
            AppiumOptionsEsticon.AddAdditionalCapability("appTopLevelWindow", topLevelWindowHandle);
            return new WindowsDriver<WindowsElement>(new Uri(DriverUrl), AppiumOptionsEsticon);
        }
        /// <summary>
        /// Zapnutí session pro vybírání oken
        /// </summary>
        /// <returns>Vrací root session se všemi otevřenými okny</returns>
        public WindowsDriver<WindowsElement> GetRootSession()
        {
            AppiumOptionsRoot = new AppiumOptions();
            AppiumOptionsRoot.AddAdditionalCapability("app", "Root");
            AppiumOptionsRoot.AddAdditionalCapability("deviceName", "WindowsPC");
            return new WindowsDriver<WindowsElement>(new Uri(DriverUrl), AppiumOptionsRoot);
        }

        /// <summary>
        /// Zapnutí session s AspeEsticonem
        /// </summary>
        /// <returns>Vrátí session, ve které je AspeEsticon</returns>
        public WindowsDriver<WindowsElement> GetEsticonSession()
        {
            AppiumOptionsEsticon = new AppiumOptions();
            AppiumOptionsEsticon.AddAdditionalCapability("app", @"C:\Program Files\IBR\AspeEsticon\Client\EstiCon.Client.exe");
            AppiumOptionsEsticon.AddAdditionalCapability("deviceName", "WindowsPC");
            return new WindowsDriver<WindowsElement>(new Uri(DriverUrl), AppiumOptionsEsticon);
        }

        public void WinAppDriverProcessStart()
        {
            var winAppInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = $"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}\\Windows Application Driver\\WinAppDriver.exe",
                WindowStyle = ProcessWindowStyle.Hidden
            };

            WinAppDriver = Process.Start(winAppInfo);
        }

        public void WinAppDriverProcessClose()
        {
            WinAppDriver.Kill();
            WinAppDriver.Close();
            WinAppDriver.Dispose();

            var prcs = Process.GetProcessesByName("WinAppDriver");
            if (!prcs.Any()) return;
            foreach (var p in prcs)
            {
                p.Kill();
            }
        }
        /// <summary>
        /// Pustit na konci každého testu, aby zavřel všechny instance AspeEsticon
        /// </summary>
        public void KillEveryInstance()
        {
            var prcs = Process.GetProcessesByName("EstiCon.Client");
            if (!prcs.Any()) return;
            foreach (var p in prcs)
            {
                p.Kill();
            }
        }

    }
}

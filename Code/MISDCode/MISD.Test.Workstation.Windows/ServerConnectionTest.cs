using MISD.Workstation.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MISD.Core;
using MISD.Workstation.Windows.WSWebService;
using System.Collections.Generic;

namespace MISD.Test.Workstation.Windows
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "ServerConnectionTest" und soll
    ///alle ServerConnectionTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class ServerConnectionTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Ruft den Testkontext auf, der Informationen
        ///über und Funktionalität für den aktuellen Testlauf bietet, oder legt diesen fest.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Zusätzliche Testattribute
        // 
        //Sie können beim Verfassen Ihrer Tests die folgenden zusätzlichen Attribute verwenden:
        //
        //Mit ClassInitialize führen Sie Code aus, bevor Sie den ersten Test in der Klasse ausführen.
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Mit ClassCleanup führen Sie Code aus, nachdem alle Tests in einer Klasse ausgeführt wurden.
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Mit TestInitialize können Sie vor jedem einzelnen Test Code ausführen.
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Mit TestCleanup können Sie nach jedem einzelnen Test Code ausführen.
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///Ein Test für "ServerConnection-Konstruktor"
        ///</summary>
        [TestMethod()]
        public void ServerConnectionConstructorTest()
        {
            ServerConnection target = new ServerConnection();
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///Ein Test für "DownloadPlugins"
        ///</summary>
        [TestMethod()]
        public void DownloadPluginsTest()
        {
            string workstationDomainName = ServerConnection.GetWorkstationName();
            string[] pluginNames = null;
            PluginFile[] actual;
            // keine plugins downloaden
            actual = ServerConnection.DownloadPlugins(workstationDomainName, pluginNames);
            Assert.IsNull(actual);

            // alle downloaden
            List<string> pluginsToDownload = new List<string>();
            foreach (PluginMetadata me in ServerConnection.GetPluginList(workstationDomainName))
            {
                pluginsToDownload.Add(me.Name);
            }
            actual = ServerConnection.DownloadPlugins(workstationDomainName, pluginsToDownload.ToArray());
            if (actual != null)
            {
                foreach (PluginFile p in actual)
                {
                    Assert.IsNotNull(p);
                }
                Assert.AreEqual(actual.Length, pluginsToDownload.Count);
            }
        }

        /// <summary>
        ///Ein Test für "GetFilters"
        ///</summary>
        [TestMethod()]
        public void GetFiltersTest()
        {
            string workstationDomainName = ServerConnection.GetWorkstationName();
            string pluginName = string.Empty;
            Tuple<string, string>[] oldFilters = null;
            Tuple<string, string>[] actual;
            actual = ServerConnection.GetFilters(workstationDomainName, pluginName, oldFilters);
            if (actual != null)
            {
                foreach (Tuple<string, string> filter in actual)
                {
                    Assert.IsNotNull(filter.Item1);
                    Assert.IsNotNull(filter.Item2);
                }
            }
        }

        /// <summary>
        ///Ein Test für "GetMainUpdateInterval"
        ///</summary>
        [TestMethod()]
        public void GetMainUpdateIntervalTest()
        {
            TimeSpan oldMainUpdateIntervall = new TimeSpan(0);
            TimeSpan actual;
            actual = ServerConnection.GetMainUpdateInterval(oldMainUpdateIntervall);
            if (actual != oldMainUpdateIntervall)
            {
                Assert.IsNotNull(actual);
                Assert.IsTrue(actual.Ticks > 0);
            }
        }

        /// <summary>
        ///Ein Test für "GetPluginList"
        ///</summary>
        [TestMethod()]
        public void GetPluginListTest()
        {
            string workstationDomainName = ServerConnection.GetWorkstationName();
            PluginMetadata[] actual;
            actual = ServerConnection.GetPluginList(workstationDomainName);
            foreach (PluginMetadata p in actual)
            {
                Assert.IsNotNull(p.Company);
                Assert.IsNotNull(p.Copyright);
                Assert.IsNotNull(p.Description);
                Assert.IsNotNull(p.FileName);
                Assert.IsNotNull(p.Indicators);
                Assert.IsTrue(p.Indicators.Count > 0);
                Assert.IsNotNull(p.Name);
                Assert.IsNotNull(p.Product);
                Assert.IsNotNull(p.Version);
            }
        }

        /// <summary>
        ///Ein Test für "GetUpdateIntervals"
        ///</summary>
        [TestMethod()]
        public void GetUpdateIntervalsTest()
        {
            string workstationDomainName = ServerConnection.GetWorkstationName();
            string pluginName = "CPU";
            Tuple<string, Nullable<long>>[] oldUpdateIntervals = null;
            Tuple<string, Nullable<long>>[] actual;
            actual = ServerConnection.GetUpdateIntervals(workstationDomainName, pluginName, oldUpdateIntervals);
            if (actual != null)
            {
                foreach (Tuple<string, long?> t in actual)
                {
                    Assert.IsNotNull(t.Item1);
                    Assert.IsTrue(t.Item2 > 0);
                }
            }
        }

        /// <summary>
        ///Ein Test für "GetWorkstationName"
        ///</summary>
        [TestMethod()]
        public void GetWorkstationNameTest()
        {
            string actual;
            actual = ServerConnection.GetWorkstationName();
            Assert.IsTrue(actual.Length > 0);
        }

        /// <summary>
        ///Ein Test für "GetWorkstationWebService"
        ///</summary>
        [TestMethod()]
        public void GetWorkstationWebServiceTest()
        {
            try
            {
                WorkstationWebServiceClient actual;
                actual = ServerConnection.GetWorkstationWebService();
                Assert.IsNotNull(actual.Endpoint);
            }
            catch (Exception)
            {
                Assert.Inconclusive("Couldn't get webservice connection");
            }

        }

        /// <summary>
        ///Ein Test für "RestoreConnection"
        ///</summary>
        [TestMethod()]
        public void RestoreConnectionTest()
        {
            try
            {
                WorkstationWebServiceClient actual;
                ServerConnection.RestoreConnection();
                actual = ServerConnection.GetWorkstationWebService();
                Assert.IsNotNull(actual);
            }
            catch (Exception)
            {
                Assert.Inconclusive("Couldn't get webservice connection");
            }
        }
    }
}

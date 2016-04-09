using MISD.Workstation.Windows.Plugins;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MISD.Core;
using System.Collections.Generic;

namespace MISD.Test.Workstation.Windows
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "PluginManagerTest" und soll
    ///alle PluginManagerTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class PluginManagerTest
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
        ///Ein Test für "GetLoadedPlugins"
        ///</summary>
        [TestMethod()]
        public void GetLoadedPluginsTest()
        {
            List<IPlugin> loadedPlugins = PluginManager.Instance.GetLoadedPlugins();
            Assert.IsNotNull(loadedPlugins, "PluginManagerTest:GetLoadedPlugins() #01");

            if (loadedPlugins.Count > 0)
            {
                foreach (IPlugin plugin in loadedPlugins)
                {
                    Assert.IsNotNull(plugin, "PluginManagerTest:GetLoadedPlugins() #02");

                    Assert.IsNotNull(plugin.GetName(), "PluginManagerTest:GetLoadedPlugins() #03");
                    Assert.AreNotEqual("", plugin.GetName(), "PluginManagerTest:GetLoadedPlugins() #04");

                    Assert.IsNotNull(plugin.GetDescription(), "PluginManagerTest:GetLoadedPlugins() #05");
                    Assert.AreNotEqual("", plugin.GetDescription(), "PluginManagerTest:GetLoadedPlugins() #06");

                    Assert.IsNotNull(plugin.GetCompany(), "PluginManagerTest:GetLoadedPlugins() #07");
                    Assert.AreNotEqual("", plugin.GetCompany(), "PluginManagerTest:GetLoadedPlugins() #08");

                    Assert.IsNotNull(plugin.GetProduct(), "PluginManagerTest:GetLoadedPlugins() #09");
                    Assert.AreNotEqual("", plugin.GetProduct(), "PluginManagerTest:GetLoadedPlugins() #10");

                    Assert.IsNotNull(plugin.GetCopyright(), "PluginManagerTest:GetLoadedPlugins() #11");
                    Assert.AreNotEqual("", plugin.GetCopyright(), "PluginManagerTest:GetLoadedPlugins() #12");

                    Assert.IsNotNull(plugin.GetVersion(), "PluginManagerTest:GetLoadedPlugins() #13");
                    Assert.AreNotEqual("", plugin.GetVersion(), "PluginManagerTest:GetLoadedPlugins() #14");
                }
            }

        }

        /// <summary>
        ///Ein Test für "LoadAvailablePlugins"
        ///</summary>
        [TestMethod()]
        public void LoadAvailablePluginsTest()
        {
            List<IPlugin> loadedPlugins = PluginManager.Instance.LoadAvailablePlugins();
            Assert.IsNotNull(loadedPlugins, "PluginManagerTest:LoadAvailablePlugins() #01");
            Assert.AreEqual(PluginManager.Instance.GetLoadedPlugins().Count, loadedPlugins.Count, "PluginManagerTest:LoadAvailablePlugins() #02");

            if (loadedPlugins.Count > 0)
            {
                foreach (IPlugin plugin in loadedPlugins)
                {
                    Assert.IsNotNull(plugin, "PluginManagerTest:LoadAvailablePlugins() #03");

                    Assert.IsNotNull(plugin.GetName(), "PluginManagerTest:LoadAvailablePlugins() #04");
                    Assert.AreNotEqual("", plugin.GetName(), "PluginManagerTest:LoadAvailablePlugins() #05");

                    Assert.IsNotNull(plugin.GetDescription(), "PluginManagerTest:LoadAvailablePlugins() #06");
                    Assert.AreNotEqual("", plugin.GetDescription(), "PluginManagerTest:LoadAvailablePlugins() #07");

                    Assert.IsNotNull(plugin.GetCompany(), "PluginManagerTest:LoadAvailablePlugins() #08");
                    Assert.AreNotEqual("", plugin.GetCompany(), "PluginManagerTest:LoadAvailablePlugins() #09");

                    Assert.IsNotNull(plugin.GetProduct(), "PluginManagerTest:LoadAvailablePlugins() #10");
                    Assert.AreNotEqual("", plugin.GetProduct(), "PluginManagerTest:LoadAvailablePlugins() #11");

                    Assert.IsNotNull(plugin.GetCopyright(), "PluginManagerTest:LoadAvailablePlugins() #12");
                    Assert.AreNotEqual("", plugin.GetCopyright(), "PluginManagerTest:LoadAvailablePlugins() #13");

                    Assert.IsNotNull(plugin.GetVersion(), "PluginManagerTest:LoadAvailablePlugins() #14");
                    Assert.AreNotEqual("", plugin.GetVersion(), "PluginManagerTest:LoadAvailablePlugins() #15");
                }
            }

        }

        /// <summary>
        ///Ein Test für "UpdatePlugins"
        ///</summary>
        [TestMethod()]
        public void UpdatePluginsTest()
        {
            List<IPlugin> updatedPlugins = PluginManager.Instance.UpdatePlugins();
            Assert.IsNotNull(updatedPlugins, "PluginManagerTest:UpdatePlugins() #01");

            Assert.AreEqual(PluginManager.Instance.GetLoadedPlugins().Count, updatedPlugins.Count, "PluginManagerTest:UpdatePlugins() #02");

        }

        /// <summary>
        ///Ein Test für "Instance"
        ///</summary>
        [TestMethod()]
        public void InstanceTest()
        {
            Assert.IsNotNull(PluginManager.Instance, "PluginManagerTest:TestGetInstance() #01");
        }
    }
}

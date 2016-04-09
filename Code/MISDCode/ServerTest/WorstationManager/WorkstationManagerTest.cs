using MISD.Server.Manager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using MISD.Core;
using System.Text;
using MISD.Server;

namespace ServerTest
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "WorkstationManagerTest" und soll
    ///alle WorkstationManagerTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class WorkstationManagerTest
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
        /// Initialisierung von Tests
        ///</summary>
        [TestInitialize()]
        [DeploymentItem("MISD.Server.exe")]
        public void MyTestInitialize()
        {
            try
            {
                MISD.Server.Database.Maintenance onMaintenance = new MISD.Server.Database.Maintenance();
                onMaintenance.Beginning = DateTime.Now;
                onMaintenance.End = null;
                MISD.Server.Database.Maintenance offMaintenance = new MISD.Server.Database.Maintenance();
              //  offMaintenance.Beginning = null;
                offMaintenance.End = DateTime.Now;
                MISD.Server.Database.MonitoredSystem testMS_On = new MISD.Server.Database.MonitoredSystem();
                testMS_On.FQDN = "Test.MS_ON";
                testMS_On.Name = "MS_Test_On";
                testMS_On.IsIgnored = true;
               // testMS_On.Maintenance = onMaintenance; 

                MISD.Server.Database.MonitoredSystem testMS_Off = new MISD.Server.Database.MonitoredSystem();

            }
            catch (Exception e){
                //logging exception
                var messageEx = new StringBuilder();
                messageEx.Append("Initializing of Testdata failed."+e.ToString());
                MISD.Server.Manager.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);
            }

        }

        /// <summary>
        ///Aufräumen nach Tests
        ///</summary>
        [TestCleanup()]
        [DeploymentItem("MISD.Server.exe")]
        public void MyTestCleanup()
        {
            try
            {
                MISD.Server.Database.MonitoredSystem testMS_On = new MISD.Server.Database.MonitoredSystem();
                MISD.Server.Database.MonitoredSystem testMS_Off = new MISD.Server.Database.MonitoredSystem();

            }
            catch (Exception e)
            {
                //logging exception
                var messageEx = new StringBuilder();
                messageEx.Append("Initializing of Testdata failed."+e.ToString());
                MISD.Server.Manager.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);
            }

        }

        /// <summary>
        ///Ein Test für "WorkstationManager-Konstruktor"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Server.exe")]
        public void WorkstationManagerConstructorTest()
        {
            WorkstationManager_Accessor target = new WorkstationManager_Accessor();
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///Ein Test für "ActivateMaintenanceMode"
        ///</summary>
        [TestMethod()]
        public void ActivateMaintenanceModeTest()
        {
            MISD.Server.Database.MonitoredSystem testMS = new MISD.Server.Database.MonitoredSystem();

            WorkstationManager_Accessor target = new WorkstationManager_Accessor(); // TODO: Passenden Wert initialisieren
            List<string> workstationDomainNames = new List<string>();
            workstationDomainNames.Add("Test.MS_ON"); 
            List<string> expected = null; // TODO: Passenden Wert initialisieren
            List<string> actual;
            actual = target.ActivateMaintenanceMode(workstationDomainNames);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }

        /// <summary>
        ///Ein Test für "AddWorkstationsToIgnoreList"
        ///</summary>
        [TestMethod()]
        public void AddWorkstationsToIgnoreListTest()
        {
            WorkstationManager_Accessor target = new WorkstationManager_Accessor(); // TODO: Passenden Wert initialisieren
            List<string> workstationDomainNames = null; // TODO: Passenden Wert initialisieren
            List<string> expected = null; // TODO: Passenden Wert initialisieren
            List<string> actual;
            actual = target.AddWorkstationsToIgnoreList(workstationDomainNames);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }

        /// <summary>
        ///Ein Test für "DeactivateMaintenanceMode"
        ///</summary>
        [TestMethod()]
        public void DeactivateMaintenanceModeTest()
        {
            WorkstationManager_Accessor target = new WorkstationManager_Accessor(); // TODO: Passenden Wert initialisieren
            List<string> workstationDomainNames = null; // TODO: Passenden Wert initialisieren
            List<string> expected = null; // TODO: Passenden Wert initialisieren
            List<string> actual;
            actual = target.DeactivateMaintenanceMode(workstationDomainNames);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }

        /// <summary>
        ///Ein Test für "GetIgnoredWorkstationNames"
        ///</summary>
        [TestMethod()]
        public void GetIgnoredWorkstationNamesTest()
        {
            WorkstationManager_Accessor target = new WorkstationManager_Accessor(); // TODO: Passenden Wert initialisieren
            List<string> expected = null; // TODO: Passenden Wert initialisieren
            List<string> actual;
            actual = target.GetIgnoredWorkstationNames();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }

        /// <summary>
        ///Ein Test für "GetIndicatorSetting"
        ///</summary>
        [TestMethod()]
        public void GetIndicatorSettingTest()
        {
            WorkstationManager_Accessor target = new WorkstationManager_Accessor(); // TODO: Passenden Wert initialisieren
            string workstationDomainName = string.Empty; // TODO: Passenden Wert initialisieren
            List<IndicatorSettings> expected = null; // TODO: Passenden Wert initialisieren
            List<IndicatorSettings> actual;
            actual = target.GetIndicatorSetting(workstationDomainName);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }

        /// <summary>
        ///Ein Test für "RemoveWorkstationsFromIgnoreList"
        ///</summary>
        [TestMethod()]
        public void RemoveWorkstationsFromIgnoreListTest()
        {
            WorkstationManager_Accessor target = new WorkstationManager_Accessor(); // TODO: Passenden Wert initialisieren
            List<string> workstationDomainNames = null; // TODO: Passenden Wert initialisieren
            List<string> expected = null; // TODO: Passenden Wert initialisieren
            List<string> actual;
            actual = target.RemoveWorkstationsFromIgnoreList(workstationDomainNames);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }

        /// <summary>
        ///Ein Test für "SetIndicatorSetting"
        ///</summary>
        [TestMethod()]
        public void SetIndicatorSettingTest()
        {
            WorkstationManager_Accessor target = new WorkstationManager_Accessor(); // TODO: Passenden Wert initialisieren
            List<IndicatorSettings> settings = null; // TODO: Passenden Wert initialisieren
            bool expected = false; // TODO: Passenden Wert initialisieren
            bool actual;
            actual = target.SetIndicatorSetting(settings);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }

        /// <summary>
        ///Ein Test für "SignIn"
        ///</summary>
        [TestMethod()]
        public void SignInTest()
        {
            WorkstationManager_Accessor target = new WorkstationManager_Accessor(); // TODO: Passenden Wert initialisieren
            string workstationName = string.Empty; // TODO: Passenden Wert initialisieren
            byte operatingSystem = 0; // TODO: Passenden Wert initialisieren
            bool expected = false; // TODO: Passenden Wert initialisieren
            bool actual;
            actual = target.SignIn(workstationName, operatingSystem);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }

        /// <summary>
        ///Ein Test für "SignOut"
        ///</summary>
        [TestMethod()]
        public void SignOutTest()
        {
            WorkstationManager_Accessor target = new WorkstationManager_Accessor(); // TODO: Passenden Wert initialisieren
            string workstationName = string.Empty; // TODO: Passenden Wert initialisieren
            bool expected = false; // TODO: Passenden Wert initialisieren
            bool actual;
            actual = target.SignOut(workstationName);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }

        /// <summary>
        ///Ein Test für "UploadIndicatorValues"
        ///</summary>
        [TestMethod()]
        public void UploadIndicatorValuesTest()
        {
            WorkstationManager_Accessor target = new WorkstationManager_Accessor(); // TODO: Passenden Wert initialisieren
            string workstationDomainName = string.Empty; // TODO: Passenden Wert initialisieren
            string pluginName = string.Empty; // TODO: Passenden Wert initialisieren
            List<Tuple<string, object, DataType, DateTime>> indicatorValues = null; // TODO: Passenden Wert initialisieren
            bool expected = false; // TODO: Passenden Wert initialisieren
            bool actual;
            actual = target.UploadIndicatorValues(workstationDomainName, pluginName, indicatorValues);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }

        /// <summary>
        ///Ein Test für "Instance"
        ///</summary>
        [TestMethod()]
        public void InstanceTest()
        {
            WorkstationManager actual;
            actual = WorkstationManager.Instance;
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }
    }
}

using MISD.Server.Manager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MISD.Core.States;
using System.Collections.Generic;

namespace ServerTest
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "MetricManagerTest" und soll
    ///alle MetricManagerTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class MetricManagerTest
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
        ///Ein Test für "GetMetric"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Server.exe")]
        public void GetMetricTest()
        {
            //Test1
            MetricManager_Accessor target = new MetricManager_Accessor();
            WorkstationManager wsManager = WorkstationManager.Instance;

            //create warning value
            List<Tuple<string,object,MISD.Core.DataType,DateTime>> values = new List<Tuple<string,object,MISD.Core.DataType,DateTime>>();
            values.Add(new Tuple<string,object,MISD.Core.DataType,DateTime>("Load", 95, MISD.Core.DataType.Byte, DateTime.Now);
            wsManager.UploadIndicatorValues(
                "NUnit_TestClient", "RAM", 
                values);

            int monitoredSystem = 85;
            string pluginName = "RAM";
            string indicator = "Load";

            string statementWarning;
            string statementCritical;

            string statementWarningExpected = "^9[1-8]$";
            string statementCriticalExpected = "^(99|100)$";

            target.GetMetric(monitoredSystem, pluginName, indicator, out statementWarning, out statementCritical);
            Assert.AreEqual(statementWarningExpected, statementWarning);
            Assert.AreEqual(statementCriticalExpected, statementCritical);
        }

        /// <summary>
        ///Ein Test für "GetMetricValue"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Server.exe")]
        public void GetMetricValueTest()
        {
            #region Test 1 Warining

            MetricManager_Accessor target = new MetricManager_Accessor();

            int monitoredSystem = 85;
            string pluginName = "RAM";
            string indicator = "Load";
            string value = "95";
            MappingState expected = MISD.Core.States.MappingState.Warning;

            MappingState actual;
            actual = target.GetMetricValue(monitoredSystem, pluginName, indicator, value);
            Assert.AreEqual(expected, actual);

            #endregion

            #region Test 2 Critical

            target = new MetricManager_Accessor();

            monitoredSystem = 85;
            pluginName = "RAM";
            indicator = "Load";
            value = "100";
            expected = MISD.Core.States.MappingState.Critical;

            actual = target.GetMetricValue(monitoredSystem, pluginName, indicator, value);
            Assert.AreEqual(expected, actual);

            #endregion

            #region Test 3 OK
            target = new MetricManager_Accessor();

            monitoredSystem = 85;
            pluginName = "RAM";
            indicator = "Load";
            value = "89";
            expected = MISD.Core.States.MappingState.OK;

            actual = target.GetMetricValue(monitoredSystem, pluginName, indicator, value);
            Assert.AreEqual(expected, actual);

            #endregion

            #region Test 4 OK
            target = new MetricManager_Accessor();

            monitoredSystem = 85;
            pluginName = "RAM";
            indicator = "Load";
            value = "1";
            expected = MISD.Core.States.MappingState.OK;

            actual = target.GetMetricValue(monitoredSystem, pluginName, indicator, value);
            Assert.AreEqual(expected, actual);

            #endregion
        }

        /// <summary>
        ///Ein Test für "SetMetric"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Server.exe")]
        public void SetMetricTest()
        {
            MetricManager_Accessor target = new MetricManager_Accessor(); // TODO: Passenden Wert initialisieren
            int monitoredSystem = 0; // TODO: Passenden Wert initialisieren
            string pluginName = string.Empty; // TODO: Passenden Wert initialisieren
            string indicator = string.Empty; // TODO: Passenden Wert initialisieren
            string valueWarn = string.Empty; // TODO: Passenden Wert initialisieren
            string valueCrit = string.Empty; // TODO: Passenden Wert initialisieren
            target.SetMetric(monitoredSystem, pluginName, indicator, valueWarn, valueCrit);
            Assert.Inconclusive("Eine Methode, die keinen Wert zurückgibt, kann nicht überprüft werden.");
        }
    }
}

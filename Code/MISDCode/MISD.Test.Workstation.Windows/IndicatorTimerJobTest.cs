using MISD.Workstation.Windows.Scheduling.TimerJobs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MISD.Core;
using System.Collections.Generic;
using MISD.Workstation.Windows.Plugins;

namespace MISD.Test.Workstation.Windows
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "IndicatorTimerJobTest" und soll
    ///alle IndicatorTimerJobTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class IndicatorTimerJobTest
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

        #region SetUp() and TearDown()
        /// <summary>
        /// Attributes to save the current state of the IndicatorTimerJob.
        /// </summary>
        IndicatorTimerJob myIndicatorJob;
        MISD.Core.IPlugin myIPlugin;
        string myIndicator;
        TimeSpan myInterval;

        /// <summary>
        /// This method is called before every test method.
        /// </summary>
        [TestInitialize()]
        public void SetUp()
        {
            myInterval = new TimeSpan(0, 0, 30);
            myIndicator = "ProcessorName";
            myIPlugin = new MyTestPlugin();
            myIndicatorJob = new IndicatorTimerJob(myIPlugin, myIndicator, myInterval);
        }
        #endregion


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
        ///Ein Test für "Indicator"
        ///</summary>
        [TestMethod()]
        public void IndicatorTest()
        {
            Assert.AreEqual(myIndicator, myIndicatorJob.Indicator, "IndicatorTimerJobTest:TestGetIndicator() #01");

            string newIndicator = "Ram";
            myIndicatorJob.Indicator = newIndicator;
            Assert.AreEqual(newIndicator, myIndicatorJob.Indicator, "IndicatorTimerJobTest:TestGetIndicator() #02");
        }


        public class MyTestPlugin : MISD.Core.IPlugin
        {

            public List<IndicatorSettings> GetIndicatorSettings()
            {
                List<IndicatorSettings> l = new List<IndicatorSettings>();
                l.Add(
                    new IndicatorSettings(
				    "cpu",		    				// Pluginname
				    "ProcessorName",				// Indicatornname
				    "",								// WorkstationDomainName
				    ".",							// FilterStatement
				    new TimeSpan (0, 0, 30),		// UpdateInterval
				    new TimeSpan (365, 0, 0, 0),	// StorageDuration
				    new TimeSpan (24, 0, 0),		// MappingDuration
				    DataType.String,				// DataType
				    "",								// Metric Warning
				    "") 							// Metric Critical
                    );
			
			    l.Add(
                    new IndicatorSettings(
				    "cpu",						
				    "Load",							
				    "",								
				    ".",								
				    new TimeSpan (0, 0, 30),		
				    new TimeSpan (365, 0, 0, 0),	
				    new TimeSpan (24, 0, 0),		
				    DataType.Byte,				
				    "^(9[1-9]|100)$",				
				    ""));


                return l;
            }

            public Platform TargetPlatform
            {
                get
                {
                    return Platform.Windows;
                }
            }

            public List<Tuple<string, object, DataType>> AcquireData()
            {
                List<string> indicatorNames = new List<string>();

                foreach (IndicatorSettings indicator in GetIndicatorSettings())
                {
                    indicatorNames.Add(indicator.IndicatorName);
                }

                return AcquireData(indicatorNames);
            }

            public List<Tuple<string, object, DataType>> AcquireData(List<string> indicatorNames)
            {
                List<Tuple<string, object, DataType>> result = new List<Tuple<string, object, DataType>>();
                return result;
            }

            public List<Tuple<string, Object, DataType>> AcquireData(string monitoredSystemName)
            {
                throw new NotImplementedException("This method is accessible for clusters only.");
            }

            public List<Tuple<string, Object, DataType>> AcquireData(string monitoredSystemName, ClusterConnection clusterConnection)
            {
                throw new NotImplementedException("This method is accessible for clusters only.");
            }

            public List<Tuple<string, Object, DataType>> AcquireData(List<string> indicatorName, string monitoredSystemName)
            {
                throw new NotImplementedException("This method is accessible for clusters only.");
            }

            public List<Tuple<string, Object, DataType>> AcquireData(List<string> indicatorName, string monitoredSystemName, ClusterConnection clusterConnection)
            {
                throw new NotImplementedException("This method is accessible for clusters only.");
            }

        }
    }
}

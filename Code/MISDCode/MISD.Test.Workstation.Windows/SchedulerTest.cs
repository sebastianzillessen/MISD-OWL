using MISD.Workstation.Windows.Scheduling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MISD.Core;

namespace MISD.Test.Workstation.Windows
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "SchedulerTest" und soll
    ///alle SchedulerTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class SchedulerTest
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
        ///Ein Test für "Instance"
        ///</summary>
        [TestMethod()]
        public void InstanceTest()
        {
            Assert.IsNotNull(MISD.Workstation.Windows.Scheduling.Scheduler.Instance, "SchedulerTest:TestGetInstance() #01");
        }
    }
}

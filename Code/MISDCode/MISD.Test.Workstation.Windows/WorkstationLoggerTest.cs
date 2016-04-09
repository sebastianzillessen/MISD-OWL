using MISD.Workstation.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MISD.Test.Workstation.Windows
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "WorkstationLoggerTest" und soll
    ///alle WorkstationLoggerTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class WorkstationLoggerTest
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
        ///Ein Test für "Close"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Workstation.Windows.exe")]
        public void CloseTest()
        {
            WorkstationLogger_Accessor.WriteLog("test log");
            Assert.IsTrue(WorkstationLogger_Accessor.myFile.BaseStream.CanWrite);
            WorkstationLogger_Accessor.Close();
            Assert.IsNull(WorkstationLogger_Accessor.myFile.BaseStream);
        }

        /// <summary>
        ///Ein Test für "WriteLog"
        ///</summary>
        [TestMethod()]
        public void WriteLogTest()
        {
           WorkstationLogger_Accessor.WriteLog("test log");
           Assert.IsTrue(WorkstationLogger_Accessor.myFile.BaseStream.CanWrite);
        }
    }
}

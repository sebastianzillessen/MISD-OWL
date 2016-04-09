using MISD.Server.Manager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace ServerTest
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "ADManagerTest" und soll
    ///alle ADManagerTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class ADManagerTest
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
        ///Ein Test für "ADManager-Konstruktor"
        ///</summary>
        [TestMethod()]
        public void ADManagerConstructorTest()
        {
            ADManager target = new ADManager();
            Assert.Inconclusive("TODO: Code zum Überprüfen des Ziels implementieren");
        }

        /// <summary>
        ///Ein Test für "FindOUinString"
        ///
        /// Ausführung von Rechner und Konto mit AD Zugriff!!
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Server.exe")]
        public void FindOUinStringTest()
        {
            //Test 1
            ADManager_Accessor target = new ADManager_Accessor();
            string distinguishedName = "CN=VISGS15,OU=VisSimLabor,OU=Pool,OU=VIS,OU=Computer,OU=VIS(US),DC=visus,DC=uni-stuttgart,DC=de";
            List<string> expected = new List<string>();
            expected.Add("VisSimLabor.Pool.Computer.VIS(US)");
            expected.Add("Pool.Computer.VIS(US)");
            expected.Add("Computer.VIS(US)");
            expected.Add("VIS(US)");
            expected.Reverse();
            List<string> actual;
            actual = target.FindOUinString(distinguishedName);
            Assert.AreEqual(expected, actual);

            //Test 2
            target = new ADManager_Accessor();
            distinguishedName = "CN=VISGS15,DC=visus,DC=uni-stuttgart,DC=de";
            expected = new List<string>();
            actual = target.FindOUinString(distinguishedName);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///Ein Test für "GetOU"
        ///</summary>
        [TestMethod()]
        public void GetOUTest()
        {
            ADManager target = new ADManager();
            string domainname = "VISGS15";
            List<string> expected = new List<string>();
            expected.Add("VisSimLabor.Pool.Computer.VIS(US)");
            expected.Add("Pool.Computer.VIS(US)");
            expected.Add("Computer.VIS(US)");
            expected.Add("VIS(US)");
            expected.Reverse();
            List<string> actual;
            actual = target.GetOU(domainname);
            Assert.AreEqual(expected, actual);
        }
    }
}

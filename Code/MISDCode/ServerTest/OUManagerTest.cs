using MISD.Server.Manager;
using MISD.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System;

namespace ServerTest
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "OUManagerTest" und soll
    ///alle OUManagerTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class OUManagerTest
    {


        private TestContext testContextInstance;
        private MISD.Server.Database.MISDDataContext db = new MISD.Server.Database.MISDDataContext();

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
        ///Ein Test für "OUManager-Konstruktor"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Server.exe")]
        public void OUManagerConstructorTest()
        {
            OUManager_Accessor target = new OUManager_Accessor();
            string fqdn = "System";
            bool expected = true;
            bool actual;
            actual = target.Exist(fqdn);
            Assert.AreEqual(expected, actual);

            fqdn = "SystemFalse";
            expected = false;
            actual = target.Exist(fqdn);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///Ein Test für "AddOU"
        ///</summary>
        [TestMethod()]
        public void AddOUTest()
        {
            //Test 1
            OUManager_Accessor target = new OUManager_Accessor();
            string name = "MyTestOU";
            int fatherOU = 0; 
            int notExpected = -1;
            int actual;
            actual = target.AddOU(name, fatherOU);
            Assert.AreNotEqual(notExpected, actual);

            //cleanup
            Assert.IsTrue(target.DeleteOU(actual), "Cleanup fails");

            //Test 2
            target = new OUManager_Accessor();
            name = "MyTestOU";
            fatherOU = 8816548; //not in database!
            int expected = -1;
            actual = target.AddOU(name, fatherOU);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///Ein Test für "CreateOU"
        ///</summary>
        [TestMethod()]
        public void CreateOUTest()
        {
            //Test1
            OUManager_Accessor target = new OUManager_Accessor();
            string name = "MyTestOU";
            string parentFQDN = "System";
            try
            {
                target.CreateOU(name, parentFQDN);
                Assert.IsTrue(true);
            }
            catch (Exception e)
            {
                Assert.IsTrue(false);
            }
            
            //cleanup
            int testOUID = (from p in db.OrganizationalUnit
                            where p.Name == name
                            select p.ID).FirstOrDefault();

            Assert.IsTrue(target.DeleteOU(testOUID), "Cleanup fails");

            //Test2
            target = new OUManager_Accessor();
            name = "MyTestOU";
            parentFQDN = "SystemFalse";
            //cleanup
            int cleanOUID = (from p in db.OrganizationalUnit
                             where p.Name == parentFQDN
                            select p.ID).FirstOrDefault();

            Assert.IsFalse(target.DeleteOU(testOUID), "Cleanup for test 2 fails");

            try
            {
                target.CreateOU(name, parentFQDN);
                Assert.IsTrue(false);
            }
            catch (Exception e)
            {
                Assert.IsTrue(true);
            }
        }

        /// <summary>
        ///Ein Test für "AssignToOU"
        ///</summary>
        [TestMethod()]
        public void AssignToOUTest()
        {
            //Test1
            string workstationDomainName = "hestia1.visus.uni-stuttgart.de";
            int newOUID = 0;
            bool expected = true;
            bool actual;
            
            int oldOUID = (from p in db.MonitoredSystem
                           where p.FQDN == workstationDomainName
                           select p.ID).FirstOrDefault();

            OUManager_Accessor target = new OUManager_Accessor();
            actual = target.AssignToOU(workstationDomainName, newOUID);
            Assert.AreEqual(expected, actual);

            //cleanup
            target.AssignToOU(workstationDomainName, oldOUID);
        }

        /// <summary>
        ///Ein Test für "ChangeOUName"
        ///</summary>
        [TestMethod()]
        public void ChangeOUNameTest()
        {
            //Test1
            int ouID = 1;
            string newName = "NewTestName";
            bool expected = true;
            bool actual;

            string oldName = (from p in db.OrganizationalUnit
                              where p.ID == ouID
                              select p.Name).FirstOrDefault();

            OUManager_Accessor target = new OUManager_Accessor();
            actual = target.ChangeOUName(ouID, newName);
            Assert.AreEqual(expected, actual);

            //cleanup
            target.ChangeOUName(ouID, oldName);

            //Test2
            ouID = 1156121561; //not in DB
            newName = "NewTestName";
            expected = false;
            target = new OUManager_Accessor();
            actual = target.ChangeOUName(ouID, newName);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///Ein Test für "DeleteOU"
        ///</summary>
        [TestMethod()]
        public void DeleteOUTest()
        {
            //Test 1
            OUManager_Accessor target = new OUManager_Accessor();
            int ouID = target.AddOU("MyTestOUforDeleteOUTest", 1);
            bool expected = true;
            bool actual;
            actual = target.DeleteOU(ouID);
            Assert.AreEqual(expected, actual);

            //Test 2
            target = new OUManager_Accessor();
            ouID = 17835;
            expected = false;
            actual = target.DeleteOU(ouID);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///Ein Test für "Exist"
        ///</summary>
        [TestMethod()]
        public void ExistTest()
        {
            //Test 1
            OUManager_Accessor target = new OUManager_Accessor();
            string fqdn = "System";
            bool expected = true;
            bool actual;
            actual = target.Exist(fqdn);
            Assert.AreEqual(expected, actual);

            //Test 2
            target = new OUManager_Accessor();
            fqdn = "SystemFalse"; //not in DB
            expected = false;
            actual = target.Exist(fqdn);
            Assert.AreEqual(expected, actual);
        }
    }
}

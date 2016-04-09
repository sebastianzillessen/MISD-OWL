using MISD.Server.Cluster;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MISD.Core;
using System.Collections.Generic;

namespace MISD.Test.Server.Cluster
{


    /// <summary>
    ///Dies ist eine Testklasse für "BrightClusterConnectionTest" und soll
    ///alle BrightClusterConnectionTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class BrightClusterConnectionTest
    {
        private static string head = "hestia1";
        private static string url=head+".visus.uni-stuttgart.de";
        private static string password = "5au-M1sd-!";
        private static string user = "misd-sau";
        private TestContext testContextInstance;
        private BrightClusterConnection target;

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
        [TestInitialize()]
        public void MyTestInitialize()
        {
            target = new BrightClusterConnection();
            target.Init(url, user, password);
        }
        
        #endregion


        /// <summary>
        ///Ein Test für "BrightClusterConnection-Konstruktor"
        ///</summary>
        [TestMethod()]
        public void BrightClusterConnectionConstructorTest()
        {
            BrightClusterConnection target = new BrightClusterConnection();
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(ClusterConnection));
            Assert.IsInstanceOfType(target, typeof(BrightClusterConnection));
            Assert.IsNull(target.Connection());
        }

        [TestMethod()]
        [ExpectedException(typeof(Exception))]
        public void BrightClusterConnectionWithoutInitNodesTest()
        {
            BrightClusterConnection target = new BrightClusterConnection();
            target.Nodes();
        }

        [TestMethod()]
        [ExpectedException(typeof(Exception))]
        public void BrightClusterConnectionWithoutInitGetLatestMetricDataTest()
        {
            BrightClusterConnection target = new BrightClusterConnection();
            target.GetLatestMetricData("bla", "bla");
        }


        [TestMethod()]
        [ExpectedException(typeof(Exception))]
        public void BrightClusterConnectionWithoutInitGetSysInfoTest()
        {
            BrightClusterConnection target = new BrightClusterConnection();
            target.GetSysinfo("bla", "bla");
        }

        /// <summary>
        ///Ein Test für "Connection"
        ///</summary>
        [TestMethod()]
        public void ConnectionTest()
        {
            BrightClusterConnection target = new BrightClusterConnection();
            Assert.IsNull(target.Connection());
            target.Init(url, user,password);
            Assert.IsNotNull(target.Connection());
            Assert.IsInstanceOfType(target.Connection(), typeof(BrightClusterShell));
            BrightClusterShell s = (BrightClusterShell)target.Connection();
            var result = s.RunCommands(new List<string> { "help" });
            Assert.IsTrue(result.Contains("alias"));
            Assert.IsTrue(result.Contains("user"));
        }

        /// <summary>
        ///Ein Test für "CopyConnection"
        ///</summary>
        [TestMethod()]
        public void CopyConnectionTest()
        {
            ClusterConnection c = target.CopyConnection();
            Assert.IsInstanceOfType(c, typeof(ClusterConnection));
            Assert.IsInstanceOfType(c, typeof(BrightClusterConnection));
            Assert.IsInstanceOfType(c.Connection(), typeof(BrightClusterShell));
            Assert.AreNotSame(c.Connection(), target.Connection());
            BrightClusterShell s1 = (BrightClusterShell) target.Connection();
            BrightClusterShell s2 = (BrightClusterShell) c.Connection();
            Assert.AreEqual(s1.password, s2.password);
            Assert.AreEqual(s1.username, s2.username);
            Assert.AreEqual(s1.url, s2.url);
            BrightClusterConnection con = (BrightClusterConnection)c;
            Assert.AreEqual(target.password, con.password);
            Assert.AreEqual(s1.password, target.password);

            Assert.AreEqual(target.username, con.username);
            Assert.AreEqual(s1.username, target.username);

            Assert.AreEqual(target.url, con.url);
            Assert.AreEqual(s1.url, target.url);
        }

        /// <summary>
        ///Ein Test für "GetLatestMetricData"
        ///</summary>
        [TestMethod()]
        public void GetLatestMetricDataTest()
        {
            var res = target.GetLatestMetricData("hestia01", "VBAT");
            Assert.IsTrue(res.Length >= 3);
            Assert.IsTrue(res.Contains(".")); // komma value
            res = target.GetLatestMetricData("hestia01", "AlertLevel:max");
            Assert.IsTrue(res.Length >= 1);
            Assert.IsFalse(res.Contains(".")); // no komma value
            
        }

        /// <summary>
        ///Ein Test für "GetSysinfo"
        ///</summary>
        [TestMethod()]
        public void GetSysinfoTest()
        {
            var res = target.GetSysinfo("hestia01", "BIOS Version ");
            Assert.IsTrue(res.Length >= 3);
            Assert.IsTrue(res.Contains(".")); // komma value
            res = target.GetSysinfo("hestia01", "Interconnect");
            Assert.IsTrue(res.Length >= 3);
            Assert.IsTrue(res.Contains("a")|res.Contains("e")|res.Contains("i")|res.Contains("u")|res.Contains("o")); // Text value
        }

        /// <summary>
        ///Ein Test für "Nodes"
        ///</summary>
        [TestMethod()]
        public void NodesTest()
        {
            List<WorkstationInfo> actual = target.Nodes();
            bool hestia1 = false;
            bool hestia2 = false;
            bool hestia01 = false;
            foreach (WorkstationInfo w in actual)
            {
                hestia1 = hestia1 || w.Name.Equals("hestia1");
                hestia2 = hestia1 || w.Name.Equals("hestia2");
                hestia01 = hestia1 || w.Name.Equals("hestia01");
            }
            Assert.IsTrue(hestia01);
            Assert.IsTrue(hestia1);
            Assert.IsTrue(hestia2);
        }

        /// <summary>
        ///Ein Test für "buildFQDN"
        ///</summary>
        [TestMethod()]
        public void buildFQDNTest()
        {
            Assert.AreEqual(target.buildFQDN(head), url);
            Assert.AreEqual(target.buildFQDN("hestia01"), "hestia01.visus.uni-stuttgart.de");
            Assert.AreEqual(target.buildFQDN("a.b.c"), "a.b.c.visus.uni-stuttgart.de");

        }

        /// <summary>
        ///Ein Test für "getHostname"
        ///</summary>
        [TestMethod()]
        public void getHostnameTest()
        {
            Assert.AreEqual(target.getHostname(url), head);
            Assert.AreEqual(target.getHostname("hestia01.visus.uni-stuttgart.de"), "hestia01");
            Assert.AreEqual(target.getHostname("a.b.c.visus.uni-stuttgart.de"), "a.b.c");
            var x = "<yxcvbnm,.-asdfghjklöä#qwertzuiopü+_:;MNBVCXY>ASDFGHJKLÖÄ'*ÜPOIUZTREWQ°!§$%&/()=?`´ß0987654321";
            // test umkehrfunktion
            Assert.AreEqual(target.getHostname(target.buildFQDN(x)),x);
        }


        /// <summary>
        ///Ein Test für viele Connections
        ///</summary>
        [TestMethod()]
        public void testMultipleConnections()
        {
            List<BrightClusterConnection> list = new List<BrightClusterConnection>();
            for (int i = 0; i < 50; i++)
            {
                list.Add((BrightClusterConnection)target.CopyConnection());
            }
        }
    }
}

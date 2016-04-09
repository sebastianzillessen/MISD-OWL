/*
 * Copyright 2012 Paul Brombosch, Ehssan Doust, David Krauss,
 * Fabian Müller, Yannic Noller, Hanna Schäfer, Jonas Scheurich,
 * Arno Schneider, Sebastian Zillessen
 *
 * This file is part of MISD-OWL, a project of the
 * University of Stuttgart (Institution VISUS, Studienprojekt Spring 2012).
 *
 * MISD-OWL is published under GNU Lesser General Public License Version 3.
 * MISD-OWL is free software, you are allowed to redistribute and/or
 * modify it under the terms of the GNU Lesser General Public License
 * Version 3 or any later version. For details see here:
 * http://www.gnu.org/licenses/lgpl.html
 *
 * MISD-OWL is distributed without any warranty, without even the
 * implied warranty of merchantability or fitness for a particular purpose.
 */

using MISD.Plugins.Windows.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MISD.Core;
using System.Collections.Generic;

namespace MISD.Test.Plugins.Windows
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "StorageTest" und soll
    ///alle StorageTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class StorageTest
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
        ///Ein Test für "Storage-Konstruktor"
        ///</summary>
        [TestMethod()]
        public void StorageConstructorTest()
        {
            Storage target = new Storage();
            Assert.Inconclusive("TODO: Code zum Überprüfen des Ziels implementieren");
        }

        /// <summary>
        ///Ein Test für "AcquireData"
        ///</summary>
        [TestMethod()]
        public void AcquireDataTest()
        {
            Storage target = new Storage(); // TODO: Passenden Wert initialisieren
            string monitoredSystemName = string.Empty; // TODO: Passenden Wert initialisieren
            ClusterConnection clusterConnection = null; // TODO: Passenden Wert initialisieren
            List<Tuple<string, object, DataType>> expected = null; // TODO: Passenden Wert initialisieren
            List<Tuple<string, object, DataType>> actual;
            actual = target.AcquireData(monitoredSystemName, clusterConnection);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }

        /// <summary>
        ///Ein Test für "AcquireData"
        ///</summary>
        [TestMethod()]
        public void AcquireDataTest1()
        {
            Storage target = new Storage(); // TODO: Passenden Wert initialisieren
            List<string> indicatorName = null; // TODO: Passenden Wert initialisieren
            string monitoredSystemName = string.Empty; // TODO: Passenden Wert initialisieren
            List<Tuple<string, object, DataType>> expected = null; // TODO: Passenden Wert initialisieren
            List<Tuple<string, object, DataType>> actual;
            actual = target.AcquireData(indicatorName, monitoredSystemName);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }

        /// <summary>
        ///Ein Test für "AcquireData"
        ///</summary>
        [TestMethod()]
        public void AcquireDataTest2()
        {
            Storage target = new Storage(); // TODO: Passenden Wert initialisieren
            List<string> indicatorName = null; // TODO: Passenden Wert initialisieren
            string monitoredSystemName = string.Empty; // TODO: Passenden Wert initialisieren
            ClusterConnection clusterConnection = null; // TODO: Passenden Wert initialisieren
            List<Tuple<string, object, DataType>> expected = null; // TODO: Passenden Wert initialisieren
            List<Tuple<string, object, DataType>> actual;
            actual = target.AcquireData(indicatorName, monitoredSystemName, clusterConnection);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }

        /// <summary>
        ///Ein Test für "AcquireData"
        ///</summary>
        [TestMethod()]
        public void AcquireDataTest3()
        {
            Storage target = new Storage(); // TODO: Passenden Wert initialisieren
            List<Tuple<string, object, DataType>> expected = null; // TODO: Passenden Wert initialisieren
            List<Tuple<string, object, DataType>> actual;
            actual = target.AcquireData();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }

        /// <summary>
        ///Ein Test für "AcquireData"
        ///</summary>
        [TestMethod()]
        public void AcquireDataTest4()
        {
            Storage target = new Storage(); // TODO: Passenden Wert initialisieren
            List<string> indicatorNames = null; // TODO: Passenden Wert initialisieren
            List<Tuple<string, object, DataType>> expected = null; // TODO: Passenden Wert initialisieren
            List<Tuple<string, object, DataType>> actual;
            actual = target.AcquireData(indicatorNames);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }

        /// <summary>
        ///Ein Test für "AcquireData"
        ///</summary>
        [TestMethod()]
        public void AcquireDataTest5()
        {
            Storage target = new Storage(); // TODO: Passenden Wert initialisieren
            string monitoredSystemName = string.Empty; // TODO: Passenden Wert initialisieren
            List<Tuple<string, object, DataType>> expected = null; // TODO: Passenden Wert initialisieren
            List<Tuple<string, object, DataType>> actual;
            actual = target.AcquireData(monitoredSystemName);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }

        /// <summary>
        ///Ein Test für "GetCapacity"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Plugins.Windows.Storage.dll")]
        public void GetCapacityTest()
        {
            Storage_Accessor target = new Storage_Accessor();
            Tuple<string, object, DataType> actual;
            actual = target.GetCapacity();
            Assert.IsTrue(Convert.ToInt32(actual.Item2) > 0);
        }

        /// <summary>
        ///Ein Test für "GetCapacityPerDrive"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Plugins.Windows.Storage.dll")]
        public void GetCapacityPerDriveTest()
        {
            Storage_Accessor target = new Storage_Accessor();
            Tuple<string, object, DataType> actual;
            actual = target.GetCapacityPerDrive();
            Assert.IsTrue(actual.Item2.ToString().Length > 0);
        }

        /// <summary>
        ///Ein Test für "GetIndicatorSettings"
        ///</summary>
        [TestMethod()]
        public void GetIndicatorSettingsTest()
        {
            Storage target = new Storage(); // TODO: Passenden Wert initialisieren
            List<IndicatorSettings> expected = null; // TODO: Passenden Wert initialisieren
            List<IndicatorSettings> actual;
            actual = target.GetIndicatorSettings();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }

        /// <summary>
        ///Ein Test für "GetLoad"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Plugins.Windows.Storage.dll")]
        public void GetLoadTest()
        {
            Storage_Accessor target = new Storage_Accessor();
            Tuple<string, object, DataType> actual;
            actual = target.GetLoad();
            Assert.IsTrue(Convert.ToByte(actual.Item2) <= 100);
            Assert.IsTrue(Convert.ToByte(actual.Item2) >= 0);
        }

        /// <summary>
        ///Ein Test für "GetLoadPerDrive"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Plugins.Windows.Storage.dll")]
        public void GetLoadPerDriveTest()
        {
            Storage_Accessor target = new Storage_Accessor();
            Tuple<string, object, DataType> actual;
            actual = target.GetLoadPerDrive();
            Assert.IsTrue(actual.ToString().Length > 0);
        }

        /// <summary>
        ///Ein Test für "GetNumberOfDrives"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Plugins.Windows.Storage.dll")]
        public void GetNumberOfDrivesTest()
        {
            Storage_Accessor target = new Storage_Accessor();
            Tuple<string, object, DataType> actual;
            actual = target.GetNumberOfDrives();
            Assert.IsTrue(Convert.ToByte(actual.Item2) > 0);
        }

        /// <summary>
        ///Ein Test für "TargetPlatform"
        ///</summary>
        [TestMethod()]
        public void TargetPlatformTest()
        {
            Storage target = new Storage();
            Platform actual;
            actual = target.TargetPlatform;
            Assert.AreEqual(actual, Platform.Windows);
        }
    }
}

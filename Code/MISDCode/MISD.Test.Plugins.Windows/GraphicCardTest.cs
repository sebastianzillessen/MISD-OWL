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

using MISD.Plugins.Windows.GraphicCard;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MISD.Core;
using System.Collections.Generic;

namespace MISD.Test.Plugins.Windows
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "GraphicCardTest" und soll
    ///alle GraphicCardTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class GraphicCardTest
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
        ///Ein Test für "GraphicCard-Konstruktor"
        ///</summary>
        [TestMethod()]
        public void GraphicCardConstructorTest()
        {
            GraphicCard target = new GraphicCard();
            Assert.Inconclusive("TODO: Code zum Überprüfen des Ziels implementieren");
        }

        /// <summary>
        ///Ein Test für "AcquireData"
        ///</summary>
        [TestMethod()]
        public void AcquireDataTest()
        {
            GraphicCard target = new GraphicCard(); // TODO: Passenden Wert initialisieren
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
            GraphicCard target = new GraphicCard(); // TODO: Passenden Wert initialisieren
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
            GraphicCard target = new GraphicCard(); // TODO: Passenden Wert initialisieren
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
            GraphicCard target = new GraphicCard(); // TODO: Passenden Wert initialisieren
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
            GraphicCard target = new GraphicCard(); // TODO: Passenden Wert initialisieren
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
            GraphicCard target = new GraphicCard(); // TODO: Passenden Wert initialisieren
            string monitoredSystemName = string.Empty; // TODO: Passenden Wert initialisieren
            List<Tuple<string, object, DataType>> expected = null; // TODO: Passenden Wert initialisieren
            List<Tuple<string, object, DataType>> actual;
            actual = target.AcquireData(monitoredSystemName);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }

        /// <summary>
        ///Ein Test für "GetIndicatorSettings"
        ///</summary>
        [TestMethod()]
        public void GetIndicatorSettingsTest()
        {
            GraphicCard target = new GraphicCard(); // TODO: Passenden Wert initialisieren
            List<IndicatorSettings> expected = null; // TODO: Passenden Wert initialisieren
            List<IndicatorSettings> actual;
            actual = target.GetIndicatorSettings();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }

        /// <summary>
        ///Ein Test für "GetNamePerDevice"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Plugins.Windows.GraphicCard.dll")]
        public void GetNamePerDeviceTest()
        {
            GraphicCard_Accessor target = new GraphicCard_Accessor(); // TODO: Passenden Wert initialisieren
            Tuple<string, object, DataType> expected = null; // TODO: Passenden Wert initialisieren
            Tuple<string, object, DataType> actual;
            actual = target.GetNamePerDevice();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }

        /// <summary>
        ///Ein Test für "GetNumberOfDevices"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Plugins.Windows.GraphicCard.dll")]
        public void GetNumberOfDevicesTest()
        {
            GraphicCard_Accessor target = new GraphicCard_Accessor(); // TODO: Passenden Wert initialisieren
            Tuple<string, object, DataType> expected = null; // TODO: Passenden Wert initialisieren
            Tuple<string, object, DataType> actual;
            actual = target.GetNumberOfDevices();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }

        /// <summary>
        ///Ein Test für "TargetPlatform"
        ///</summary>
        [TestMethod()]
        public void TargetPlatformTest()
        {
            GraphicCard target = new GraphicCard(); // TODO: Passenden Wert initialisieren
            Platform actual;
            actual = target.TargetPlatform;
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }
    }
}

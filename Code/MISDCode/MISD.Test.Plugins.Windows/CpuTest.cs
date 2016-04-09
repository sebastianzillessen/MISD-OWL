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

using MISD.Plugins.Windows.CPU;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MISD.Core;
using System.Collections.Generic;

namespace MISD.Test.Plugins.Windows
{


    /// <summary>
    /// This is the test class for the CPU Plugin Windows.
    /// </summary>
    [TestClass()]
    public class CpuTest
    {
        /// <summary>
        /// Tests the constructor
        ///</summary>
        [TestMethod()]
        public void CpuConstructorTest()
        {
            CPU target = new CPU();
            Assert.IsNotNull(target);
        }

        /// <summary>
        /// Tests AcquireData for all indicators
        ///</summary>
        [TestMethod()]
        public void AcquireDataTest()
        {
            CPU target = new CPU();
            List<Tuple<string, object, DataType>> actual;
            actual = target.AcquireData();
            foreach (Tuple<string, object, DataType> tup in actual)
            {
                Assert.IsNotNull(tup.Item1);
                switch (tup.Item3)
                {
                    case DataType.String: Assert.IsNotNull(Convert.ToString(tup.Item2)); break;
                    case DataType.Int: Assert.IsNotNull(Convert.ToInt32(tup.Item2)); break;
                    case DataType.Float: Assert.IsNotNull(Convert.ToDouble(tup.Item2)); break;
                    case DataType.Byte: Assert.IsNotNull(Convert.ToByte(tup.Item2)); break;
                    default: Assert.Fail(); break;
                }
            }
        }

        /// <summary>
        /// Tests AcquireData for a selection of indicators
        ///</summary>
        [TestMethod()]
        public void AcquireDataTest1()
        {
            CPU_Accessor target = new CPU_Accessor();
            List<string> indicatorName = new List<string>();
            indicatorName.Add(target.indicators[0].IndicatorName);
            indicatorName.Add(target.indicators[3].IndicatorName);
            indicatorName.Add(target.indicators[5].IndicatorName);
            List<Tuple<string, object, DataType>> actual;
            actual = target.AcquireData(indicatorName);
            Assert.IsTrue(actual.Count == 3);
            foreach (Tuple<string, object, DataType> tup in actual)
            {
                Assert.IsNotNull(tup.Item1);
                switch (tup.Item3)
                {
                    case DataType.String: Assert.IsNotNull(Convert.ToString(tup.Item2)); break;
                    case DataType.Int: Assert.IsNotNull(Convert.ToInt32(tup.Item2)); break;
                    case DataType.Float: Assert.IsNotNull(Convert.ToDouble(tup.Item2)); break;
                    case DataType.Byte: Assert.IsNotNull(Convert.ToByte(tup.Item2)); break;
                    default: Assert.Fail(); break;
                }
            }
        }
        /// <summary>
        /// Tests GetDescription
        ///</summary>
        [TestMethod()]
        public void GetDescriptionTest()
        {
            CPU target = new CPU();
            string actual;
            actual = target.GetDescription();
            Assert.IsNotNull(actual);
        }

        /// <summary>
        /// Tests GetName
        ///</summary>
        [TestMethod()]
        public void GetNameTest()
        {
            CPU target = new CPU();
            string actual;
            actual = target.GetName();
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///Tests GetOverallLoad
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Plugins.Windows.CPU.dll")]
        public void GetLoadTest()
        {
            CPU_Accessor target = new CPU_Accessor();
            Tuple<string, object, DataType> actual;
            actual = target.GetLoad();
            Assert.IsTrue(Convert.ToByte(actual.Item2) <= 100 &&
                Convert.ToByte(actual.Item2) >= 0);
        }

        /// <summary>
        ///Tests GetProcessorName
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Plugins.Windows.CPU.dll")]
        public void GetProcessorNameTest()
        {
            CPU_Accessor target = new CPU_Accessor();
            Tuple<string, object, DataType> actual;
            actual = target.GetProcessorName();
            Assert.IsNotNull(actual.Item2);
        }

        /// <summary>
        ///Tests GetTemperature
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Plugins.Windows.CPU.dll")]
        public void GetTemperatureTest()
        {
            CPU_Accessor target = new CPU_Accessor();
            Tuple<string, object, DataType> actual;
            actual = target.GetTemperature();
            Assert.IsTrue(Convert.ToByte(actual.Item2) < 150 &&
                Convert.ToByte(actual.Item2) > 0);
        }

        /// <summary>
        ///Tests getNumberOfCores
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Plugins.Windows.CPU.dll")]
        public void GetNumberOfCoresTest()
        {
            CPU_Accessor target = new CPU_Accessor();
            Tuple<string, object, DataType> actual;
            actual = target.GetNumberOfCores();
            Assert.IsTrue(Convert.ToInt32(actual.Item2) > 0 && Convert.ToInt32(actual.Item2) < 1024);
        }

        /// <summary>
        ///Tests GetLoadPerCore
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Plugins.Windows.CPU.dll")]
        public void GetLoadPerCoreTest()
        {
            CPU_Accessor target = new CPU_Accessor();
            Tuple<string, object, DataType> actual;
            actual = target.GetLoadPerCore();
            string[] result = actual.Item2.ToString().Split(Convert.ToChar(";"));
            foreach (string s in result)
            {
                Assert.IsTrue(Convert.ToInt32(s) <= 100 && Convert.ToInt32(s) >= 0);
            }
        }
    }
}

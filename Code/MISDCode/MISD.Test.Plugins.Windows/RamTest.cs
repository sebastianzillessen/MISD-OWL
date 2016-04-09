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

using MISD.Plugins.Windows.RAM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using MISD.Core;

namespace MISD.Test.Plugins.Windows
{


    /// <summary>
    ///This is the test class for the RAM Plugin
    ///</summary>
    [TestClass()]
    public class RAMTest
    {

        /// <summary>
        ///Ein Test für "AcquireData"
        ///</summary>
        [TestMethod()]
        public void AcquireDataTest()
        {
            RAM_Accessor target = new RAM_Accessor();
            List<string> indicatorNames = new List<string>();
            indicatorNames.Add(target.indicators[0].IndicatorName);
            indicatorNames.Add(target.indicators[2].IndicatorName);
            indicatorNames.Add(target.indicators[3].IndicatorName);

            List<Tuple<string, object, DataType>> actual;
            actual = target.AcquireData(indicatorNames);
            Assert.IsTrue(actual.Count == 3);

            foreach (Tuple<string, object, DataType> tup in actual)
            {
                Assert.IsNotNull(tup.Item1);
                switch (tup.Item3)
                {
                    case DataType.Byte: Assert.IsNotNull(Convert.ToByte(tup.Item2)); break;
                    case DataType.Float: Assert.IsNotNull(Convert.ToDouble(tup.Item2)); break;
                    case DataType.Int: Assert.IsNotNull(Convert.ToInt32(tup.Item2)); break;
                    case DataType.String: Assert.IsNotNull(Convert.ToString(tup.Item2)); break;
                    default: Assert.Fail(); break;
                }
            }
        }

        /// <summary>
        /// Tests AcquireData
        ///</summary>
        [TestMethod()]
        public void AcquireDataTest1()
        {
            RAM target = new RAM();
            List<Tuple<string, object, DataType>> actual;
            actual = target.AcquireData();
            foreach (Tuple<string, object, DataType> tup in actual)
            {
                Assert.IsNotNull(tup.Item1);
                switch (tup.Item3)
                {
                    case DataType.Byte: Assert.IsNotNull(Convert.ToByte(tup.Item2)); break;
                    case DataType.Float: Assert.IsNotNull(Convert.ToDouble(tup.Item2)); break;
                    case DataType.Int: Assert.IsNotNull(Convert.ToInt32(tup.Item2)); break;
                    case DataType.String: Assert.IsNotNull(Convert.ToString(tup.Item2)); break;
                    default: Assert.Fail(); break;
                }
            }
        }


        /// <summary>
        /// Tests GetDefaultIndicatorFilters
        ///</summary>
        [TestMethod()]
        public void GetDefaultIndicatorFiltersTest()
        {
            RAM target = new RAM();
            foreach (IndicatorSettings s in target.GetIndicatorSettings())
            {
                Assert.IsNotNull(s.FilterStatement);
            }
        }

        /// <summary>
        /// Tests GetDefaultIndicatorMetrics
        ///</summary>
        [TestMethod()]
        public void GetDefaultIndicatorMetricsTest()
        {
            RAM target = new RAM();
            foreach (IndicatorSettings s in target.GetIndicatorSettings())
            {
                Assert.IsNotNull(s.MetricWarning);
                Assert.IsNotNull(s.MetricCritical);
            }
        }

        /// <summary>
        /// Test GetDefaultMappingDuration
        ///</summary>
        [TestMethod()]
        public void GetDefaultMappingDurationTest()
        {
            RAM target = new RAM();
            foreach (IndicatorSettings s in target.GetIndicatorSettings())
            {
                Assert.IsNotNull(s.MappingDuration);
            }
        }

        /// <summary>
        /// Tests GetDefaultStorageDuration
        ///</summary>
        [TestMethod()]
        public void GetDefaultStorageDurationTest()
        {
            RAM target = new RAM();
            foreach (IndicatorSettings s in target.GetIndicatorSettings())
            {
                Assert.IsTrue(s.StorageDuration > new TimeSpan(0));
            }
        }

        /// <summary>
        /// Tests GetDefaultUpdateIntervals
        ///</summary>
        [TestMethod()]
        public void GetDefaultUpdateIntervalsTest()
        {
            RAM target = new RAM();
            foreach (IndicatorSettings s in target.GetIndicatorSettings())
            {
                Assert.IsTrue(s.UpdateInterval.Ticks > 0);
            }
        }

        /// <summary>
        ///Tests GetDescription
        ///</summary>
        [TestMethod()]
        public void GetDescriptionTest()
        {
            RAM target = new RAM();
            string actual;
            actual = target.GetDescription();
            Assert.IsNotNull(actual);
        }

        /// <summary>
        /// Tests GetIndicatorNames
        ///</summary>
        [TestMethod()]
        public void GetIndicatorNamesTest()
        {
            RAM target = new RAM();
            foreach (IndicatorSettings s in target.GetIndicatorSettings())
            {
                Assert.IsNotNull(s.IndicatorName);
            }
        }

        /// <summary>
        /// Tests GetLoad
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Plugins.Windows.RAM.dll")]
        public void GetLoadTest()
        {
            RAM_Accessor target = new RAM_Accessor();
            Tuple<string, object, DataType> actual;
            actual = target.GetLoad();
            Assert.IsTrue(Convert.ToByte(actual.Item2) <= 100 && Convert.ToByte(actual.Item2) >= 0);
        }

        /// <summary>
        /// Tests GetName
        ///</summary>
        [TestMethod()]
        public void GetNameTest()
        {
            RAM target = new RAM();
            string actual;
            actual = target.GetName();
            Assert.IsNotNull(actual);
        }

        /// <summary>
        /// Tests GetSize
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Plugins.Windows.RAM.dll")]
        public void GetSizeTest()
        {
            RAM_Accessor target = new RAM_Accessor();
            Tuple<string, object, DataType> actual;
            actual = target.GetSize();
            Assert.IsTrue(Convert.ToInt32(actual.Item2) >= 0);
        }

        /// <summary>
        /// Tests GetSwapLoad
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Plugins.Windows.RAM.dll")]
        public void GetSwapLoadTest()
        {
            RAM_Accessor target = new RAM_Accessor();
            Tuple<string, object, DataType> actual;
            actual = target.GetSwapLoad();
            Assert.IsTrue(Convert.ToByte(actual.Item2) >= 0 && Convert.ToByte(actual.Item2) <= 100);
        }

        /// <summary>
        /// Test GetSwapSize
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Plugins.Windows.RAM.dll")]
        public void GetSwapSizeTest()
        {
            RAM_Accessor target = new RAM_Accessor();
            Tuple<string, object, DataType> actual;
            actual = target.GetSwapSize();
            Assert.IsTrue(Convert.ToInt32(actual.Item2) >= 0);
        }
    }
}

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

using MISD.Plugins.Windows.NetworkAdapter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MISD.Core;
using System.Collections.Generic;

namespace MISD.Test.Plugins.Windows
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "NetworkAdapterTest" und soll
    ///alle NetworkAdapterTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class NetworkAdapterTest
    {
        /// <summary>
        ///Ein Test für "GetDownPerAdapter"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Plugins.Windows.NetworkAdapter.dll")]
        public void GetDownPerAdapterTest()
        {
            NetworkAdapter_Accessor target = new NetworkAdapter_Accessor();
            Tuple<string, object, DataType> actual;
            actual = target.GetDownPerAdapter();
            Assert.IsFalse(actual.Item2.ToString().Equals(""));
        }

        /// <summary>
        ///Ein Test für "GetIPPerAdapter"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Plugins.Windows.NetworkAdapter.dll")]
        public void GetIPPerAdapterTest()
        {
            NetworkAdapter_Accessor target = new NetworkAdapter_Accessor();
            Tuple<string, object, DataType> actual;
            actual = target.GetIPPerAdapter();
            Assert.IsFalse(actual.Item2.ToString().Equals(""));
        }

        /// <summary>
        ///Ein Test für "GetMACPerAdapter"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Plugins.Windows.NetworkAdapter.dll")]
        public void GetMACPerAdapterTest()
        {
            NetworkAdapter_Accessor target = new NetworkAdapter_Accessor();
            Tuple<string, object, DataType> actual;
            actual = target.GetMACPerAdapter();
            Assert.IsFalse(actual.Item2.ToString().Equals(""));
        }

        /// <summary>
        ///Ein Test für "GetNamePerAdapter"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Plugins.Windows.NetworkAdapter.dll")]
        public void GetNamePerAdapterTest()
        {
            NetworkAdapter_Accessor target = new NetworkAdapter_Accessor();
            Tuple<string, object, DataType> actual;
            actual = target.GetNamePerAdapter();
            Assert.IsFalse(actual.Item2.ToString().Equals(""));
        }

        /// <summary>
        ///Ein Test für "GetNumberOfAdapters"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Plugins.Windows.NetworkAdapter.dll")]
        public void GetNumberOfAdaptersTest()
        {
            NetworkAdapter_Accessor target = new NetworkAdapter_Accessor();
            Tuple<string, object, DataType> actual;
            actual = target.GetNumberOfAdapters();
            Assert.IsTrue(Convert.ToInt32(actual.Item2) > 0);
        }

        /// <summary>
        ///Ein Test für "GetUpPerAdapter"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Plugins.Windows.NetworkAdapter.dll")]
        public void GetUpPerAdapterTest()
        {
            NetworkAdapter_Accessor target = new NetworkAdapter_Accessor();
            Tuple<string, object, DataType> actual;
            actual = target.GetUpPerAdapter();
            Assert.IsFalse(actual.Item2.ToString().Equals(""));
        }
    }
}

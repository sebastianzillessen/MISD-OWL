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

using MISD.Plugins.Windows.OS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MISD.Core;
using System.Collections.Generic;

namespace MISD.Test.Plugins.Windows
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "OSTest" und soll
    ///alle OSTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class OSTest
    {

        /// <summary>
        ///Ein Test für "GetName"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Plugins.Windows.OS.dll")]
        public void GetNameTest()
        {
            OS_Accessor target = new OS_Accessor();
            Tuple<string, object, DataType> actual;
            actual = target.GetName();
            Assert.IsTrue(actual.Item2.ToString().Contains("Windows"));
        }

        /// <summary>
        ///Ein Test für "GetUptime"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Plugins.Windows.OS.dll")]
        public void GetUptimeTest()
        {
            OS_Accessor target = new OS_Accessor();
            Tuple<string, object, DataType> actual;
            actual = target.GetUptime();
            Assert.IsTrue(TimeSpan.Parse(actual.Item2.ToString()).Ticks > 0);
        }

        /// <summary>
        ///Ein Test für "GetVersion"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Plugins.Windows.OS.dll")]
        public void GetVersionTest()
        {
            OS_Accessor target = new OS_Accessor();
            Tuple<string, object, DataType> actual;
            actual = target.GetVersion();
            Assert.IsFalse(actual.Item2.ToString().Equals(""));
        }
    }
}

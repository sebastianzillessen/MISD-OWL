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

using MISD.Plugins.Windows.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MISD.Core;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace MISD.Test.Plugins.Windows
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "EventsTest" und soll
    ///alle EventsTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class EventsTest
    {
        /// <summary>
        ///Ein Test für "GetEvent"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Plugins.Windows.Events.dll")]
        public void GetEventTest()
        {
            Events_Accessor target = new Events_Accessor();
            Tuple<string, object, DataType> actual;
            EventLog log = new EventLog();
            log.Source = "test";
            log.WriteEntry("test entry0", EventLogEntryType.Information);
            log.WriteEntry("test entry1", EventLogEntryType.Error);
            log.WriteEntry("test entry2", EventLogEntryType.Warning);
            log.WriteEntry("test entry3", EventLogEntryType.FailureAudit);
            actual = target.GetEvent();
            Assert.IsTrue(actual.Item2.ToString().Contains("entry0"));
            Assert.IsTrue(actual.Item2.ToString().Contains("entry1"));
            Assert.IsTrue(actual.Item2.ToString().Contains("entry2"));
            Assert.IsTrue(actual.Item2.ToString().Contains("entry3"));
        }
    }
}

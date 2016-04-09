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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Diagnostics;
using MISD.Server;
using MISD.Core;
using MISD.Server.Manager;

namespace ServerTest
{  
    /// <summary>
    ///This is the test class for Logger. It contains component tests for each component.
    ///</summary>
    [TestClass()]
    public class LoggerTest
    {
        /// <summary>
        ///Testing the logger constructor
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Server.exe")]
        public void LoggerConstructorTest()
        {
            MISD.Server.Manager.Logger target = MISD.Server.Manager.Logger.Instance;
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///Testing the creation of the debug textfile
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Server.exe")]
        public void CreateDebugFileTest()
        {
            Logger_Accessor target = new Logger_Accessor();
            Logger init = Logger.Instance;
            Assert.IsTrue(Directory.Exists(target.fileDirectory));
            Assert.IsTrue(File.Exists(target.fileDirectory + target.fileName));
        }

        /// <summary>
        ///Testing the creation of the eventlog source
        ///</summary>
        [TestMethod()]
        [DeploymentItem("MISD.Server.exe")]
        public void CreateEventLogTest()
        {
            Logger target = Logger.Instance;
            Assert.IsTrue(EventLog.SourceExists("MISD"));
        }

        /// <summary>
        ///Testing Singleton pattern
        ///</summary>
        [TestMethod()]
        public void GetInstanceTest()
        {
            Logger expected = Logger.Instance;
            Logger actual = Logger.Instance;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///Testing WriteEntry by creating 3 eventlogs and 3 debug textfile entries.
        ///</summary>
        [TestMethod()]
        public void WriteEntryTest()
        {
            Logger_Accessor target = new Logger_Accessor();
            
            //test windows event log
            target.misdLog.Clear();
            target.WriteEntry("Log1", LogType.Exception);
            target.WriteEntry("Log2", LogType.Info);
            target.WriteEntry("Log3", LogType.Warning);
            Assert.AreEqual(3, target.misdLog.Entries.Count);
            target.misdLog.Clear();
            
            //test debug file
            Program_Accessor.DebugMode = true;
            target.WriteEntry("Log1", LogType.Debug);
            target.WriteEntry("Log2", LogType.Debug);
            target.WriteEntry("Log3", LogType.Debug);
            target.debugFile.Close();
            StreamReader reader = new StreamReader(target.fileDirectory + target.fileName);
            string s = reader.ReadToEnd();
            Assert.IsTrue(s.Contains("Log1"));
            Assert.IsTrue(s.Contains("Log2"));
            Assert.IsTrue(s.Contains("Log3"));
            reader.Close();
            File.Create(target.fileDirectory + target.fileName).Close();
        }
    }
}

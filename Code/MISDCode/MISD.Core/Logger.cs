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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace MISD.Core
{

    /// <summary>
    /// The Logger class allows creating log entries of four different types.
    /// The type of a log entry is specified through the LogType enumeration.
    /// A debug entry will be written into a separate event source (as informational event log).
    /// MISD Debug logs messages for both, workstation and server debug messages.
    /// Each of the other types is mapped to a windows Eventlog event.
    /// </summary>
    public class Logger
    {
        private static volatile Logger instance;
        private static object syncRoot = new Object();
        private EventLog misdLog;
        private EventLog misdWorkstationLog;
        private EventLog misdDebug;


        /// <summary>
        /// Singleton field. Use this one to get a Logger instance.
        /// </summary>
        public static Logger Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Logger();
                    }
                }

                return instance;
            }
        }


        /// <summary>
        /// Logger Constructor, creates the Windows EventLog source and a debug textfile.
        /// </summary>
        private Logger()
        {
            CreateEventLog();
        }

        #region Initializing

        /// <summary>
        /// Creates a Windows EventLog sources for logging.
        /// MISD Log logs server events, MISD Workstation Log logs events sent by workstations via webservice.
        /// MISD Debug logs messages for both, workstation and server debug messages.
        /// </summary>
        private void CreateEventLog()
        {
            // Create the MISD event source
            if (!EventLog.SourceExists("MISD"))
            {
                EventLog.CreateEventSource("MISD", "MISD Log");
            }

            // Create the EventLog object and assign it to the source
            misdLog = new EventLog();
            misdLog.Source = "MISD";

            // Create the MISD event source
            if (!EventLog.SourceExists("MISD Debug"))
            {
                EventLog.CreateEventSource("MISD Debug", "MISD Debug Log");
            }

            // Create the EventLog object and assign it to the source
            misdDebug = new EventLog();
            misdDebug.Source = "MISD Debug";

            // Create the MISD event source for logging workstation events
            if (!EventLog.SourceExists("MISD WS"))
            {
                EventLog.CreateEventSource("MISD WS", "MISD Workstation Log");
            }

            // Create the EventLog object and assign it to the source
            misdWorkstationLog = new EventLog();
            misdWorkstationLog.Source = "MISD WS";
        }

        #endregion

        #region Logging

        /// <summary>
        /// Writes an entry to either misd source or debug source.
        /// </summary>
        /// <param name="message">Entry message to be written.</param>
        /// <param name="type">Type of the message.</param>
        public void WriteEntry(string message, LogType type)
        {
            // Get the log prefix
            StackTrace trace = new StackTrace();

            var callingMethodName = trace.GetFrame(1).GetMethod().Name;
            if (callingMethodName == ".ctor") callingMethodName = "Constructor";

            var callingClassName = trace.GetFrame(1).GetMethod().DeclaringType.Name;

            message = message + "\n(called by: " + callingClassName + "." + callingMethodName + ")";

            // Write entry either in misd source or debug source.
            switch (type)
            {
                case LogType.Debug:
                    misdDebug.WriteEntry(message, EventLogEntryType.Information);
                    break;
                case LogType.Exception:
                    misdLog.WriteEntry(message, EventLogEntryType.Error);
                    break;
                case LogType.Warning:
                    misdLog.WriteEntry(message, EventLogEntryType.Warning);
                    break;
                case LogType.Info:
                    misdLog.WriteEntry(message, EventLogEntryType.Information);
                    break;
                default:
                    misdLog.WriteEntry(message, EventLogEntryType.Information);
                    break;
            }

            trace = null;
        }

        /// <summary>
        /// Writes an entry to either misd source or debug source.
        /// </summary>
        /// <param name="message">Entry message to be written.</param>
        /// <param name="e">Exception to be appended.</param>
        /// <param name="type">Type of the message.</param>
        public void WriteEntry(string message, Exception e, LogType type)
        {
            StackTrace trace = new StackTrace();

            var callingMethodName = trace.GetFrame(1).GetMethod().Name;
            if (callingMethodName == ".ctor") callingMethodName = "Constructor";

            var callingClassName = trace.GetFrame(1).GetMethod().DeclaringType.Name;

            message = message + "\n(called by: " + callingClassName + "." + callingMethodName + ")";

            message += "\nException details:\n" + e.ToString();

            // Write entry either in misd source or debug source.
            switch (type)
            {
                case LogType.Debug:
                    misdDebug.WriteEntry(message, EventLogEntryType.Information);
                    break;
                case LogType.Exception:
                    misdLog.WriteEntry(message, EventLogEntryType.Error);
                    break;
                case LogType.Warning:
                    misdLog.WriteEntry(message, EventLogEntryType.Warning);
                    break;
                case LogType.Info:
                    misdLog.WriteEntry(message, EventLogEntryType.Information);
                    break;
                default:
                    misdLog.WriteEntry(message, EventLogEntryType.Information);
                    break;
            }

            trace = null;
        }

        /// <summary>
        /// Writes an Workstationentry to either misd source or debug source.
        /// </summary>
        /// <param name="message">Entry message to be written.</param>
        /// <param name="type">Type of the message.</param>
        public void WriteWorkstationEntry(string message, LogType type)
        {
            if (!PlatformID.Win32NT.Equals(Environment.OSVersion.Platform))
            {
                //Linux
                Console.WriteLine("LOG: [" + type + "]" + message);
            }
            else
            {
                // Windows
                // Write entry either in misd source or debug source.
                switch (type)
                {
                    case LogType.Debug: misdDebug.WriteEntry(message, EventLogEntryType.Information); break;
                    case LogType.Exception: misdWorkstationLog.WriteEntry(message, EventLogEntryType.Error); break;
                    case LogType.Info: misdWorkstationLog.WriteEntry(message, EventLogEntryType.Information); break;
                    case LogType.Warning: misdWorkstationLog.WriteEntry(message, EventLogEntryType.Warning); break;
                    default: misdWorkstationLog.WriteEntry(message, EventLogEntryType.Information); break;
                }
            }
        }

        #endregion
    }
}

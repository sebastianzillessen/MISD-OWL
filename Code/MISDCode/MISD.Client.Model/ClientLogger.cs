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

using MISD.Core;

namespace MISD.Client.Model
{
    public class ClientLogger
    {
        #region Singleton

        private static volatile ClientLogger instance;
        private static object syncRoot = new Object();

        /// <summary>
        /// Initializes a new instance of the ClientLogger class.
        /// </summary>
        private ClientLogger()
        {
            CreateEventLog();
        }

        /// <summary>
        /// Gets the singleton instance of this class.
        /// </summary>
        public static ClientLogger Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ClientLogger();
                    }
                }

                return instance;
            }
        }

        #endregion

        #region Fields

        private EventLog misdClientlog;

        #endregion

        #region Initializing

        /// <summary>
        /// Creates a Windows EventLog sources for logging.
        /// </summary>
        private void CreateEventLog()
        {
            try
            {
                // Create the MISD event source
                if (!EventLog.SourceExists("MISD"))
                {
                    EventLog.CreateEventSource("MISD", "MISD Client Log");
                }

                // Create the EventLog object and assign it to the source
                misdClientlog = new EventLog();
                misdClientlog.Source = "MISD";
            }
            catch (Exception)
            {
                // no privileges?
            }
        }

        #endregion

        #region Logging

        public void WriteEntry(string message, LogType type)
        {
            // Get the log prefix
            StackTrace trace = new StackTrace();

            var callingMethodName = trace.GetFrame(1).GetMethod().Name;
            if (callingMethodName == ".ctor") callingMethodName = "Constructor";

            var callingClassName = trace.GetFrame(1).GetMethod().DeclaringType.Name;

            message = callingClassName + "." + callingMethodName + ": " + message;

            try
            {
                switch (type)
                {
                    case LogType.Debug:
                        misdClientlog.WriteEntry(message, EventLogEntryType.Information);
                        break;
                    case LogType.Exception:
                        misdClientlog.WriteEntry(message, EventLogEntryType.Error);
                        break;
                    case LogType.Warning:
                        misdClientlog.WriteEntry(message, EventLogEntryType.Warning);
                        break;
                    case LogType.Info:
                        misdClientlog.WriteEntry(message, EventLogEntryType.Information);
                        break;
                    default:
                        misdClientlog.WriteEntry(message, EventLogEntryType.Information);
                        break;
                }
            }
            catch (Exception)
            {
                // no source available?
            }
            finally
            {
                trace = null;
            }
        }

        public void WriteEntry(string message, Exception e, LogType type)
        {
            StackTrace trace = new StackTrace();

            var callingMethodName = trace.GetFrame(1).GetMethod().Name;
            if (callingMethodName == ".ctor") callingMethodName = "Constructor";

            var callingClassName = trace.GetFrame(1).GetMethod().DeclaringType.Name;

            message = callingClassName + "." + callingMethodName + ": " + message;

            message += "\nException details:\n" + e.ToString();

            try
            {
                switch (type)
                {
                    case LogType.Debug:
                        misdClientlog.WriteEntry(message, EventLogEntryType.Information);
                        break;
                    case LogType.Exception:
                        misdClientlog.WriteEntry(message, EventLogEntryType.Error);
                        break;
                    case LogType.Warning:
                        misdClientlog.WriteEntry(message, EventLogEntryType.Warning);
                        break;
                    case LogType.Info:
                        misdClientlog.WriteEntry(message, EventLogEntryType.Information);
                        break;
                    default:
                        misdClientlog.WriteEntry(message, EventLogEntryType.Information);
                        break;
                }
            }
            catch (Exception)
            {
                // no source available?
            }
            finally
            {
                trace = null;
            }
        }

        #endregion
    }
}
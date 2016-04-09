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
using System.IO;

namespace MISD.Workstation.Linux
{
	/// <summary>
	/// Workstation logger.
	/// This class provides the log functions for developers:
	/// - logging in a log file
	/// - logging in the terminal
	/// - logging on the server
	/// </summary>
	public class WorkstationLogger
	{
        #region Singleton
		/// <summary>
		/// The singleton instance of WorkstationLogger.
		/// </summary>
        private static volatile WorkstationLogger instance;

		/// <summary>
		/// The sync root for locking.
		/// </summary>
		private static object syncRoot = new Object();

        /// <summary>
        /// Property to access the class.
        /// </summary>
        public static WorkstationLogger Instance
        {
            get
            {
                if (instance == null)
                {
					lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new WorkstationLogger();
                    }
                }
                return instance;
            }
        }
        #endregion

		#region Properties
		/// <summary>
		/// Whether the messages should be logged in the log file.
		/// </summary>
		private bool logMode = false;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="MISD.Workstation.Linux.WorkstationLogger"/> log mode.
		/// </summary>
		/// <value>
		/// <c>true</c> if the messages should be logged in the log file; otherwise, <c>false</c>.
		/// </value>
		public bool LogMode {
			get {
				return this.logMode;
			}
			set {
				this.logMode = value;
			}
		}

		/// <summary>
		/// Whether the messages should be logged in the terminal.
		/// </summary>
		private bool consoleMode = false;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="MISD.Workstation.Linux.WorkstationLogger"/> console mode.
		/// </summary>
		/// <value>
		/// <c>true</c> if the messages should be logged in the terminal; otherwise, <c>false</c>.
		/// </value>
		public bool ConsoleMode {
			get {
				return this.consoleMode;
			}
			set {
				this.consoleMode = value;
			}
		}

		/// <summary>
		/// The path to the log file.
		/// </summary>
		private string logPath = "/tmp/misd/";

		/// <summary>
		/// Gets or sets the path of the log file.
		/// </summary>
		/// <value>
		/// The path of the log file.
		/// </value>
		public string LogPath {
			get {
				return this.logPath;
			}
			set {
				this.logPath = value;
			}
		}

		/// <summary>
		/// The name of the log file.
		/// </summary>
		private string logFileName = "misdlog.log";

		/// <summary>
		/// Gets or sets the name of the log file.
		/// </summary>
		/// <value>
		/// The name of the log file.
		/// </value>
		public string LogFileName {
			get {
				return this.logFileName;
			}
			set {
				this.logFileName = value;
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="MISD.Workstation.Linux.WorkstationLogger"/> class.
		/// </summary>
		private WorkstationLogger ()
		{
			// Do nothing.
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Writes the log.
		/// </summary>
		/// <param name='message'>
		/// The message as string.
		/// </param>
		/// <param name='logType'>
		/// MISD.Core.LogType.
		/// </param>
		/// <param name='loggingOnServer'>
		/// Whether the message should be logged on the server too.
		/// </param>
		public void WriteLog (string message, MISD.Core.LogType logType, bool loggingOnServer)
		{
			string stringToLog = "";
			stringToLog += "[MISD] [" + logType.ToString () + "] " + System.DateTime.Now.ToString () + ": ";
			stringToLog += message;

			if (loggingOnServer)
			{
				ServerConnection.Instance.WriteLog(message, logType);
			}
			if (this.LogMode)
			{
				WriteLoggingFile (stringToLog);
			}
			if (this.ConsoleMode)
			{
				Console.WriteLine (stringToLog);
			}
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Writes the logging file.
		/// </summary>
		/// <param name='stringToLog'>
		/// String to log.
		/// </param>
		private void WriteLoggingFile (string stringToLog)
		{
			try {
				// Create the folder path, if not existant.
				System.IO.Directory.CreateDirectory(this.LogPath);

				// Write the log file.
				StreamWriter logFile = new StreamWriter (this.LogPath + this.LogFileName, true);
				logFile.WriteLine (stringToLog);
				logFile.Close ();
			} catch (Exception e) {
				Console.WriteLine (e);
			}
		}
		#endregion
	}
}


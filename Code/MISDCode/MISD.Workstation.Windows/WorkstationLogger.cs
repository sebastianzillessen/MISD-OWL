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

namespace MISD.Workstation.Windows
{
	/// <summary>
	/// Workstation logger.
	/// This class provides the log functions for developers:
	/// - logging in a log file
	/// </summary>
	public class WorkstationLogger
	{
		// The path of the log file.
        private static string logPath = AppDomain.CurrentDomain.BaseDirectory + @"log.txt";
        private static StreamWriter myFile = new StreamWriter(logPath);

		/// <summary>
		/// Writes the log.
		/// </summary>
		/// <param name='message'>
		/// The message as string with time as prefix
		/// </param>
		public static void WriteLog (string message)
		{
            // autoflush for instant output to file
            myFile.AutoFlush = true;
            myFile.WriteLine(DateTime.Now.ToString() + ": " + message);
		}

        internal static void Close()
        {
            myFile.Close();
        }
    }
}


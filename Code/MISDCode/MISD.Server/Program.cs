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
using System.ComponentModel;
using System.ServiceProcess;
using System.Threading;
using MISD.Server.Properties;

namespace MISD.Server
{
    public class Program
    {
        private static bool debugMode = Settings.Default.DebugMode;
        public static bool DebugMode
        {
            get { return debugMode; }
            set { debugMode = value; }
        }

        /// <summary>
        /// Gets a value that indicates whether the server should be run as a standalone application rather than as a Windows Service.
        /// </summary>
        public static bool IsPerfMonSession
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Method to start the server.
        /// </summary>
        public static void Main(string[] args)
        {
            Settings.Default.PropertyChanged += new PropertyChangedEventHandler(OnPropertyChanged);

            if (IsPerfMonSession)
            {
                var thread = new Thread(new ThreadStart(InternalServer.Instance.Start));

                thread.Start();

                // 30 seconds sample duration if this is a perfmon session
                TimeSpan sampleDuration = new TimeSpan(0, 1, 0);
                DateTime beginning = DateTime.Now;

                while (true)
                {
                    Thread.Sleep(2000);
                }

                thread.Abort();
                InternalServer.Instance.Stop();
            }
            else
            {
                try
                {
                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[] 
			        { 
				        new ServerService() 
			        };
                    ServiceBase.Run(ServicesToRun);
                }
                catch (Exception e)
                {
                    MISD.Core.Logger.Instance.WriteEntry("Program_Main: Problem starting server, " + e.ToString(), Core.LogType.Warning);
                }
            }
        }

        /// <summary>
        /// This method updates the DebugMode field whenever a property is changed.
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event parameters</param>
        public static void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            debugMode = Settings.Default.DebugMode;
        }
    }
}

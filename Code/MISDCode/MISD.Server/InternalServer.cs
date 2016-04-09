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
using System.ServiceModel;
using System.ServiceModel.Description;
using MISD.Core;
using MISD.Server.Manager;
using MISD.Server.Properties;
using MISD.Server.Scheduling;
using MISD.Server.Services;

namespace MISD.Server
{
    /// <summary>
    /// This class provides some basic service hosting functionality.
    /// </summary>
    public sealed class InternalServer
    {
        #region Singleton

        private static volatile InternalServer instance;
        private static object syncRoot = new Object();

        private InternalServer() { }

        /// <summary>
        /// Gets the singleton instance of this class.
        /// </summary>
        public static InternalServer Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new InternalServer();
                    }
                }

                return instance;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Signals, that the MISD server must be shut down.
        /// </summary>
        public event EventHandler ShutdownRequired;

        /// <summary>
        /// Triggers the InternalServer.ShutdownRequired event.
        /// </summary>
        private void OnShutdownRequired()
        {
            if (this.ShutdownRequired != null)
            {
                this.ShutdownRequired(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Indicates whether the Bootstrapper had errors while initializing the MISD server componenents.
        /// </summary>
        private bool HasInitializationErrors
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the client web service's service host.
        /// </summary>
        private ServiceHost ClientWebService
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the workstation web service's service host.
        /// </summary>
        private ServiceHost WorkstationWebService
        {
            get;
            set;
        }

        #endregion

        #region Methods

        /// <summary>
        /// This method opens the webservices and triggers the initialization.
        /// </summary>
        public void Start()
        {
            this.HasInitializationErrors = false;

            // Create the service hosts.
            this.ClientWebService = new ServiceHost(typeof(ClientWebService));
            this.WorkstationWebService = new ServiceHost(typeof(WorkstationWebService));

            // Open the service hosts.
            this.ClientWebService.Open();
            this.WorkstationWebService.Open();

            // Register for Bootstrapper events.
            Bootstrapper.Instance.InitializationError += new UnhandledExceptionEventHandler(Instance_InitializationError);
            Bootstrapper.Instance.InitializationComplete += new EventHandler(Instance_InitializationComplete);

            // Start the lazy initialization.
            Bootstrapper.Instance.BeginInitialize();
        }

        /// <summary>
        /// Gets called if the Bootstrapper runs in an initialization error.
        /// </summary>
        private void Instance_InitializationError(object sender, UnhandledExceptionEventArgs e)
        {
            this.HasInitializationErrors = true;

            // Write the exception to the event log.
            Logger.Instance.WriteEntry("Initialization failed: Manager Registration Process " + e.ExceptionObject.ToString(), LogType.Exception);

            // Shut down the server if necessary.
            if (e.IsTerminating)
            {
                this.Stop();
                this.OnShutdownRequired();
            }
        }

        /// <summary>
        /// Gets called if the Bootstrapper did finish its initialization.
        /// </summary>
        private void Instance_InitializationComplete(object sender, EventArgs e)
        {
            if (this.HasInitializationErrors)
            {
                // Write an info message to the event log.
                Logger.Instance.WriteEntry("The MISD OWL Server started with errors.", LogType.Warning);
            }
            else
            {
                // Write an info message to the event log.
                Logger.Instance.WriteEntry("The MISD OWL Server started successfully.", LogType.Info);
            }
        }

        /// <summary>
        /// This method shuts down the server by closing the service hosts and stopping the schedulers.
        /// </summary>
        public void Stop()
        {
            foreach (var connection in Cluster.HpcClusterConnection.HpcClusterConnectionObjects)
            {
                connection.Dispose();
            }
   
            this.ClientWebService.Close();
            this.WorkstationWebService.Close();

            MainScheduler.Instance.Stop();
        }

        #endregion
    }
}

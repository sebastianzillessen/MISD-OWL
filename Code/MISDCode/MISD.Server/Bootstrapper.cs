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
using System.Linq;
using System.Threading;
using MISD.Core;
using MISD.Server.Manager;
using MISD.Server.Scheduling;
using MISD.Server.Database;
using System.Collections.Generic;

namespace MISD.Server
{
    /// <summary>
    /// This class provides initialization functionality for the server components.
    /// </summary>
    public sealed class Bootstrapper
    {
        #region Singleton

        private static volatile Bootstrapper instance;
        private static object syncRoot = new Object();

        private Bootstrapper() { }

        /// <summary>
        /// Gets the singleton instance of this class.
        /// </summary>
        public static Bootstrapper Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Bootstrapper();
                    }
                }

                return instance;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Signals, that the Bootstrapper has finished initializing the MISD server components.
        /// </summary>
        public event EventHandler InitializationComplete;

        /// <summary>
        /// Triggers the Bootstrapper.InitializationComplete event.
        /// </summary>
        private void OnInitializationComplete()
        {
            if (this.InitializationComplete != null)
            {
                this.InitializationComplete(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Signals, that the Bootstrapper has failed initializing the MISD server components.
        /// </summary>
        public event UnhandledExceptionEventHandler InitializationError;

        /// <summary>
        /// Triggers the Bootstrapper.InitializationError event.
        /// </summary>
        private void OnInitializationError(Exception e, bool isTerminating)
        {
            if (this.InitializationError != null)
            {
                this.InitializationError(this, new UnhandledExceptionEventArgs(e, isTerminating));
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invokes the Bootstrapper.Initialize method on a separate thread.
        /// </summary>
        public void BeginInitialize()
        {
            new Thread(new ThreadStart(Initialize)).Start();
        }

        /// <summary>
        /// Initializes the MISD server components.
        /// </summary>
        public void Initialize()
        {
            try
            {
                // Initialize the plugin manager.
                PluginManager.Instance.UpdateDatabase();
                PluginManager.Instance.InitializeMEF();
                PluginManager.Instance.InitializeFileSystemWatchers();
            }
            catch (Exception e)
            {
                // The initialization failed.
                this.OnInitializationError(e, false);
            }

            try
            {
                // Initalize the cleaner
                MainScheduler.Instance.RegisterScheduler(CleanerJobScheduler.Instance);
            }
            catch (Exception e)
            {
                // The initialization failed.
                this.OnInitializationError(e, false);
            }

            try
            {
                MainScheduler.Instance.RegisterScheduler(MailScheduler.Instance);
            }
            catch (Exception e)
            {
                // The initialization failed.
                this.OnInitializationError(e, false);
            }

            try
            {
                MainScheduler.Instance.RegisterScheduler(MetaClusterManager.Instance);
            }
            catch (Exception e)
            {
                // The initialization failed.
                this.OnInitializationError(e, false);
            }

            try
            {
                this.InitializeClusters();
            }
            catch (Exception e)
            {
                // The initialization failed.
                this.OnInitializationError(e, false);
            }

            try
            {
                MainScheduler.Instance.RegisterScheduler(GlobalScheduler.Instance);
            }
            catch (Exception e)
            {
                // The initialization failed.
                this.OnInitializationError(e, false);
            }

            try
            {
                MainScheduler.Instance.Start();
            }
            catch (Exception e)
            {
                // The initialization failed.
                this.OnInitializationError(e, true);
            }

            this.OnInitializationComplete();
        }

        #endregion

        /// <summary>
        /// Initializes the clusters with the credentials stored in the database.
        /// </summary>
        private void InitializeClusters()
        {
            try
            {
                var credentials = from p in DataContextFactory.CreateReadOnlyDataContext().ClusterCredential
                                  select p;

                var threads = new List<Thread>();

                foreach (var current in credentials)
                {
                    try
                    {
                        var clusterman = new ClusterManager();

                        var thread = new Thread(new ParameterizedThreadStart(clusterman.Initialize));

                        threads.Add(thread);
                        thread.Start(current);
                    }
                    catch (Exception e)
                    {
                        threads.ElementAt(threads.Count - 1).Abort();
                        MISD.Core.Logger.Instance.WriteEntry("Bootstrapper_InitializeClusters: Problem initializing custer with headnode " + current.HeadNodeUrl + ", " + e.StackTrace, LogType.Exception);
                    }
                }

                threads.ForEach(p => p.Join());
            }
            catch (Exception e)
            {
                MISD.Core.Logger.Instance.WriteEntry("Bootstrapper_InitializeClusters: Problem initializing clusters, " + e.StackTrace, LogType.Exception);
            }
        }
    }
}

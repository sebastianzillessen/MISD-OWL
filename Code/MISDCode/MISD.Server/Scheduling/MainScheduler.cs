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
using MISD.Core.Scheduling;
using MISD.Server.Manager;

namespace MISD.Server.Scheduling
{
    public class MainScheduler : SchedulerBase
    {
        #region Singleton

        private static volatile MainScheduler instance;
        private static object syncRoot = new Object();

        /// <summary>
        /// Property to access the class.
        /// </summary>
        public static MainScheduler Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new MainScheduler();
                    }
                }

                return instance;
            }
        }

        #endregion

        #region Properties

        List<SchedulerBase> schedulers = new List<SchedulerBase>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the Scheduler class.
        /// </summary>
        private MainScheduler()
        {
        }

        #endregion

        #region Base Methods

        protected override void Initialize()
        {
            //add a main refresh timer job
            this.Jobs.Add(new MainRefreshTimerJob(Properties.Settings.Default.MainUpdateInterval));
        }

        /// <summary>
        /// Refresh the schedulers and the main refresh job
        /// </summary>
        public override void RefreshJobs()
        {
            //refresh schedulers
            foreach (SchedulerBase scheduler in schedulers)
            {
                try
                {
                    scheduler.RefreshJobs();
                }
                catch (Exception e)
                {
                    //logging
                    var messageEx1 = new StringBuilder();
                    messageEx1.Append("MainScheduler_RefreshJobs: ");
                    messageEx1.Append("Can't refresh scheduler " + scheduler.GetType().ToString() + ". " + e.ToString());
                    MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), Core.LogType.Exception);
                }
            }
        }

        /// <summary>
        /// Stop all schedulers und the timer jobs of this scheduler.
        /// </summary>
        public override void Stop()
        {
            //stop jobs
            foreach (var job in this.Jobs) job.Stop();

            //stop schedulers
            foreach (var scheduler in this.schedulers)
            {
                scheduler.Stop();
            }
        }

        /// <summary>
        /// Start all schedulers und the timer jobs of this scheduler.
        /// </summary>
        public override void Start()
        {
            //start jobs
            base.Start();

            //start schedulers
            foreach (var scheduler in this.schedulers)
            {
                scheduler.Start();
            }

        }

        #endregion

        #region Methods

        /// <summary>
        /// Register a scheduler to call the refresh method
        /// </summary>
        /// <param name="scheduler">SchedulerBase to refresh. </param>
        public void RegisterScheduler(SchedulerBase scheduler)
        {
            this.schedulers.Add(scheduler);

            ////logging
            //var message = new StringBuilder();
            //message.Append("MainScheduler_RegisterScheduler: ");
            //message.Append("Scheduler " + scheduler.ToString() + " registerd.");
            //Logger.Instance.WriteEntry(message.ToString(), Core.LogType.Info);
        }

        /// <summary>
        /// Unregister a scheduler to call the refresh method
        /// </summary>
        /// <param name="scheduler">SchedulerBase to refresh. </param>
        public void UnregisterScheduler(SchedulerBase scheduler)
        {
            if (this.schedulers.Remove(scheduler))
            {
                //logging
                var message = new StringBuilder();
                message.Append("MainScheduler_RegisterScheduler: ");
                message.Append("Scheduler unregisterd.");
                MISD.Core.Logger.Instance.WriteEntry(message.ToString(), Core.LogType.Info);
            }
            else
            {
                //logging
                var message = new StringBuilder();
                message.Append("MainScheduler_RegisterScheduler: ");
                message.Append("Scheduler unregister failed.");
                MISD.Core.Logger.Instance.WriteEntry(message.ToString(), Core.LogType.Exception);
            }
        }

        #endregion
    }
}

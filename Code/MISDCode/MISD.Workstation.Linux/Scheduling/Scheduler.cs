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
using System.Threading;

using MISD.Core;
using MISD.Core.Scheduling;
using MISD.Workstation.Linux.Scheduling.TimerJobs;
using MISD.Workstation.Linux.Plugins;

namespace MISD.Workstation.Linux.Scheduling
{
    /// <summary>
    /// Provides scheduling functionality for a variable number of timer jobs.
    /// </summary>
    public class Scheduler : SchedulerBase
    {
        #region Singleton

		/// <summary>
        /// Instance of the singleton Scheduler.
        /// </summary>
        private static volatile Scheduler instance;

		/// <summary>
		/// The sync root for locking.
		/// </summary>
		private static object syncRoot = new Object();

        /// <summary>
        /// Property to access the class.
        /// </summary>
        public static Scheduler Instance
        {
            get
            {
                if (instance == null)
                {
					lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Scheduler();
                    }
                }
                return instance;
            }
        }

        #endregion

        #region Constructors

        private Scheduler()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes this scheduler by creating its timer jobs.
        /// </summary>
        protected override void Initialize()
        {
			WorkstationLogger.Instance.WriteLog ("Initialize...", LogType.Debug, false);

            // Load plugins from the workstation
   	        List<IPlugin> loadedPlugins = PluginManager.Instance.LoadAvailablePlugins();
			WorkstationLogger.Instance.WriteLog ("loaded Plugins: " + loadedPlugins.Count.ToString(), LogType.Debug, false);

			// Synchronize the plugins with the server
            PluginManager.Instance.UpdatePlugins();


            // Start the schedulers for all indicators
            foreach (IPlugin p in PluginManager.Instance.GetLoadedPlugins())
            {
				foreach (IndicatorSettings indicatorSetting in p.GetIndicatorSettings ())
				{					
					TimeSpan updateInterval = new TimeSpan(indicatorSetting.UpdateInterval.Ticks);
                    Jobs.Add(new IndicatorTimerJob(p, indicatorSetting.IndicatorName, updateInterval));
				}
			}
			
            // Start the main update scheduler
            Jobs.Add(new MainUpdateTimerJob());
        }

        /// <summary>
        /// Refreshes the sheduler, if there are new plugins available.
        /// </summary>
        public override void RefreshJobs()
        {
            // Start the schedulers for all indicators
            foreach (IPlugin p in PluginManager.Instance.GetLoadedPlugins())
            {
				foreach (IndicatorSettings indicatorSetting in p.GetIndicatorSettings ())
				{
					TimeSpan updateInterval = new TimeSpan(indicatorSetting.UpdateInterval.Ticks);
                    Jobs.Add(new IndicatorTimerJob(p, indicatorSetting.IndicatorName, updateInterval));
				}
            }
        }

        /// <summary>
        /// Stop all jobs of a plugin
        /// </summary>
        /// <param name="plugin"></param>
        public void StopPluginScheduling(IPlugin plugin)
        {
			List<TimerJobBase> jobsToStop = new List<TimerJobBase>();
			
            foreach (TimerJobBase job in this.Jobs)
            {
                if (job.ID.Contains(plugin.GetName()))
                {
					jobsToStop.Add(job);
                }
            }
			
			foreach (TimerJobBase job in jobsToStop)
            {
            	job.Dispose();
                Jobs.Remove(job);
            }
        }

        #endregion
    }
}

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

using MISD.Core;
using MISD.Core.Scheduling;
using MISD.Workstation.Windows.Plugins;
using MISD.Workstation.Windows.Scheduling.TimerJobs;
using System;
using System.Linq;
using System.Collections.Generic;

namespace MISD.Workstation.Windows.Scheduling
{
    /// <summary>
    /// Provides scheduling functionality for a variable number of timer jobs.
    /// </summary>
    public class Scheduler : SchedulerBase
    {
        #region Singleton

        private static Scheduler instance;

        /// <summary>
        /// Property to access the class.
        /// </summary>
        public static Scheduler Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Scheduler();
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
        /// Initializes the scheduler.
        /// </summary>
        protected override void Initialize()
        {
            // Load plugins
            try
            {
                WorkstationLogger.WriteLog("Loading local plugins.");
                PluginManager.Instance.LoadAvailablePlugins();
                WorkstationLogger.WriteLog("Loaded plugins: " + PluginManager.Instance.GetLoadedPlugins().Count);
                WorkstationLogger.WriteLog("Updating plugins from remote computer...");
                PluginManager.Instance.UpdatePlugins();

                // Add the schedulers for all indicators
                foreach (IPlugin p in PluginManager.Instance.GetLoadedPlugins())
                {
                    foreach (IndicatorSettings indicatorSetting in p.GetIndicatorSettings())
                    {
                        WorkstationLogger.WriteLog("Registering " + p.GetName() + "." + indicatorSetting.IndicatorName +
                            " at: " + indicatorSetting.UpdateInterval.Ticks/10000000 + " seconds");
                        TimeSpan updateInterval = new TimeSpan(indicatorSetting.UpdateInterval.Ticks);
                        Jobs.Add(new IndicatorTimerJob(p, indicatorSetting.IndicatorName, updateInterval));
                    }
                }

                // Add the main update scheduler
                Jobs.Add(new MainUpdateTimerJob());

                // Start all timerjobs
                foreach (TimerJobBase job in Jobs)
                {
                    job.Start();
                }
            }
            catch (Exception e)
            {
                WorkstationLogger.WriteLog("Error: " + e.Message);
            }
        }

        /// <summary>
        /// Refreshes the sheduler if there are new plugins available.
        /// </summary>
        public override void RefreshJobs()
        {
            // Start the schedulers for all indicators
            foreach (IPlugin p in PluginManager.Instance.GetLoadedPlugins())
            {
                foreach (IndicatorSettings indicatorSetting in p.GetIndicatorSettings())
                {
                    TimeSpan updateInterval = new TimeSpan(indicatorSetting.UpdateInterval.Ticks);
                    TimerJobBase job = new IndicatorTimerJob(p, indicatorSetting.IndicatorName, updateInterval);
                    Jobs.Add(job);
                    job.Start();
                }
            }
        }


        /// <summary>
        /// Stop all jobs of a plugin
        /// </summary>
        /// <param name="plugin"></param>
        public void StopPluginScheduling(Core.IPlugin plugin)
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
                job.Stop();
                job.Dispose();
                Jobs.Remove(job);
            }
        }

        #endregion
    }
}

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
using MISD.Workstation.Linux.Scheduling;
using MISD.Workstation.Linux.Plugins;
using MISD.Core;
using System.Threading;

namespace MISD.Workstation.Linux.Scheduling.TimerJobs
{
    /// <summary>
    /// The timerjob for the main update interval.
    /// </summary>
    public class MainUpdateTimerJob : TimerJobBase
    {
        #region Constructor
		
        /// <summary>
        /// Initializes the timerjob for the main update interval.
        /// </summary>
        public MainUpdateTimerJob()
        {
            this.ID = "MainUpdateTimerJob";
			this.Interval = ServerConnection.Instance.GetMainUpdateInterval(new TimeSpan(1, 0, 0));
        }

        #endregion

        #region Methods
        protected override void Loop()
        {
            Random random = new Random();
            try
            {
                Thread.Sleep(random.Next(1, 20) * 1000);

                var start = DateTime.Now;

                while (this.IsStarted)
                {
                    var sleepTime = this.Interval - (DateTime.Now - start);
                    if (sleepTime < TimeSpan.FromSeconds(1))
                    {
                        sleepTime = TimeSpan.FromSeconds(1);
                    }

                    Thread.Sleep((int)sleepTime.TotalMilliseconds);

                    start = DateTime.Now;

                    this.TimerTickAsync();
                }
            }
            catch (ThreadAbortException)
            {
                // Return
            }
        }

        /// <summary>
        /// The method that gets the new update interval and start the refresh for all timerjobs.
        /// </summary>
        protected override void TimerTickAsync()
        {
			WorkstationLogger.Instance.WriteLog ("Updating Main Updatge Timer JOB", LogType.Debug, false);
            // Update its own interval
			this.Interval = ServerConnection.Instance.GetMainUpdateInterval(this.Interval);

			WorkstationLogger.Instance.WriteLog ("Updated MainUpdateInterval to "+this.Interval, LogType.Debug, false);
            
			// Update plugins
            if (PluginManager.Instance.UpdatePlugins().Count > 0)
            {
				WorkstationLogger.Instance.WriteLog ("Refreshing Scheduler", LogType.Debug, false);

				// PluginManager.Instance.UpdatePlugins() deletes all timerjobs, so the timerjobs have to be reinitialized
                Scheduler.Instance.RefreshJobs();
            }
            else
            {
				WorkstationLogger.Instance.WriteLog ("Refresh Update Intervall", LogType.Debug, false);
                // If no new plugins are loaded, the intervals need to be refreshed
                RefreshUpdateIntervals();
            }
        }

        /// <summary>
        /// Refreshs the update intervals for all indicators.
        /// </summary>
        private void RefreshUpdateIntervals()
        {
            Dictionary<string, TimeSpan> intervals = new Dictionary<string, TimeSpan>();
            List<Core.IPlugin> plugins = PluginManager.Instance.LoadAvailablePlugins();
            List<Tuple<string, string, long>> tmpUpdateIntervals = GetAllUpdateIntervals(plugins);
            string ID;
            TimeSpan currentInterval;
            foreach (Tuple<string, string, long> current in tmpUpdateIntervals)
            {
				ID = current.Item1 + "." + current.Item2;
	            currentInterval = TimeSpan.FromTicks((long)current.Item3);
				if (intervals.ContainsKey(ID))
				{
					intervals.Remove(ID);
				}
				intervals.Add(ID, currentInterval);
				
            }
			

            Scheduler.Instance.RefreshUpdateInterval(intervals);
        }

        /// <summary>
        /// Gets all update intervals for a list of plugins.
        /// </summary>
        /// <param name="plugins">A list containing plugins.</param>
        /// <returns>A list containing PluginName | IndicatorName | IndicatorUpdateInterval</returns>
        private List<Tuple<string, string, long>> GetAllUpdateIntervals(List<Core.IPlugin> plugins)
        {
            List<Tuple<string, string, long>> allUpdateIntervals = new List<Tuple<string, string, long>>();
            foreach (Core.IPlugin currentPlugin in plugins)
            {
				WorkstationLogger.Instance.WriteLog ("GetAllUpdateIntervalls: "+currentPlugin.GetName(), LogType.Debug, false);
				
				foreach(IndicatorSettings indicator in currentPlugin.GetIndicatorSettings()){
					
					long timespan = ServerConnection.Instance.GetUpdateInterval(currentPlugin.GetName(), indicator.IndicatorName, indicator.UpdateInterval.Ticks
					);
					WorkstationLogger.Instance.WriteLog ("GetAllUpdateIntervalls: "+currentPlugin.GetName()+ "|"+indicator.IndicatorName+ "|"+timespan, LogType.Debug, false);

					allUpdateIntervals.Add(new Tuple<string, string, long>
                        (currentPlugin.GetName(), indicator.IndicatorName, timespan));
				}
            }
            return allUpdateIntervals;
        }

        #endregion
    }
}

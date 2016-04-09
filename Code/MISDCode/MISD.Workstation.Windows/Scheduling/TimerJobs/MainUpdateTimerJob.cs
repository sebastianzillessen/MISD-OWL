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
using MISD.Workstation.Windows.Scheduling;
using MISD.Workstation.Windows.Plugins;
using MISD.Core;
using System.Threading;

namespace MISD.Workstation.Windows.Scheduling.TimerJobs
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
            TimeSpan oldInterval = this.Interval;
            this.Interval = ServerConnection.GetMainUpdateInterval(oldInterval);
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
            WorkstationLogger.WriteLog("Main update interval expired.");
            // Update its own interval
            TimeSpan oldInterval = this.Interval;
            this.Interval = ServerConnection.GetMainUpdateInterval(oldInterval);

            // Update plugins

            if (PluginManager.Instance.UpdatePlugins().Count > 0)
            {
                // PluginManager.Instance.UpdatePlugins() deletes all timerjobs, so the timerjobs have to be reinitialized
                Scheduler.Instance.RefreshJobs();
                RefreshUpdateIntervals();
            }
            else
            {
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

            List<string> pluginNames = new List<string>();
            List<Core.IPlugin> plugins = PluginManager.Instance.LoadAvailablePlugins();
            foreach (Core.IPlugin current in plugins)
            {
                pluginNames.Add(current.GetName());
            }

            List<Tuple<string, string, long?>> tmpUpdateIntervals = GetAllUpdateIntervals(pluginNames);
            string ID;
            TimeSpan currentInterval;
            foreach (Tuple<string, string, long?> current in tmpUpdateIntervals)
            {
                WorkstationLogger.WriteLog("New intervals: " + current.Item1 + " " +
                    current.Item2 + " " + current.Item3/10000000 + " seconds");
                ID = current.Item1 + "." + current.Item2;
                currentInterval = TimeSpan.FromTicks((long)current.Item3);
                intervals.Add(ID, currentInterval);
            }

            Scheduler.Instance.RefreshUpdateInterval(intervals);
        }

        /// <summary>
        /// Gets all update intervals for a list of plugins.
        /// </summary>
        /// <param name="plugins">A list containing plugins.</param>
        /// <returns>A list containing PluginName | IndicatorName | IndicatorUpdateInterval</returns>
        private List<Tuple<string, string, long?>> GetAllUpdateIntervals(List<string> plugins)
        {
            List<Tuple<string, string, long?>> allUpdateIntervals = new List<Tuple<string, string, long?>>();
            List<Tuple<string, long?>> indicatorUpdateIntervals = new List<Tuple<string, long?>>();
            foreach (string currentPlugin in plugins)
            {
                // collect old update intervals (are used in case of WebService failure)
                List<Tuple<string, long?>> oldIndicatorIntervals = new List<Tuple<string, long?>>();
                foreach (IPlugin p in PluginManager.Instance.GetLoadedPlugins())
                {
                    // pick current plugin
                    if (p.GetName().Equals(currentPlugin))
                    {
                        // list all indicators with old update intervals 
                        foreach (IndicatorSettings s in p.GetIndicatorSettings())
                        {
                            oldIndicatorIntervals.Add(new Tuple<string, long?>(s.IndicatorName, s.UpdateInterval.Ticks));
                        }
                    }
                }
                indicatorUpdateIntervals = ServerConnection.GetUpdateIntervals(currentPlugin, oldIndicatorIntervals.ToArray()).ToList();
                foreach (Tuple<string, long?> currentInterval in indicatorUpdateIntervals)
                {
                    allUpdateIntervals.Add(new Tuple<string, string, long?>
                        (currentPlugin, currentInterval.Item1, currentInterval.Item2));
                }
            }
            return allUpdateIntervals;
        }

        #endregion
    }
}

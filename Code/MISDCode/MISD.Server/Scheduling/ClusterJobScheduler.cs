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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Timers;
using MISD.Core;
using MISD.Server.Manager;

namespace MISD.Server.Scheduling
{
    /// <summary>
    /// Sheduling class for the server-sided cluster data acquisition.
    /// </summary>
    class ClusterJobScheduler
    {
        #region Singleton

        private static volatile ClusterJobScheduler instance;
        private static object syncRoot = new Object();

        /// <summary>
        /// Property to access the class.
        /// </summary>
        public static ClusterJobScheduler Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ClusterJobScheduler();
                    }
                }

                return instance;
            }
        }

        #endregion

        #region Fields

        // the timer
        private System.Timers.Timer timer;
        private int tickNumber = 0;

        // the managed systems and indicators
        private List<Tuple<int, WorkstationInfo, IPlugin, string, ClusterConnection, Platform, int>> jobs;

        // callback
        WaitCallback callback;

        #endregion

        #region Constructor

        private ClusterJobScheduler()
        {
            jobs = new List<Tuple<int, WorkstationInfo, IPlugin, string, ClusterConnection, Platform, int>>();

            timer = new System.Timers.Timer();
            // tick handler
            timer.Elapsed += timer_Elapsed;
            // tick each second
            timer.Interval = 1000;

            // set thread pool sizes
            ThreadPool.SetMaxThreads(256, 256);

            // set callback
            callback = new WaitCallback(UpdateValues);
        }

        #endregion

        #region Tick

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // increase tick
            tickNumber += 1;

            // reset to avoid overlfow
            if (tickNumber > int.MaxValue / 2)
            {
                tickNumber = 0;
            }

            // check which jobs need to run
            foreach (var job in jobs)
            {
                if ((tickNumber - job.Item7) > 0 && (tickNumber + 1 - job.Item7) % job.Item1 == 1)
                {
                    ThreadPool.QueueUserWorkItem(callback, job);
                }
            }
        }

        #endregion

        #region Methods

        public void Start()
        {
            // start the timer
            timer.Start();
        }

        public void Stop()
        {
            // stop the timer
            timer.Stop();
        }

        #region Manage Values

        public void UpdateValues(Object job)
        {
            Tuple<int, WorkstationInfo, IPlugin, string, ClusterConnection, Platform, int> currentJob = (Tuple<int, WorkstationInfo, IPlugin, string, ClusterConnection, Platform, int>)job;
            UpdateValues(currentJob.Item2, currentJob.Item3, currentJob.Item4, currentJob.Item5, currentJob.Item6);
        }

        /// <summary>
        /// The method that aquires, filters and sends the indicator values.
        /// </summary>
        public void UpdateValues(WorkstationInfo node, IPlugin plugin, string indicator, ClusterConnection connection, Platform platform)
        {
            try
            {
                if (node.IsAvailable && node.CurrentOS == platform.ToString())
                {
                    // Aquire data
                    var values = GetValues(node, plugin, indicator, connection);

                    if (values != null && values.Count > 0 && values.First().Item2 != null)
                    {
                        // Filter values
                        var filteredValues = FilterValues(node, plugin, indicator, values);

                        // Send values
                        if (filteredValues != null && filteredValues.Count > 0)
                        {
                            var valuesToSend = (from p in filteredValues
                                                select new Tuple<string, Object, MISD.Core.DataType, DateTime>(indicator, p.Item2, p.Item3, DateTime.Now)).ToList();

                            WorkstationManager.Instance.UploadIndicatorValues(node.ID, plugin.GetName(), valuesToSend);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MISD.Core.Logger.Instance.WriteEntry("ClusterJobScheduler_UpdateValues: Problem with node " + node.Name + " and plugin " + plugin.GetName() + " and indicator " + indicator + ": " + e.ToString(), LogType.Exception);
            }
        }

        /// <summary>
        /// Method to aquire data with the plugin.
        /// </summary>
        /// <returns>A list containing IndicatorName | IndicatorValue | IndicatorValueDataType.</returns>
        private List<Tuple<string, object, DataType>> GetValues(WorkstationInfo node, IPlugin plugin, string indicator, ClusterConnection connection)
        {
            string name = node.FQDN.Split('.').FirstOrDefault();

            if (name != null)
            {
                var temp = plugin.AcquireData(new List<String> { indicator }, name, connection);

                if (temp == null)
                {
                    temp = new List<Tuple<string, object, DataType>>();
                }

                return temp;
            }

            return null;
        }

        /// <summary>
        /// Method to filter the values.
        /// </summary>
        /// <param name="unfilteredValues">The list of all values.</param>
        /// <returns>A list containing IndicatorName | IndicatorValue | IndicatorValueDataType.</returns>
        private List<Tuple<string, object, MISD.Core.DataType>> FilterValues(WorkstationInfo node, IPlugin plugin, string indicator, List<Tuple<string, object, MISD.Core.DataType>> unfilteredValues)
        {
            List<Tuple<string, object, MISD.Core.DataType>> result;

            var filteredValues = (from p in unfilteredValues
                                  where FilterManager.Instance.GetFilterValue(node.ID, plugin.GetName(), indicator, p.Item1)
                                  select p);

            if (filteredValues == null)
            {
                result = new List<Tuple<string, object, MISD.Core.DataType>>();
            }
            else
            {
                result = filteredValues.ToList();
            }

            return result;
        }

        #endregion

        #region Manage Jobs

        /// <summary>
        /// Adds the job for a monitored system in combination with a plugin, indicator and update intervals.
        /// </summary>
        /// <param name="interval">The update interval in seconds</param>
        /// <param name="node">The monitored system</param>
        /// <param name="plugin">The plugin</param>
        /// <param name="indicator">The indicator</param>
        /// <param name="connection">The cluster connection</param>
        /// <param name="platform">The platform</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddJob(int interval, WorkstationInfo node, IPlugin plugin, string indicator, ClusterConnection connection, Platform platform)
        {
            if (interval >= 1 && node != null && plugin != null && indicator != "" && connection != null)
            {
                int offset = new Random().Next(1, 30);
                jobs.Add(new Tuple<int, WorkstationInfo, IPlugin, string, ClusterConnection, Platform, int>(interval, node, plugin, indicator, connection, platform, offset));
            }
        }

        /// <summary>
        /// Updates a job for a monitored system.
        /// </summary>
        /// <param name="newInterval">The new update interval in seconds</param>
        /// <param name="nodeID">The ID of the monitored system</param>
        /// <param name="pluginName">The name of the plugin</param>
        /// <param name="indicatorName">The name of the indicator</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void UpdateJob(int newInterval, int nodeID, string pluginName, string indicatorName)
        {
            if (newInterval >= 1)
            {
                Tuple<int, WorkstationInfo, IPlugin, string, ClusterConnection, Platform, int> oldJob = null;
                foreach (var job in jobs)
                {
                    if (job.Item2.ID.Equals(nodeID) && job.Item3.GetName().Equals(pluginName) && job.Item4.Equals(indicatorName))
                    {
                        oldJob = job;
                    }
                }

                if (oldJob != null)
                {
                    AddJob(newInterval, oldJob.Item2, oldJob.Item3, oldJob.Item4, oldJob.Item5, oldJob.Item6);
                    RemoveJob(oldJob.Item2, oldJob.Item3, oldJob.Item4);
                }
            }
        }

        /// <summary>
        /// Removes a job for a monitored system.
        /// </summary>
        /// <param name="node">The monitored system</param>
        /// <param name="plugin">The plugin</param>
        /// <param name="indicator">The indicator</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void RemoveJob(WorkstationInfo node, IPlugin plugin, string indicator)
        {
            List<Tuple<int, WorkstationInfo, IPlugin, string, ClusterConnection, Platform, int>> toBeRemoved = new List<Tuple<int, WorkstationInfo, IPlugin, string, ClusterConnection, Platform, int>>();

            foreach (var job in jobs)
            {
                if (job.Item2.ID.Equals(node.ID) && job.Item3.Equals(plugin) && job.Item4.Equals(indicator))
                {
                    toBeRemoved.Add(job);
                }
            }

            foreach (var job in toBeRemoved)
            {
                jobs.Remove(job);
            }

            toBeRemoved.Clear();
        }

        /// <summary>
        /// Removes all jobs for a monitored system.
        /// </summary>
        /// <param name="node">The monitored system</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void RemoveAllJobsForNode(WorkstationInfo node)
        {
            List<Tuple<int, WorkstationInfo, IPlugin, string, ClusterConnection, Platform, int>> toBeRemoved = new List<Tuple<int, WorkstationInfo, IPlugin, string, ClusterConnection, Platform, int>>();

            foreach (var job in jobs)
            {
                if (job.Item2.ID.Equals(node.ID))
                {
                    toBeRemoved.Add(job);
                }
            }

            foreach (var job in toBeRemoved)
            {
                jobs.Remove(job);
            }

            toBeRemoved.Clear();
        }

        /// <summary>
        /// Removes all jobs for a plugin.
        /// </summary>
        /// <param name="plugin">The plugin</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void RemoveAllJobsForPlugin(IPlugin plugin)
        {
            List<Tuple<int, WorkstationInfo, IPlugin, string, ClusterConnection, Platform, int>> toBeRemoved = new List<Tuple<int, WorkstationInfo, IPlugin, string, ClusterConnection, Platform, int>>();

            foreach (var job in jobs)
            {
                if (job.Item3.Equals(plugin))
                {
                    toBeRemoved.Add(job);
                }
            }

            foreach (var job in toBeRemoved)
            {
                jobs.Remove(job);
            }

            toBeRemoved.Clear();
        }

        #endregion

        #endregion
    }
}

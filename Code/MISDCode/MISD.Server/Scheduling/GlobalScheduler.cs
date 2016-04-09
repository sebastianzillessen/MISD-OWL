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

using MISD.Core;
using MISD.Server.Scheduling;
using MISD.Server.Manager;
using MISD.Core.Scheduling;
using System.Reflection;

namespace MISD.Server.Scheduling
{
    /// <summary>
    /// A management class for the server-sided global data acquisition.
    /// </summary>
    public class GlobalScheduler : SchedulerBase
    {
        #region Singleton

        private static volatile GlobalScheduler instance;
        private static object syncRoot = new Object();

        /// <summary>
        /// Property to access the class.
        /// </summary>
        public static GlobalScheduler Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new GlobalScheduler();
                    }
                }

                return instance;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// The plugins.
        /// </summary>
        protected List<IPlugin> serverPlugins;

        /// <summary>
        /// The monitored systems.
        /// </summary>
        protected List<WorkstationInfo> monitoredSystems;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the Scheduler class.
        /// </summary>
        private GlobalScheduler()
        {
            PluginManager.Instance.ServerPluginsChanged += new EventHandler(Instance_ServerPluginsChanged);
        }

        #endregion

        #region Methods

        void Instance_ServerPluginsChanged(object sender, EventArgs e)
        {
            this.RefreshPlugins();
        }

        /// <summary>
        /// Initializes the scheduler.
        /// </summary>
        protected override void Initialize()
        {
            using (var dataContext = MISD.Server.Database.DataContextFactory.CreateReadOnlyDataContext())
            {
                serverPlugins = new List<IPlugin>();
                monitoredSystems = new List<WorkstationInfo>();

                if (PluginManager.Instance.ServerPlugins != null)
                {
                    serverPlugins = PluginManager.Instance.ServerPlugins.ToList();

                    monitoredSystems = (from p in dataContext.MonitoredSystem
                                        where p.IsAvailable == true
                                        select new WorkstationInfo()
                                        {
                                            ID = p.ID,
                                            Name = p.Name,
                                            // State is initialized with OK
                                            State = MISD.Core.MappingState.OK,
                                            FQDN = p.FQDN,
                                            IsAvailable = p.IsAvailable,
                                            CurrentOS = ((Platform)p.OperatingSystem).ToString(),
                                            MacAddress = p.MacAddress
                                        }).ToList();

                    this.CreateTimerJobs(serverPlugins, monitoredSystems);
                }
            }
        }

        /// <summary>
        /// Starts all timerjobs.
        /// </summary>
        public void StartJobs()
        {
            // old approach
            //foreach (var current in timerJobs)
            //{
            //    current.Start();
            //}

            // new approach
            GlobalJobScheduler.Instance.Start();
        }

        /// <summary>
        /// Stops all timerjobs.
        /// </summary>
        public void StopJobs()
        {
            // old approach
            //foreach (var current in timerJobs)
            //{
            //    current.Stop();
            //}

            // new approach
            GlobalJobScheduler.Instance.Stop();
        }

        /// <summary>
        /// Refreshes the scheduler if there are new plugins are available.
        /// </summary>
        public void RefreshPlugins()
        {
            #region Initialize Lists

            List<IPlugin> currentPlugins = PluginManager.Instance.ServerPlugins.ToList();
            List<IPlugin> oldPlugins = new List<IPlugin>(serverPlugins);
            List<IPlugin> doublePluginsCurrent = new List<IPlugin>();
            List<IPlugin> doublePluginsOld = new List<IPlugin>();

            #endregion

            #region Find diff

            foreach (IPlugin current in currentPlugins)
            {
                foreach (IPlugin old in oldPlugins)
                {
                    if ((current.GetName() == old.GetName()) && (current.GetVersion() == old.GetVersion()))
                    {
                        doublePluginsCurrent.Add(current);
                        doublePluginsOld.Add(old);
                    }
                }
            }

            foreach (IPlugin current in doublePluginsCurrent)
            {
                currentPlugins.Remove(current);
            }
            foreach (IPlugin current in doublePluginsOld)
            {
                oldPlugins.Remove(current);
            }

            #endregion

            #region Add new timerjobs and delete old timerjobs

            List<IPlugin> newPlugins = new List<IPlugin>();
            foreach (IPlugin current in currentPlugins)
            {
                newPlugins.Add(current);
            }

            if (newPlugins.Count > 0)
            {
                CreateTimerJobs(newPlugins, monitoredSystems);
            }

            // old approach
            //List<TimerJobBase> toBeRemoved = new List<TimerJobBase>();
            //foreach (IPlugin old in oldPlugins)
            //{
            //    foreach (TimerJobBase currentJob in Jobs)
            //    {
            //        if (currentJob.ID.Contains(old.GetName()))
            //        {
            //            toBeRemoved.Add(currentJob);
            //        }
            //    }
            //}
            //foreach (TimerJobBase current in toBeRemoved)
            //{
            //    Jobs.Remove(current);
            //    current.Stop();
            //    current.Dispose();
            //}

            // new approach
            foreach (IPlugin old in oldPlugins)
            {
                GlobalJobScheduler.Instance.RemoveAllJobsForPlugin(old);
            }

            #endregion

            StartJobs();
        }

        /// <summary>
        /// Refreshes the update intervals.
        /// </summary>
        /// <param name="monitoredSystemID">The ID of the monitored system to be refreshed.</param>
        /// <param name="plugin">The plugin to be refreshed.</param>
        /// <param name="newIntervals">A list of Tuples (IndicatorName | UpdateInterval) with the new intervals.</param>
        public void RefreshIntervals(int monitoredSystemID, string plugin, List<Tuple<string, TimeSpan>> newIntervals)
        {
            // old approach
            //foreach (TimerJobBase current in Jobs)
            //{
            //    foreach (Tuple<string, TimeSpan> currentInterval in newIntervals)
            //    {
            //        if (current.ID.Equals(monitoredSystemID + "." + plugin + "." + currentInterval.Item1))
            //        {
            //            current.Interval = currentInterval.Item2;
            //        }
            //    }
            //}

            //new apporach
            try
            {
                foreach (var interval in newIntervals)
                {
                    GlobalJobScheduler.Instance.UpdateJob((int)Math.Round(interval.Item2.TotalSeconds), monitoredSystemID, plugin, interval.Item1);
                }
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("GlobalScheduler_RefreshIntervals: Problem refreshing update intervals for monitored system with ID " + monitoredSystemID + " and plugin " + plugin + ", Exception: " + e.ToString(), LogType.Exception);
            }
        }

        /// <summary>
        /// Refreshes the scheduler if there are new or removed systems.
        /// </summary>
        public override void RefreshJobs()
        {
            using (var dataContext = MISD.Server.Database.DataContextFactory.CreateReadOnlyDataContext())
            {
                #region Initialize Lists

                List<WorkstationInfo> currentSystems = (from sys in dataContext.MonitoredSystem
                                      where sys.IsAvailable == true
                                      select new Core.WorkstationInfo()
                                      {
                                          ID = sys.ID,
                                          Name = sys.Name,
                                          State = MISD.Core.MappingState.OK,
                                          FQDN = sys.FQDN,
                                          IsAvailable = sys.IsAvailable,
                                          CurrentOS = ((Core.Platform)sys.OperatingSystem).ToString(),
                                          MacAddress = sys.MacAddress
                                      }).ToList();

                List<WorkstationInfo> oldSystems = new List<WorkstationInfo>(monitoredSystems);
                List<WorkstationInfo> doubleSystemsCurrent = new List<WorkstationInfo>();
                List<WorkstationInfo> doubleSystemsOld = new List<WorkstationInfo>();

                #endregion

                #region Find diff

                foreach (WorkstationInfo current in currentSystems)
                {
                    foreach (WorkstationInfo old in oldSystems)
                    {
                        if (current.MacAddress == old.MacAddress)
                        {
                            doubleSystemsCurrent.Add(current);
                            doubleSystemsOld.Add(old);
                        }
                    }
                }

                foreach (WorkstationInfo current in doubleSystemsCurrent)
                {
                    currentSystems.Remove(current);
                }
                foreach (WorkstationInfo current in doubleSystemsOld)
                {
                    oldSystems.Remove(current);
                }

                #endregion

                #region Add new timerjobs and delete old timerjobs

                List<WorkstationInfo> newSystems = new List<WorkstationInfo>();
                foreach (WorkstationInfo current in currentSystems)
                {
                    newSystems.Add(current);
                }

                if (newSystems.Count > 0)
                {
                    CreateTimerJobs(serverPlugins, newSystems);
                }

                // old approach
                //List<TimerJobBase> toBeRemoved = new List<TimerJobBase>();
                //foreach (WorkstationInfo old in oldSystems)
                //{
                //    foreach (TimerJobBase currentJob in Jobs)
                //    {
                //        if (currentJob.ID.Contains(old.MacAddress + "."))
                //        {
                //            toBeRemoved.Add(currentJob);
                //        }
                //    }
                //}

                //foreach (TimerJobBase current in toBeRemoved)
                //{
                //    Jobs.Remove(current);
                //    current.Stop();
                //    current.Dispose();
                //}

                // new approach
                foreach (WorkstationInfo old in oldSystems)
                {
                    GlobalJobScheduler.Instance.RemoveAllJobsForNode(old);
                }

                #endregion

                StartJobs();
            }
        }

        /// <summary>
        /// Creates timerjobs for several plugins in combination with several systems.
        /// </summary>
        /// <param name="serverPlugins">The plugins.</param>
        /// <param name="systems">The systems.</param>
        private void CreateTimerJobs(List<IPlugin> plugins, List<WorkstationInfo> systems)
        {
            try
            {
                if (plugins != null && plugins.Count > 0 && systems != null && systems.Count > 0)
                {
                    foreach (WorkstationInfo current in systems)
                    {
                        foreach (IPlugin currentPlugin in plugins)
                        {
                            foreach (IndicatorSettings currentIndicator in currentPlugin.GetIndicatorSettings())
                            {
                                TimeSpan updateInterval = new TimeSpan(UpdateIntervalManager.Instance.GetUpdateInterval(current.ID, currentPlugin.GetName(), currentIndicator.IndicatorName));
                                // Old approach
                                //Jobs.Add(new GlobalTimerJob(current, currentPlugin, currentIndicator.IndicatorName, updateInterval));
                                // New approach
                                GlobalJobScheduler.Instance.AddJob((int)Math.Round(updateInterval.TotalSeconds), current, currentPlugin, currentIndicator.IndicatorName);
                            }
                        }
                    }
                    GlobalJobScheduler.Instance.Start();
                }
            }
            catch (Exception e)
            {
                MISD.Core.Logger.Instance.WriteEntry("GlobalScheduler_CreateTimerJobs: Problem adding timerjobs, " + e.ToString(), LogType.Exception);
            }
        }

        #endregion
    }
}

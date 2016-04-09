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
using MISD.Core.Scheduling;
using MISD.Server.Cluster;
using MISD.Server.Database;
using MISD.Server.Scheduling;

namespace MISD.Server.Manager
{
    /// <summary>
    /// A management class for a single cluster.
    /// </summary>
    public class ClusterManager
    {
        #region Properties

        /// <summary>
        /// Gets or sets the timer jobs.
        /// </summary>
        protected List<TimerJobBase> timerJobs;

        /// <summary>
        /// The nodes of the cluster.
        /// </summary>
        protected List<WorkstationInfo> clusterNodes;

        /// <summary>
        /// The plugins.
        /// </summary>
        protected List<IPlugin> clusterPlugins;

        /// <summary>
        /// The type of the cluster.
        /// </summary>
        public Platform clusterType;

        /// <summary>
        /// The cluster connection.
        /// </summary>
        protected ClusterConnection clusterConnection;

        /// <summary>
        /// The cluster connection.
        /// </summary>
        public int ID;

        /// <summary>
        /// The cluster OU ID.
        /// </summary>
        protected int OUID;

        #endregion

        #region Constructors

        public ClusterManager()
        {
        }

        /// <summary>
        /// Initializes a cluster manager and starts the acquisition for all nodes.
        /// </summary>
        /// <param name="url">The url of the head-node.</param>
        /// <param name="username">The username to login.</param>
        /// <param name="password">The password to login.</param>
        /// <param name="platform">The platform of the cluster.</param>
        /// <param name="id">An ID for the cluster, to be able to have multiple clusters of one platform.</param>
        public void Initialize(object cred)
        {
            try
            {
                var credentials = (ClusterCredential)cred;

                timerJobs = new List<TimerJobBase>();
                clusterType = (Platform)credentials.Platform;
                ID = credentials.ID;

                if (!OUManager.Instance.Exists(clusterType.ToString() + ID))
                {
                    OUManager.Instance.CreateOU(clusterType.ToString() + ID, null, DateTime.Now);
                }
                using (var datacontext = DataContextFactory.CreateReadOnlyDataContext())
                {
                    var OU = (from p in datacontext.OrganizationalUnit
                              where p.FQDN.Equals(clusterType.ToString() + ID)
                              select p).First();
                    this.OUID = OU.ID;
                }

                switch (clusterType)
                {
                    case Platform.Bright:
                        clusterConnection = new BrightClusterConnection();
                        clusterConnection.Init(credentials.HeadNodeUrl, credentials.Username, credentials.Password);
                        clusterNodes = clusterConnection.GetNodes();

                        if (PluginManager.Instance.BrightPlugins != null)
                        {
                            clusterPlugins = PluginManager.Instance.BrightPlugins.ToList();
                            this.AddNodes(clusterNodes);
                        }
                        break;
                    case Platform.HPC:
                        clusterConnection = new HpcClusterConnection();
                        clusterConnection.Init(credentials.HeadNodeUrl, credentials.Username, credentials.Password);
                        clusterNodes = clusterConnection.GetNodes();
                        if (PluginManager.Instance.HPCPlugins != null)
                        {
                            clusterPlugins = PluginManager.Instance.HPCPlugins.ToList();
                            this.AddNodes(clusterNodes);
                        }
                        break;
                    default:
                        // logging unsupported platform
                        var messageEx = new StringBuilder();
                        messageEx.Append("ClusterManager_AddNodes: ");
                        messageEx.Append("The platform " + clusterType.ToString());
                        messageEx.Append(" is not supported.");
                        MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);
                        break;
                }

                // Register this cluster at the metacluster manager
                MetaClusterManager.Instance.AddCluster(this);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("ClusterManager_Initialize: Problem initilizing cluster, " + e.ToString(), LogType.Exception);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts all timerjobs.
        /// </summary>
        public void StartJobs()
        {
            //old approach
            //foreach (var current in timerJobs)
            //{
            //    current.Start();
            //}

            // new approach
            ClusterJobScheduler.Instance.Start();
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
            ClusterJobScheduler.Instance.Stop();
        }

        /// <summary>
        /// Removes all timerjobs.
        /// </summary>
        public void RemoveJobs()
        {
            // old approach
            foreach (var current in timerJobs)
            {
                current.Stop();
                current.Dispose();
            }
            timerJobs.Clear();
        }

        /// <summary>
        /// Refreshes the nodes, deletes old nodes and adds new nodes with the corresponding timer jobs.
        /// </summary>
        public void RefreshNodes()
        {
            #region Initialize Lists

            List<WorkstationInfo> currentNodes = clusterConnection.GetNodes();
            List<WorkstationInfo> oldNodes = new List<WorkstationInfo>(clusterNodes);
            List<WorkstationInfo> doubleNodesCurrent = new List<WorkstationInfo>();
            List<WorkstationInfo> doubleNodesOld = new List<WorkstationInfo>();

            #endregion

            #region Find diff

            foreach (WorkstationInfo current in currentNodes)
            {
                foreach (WorkstationInfo old in oldNodes)
                {
                    if (current.MacAddress == old.MacAddress)
                    {
                        doubleNodesCurrent.Add(current);
                        doubleNodesOld.Add(old);
                    }
                }
            }

            foreach (WorkstationInfo current in doubleNodesCurrent)
            {
                currentNodes.Remove(current);
            }
            foreach (WorkstationInfo current in doubleNodesOld)
            {
                oldNodes.Remove(current);
            }

            #endregion

            #region Add new timerjobs and delete old timerjobs

            List<WorkstationInfo> newNodes = new List<WorkstationInfo>();
            foreach (WorkstationInfo current in currentNodes)
            {
                newNodes.Add(current);
            }

            if (newNodes.Count > 0)
            {
                AddNodes(newNodes);
            }

            // old approach
            //List<TimerJobBase> toBeDeleted = new List<TimerJobBase>();
            //foreach (WorkstationInfo old in oldNodes)
            //{
            //    var timerjob = (from node in timerJobs
            //                    where node.ID.Contains(old.MacAddress + ".")
            //                    select node).First();
            //    toBeDeleted.Add(timerjob);
            //}

            // old approach
            //foreach (TimerJobBase current in toBeDeleted)
            //{
            //    timerJobs.Remove(current);
            //    current.Stop();
            //    current.Dispose();
            //}

            // new approach
            foreach (WorkstationInfo old in oldNodes)
            {
                ClusterJobScheduler.Instance.RemoveAllJobsForNode(old);
            }

            #endregion

            StartJobs();
        }

        /// <summary>
        /// Refreshes the timerjobs for new update intervals.
        /// </summary>
        /// <param name="monitoredSystemID">The ID of the monitored system to be refreshed.</param>
        /// <param name="plugin">The plugins with new update intervals.</param>
        /// <param name="newIntervals">A list of Tuples (IndicatorName | UpdateInterval) with the new intervals.</param>
        public void RefreshUpdateIntervals(int monitoredSystemID, string plugin, List<Tuple<string, TimeSpan>> newIntervals)
        {
            // old approach
            //foreach (TimerJobBase current in timerJobs)
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
                    ClusterJobScheduler.Instance.UpdateJob((int)Math.Round(interval.Item2.TotalSeconds), monitoredSystemID, plugin, interval.Item1);
                }
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("Problem refreshing update intervals for monitored system with ID " + monitoredSystemID + " and plugin " + plugin + ", Exception: " + e.ToString(), LogType.Exception);
            }
        }

        /// <summary>
        /// Refreshes the timerjobs for new or removed plugins.
        /// </summary>
        public void RefreshPlugins()
        {
            #region Initialize Lists

            List<IPlugin> currentPlugins = new List<IPlugin>();
            List<IPlugin> oldPlugins = new List<IPlugin>(clusterPlugins);
            List<IPlugin> doublePluginsCurrent = new List<IPlugin>();
            List<IPlugin> doublePluginsOld = new List<IPlugin>();

            switch (clusterType)
            {
                case MISD.Core.Platform.Bright:
                    if (PluginManager.Instance.BrightPlugins != null)
                    {
                        currentPlugins = PluginManager.Instance.BrightPlugins.ToList();
                        clusterPlugins = currentPlugins;
                    }
                    break;
                case MISD.Core.Platform.HPC:
                    if (PluginManager.Instance.HPCPlugins != null)
                    {
                        currentPlugins = PluginManager.Instance.HPCPlugins.ToList();
                        clusterPlugins = currentPlugins;
                    }
                    break;
                default:
                    // logging unsupported plattform
                    var messageEx = new StringBuilder();
                    messageEx.Append("ClusterManager_RefreshPlugins: ");
                    messageEx.Append("The plattform" + clusterType.ToString() + " ");
                    messageEx.Append("is not supported.");
                    MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);
                    break;
            }

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
                CreateTimerJobs(clusterNodes, newPlugins, this.clusterConnection);
            }

            // old approach
            //List<TimerJobBase> toBeRemoved = new List<TimerJobBase>();
            //foreach (IPlugin old in oldPlugins)
            //{
            //    foreach (TimerJobBase currentJob in timerJobs)
            //    {
            //        if (currentJob.ID.Contains(old.GetName()))
            //        {
            //            toBeRemoved.Add(currentJob);
            //        }
            //    }
            //}

            //foreach (TimerJobBase current in toBeRemoved)
            //{
            //    timerJobs.Remove(current);
            //    current.Stop();
            //    current.Dispose();
            //}

            // new approach
            foreach (IPlugin old in oldPlugins)
            {
                ClusterJobScheduler.Instance.RemoveAllJobsForPlugin(old);
            }

            #endregion

            StartJobs();
        }

        /// <summary>
        /// Registers a new node in the database.
        /// </summary>
        /// <param name="node">The node to be registered.</param>
        /// <param name="OUName">The name of the OU for the node.</param>
        private void RegisterNode(WorkstationInfo node, string OUName)
        {
            using (var dataContext = DataContextFactory.CreateDataContext())
            {
                Database.MonitoredSystem monitoredSystem = null;
                try
                {
                    try
                    {
                        monitoredSystem = PrecompiledQueries.GetMonitoredSystemByID(dataContext, PrecompiledQueries.GetMonitoredSystemIDByMAC(dataContext, node.MacAddress));
                    }
                    catch (Exception)
                    {
                        monitoredSystem = null;
                    }

                    // monitored system is not yet known in the db
                    if (monitoredSystem == null)
                    {
                        monitoredSystem = new MonitoredSystem();
                        monitoredSystem.OrganizationalUnitID = this.OUID;
                        monitoredSystem.Name = node.Name;
                        monitoredSystem.FQDN = node.FQDN;
                        monitoredSystem.IsAvailable = node.IsAvailable;
                        monitoredSystem.IsIgnored = false;
                        monitoredSystem.OperatingSystem = (byte) PlatformHelper.ParsePlatform(node.CurrentOS);
                        monitoredSystem.MacAddress = node.MacAddress;

                        dataContext.MonitoredSystem.InsertOnSubmit(monitoredSystem);
                        dataContext.SubmitChanges();

                        //logging info
                        var messageOK = new StringBuilder();
                        messageOK.Append("ClusterManager_RegisterNode: ");
                        messageOK.Append("Cluster node " + monitoredSystem.Name + " ");
                        messageOK.Append("(" + monitoredSystem.OperatingSystem.ToString() + ") ");
                        messageOK.Append("is now added to the system.");
                        MISD.Core.Logger.Instance.WriteEntry(messageOK.ToString(), LogType.Info);
                    }
                    else
                    {
                        if (node.IsAvailable)
                        {
                            monitoredSystem.IsAvailable = node.IsAvailable;
                            monitoredSystem.Name = node.Name;
                            monitoredSystem.FQDN = node.FQDN;
                            monitoredSystem.OperatingSystem = (byte) PlatformHelper.ParsePlatform(node.CurrentOS);
                            monitoredSystem.OrganizationalUnitID = this.OUID;
                        }
                        else
                        {
                            // no changes necessary as the node might be booted under a different cluster
                        }
                        dataContext.SubmitChanges();
                    }
                    node.ID = monitoredSystem.ID;
                }
                catch (Exception e)
                {
                    //logging exception
                    string workstationLogName;
                    string osLogName;
                    if (monitoredSystem != null)
                    {
                        workstationLogName = monitoredSystem.Name;
                        osLogName = monitoredSystem.OperatingSystem.ToString();
                    }
                    else
                    {
                        workstationLogName = "[unkown]";
                        osLogName = "system unkown";
                    }

                    var messageEx2 = new StringBuilder();
                    messageEx2.Append("ClusterManager_RegisterNode: ");
                    messageEx2.Append("Cluster node" + workstationLogName + " ");
                    messageEx2.Append("(" + osLogName + ") ");
                    messageEx2.Append("sign in has failed. " + e.ToString());
                    MISD.Core.Logger.Instance.WriteEntry(messageEx2.ToString(), LogType.Exception);
                }
            }
        }

        /// <summary>
        /// Adds new nodes to the system.
        /// </summary>
        /// <param name="monitoredSystems">A list containing the nodes to be added.</param>
        private void AddNodes(object monitoredSystems)
        {
            var nodes = (List<WorkstationInfo>)monitoredSystems;

            foreach (WorkstationInfo current in nodes)
            {
                RegisterNode(current, clusterType.ToString() + ID);
            }

            PluginManager.Instance.UpdateDatabase();

            foreach (WorkstationInfo current in nodes)
            {
                ClusterConnection nodeConnection = clusterConnection.CopyConnection();

                foreach (IPlugin currentPlugin in clusterPlugins)
                {
                    CreateTimerJobs(new List<WorkstationInfo> { current }, new List<IPlugin> { currentPlugin }, nodeConnection);
                }
            }
        }

        /// <summary>
        /// Creates timerjobs for several plugins in combination with several systems.
        /// </summary>
        /// <param name="systems">The systems.</param>
        /// <param name="plugins">The plugins.</param>
        /// <param name="connection">The cluster connection.</param>
        private void CreateTimerJobs(List<WorkstationInfo> systems, List<IPlugin> plugins, ClusterConnection connection)
        {
            try
            {
                if (plugins != null && plugins.Count > 0 && systems != null && systems.Count > 0 && connection != null)
                {
                    foreach (WorkstationInfo current in systems)
                    {
                        foreach (IPlugin currentPlugin in plugins)
                        {
                            foreach (IndicatorSettings currentIndicator in currentPlugin.GetIndicatorSettings())
                            {
                                TimeSpan updateInterval = new TimeSpan(UpdateIntervalManager.Instance.GetUpdateInterval(current.ID, currentPlugin.GetName(), currentIndicator.IndicatorName));
                                // Old approach
                                //timerJobs.Add(new ClusterTimerJob(current, connection, currentPlugin, currentIndicator.IndicatorName, updateInterval));
                                // New approach
                                ClusterJobScheduler.Instance.AddJob((int)Math.Round(updateInterval.TotalSeconds), current, currentPlugin, currentIndicator.IndicatorName, connection, clusterType);
                            }
                        }
                    }
                    ClusterJobScheduler.Instance.Start();
                }
            }
            catch (Exception e)
            {
                MISD.Core.Logger.Instance.WriteEntry("ClusterManager_CreateTimerJobs: Problem adding timerjobs, " + e.ToString(), LogType.Exception);
            }
        }

        #endregion
    }
}

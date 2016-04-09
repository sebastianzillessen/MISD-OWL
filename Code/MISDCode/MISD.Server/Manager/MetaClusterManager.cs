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

namespace MISD.Server.Manager
{
    /// <summary>
    /// A management class for all clusters.
    /// </summary>
    public class MetaClusterManager : SchedulerBase
    {
        #region Singleton

        private static volatile MetaClusterManager instance;
        private static object syncRoot = new Object();

        /// <summary>
        /// Property to access the class.
        /// </summary>
        public static MetaClusterManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new MetaClusterManager();
                    }
                }

                return instance;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the timer jobs.
        /// </summary>
        public List<ClusterManager> clusterManagers;

        #endregion

        #region Constructor

        private MetaClusterManager()
        {
        }

        #endregion

        #region Methods

        protected override void Initialize()
        {
            clusterManagers = new List<ClusterManager>();

            PluginManager.Instance.BrightPluginsChanged += new EventHandler(Instance_BrightPluginsChanged);
            PluginManager.Instance.HPCPluginsChanged += new EventHandler(Instance_HPCPluginsChanged);
        }

        void Instance_BrightPluginsChanged(object sender, EventArgs e)
        {
            foreach (ClusterManager current in clusterManagers)
            {
                if (current.clusterType.Equals(MISD.Core.Platform.Bright))
                {
                    current.RefreshPlugins();
                }
            }
        }

        void Instance_HPCPluginsChanged(object sender, EventArgs e)
        {
            foreach (ClusterManager current in clusterManagers)
            {
                if (current.clusterType.Equals(MISD.Core.Platform.HPC))
                {
                    current.RefreshPlugins();
                }
            }
        }

        /// <summary>
        /// Refreshes the nodes in all clusters.
        /// </summary>
        public override void RefreshJobs()
        {
            foreach (ClusterManager current in clusterManagers)
            {
                current.RefreshNodes();
            }
        }

        /// <summary>
        /// Adds a new cluster and starts all timerjobs..
        /// </summary>
        /// <param name="clusterManager">The cluster manager representing the cluster.</param>
        public void AddCluster(ClusterManager clusterManager)
        {
            clusterManagers.Add(clusterManager);
            clusterManager.StartJobs();

            MISD.Core.Logger.Instance.WriteEntry("MetaClusterManager_AddCluster: Cluster " + clusterManager.ToString() + " added", Core.LogType.Info);
        }

        /// <summary>
        /// Removes a cluster.
        /// </summary>
        /// <param name="clusterManager">The cluster manager representing the cluster to be removed.</param>
        public void RemoveCluster(ClusterManager clusterManager)
        {
            clusterManager.RemoveJobs();
            clusterManagers.Remove(clusterManager);

            MISD.Core.Logger.Instance.WriteEntry("MetaClusterManager_AddCluster: Cluster " + clusterManager.ToString() + " removed", Core.LogType.Info);
        }

        /// <summary>
        /// Updates the update intervals for a node in combination with a plugin.
        /// </summary>
        /// <param name="node">The ID of the node to be updated.</param>
        /// <param name="plugin">The plugins with new update intervals.</param>
        /// <param name="newIntervals">A list of Tuples (IndicatorName | UpdateInterval) with the new intervals.</param>
        public void RefreshUpdateIntervals(int nodeID, string plugin, List<Tuple<string, TimeSpan>> newIntervals)
        {
            foreach (ClusterManager current in clusterManagers)
            {
                current.RefreshUpdateIntervals(nodeID, plugin, newIntervals);
            }
        }

        #endregion
    }
}

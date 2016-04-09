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
using MISD.Server.Manager;
using MISD.Core;
using MISD.Server.Database;
using System.Data.Linq;
using System.Runtime.CompilerServices;

namespace MISD.Server.Manager
{
    public class UpdateIntervalManager
    {
        #region Singleton

        private static volatile UpdateIntervalManager instance;
        private static object syncRoot = new Object();

        /// <summary>
        /// Property to access the class.
        /// </summary>
        public static UpdateIntervalManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new UpdateIntervalManager();
                    }
                }

                return instance;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the Scheduler class.
        /// </summary>
        private UpdateIntervalManager()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get the main update interval.
        /// </summary>
        /// <returns>The given update interval of the properties, or the standart: 15 min</returns>
        public TimeSpan GetMainUpdateInterval()
        {
            TimeSpan standard = new TimeSpan(0, 15, 0);
            TimeSpan critical = new TimeSpan(1, 0, 0, 0);
            //Check the properties intervall
            if (Properties.Settings.Default.MainUpdateInterval > critical)
            {
                //set standart
                Properties.Settings.Default.MainUpdateInterval = standard;

                //logging exception
                var messageEx1 = new StringBuilder();
                messageEx1.Append("WorkstationWebService_GetMainUpdateInterval: ");
                messageEx1.Append("The main update interval in the settings is very critical (over " + critical.ToString() + "). ");
                messageEx1.Append("MISD OWL has changed the main update interval to " + standard.ToString() + ".");
                MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Warning);
            }
            return Properties.Settings.Default.MainUpdateInterval;
        }

        public static readonly Func<MISDDataContext, int, int, IEnumerable<Tuple<string, TimeSpan>>> GetUpdateIntervalsByMonitoredSystemIDAndPluginName = CompiledQuery.Compile<MISDDataContext, int, int, IEnumerable<Tuple<string, TimeSpan>>>((dataContext, monitoredSystemID, pluginMetadataID) => dataContext.Indicator.Where(p => (p.MonitoredSystemID == monitoredSystemID) && (p.PluginMetadataID == pluginMetadataID)).Select(p => new Tuple<string, TimeSpan>(p.Name, new TimeSpan(((long)p.UpdateInterval == 0) ? long.MaxValue : (long)p.UpdateInterval))));

        /// <summary>
        /// Gets all update intervals for a specific system in combination with a plugin.
        /// </summary>
        /// <param name="monitoredSystemID">The ID of the system to get the update intervals from.</param>
        /// <param name="pluginName">The plugin to get the update intervals form.</param>
        /// <returns>A list containing tuples of IndicatorName | UpdateInterval.</returns>
        public List<Tuple<string, TimeSpan>> GetUpdateIntervals(int monitoredSystemID, string pluginName)
        {
            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {
                return GetUpdateIntervalsByMonitoredSystemIDAndPluginName(dataContext, monitoredSystemID, PrecompiledQueries.GetPluginMetadataIDByName(dataContext, pluginName)).ToList();
            }
        }

        public Func<MISDDataContext, int, string, string, long> GetUpdateIntervalFunc =
            CompiledQuery.Compile<MISDDataContext, int, string, string, long>((dataContext, monitoredSystemID, pluginMetadataName, indicatorName) => dataContext.Indicator.Where(p => (p.MonitoredSystemID == monitoredSystemID) &&
                (p.PluginMetadata.Name == pluginMetadataName) && (p.Name == indicatorName)).Select(p => p.UpdateInterval).FirstOrDefault());

        /// <summary>
        /// Gets an update interval for a specific system in combination with a plugin and an indicator.
        /// </summary>
        /// <param name="monitoredSystemID">The ID of the system to get the update interval from.</param>
        /// <param name="pluginName">The plugin to get the update intervals from.</param>
        /// <param name="indicatorName">The indicator to get the update interval from.</param>
        /// <returns>A long containing the update interval.</returns>
        public long GetUpdateInterval(int monitoredSystemID, string pluginName, string indicatorName)
        {
            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {
                try
                {
                    var result = GetUpdateIntervalFunc(dataContext, monitoredSystemID, pluginName, indicatorName);
                    if (result <= 0)
                    {
                        result = long.MaxValue;
                    }
                    return result;
                }
                catch (Exception e)
                {
                    //logging exception
                    var messageEx1 = new StringBuilder();
                    messageEx1.Append("WorkstationWebService_GetUpdateInterval: ");
                    messageEx1.Append("Problems getting update intervals fo " + monitoredSystemID + " and " + pluginName + " and " + indicatorName + ", returing long.MaxValue. " + e.ToString());
                    MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Warning);

                    return long.MaxValue;
                }
            }
        }

        /// <summary>
        /// Sets a single update interval for a specific indicator of a system.
        /// </summary>
        /// <param name="monitoredSystemID">The ID of the system to set the update interval.</param>
        /// <param name="pluginName">The plugin to set the update interval.</param>
        /// <param name="indicator">The indicator to set the update interval of.</param>
        /// <param name="time">The new update interval.</param>
        /// <returns>A list containing all indicators in combination with the update interval for each indicator.</returns>
        public List<Tuple<string, TimeSpan>> SetUpdateIntervals(int monitoredSystemID, string pluginName, string indicator, TimeSpan time)
        {
            using (var dataContext = DataContextFactory.CreateDataContext())
            {
                var interval = (from ind in dataContext.Indicator
                                let system = ind.MonitoredSystem.ID == monitoredSystemID
                                let plugin = ind.PluginMetadata.Name == pluginName
                                let indicatorname = ind.Name == indicator
                                where system && plugin && indicatorname
                                select ind).FirstOrDefault();

                if (interval == null)
                {
                    Logger.Instance.WriteEntry("UpdateIntervalManager_SetUpdateIntervals: Cound not find the indicator (name: " + indicator + "), returning empty list.", LogType.Warning);
                    return new List<Tuple<string, TimeSpan>>();
                }

                interval.UpdateInterval = time.Ticks;

                dataContext.SubmitChanges();

                // update server-sided scheduling
                try
                {
                    MISD.Server.Manager.MetaClusterManager.Instance.RefreshUpdateIntervals(monitoredSystemID, pluginName, new List<Tuple<string, TimeSpan>> { new Tuple<string, TimeSpan>(indicator, time) });
                    MISD.Server.Scheduling.GlobalScheduler.Instance.RefreshIntervals(monitoredSystemID, pluginName, new List<Tuple<string, TimeSpan>> { new Tuple<string, TimeSpan>(indicator, time) });
                }
                catch (Exception e)
                {
                    Logger.Instance.WriteEntry("UpdateIntervalManager_SetUpdateInverals: Problem updating server-sided scheudling intervals, " + e.ToString(), LogType.Exception);
                }

                return this.GetUpdateIntervals(monitoredSystemID, pluginName);
            }
        }

        /// <summary>
        /// Gets all update intervals for a certain plugin of a given workstation .
        /// </summary>
        /// <param name ="monitoredSystemID">The ID of the workstation.</param>
        /// <param name ="pluginName">The name of the plugin.</param>
        /// <returns>A list containing tuples of: IndicatorName | Duration.</returns>
        public List<Tuple<string, long?>> GetUpdateIntervalsLong(int monitoredSystemID, string pluginName)
        {
            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {
                var indicators = from p in dataContext.Indicator
                                 let monitoredSystem = p.MonitoredSystem.ID == monitoredSystemID
                                 let plugin = p.PluginMetadata.Name == pluginName
                                 where monitoredSystem && plugin
                                 select p;
                var intervals = from p in indicators
                                select new Tuple<string, long?>(p.Name, p.UpdateInterval);
                return intervals.ToList();
            }
        }

        #endregion
    }
}
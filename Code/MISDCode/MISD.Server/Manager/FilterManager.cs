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
using System.Text.RegularExpressions;
using MISD.Server.Database;
using System.Runtime.CompilerServices;
using System.Data.Linq;
using MISD.Core;
using MISD.RegExUtil;

namespace MISD.Server.Manager
{
    public class FilterManager
    {
        #region Singleton

        private static volatile FilterManager instance;
        private static object syncRoot = new Object();

        /// <summary>
        /// Property to access the class.
        /// </summary>
        public static FilterManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new FilterManager();
                    }
                }

                return instance;
            }
        }

        #endregion

        #region Properties

        private CacheManager<string, string> cacheMan;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the Scheduler class.
        /// </summary>
        private FilterManager()
        {
            cacheMan = new CacheManager<string, string>();
        }

        #endregion

        #region Get FilterValue

        /// <summary>
        /// Gets the filter-value for an indicator-value from a specific monitored systems.
        /// </summary>
        /// <param name="monitoredSystemID">The ID of the systems that aquired the value.</param>
        /// <param name="pluginName">The plugin that aquired the value.</param>
        /// <param name="indicator">The indicator that aquired the value.</param>
        /// <param name="value">The indicator-value itself.</param>
        /// <returns>True if the value passes the filter, false if not.</returns>
        public bool GetFilterValue(int monitoredSystemID, string pluginName, string indicator, string value)
        {
            Boolean passed = false;

            string filter = GetFilter(monitoredSystemID, pluginName, indicator);


            // filter only if there's a valid statement, else return all values
            if (filter == null || filter.Trim().Equals("") || filter.Trim().Equals("."))
            {
                passed = true;
                return passed;
            }

            // does the value pass the filter
            if (RegExUtility.Match(value, filter))
            {
                passed = true;
            }

            return passed;
        }

        #endregion

        #region Get Filter

        /// <summary>
        /// Gets the filter for a specific monitored system and an indicator.
        /// </summary>
        /// <param name="monitoredSystemID">The ID of the system that belongs to the filter.</param>
        /// <param name="pluginName">The plugins for the filter.</param>
        /// <param name="indicator">The indicator for the filter.</param>
        /// <returns>A string containing the expression for the filter.</returns>
        public string GetFilter(int monitoredSystemID, string pluginName, string indicator)
        {
            string key = monitoredSystemID.ToString() + "." + pluginName + "." + indicator;
            string filter = cacheMan.Get(key) as string;

            if (filter != null)
            {
                return filter;
            }
            else
            {
                return AddFilterFromDatabase(monitoredSystemID, pluginName, indicator);
            }
        }

        /// <summary>
        /// Adds a filter to the availableFilters by reloading it from the database and removing it from the available filtes first
        /// </summary>
        /// <param name="monitoredSystemID">The ID of the monitored System.</param>
        /// <param name="pluginName">The name of the Plugin.</param>
        /// <param name="indicator">The indicator name.</param>
        /// <returns>The filterStatement as string (to not have to look it up again).</returns>
        private string AddFilterFromDatabase(int monitoredSystemID, string pluginName, string indicator)
        {
            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {
                var indi = (from p in dataContext.Indicator
                            let a = p.MonitoredSystemID == monitoredSystemID
                            let b = p.PluginMetadata.Name == pluginName
                            let c = p.Name == indicator
                            where a && b && c
                            select p).FirstOrDefault();

                string filter = indi.FilterStatement;
                UpdateFilter(monitoredSystemID, pluginName, indicator, filter);
                return filter;
            }
        }

        /// <summary>
        /// updates a filter value in the local cache of this class. Removes old values first.
        /// </summary>
        /// <param name="monitoredSystemID">The ID of the monitored system.</param>
        /// <param name="pluginName">The plugin name.</param>
        /// <param name="indicator">The indicator name.</param>
        /// <param name="filter">The filter statement.</param>
        private void UpdateFilter(int monitoredSystemID, string pluginName, string indicator, string filter)
        {
            string key = monitoredSystemID.ToString() + "." + pluginName + "." + indicator;
            cacheMan.Add(key, filter);
        }

        #endregion

        #region Set Filter

        /// <summary>
        /// Sets the filter for a specific monitored system and an indicator.
        /// </summary>
        /// <param name="monitoredSystemID">The ID of the monitored system whose filter should be set.</param>
        /// <param name="pluginName">The plugins for the filter.</param>
        /// <param name="indicator">The indicator for the filter.</param>
        /// <param name="filterValue">The expression for the filter.</param>
        public void SetFilter(int monitoredSystemID, string pluginName, string indicator, string filterValue)
        {
            using (var dataContext = DataContextFactory.CreateDataContext())
            {
                var indi = (from p in dataContext.Indicator
                            let a = p.MonitoredSystemID == monitoredSystemID
                            let b = p.PluginMetadata.Name == pluginName
                            let c = p.Name == indicator
                            where a && b && c
                            select p).FirstOrDefault();

                if (indi != null)
                {
                    indi.FilterStatement = filterValue;

                    dataContext.SubmitChanges();
                    UpdateFilter(monitoredSystemID, pluginName, indicator, filterValue);
                }
                else
                {
                    Logger.Instance.WriteEntry("FilterManager_SetFilter: Could not find the indicator (name: " + indicator + ")", LogType.Warning);
                }
            }
        }

        #endregion

        #region GetFilters

        private Func<MISDDataContext, int, string, IEnumerable<Tuple<string, string>>> GetFiltersForMonitoredSystem = CompiledQuery.Compile<MISDDataContext, int, string, IEnumerable<Tuple<string, string>>>((dataContext, monitoredSystemID, pluginName) => dataContext.Indicator.Where(p => p.MonitoredSystemID == monitoredSystemID && p.PluginMetadata.Name == pluginName).Select(p => new Tuple<string, string>(p.Name, p.FilterStatement)));

        /// <summary>
        /// Gets all filters for a certain plugin of a given workstation.
        /// </summary>
        /// <param name="monitoredSystemID">The ID of the monitored system.</param>
        /// <param name="pluginName">The name of the plugin.</param>
        /// <returns>A list containing tuples of: IndicatorName | FilterStatement.</returns>
        public List<Tuple<string, string>> GetFilters(int monitoredSystemID, string pluginName)
        {
            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {
                return GetFiltersForMonitoredSystem(dataContext, monitoredSystemID, pluginName).ToList();
            }
        }

        #endregion
    }
}

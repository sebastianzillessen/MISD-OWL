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
using System.Runtime.CompilerServices;
using MISD.Server.Database;
using System.Data.Linq;
using MISD.Core;
using MISD.RegExUtil;

namespace MISD.Server.Manager
{
    class MetricManager
    {
        #region Singleton

        private static volatile MetricManager instance;
        private static object syncRoot = new Object();

        /// <summary>
        /// Property to access the class.
        /// </summary>
        public static MetricManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new MetricManager();
                    }
                }

                return instance;
            }
        }

        #endregion

        #region Properties

        private CacheManager<string, Tuple<string, string>> cacheMan;

        #endregion

        #region Constructors

        private MetricManager()
        {
            cacheMan = new CacheManager<string, Tuple<string, string>>();
        }

        #endregion

        #region Get MetricValue

        /// <summary>
        /// Gets the mapping state for an indicator-value from a specific monitored systems.
        /// </summary>
        /// <param name="monitoredSystemID">The ID of the system that aquired the value.</param>
        /// <param name="pluginName">The plugin that aquired the value.</param>
        /// <param name="indicator">The indicator that aquired the value.</param>
        /// <param name="value">The indicator-value itself.</param>
        /// <returns>The appropiate mapping state.</returns>
        public MappingState GetMetricValue(int monitoredSystemID, string pluginName, string indicator, string value)
        {
            MappingState state = MappingState.OK;

            #region Metric Statements

            string statementWarning;
            string statementCritical;

            this.GetMetric(monitoredSystemID, pluginName, indicator, out statementWarning, out statementCritical);

            #endregion

            // Is the state Warning?
            if (statementWarning != "" && RegExUtility.Match(value, statementWarning))
            {
                state = MappingState.Warning;
            }

            // Is the state Critical?
            if (statementCritical != "" && RegExUtility.Match(value, statementCritical))
            {
                state = MappingState.Critical;
            }

            return state;
        }

        #endregion

        #region Get Metric

        public Func<MISDDataContext, int, string, string, Tuple<string, string>> GetFilterStatements = CompiledQuery.Compile<MISDDataContext, int, string, string, Tuple<string, string>>(
            (dataContext, monitoredSystemID, pluginName, indicatorName) => dataContext.Indicator.Where(p => (p.MonitoredSystemID == monitoredSystemID) &&
                (p.PluginMetadata.Name == pluginName) && (p.Name == indicatorName)).Select(p => new Tuple<string, string>(p.StatementWarning, p.StatementCritical)).First());

        /// <summary>
        /// Gets the metric for a specific monitored system and an indicator.
        /// </summary>
        /// <param name="monitoredSystemID">The ID of the system that belongs to the metric.</param>
        /// <param name="pluginName">The plugins for the metric.</param>
        /// <param name="indicator">The indicator for the metric.</param>
        /// <returns>A tuple containing the expressions (for warning and critical) of the metric.</returns>
        public void GetMetric(int monitoredSystemID, string pluginName, string indicator, out string statementWarning, out string statementCritical)
        {
            try
            {
                using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
                {
                    string key = monitoredSystemID.ToString() + "." + pluginName + "." + indicator;
                    Tuple<string, string> result = cacheMan.Get(key) as Tuple<string, string>;

                    if (result == null)
                    {
                        result = GetFilterStatements(dataContext, monitoredSystemID, pluginName, indicator);
                    }

                    statementWarning = result.Item1;
                    statementCritical = result.Item2;
                }
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("MetricManager_GetMetric: Problem getting metric for system " + monitoredSystemID + " and plugin " + pluginName + " and indicator " + indicator + ", returning null. " + e.ToString(), LogType.Exception);
                statementWarning = null;
                statementCritical = null;
            }
        }

        #endregion

        #region Set Metric

        /// <summary>
        /// Sets the metric for a specific monitored system and an indicator.
        /// </summary>
        /// <param name="monitoredSystemID">The ID of the system whose metric should be set.</param>
        /// <param name="pluginName">The plugins for the metric.</param>
        /// <param name="indicator">The indicator for the metric.</param>
        /// <param name="valueWarn">The expression for mapping values to "warning".</param>
        /// <param name="valueCrit">The expression for mapping values to "critical".</param>
        public void SetMetric(int monitoredSystemID, string pluginName, string indicator, string valueWarn, string valueCrit)
        {
            try
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
                        indi.StatementWarning = valueWarn;
                        indi.StatementCritical = valueCrit;

                        dataContext.SubmitChanges();

                        // update local cache
                        string key = monitoredSystemID.ToString() + "." + pluginName + "." + indicator;
                        Tuple<string, string> value = new Tuple<string, string>(valueWarn, valueCrit);
                        cacheMan.Add(key, value);
                    }
                    else
                    {
                        Logger.Instance.WriteEntry("MetricManager_SetMetric: Could not find the indicator to be updated (name: " + indicator + ")", LogType.Warning);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("MetricManager_SetMetric: Problem setting metric for system " + monitoredSystemID + " and plugin " + pluginName + " and indicator " + indicator + ". " + e.ToString(), LogType.Exception);
            }
        }

        #endregion
    }
}

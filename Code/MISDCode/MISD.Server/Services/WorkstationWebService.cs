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
using MISD.Server.Database;
using MISD.Server.Email;
using MISD.Server.Manager;
using System.Data.Linq;

namespace MISD.Server.Services
{
    /// <summary>
    /// This WebService provides functionality for observing workstations.
    /// </summary>
    public class WorkstationWebService : IWorkstationWebService
    {
        #region Main Intervall Update

        public TimeSpan GetMainUpdateInterval()
        {
            return MISD.Server.Manager.UpdateIntervalManager.Instance.GetMainUpdateInterval();
        }

        #endregion

        #region Server Login

        public bool SignIn(string monitoredSystemFQDN, string monitoredSystemMAC, byte operatingSystem)
        {
            return MISD.Server.Manager.WorkstationManager.Instance.SignIn(monitoredSystemFQDN, monitoredSystemMAC, operatingSystem);
        }

        public bool SignOut(string monitoredSystemMAC)
        {
            return MISD.Server.Manager.WorkstationManager.Instance.SignOut(monitoredSystemMAC);
        }

        #endregion

        #region Plugin Management

        public List<Core.PluginMetadata> GetPluginList(string monitoredSystemMAC)
        {
            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {
                int monitoredSystemID = PrecompiledQueries.GetMonitoredSystemIDByMAC(dataContext, monitoredSystemMAC);
                return MISD.Server.Manager.PluginManager.Instance.GetPluginList(monitoredSystemID);
            }
        }

        public List<Core.PluginFile> DownloadPlugins(string monitoredSystemMAC, List<string> pluginNames)
        {
            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {
                int monitoredSystemID = PrecompiledQueries.GetMonitoredSystemIDByMAC(dataContext, monitoredSystemMAC);
                byte monitoredSystemPlatform = PrecompiledQueries.GetMonitoredSystemPlatformByID(dataContext, monitoredSystemID);
                return MISD.Server.Manager.PluginManager.Instance.DownloadPlugins((Core.Platform) monitoredSystemPlatform, pluginNames);
            }
        }

        public bool UploadIndicatorValue(string monitoredSystemMAC, string pluginName, string indicatorValueName, object value, DataType valueDataType, DateTime aquiredTimestamp)
        {
            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {
                int monitoredSystemID = PrecompiledQueries.GetMonitoredSystemIDByMAC(dataContext, monitoredSystemMAC);
                //Manager.Logger.Instance.WriteEntry("Reveiving Data from " + monitoredSystemMAC + " via single UploadIndicatorValue", LogType.Info);
                List<Tuple<string, object, DataType, DateTime>> list = new List<Tuple<string, object, DataType, DateTime>>();
                list.Add(new Tuple<string, object, DataType, DateTime>(indicatorValueName, value, valueDataType, aquiredTimestamp));
                return MISD.Server.Manager.WorkstationManager.Instance.UploadIndicatorValues(monitoredSystemID, pluginName, list);
            }
        }

        public bool UploadIndicatorValues(string monitoredSystemMAC, string pluginName, List<Tuple<string, Object, MISD.Core.DataType, DateTime>> indicatorValues)
        {
            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {
                int monitoredSystemID = PrecompiledQueries.GetMonitoredSystemIDByMAC(dataContext, monitoredSystemMAC);
                return MISD.Server.Manager.WorkstationManager.Instance.UploadIndicatorValues(monitoredSystemID, pluginName, indicatorValues);
            }
        }

        public List<Tuple<string, string>> GetFilters(string monitoredSystemMAC, string pluginName)
        {
            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {
                int monitoredSystemID = PrecompiledQueries.GetMonitoredSystemIDByMAC(dataContext, monitoredSystemMAC);
                return MISD.Server.Manager.FilterManager.Instance.GetFilters(monitoredSystemID, pluginName);
            }
        }

        public string GetFilter(string monitoredSystemMAC, string pluginName, string indicatorName)
        {
            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {
                int monitoredSystemID = PrecompiledQueries.GetMonitoredSystemIDByMAC(dataContext, monitoredSystemMAC);
                return FilterManager.Instance.GetFilter(monitoredSystemID, pluginName, indicatorName);
            }
        }

        public List<Tuple<string, long?>> GetUpdateIntervals(string monitoredSystemMAC, string pluginName)
        {
            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {
                int monitoredSystemID = PrecompiledQueries.GetMonitoredSystemIDByMAC(dataContext, monitoredSystemMAC);
                return MISD.Server.Manager.UpdateIntervalManager.Instance.GetUpdateIntervalsLong(monitoredSystemID, pluginName);
            }
        }

        public long GetUpdateInterval(string monitoredSystemMAC, string pluginName, string indicatorName)
        {
            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {
                int monitoredSystemID = PrecompiledQueries.GetMonitoredSystemIDByMAC(dataContext, monitoredSystemMAC);
                return MISD.Server.Manager.UpdateIntervalManager.Instance.GetUpdateInterval(monitoredSystemID, pluginName, indicatorName);
            }
        }

        public void WriteLog(string message, LogType type)
        {
            MISD.Core.Logger.Instance.WriteWorkstationEntry(message, type);
        }

        #endregion
    }
}
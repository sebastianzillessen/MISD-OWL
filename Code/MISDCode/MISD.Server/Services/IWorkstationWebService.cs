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
using System.ServiceModel;
using System.Runtime.Serialization;
using MISD.Core;
using MISD.Server.Manager;

namespace MISD.Server.Services
{
    /// <summary>
    /// Defines the methods that are exposed via the Workstation Web Service.
    /// </summary>
    [ServiceContract]
    public interface IWorkstationWebService
    {
        #region Main Intervall Update

        /// <summary>
        /// Gets the update interval for services, for the indicators and for the filters.
        /// </summary>
        /// <returns>The main update interval.</returns>>
        [OperationContract]
        TimeSpan GetMainUpdateInterval();

        #endregion

        #region Server Login

        /// <summary>
        /// This method registers a workstation on the server.
        /// </summary>
        /// <param name="monitoredSystemFQDN">The FQDN of the Workstation, it must match the name that is listed in the ActiveDirectory.</param>
        /// <param name="monitoredSystemMAC">The MAC of the Workstation.</param>
        /// <param name="operatingSystem">The operating system of the workstation.</param>
        /// <returns>A value that indicates, whether the method call was successful or not.</returns>
        [OperationContract]
        bool SignIn(string monitoredSystemFQDN, string monitoredSystemMAC, byte operatingSystem);

        /// <summary>
        /// This method tells the server that the workstation is shutting down.
        /// </summary>
        /// <param name="monitoredSystemMAC">The MAC of the workstation.</param>
        /// <returns>A value that indicates, whether the method call was successful or not.</returns>
        [OperationContract]
        bool SignOut(string monitoredSystemMAC);

        #endregion

        #region Plugin Management

        /// <summary>
        /// Gets a list of all plugins, that are currently available on the server.
        /// </summary>
        /// <param name="monitoredSystemMAC">The MAC of the workstation that is requesting.</param>
        /// <returns>The metadata of all plugins that are available on the server, individually created for the specified workstation.</returns>
        [OperationContract]
        List<PluginMetadata> GetPluginList(string monitoredSystemMAC);

        /// <summary>
        /// Downloads the plugins that match the given names.
        /// </summary>
        /// <remarks>
        /// This method gets called only by workstations.
        /// </remarks>
        /// <param name="monitoredSystemMAC">The MAC of the workstation that wants to download the plugins.</param>
        /// <param name="pluginNames">The names of the plugins that shall be downloaded.</param>
        /// <returns>The plugin files that are specific for the given workstation.</returns>
        [OperationContract]
        List<PluginFile> DownloadPlugins(string monitoredSystemMAC, List<string> pluginNames);

        /// <summary>
        /// This method transfers indicator values to the server.
        /// </summary>
        /// <param name="monitoredSystemMAC">The MAC of the workstation.</param>
        /// <param name="pluginName">The name of the plugin.</param>
        /// <param name="indicatorValues">A list containing tuples of: Indicator | IndicatorValue | Datatype | DateTime.
        /// These do not have to be all plugin values of the given plugin.
        /// The DateTime is the time when the value was acquired. </param>
        /// <returns>A value that indicates, whether the method call was successful or not.</returns>
        [OperationContract]
        bool UploadIndicatorValues(string monitoredSystemMAC, string pluginName, List<Tuple<string, Object, MISD.Core.DataType, DateTime>> indicatorValues);

        /// <summary>
        /// This method transfers a single indicator value to the server.
        /// </summary>
        /// <param name="monitoredSystemMAC">The MAC of the workstation.</param>
        /// <param name="pluginName">The name of the plugin.</param>
        /// <param name="indicatorValueName">The name of the indicator.</param>
        /// <param name="value">The value itself.</param>
        /// <param name="valueDataType">The datatype of the value.</param>
        /// <param name="aquiredTimestamp">The timestamp of the value.</param>
        /// <returns>A value that indicates, whether the method call was successful or not.</returns>
        [OperationContract]
        bool UploadIndicatorValue(string monitoredSystemMAC, string pluginName, string indicatorValueName, object value, DataType valueDataType, DateTime aquiredTimestamp);

        /// <summary>
        /// Gets all filters for a certain plugin of a given workstation.
        /// </summary>
        /// <param name="monitoredSystemMAC">The MAC of the workstation.</param>
        /// <param name="pluginName">The name of the plugin.</param>
        /// <returns>A list containing tuples of: IndicatorName | FilterStatement.</returns>
        [OperationContract]
        List<Tuple<string, string>> GetFilters(string monitoredSystemMAC, string pluginName);

        /// <summary>
        /// Gets the filters for a certain plugin of a given workstation and an indicator name.
        /// </summary>
        /// <param name="monitoredSystemMAC">The MAC of the workstation.</param>
        /// <param name="pluginName">The name of the plugin.</param>
        /// <param name="indicatorName">the name of the indicator</param>
        /// <returns>the FilterStatement as string..</returns>
        [OperationContract]
        string GetFilter(string monitoredSystemMAC, string pluginName, string indicatorName);

        /// <summary>
        /// Gets all update intervals for a certain plugin of a given workstation.
        /// </summary>
        /// <param name="monitoredSystemMAC">The MAC of the workstation.</param>
        /// <param name="pluginName">The name of the plugin.</param>
        /// <returns>A list containing tuples of: IndicatorName | Duration.</returns>
        [OperationContract]
        List<Tuple<string, long?>> GetUpdateIntervals(string monitoredSystemMAC, string pluginName);

        /// <summary>
        /// Gets update interval for a certain plugin of a given workstation and a given indicatorname.
        /// </summary>
        /// <param name="monitoredSystemMAC">The MAC of the workstation.</param>
        /// <param name="pluginName">The name of the plugin.</param
        /// <param name="indicatorName">the name of the indicator</param>
        /// <returns>the duration of the UpdateIntervall.</returns>
        [OperationContract]
        long GetUpdateInterval(string monitoredSystemMAC, string pluginName, string indicatorName);
   
        /// <summary>
        /// Logs an event that happened on a workstation to the server's windows event log.
        /// Debug messages are written into a text file.
        /// </summary>
        /// <param name="message">event description</param>
        /// <param name="type">type of the event</param>
        /// <returns></returns>
        [OperationContract]
        void WriteLog(string message, LogType type);

        #endregion
    }
}

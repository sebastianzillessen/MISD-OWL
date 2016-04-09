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

using MISD.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MISD.Core
{
    /// <summary>
    /// This interface must be implemented by all plugins.
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// Contains all indicator specific information and standard values
        /// </summary>
        /// <returns>
        /// A list containing an IndicatorSettings-object for each indicator
        /// </returns>
        List<IndicatorSettings> GetIndicatorSettings();

        /// <summary>
        /// Gets the target platform of this IPlugin.
        /// </summary>
        Platform TargetPlatform { get; }

        /// <summary>
        /// Acquires all data that can be retrieved from this plugin.
        /// </summary>
        /// <returns>
        /// A list containing tuples of: IndicatorName | IndicatorValue | DataType.
        /// </returns>
        List<Tuple<string, Object, DataType>> AcquireData();

        /// <summary>
        /// Acquires all data that can be retrieved from this plugin.
        /// </summary>
        /// <param name="monitoredSystemMAC">The MAC address of the monitored system.</param>
        /// <returns>
        /// A list containing tuples of: IndicatorName | IndicatorValue | DataType.
        /// </returns>
        List<Tuple<string, Object, DataType>> AcquireData(string monitoredSystemMAC);

        /// <summary>
        /// Acquires all data that can be retrieved from this plugin.
        /// </summary>
        /// <param name="monitoredSystem">The name of the monitored system.</param>
        /// <param name="clusterConnection">The cluster connection.</param>
        /// <returns>
        /// A list containing tuples of: IndicatorName | IndicatorValue | DataType.
        /// </returns>
        List<Tuple<string, Object, DataType>> AcquireData(string monitoredSystem, ClusterConnection clusterConnection);

        /// <summary>
        /// Acquires the data of the specified plugin values.
        /// </summary>
        /// <param name="indicatorName">The names of the indicators that shall be retrieved.</param>
        /// <returns>
        /// A list containing tuples of: Indicatorname | IndicatorValue | DataType.
        /// </returns>
        List<Tuple<string, Object, DataType>> AcquireData(List<string> indicatorName);

        /// <summary>
        /// Acquires the data of the specified plugin values.
        /// </summary>
        /// <param name="indicatorName">The names of the indicators that shall be retrieved.</param>
        /// <param name="monitoredSystem">The name of the monitored system.</param>
        /// <returns>
        /// A list containing tuples of: Indicatorname | IndicatorValue | DataType.
        /// </returns>
        List<Tuple<string, Object, DataType>> AcquireData(List<string> indicatorName, string monitoredSystem);

        /// <summary>
        /// Acquires the data of the specified plugin values.
        /// </summary>
        /// <param name="indicatorName">The names of the indicators that shall be retrieved.</param>
        /// <param name="monitoredSystem">The name of the monitored system.</param>
        /// <param name="clusterConnection">The cluster connection.</param>
        /// <returns>
        /// A list containing tuples of: Indicatorname | IndicatorValue | DataType.
        /// </returns>
        List<Tuple<string, Object, DataType>> AcquireData(List<string> indicatorName, string monitoredSystem, ClusterConnection clusterConnection);
    }
}

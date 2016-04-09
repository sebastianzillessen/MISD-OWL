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
using System.Runtime.Serialization;

namespace MISD.Core
{
    /// <summary>
    /// Contains information about a single indicator for a monitored system.
    /// It belongs to a plugin and a monitored system and contains the filterStatement, the update intervall, the storage duration, the data type and the metric settings.
    /// Each Plugin defines this IndicatorSettings for its indicators.
    /// 
    /// </summary>
    [DataContract]
    public class IndicatorSettings
    {
        public IndicatorSettings() { }

        public IndicatorSettings(string pluginName, string indicatorName, string monitoredSystemMAC, string filterStatement,
                                  TimeSpan updateInterval, TimeSpan storageDuration, TimeSpan mappingDuration, DataType dataType,
                                  string metricWarning, string metricCritical)
        {
            this.PluginName = pluginName;
            this.IndicatorName = indicatorName;
            this.MonitoredSystemMAC = monitoredSystemMAC;
            this.FilterStatement = filterStatement;
            this.UpdateInterval = updateInterval;
            this.StorageDuration = storageDuration;
            this.MappingDuration = mappingDuration;
            this.DataType = dataType;
            this.MetricWarning = metricWarning;
            this.MetricCritical = metricCritical;
        }

        [DataMember]
        public string PluginName { get; set; }

        [DataMember]
        public string IndicatorName { get; set; }

        [DataMember]
        public string MonitoredSystemMAC { get; set; }

        [DataMember]
        public string FilterStatement { get; set; }

        [DataMember]
        public TimeSpan UpdateInterval { get; set; }

        [DataMember]
        public TimeSpan StorageDuration { get; set; }

        [DataMember]
        public TimeSpan MappingDuration { get; set; }

        [DataMember]
        public DataType DataType { get; set; }

        [DataMember]
        public string MetricWarning { get; set; }

        [DataMember]
        public string MetricCritical { get; set; }

        [DataMember]
        public MappingState IndicatorMapping { get; set; }
    }
}

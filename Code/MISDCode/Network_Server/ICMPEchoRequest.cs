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
using System.ComponentModel.Composition;
using System.Reflection;

namespace MISD.Plugins.Server.ICMPEchoRequest
{
    [Export(typeof(IPlugin))]
    public class ICMPEchoRequest : IPlugin
    {
        #region private common information
        private static string pluginName = ((AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title;

        private List<IndicatorSettings> indicators = new List<IndicatorSettings>
		{
			new IndicatorSettings(
                pluginName,						// Pluginname
				"Duration",			    		// Indicatornname
				"",								// WorkstationDomainName
				"",								// FilterStatement
				new TimeSpan (0, 0, 30),		// UpdateInterval
				new TimeSpan (365, 0, 0, 0),	// StorageDuration
				new TimeSpan (24, 0, 0),		// MappingDuration
				DataType.Int,					// DataType
				"",								// Metric Warning
				"")                             // Metric Critical
        };

        #endregion

        #region public common information methods

        /// <summary>
        /// Contains all indicator specific information and standard values
        /// </summary>
        /// <returns>
        /// A list containing an IndicatorSettings-object for each indicator
        /// </returns>
        public List<IndicatorSettings> GetIndicatorSettings()
        {
            return indicators;
        }

        public Platform TargetPlatform
        {
            get { return Platform.Server; }
        }

        #endregion

        #region public methods for data acquisation
        /// <summary>
        /// Acquires all data that can be retrieved from this plugin .
        /// </summary>
        /// <returns>A list containing tuples of: IndicatorName IndicatorValue DataType.</returns>
        public List<Tuple<string, object, DataType>> AcquireData(string monitoredSystemName)
        {
            List<string> indicatorNames = new List<string>();

            foreach (IndicatorSettings indicator in indicators)
            {
                indicatorNames.Add(indicator.IndicatorName);
            }

            return AcquireData(indicatorNames, monitoredSystemName);
        }

        /// <summary>
        /// Acquires the data of the specified plugin values .
        /// </summary>
        /// <param name =" indicatorNames "> The names of the indicators that shall b retrieved .</param>
        /// /// <param name =" monitoredSystemName "> The FQDN of the monitored system. </param>
        /// <returns>A list containing tuples of: Indicatorname | IndicatorValue | DataType.</returns>
        public List<Tuple<string, object, DataType>> AcquireData(List<string> indicatorName, string monitoredSystemName)
        {
            List<Tuple<string, object, DataType>> result = new List<Tuple<string, object, DataType>>();

            foreach (string indicator in indicatorName)
            {
                if (indicator.Equals(indicators[0].IndicatorName))
                {
                    //Get roundtrip time
                    result.Add(this.GetRoundtripTime(monitoredSystemName));
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            return result;
        }

        #endregion

        #region public methods for data acquisatio - not implemented

        /// <summary>
        /// Acquires the data of the specified plugin values.
        /// </summary>
        /// <returns>A list containing tuples of: Indicatorname | IndicatorValue | DataType.</returns>
        public List<Tuple<string, object, DataType>> AcquireData()
        {
            throw new NotImplementedException("This method is accessible for monitored systems only.");
        }

        /// <summary>
        /// Acquires the data of the specified plugin values.
        /// </summary>
        /// <param name="indicatorName"></param>
        /// <returns>A list containing tuples of: Indicatorname | IndicatorValue | DataType.</returns>
        public List<Tuple<string, object, DataType>> AcquireData(List<string> indicatorName)
        {
            throw new NotImplementedException("This method is accessible for monitored systems only.");
        }

        /// <summary>
        /// Acquires the data of the specified plugin values.
        /// </summary>
        /// <param name="monitoredSystemName"></param>
        /// <param name="clusterConnection"></param>
        /// <returns>A list containing tuples of: Indicatorname | IndicatorValue | DataType.</returns>
        public List<Tuple<string, object, DataType>> AcquireData(string monitoredSystemName, Core.ClusterConnection clusterConnection)
        {
            throw new NotImplementedException("This method is accessible for clusters only.");
        }

        /// <summary>
        /// Acquires the data of the specified plugin values.
        /// </summary>
        /// <param name="indicatorName"></param>
        /// <param name="monitoredSystemName"></param>
        /// <param name="clusterConnection"></param>
        /// <returns>A list containing tuples of: Indicatorname | IndicatorValue | DataType.</returns>
        public List<Tuple<string, object, DataType>> AcquireData(List<string> indicatorName, string monitoredSystemName, Core.ClusterConnection clusterConnection)
        {
            throw new NotImplementedException("This method is accessible for clusters only.");
        }

        #endregion

        #region Send Ping

        /// <summary>
        /// Send a ping to a workstation
        /// </summary>
        /// <param name="adress">The FQDN of the monitored system. </param>
        /// <returns>Ping result. Object is null if the monitored system is not available.</returns>
        private Tuple<string, object, DataType> GetRoundtripTime(string domainName)
        {
            var ping = new System.Net.NetworkInformation.Ping();
            
            try
            {
                var pingResult = ping.Send(domainName, Properties.Settings.Default.PingTimeout);
                if (pingResult.Status == System.Net.NetworkInformation.IPStatus.Success)
                {
                    return new Tuple<string, object, DataType>(indicators[0].IndicatorName, pingResult.RoundtripTime, DataType.Int);
                }
                else
                {
                    return new Tuple<string, object, DataType>(indicators[0].IndicatorName, null, DataType.Int);
                }
               
            }
            catch (Exception)
            {
                return new Tuple<string, object, DataType>(indicators[0].IndicatorName, -1, DataType.Int);
            }
        }

        #endregion
    }
}

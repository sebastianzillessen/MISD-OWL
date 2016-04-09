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
using System.Reflection;
using MISD.Core;
using System.Management;
using System.ComponentModel.Composition;

namespace MISD.Plugins.Windows.OS
{

    /// <summary>
    /// This plugin gets data about the operating system.
    /// All indicators are hold in a list of tuples with their default settings.
    /// </summary>
    [Export(typeof(IPlugin))]
    public class OS : IPlugin
    {

        #region private common information
        private static string pluginName = ((AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title;

        private List<IndicatorSettings> indicators = new List<IndicatorSettings>
		{
			new IndicatorSettings(
				pluginName,						// Pluginname
				"Name",							// Indicatornname
				"",								// WorkstationDomainName
				".",							// FilterStatement
				new TimeSpan (24, 0, 0),		// UpdateInterval
				new TimeSpan (365, 0, 0, 0),	// StorageDuration
				new TimeSpan (24, 0, 0),		// MappingDuration
				DataType.String,				// DataType
				"",								// Metric Warning
				""),							// Metric Critical
			
			new IndicatorSettings(
				pluginName,						
				"Version",							
				"",								
				".",								
				new TimeSpan (24, 0, 0),		
				new TimeSpan (365, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.String,				
				"",				
				""),							
				
			new IndicatorSettings(
				pluginName,						
				"Uptime",				
				"",								
				".",								
				new TimeSpan (1, 0, 0),		
				new TimeSpan (365, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.String,				
				"",		
				"")
		};
        /// <summary>
        /// The delegate to call the data-acquire-methods.
        /// </summary>
        private delegate Tuple<string, object, DataType> indicator_delegate();

        /// <summary>
        /// The indicator dictionary.
        /// </summary>
        private Dictionary<string, indicator_delegate> indicatorDictionary = new Dictionary<string, indicator_delegate>();
        #endregion

        #region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="CPU.Cpu"/> class.
		/// Fills the indicatorDictionary.
		/// </summary>
		public OS ()
		{
			// Fill the indicatorDictionary
			indicatorDictionary.Add(indicators [0].IndicatorName, GetName);
			indicatorDictionary.Add(indicators [1].IndicatorName, GetVersion);
			indicatorDictionary.Add(indicators [2].IndicatorName, GetUptime);
		}
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

        #endregion

        #region public methods for data acquisation

        /// <summary>
        /// Acquires all data that can be retrieved from this plugin .
        /// </summary>
        /// <returns>A list containing tuples of: IndicatorName | IndicatorValue | DataType.</returns>
        public List<Tuple<string, object, DataType>> AcquireData()
        {
            List<string> indicatorNames = new List<string>();

            foreach (IndicatorSettings indicator in indicators)
            {
                indicatorNames.Add(indicator.IndicatorName);
            }

            return AcquireData(indicatorNames);

        }

        /// <summary>
        /// Acquires the data of the specified plugin values .
        /// </summary>
        /// <param name =" indicatorNames "> The names of the indicators that shall b retrieved .</param>
        /// <returns>A list containing tuples of: Indicatorname | IndicatorValue | DataType.</returns>
        public List<Tuple<string, object, DataType>> AcquireData(List<string> indicatorNames)
        {
            List<Tuple<string, object, DataType>> result = new List<Tuple<string, object, DataType>>();

            try
            {
                foreach (string indicatorName in indicatorNames)
                {
                    result.Add(indicatorDictionary[indicatorName].Invoke());
                }
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentOutOfRangeException();
            }

            return result;
        }

        /// <summary>
        /// Acquires all data that can be retrieved from this plugin.
        /// </summary>
        /// <returns>
        /// A list containing tuples of: IndicatorName | IndicatorValue | DataType.
        /// </returns>
        public List<Tuple<string, object, DataType>> AcquireData(string monitoredSystemName)
        {
            throw new NotImplementedException("This method is accessible for clusters only.");
        }

        /// <summary>
        /// Acquires all data that can be retrieved from this plugin.
        /// </summary>
        /// <returns>
        /// A list containing tuples of: IndicatorName | IndicatorValue | DataType.
        /// </returns>
        public List<Tuple<string, object, DataType>> AcquireData(string monitoredSystemName, Core.ClusterConnection clusterConnection)
        {
            throw new NotImplementedException("This method is accessible for clusters only.");
        }

        /// <summary>
        /// Acquires the data of the specified plugin values.
        /// </summary>
        /// <param name="indicatorName">The names of the indicators that shall be retrieved.</param>
        /// <returns>
        /// A list containing tuples of: Indicatorname | IndicatorValue | DataType.
        /// </returns>
        public List<Tuple<string, object, DataType>> AcquireData(List<string> indicatorName, string monitoredSystemName)
        {
            throw new NotImplementedException("This method is accessible for clusters only.");
        }

        /// <summary>
        /// Acquires the data of the specified plugin values.
        /// </summary>
        /// <param name="indicatorName">The names of the indicators that shall be retrieved.</param>
        /// <returns>
        /// A list containing tuples of: Indicatorname | IndicatorValue | DataType.
        /// </returns>
        public List<Tuple<string, object, DataType>> AcquireData(List<string> indicatorName, string monitoredSystemName, Core.ClusterConnection clusterConnection)
        {
            throw new NotImplementedException("This method is accessible for clusters only.");
        }

        #endregion

        #region private methods for every indicator

        /// <summary>
        /// This gets version of the currently active OS through WMI.
        /// </summary>
        /// <returns>Indicator name, name of the OS, DataType of object</returns>
        private Tuple<string, object, DataType> GetName()
        {
            string OSName = "";
            ManagementObjectSearcher processorSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            foreach (ManagementObject obj in processorSearcher.Get())
            {
                OSName = obj["Caption"].ToString();
            }
            return new Tuple<string, object, DataType>(indicators[0].IndicatorName, OSName, DataType.String);
        }

        /// <summary>
        /// This gets version of the currently active OS.
        /// </summary>
        /// <returns>Indicator name, percentage of overall load, DataType of object</returns>
        private Tuple<string, object, DataType> GetVersion()
        {
            string version = "";
            ManagementObjectSearcher processorSearcher = new ManagementObjectSearcher("SELECT Version FROM Win32_OperatingSystem");
            foreach (ManagementObject obj in processorSearcher.Get())
            {
                version = obj["Version"].ToString();
            }
            return new Tuple<string, object, DataType>(indicators[1].IndicatorName, version, DataType.String);
        }

        /// <summary>
        /// This gets the uptime of the currently active OS.
        /// </summary>
        /// <returns>Indicator name, maximum of all core temperatures, DataType of object</returns>
        private Tuple<string, object, DataType> GetUptime()
        {
            int uptimeSec = 0;
            TimeSpan uptime;
            string uptimeStr;
            ManagementObjectSearcher processorSearcher = new ManagementObjectSearcher("SELECT SystemUpTime FROM Win32_PerfFormattedData_PerfOS_System");
            foreach (ManagementObject obj in processorSearcher.Get())
            {
                //uptime in seconds
                uptimeSec = Convert.ToInt32(obj["SystemUpTime"]);
            }

            uptime = new TimeSpan(0, 0, uptimeSec);
            uptimeStr = uptime.ToString();

            return new Tuple<string, object, DataType>(indicators[2].IndicatorName, uptimeStr, DataType.String);
        }
        #endregion


        public Platform TargetPlatform
        {
            get { return Platform.Windows; }
        }

    }
}

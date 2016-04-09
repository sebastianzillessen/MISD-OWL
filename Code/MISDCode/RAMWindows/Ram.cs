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
using System.Management;
using System.Reflection;

namespace MISD.Plugins.Windows.RAM
{
    /// <summary>
    /// This plugin gets data about the ram and the swap.
    /// All indicators are hold in a list with their default settings.
    /// </summary>
    [Export(typeof(IPlugin))]
    public class RAM : IPlugin
    {
        #region private common information
        private static string pluginName = ((AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title;

        private List<IndicatorSettings> indicators = new List<IndicatorSettings>
		{
			new IndicatorSettings(
				pluginName,						// Pluginname
				"Size",							// Indicatornname
				"",								// WorkstationDomainName
				".",							// FilterStatement
				new TimeSpan (24, 0, 0),		// UpdateInterval
				new TimeSpan (365, 0, 0, 0),	// StorageDuration
				new TimeSpan (24, 0, 0),		// MappingDuration
				DataType.Int,					// DataType
				"",								// Metric Warning
				""),							// Metric Critical
			
			new IndicatorSettings(
				pluginName,						
				"Load",							
				"",								
				".",								
				new TimeSpan (0, 0, 30),		
				new TimeSpan (31, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.Byte,				
				"^9[1-8]$",				
				"^(99|100)$"),							
				
			new IndicatorSettings(
				pluginName,						
				"SwapSize",				
				"",								
				".",								
				new TimeSpan (24, 0, 0),		
				new TimeSpan (365, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.Int,				
				"",		
				""),
				
			new IndicatorSettings(
				pluginName,						
				"SwapLoad",				
				"",								
				".",								
				new TimeSpan (0, 0, 30),		
				new TimeSpan (31, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.Byte,				
				"^9[1-5]$",		
				"^(9[6-9]|100)$"),
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
		/// Initializes a new instance of the <see cref="RAM.RAM"/> class.
		/// Fills the indicatorDictionary.
		/// </summary>
		public RAM ()
		{
			// Fill the indicatorDictionary
			indicatorDictionary.Add(indicators [0].IndicatorName, GetSize);
			indicatorDictionary.Add(indicators [1].IndicatorName, GetLoad);
			indicatorDictionary.Add(indicators [2].IndicatorName, GetSwapSize);
			indicatorDictionary.Add(indicators [3].IndicatorName, GetSwapLoad);
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
        /// <returns>A list containing tuples of: IndicatorName IndicatorValue DataType.</returns>
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
           	List<Tuple<string, object, DataType>> result = new List<Tuple<string, object, DataType>> ();
		
			try {
				foreach (string indicatorName in indicatorNames) {
					result.Add (indicatorDictionary [indicatorName].Invoke ());
				}
			} catch (KeyNotFoundException) {
				throw new ArgumentOutOfRangeException ();
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
        public List<Tuple<string, object, DataType>> AcquireData(string monitoredSystemName, MISD.Core.ClusterConnection clusterConnection)
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
        public List<Tuple<string, object, DataType>> AcquireData(List<string> indicatorName, string monitoredSystemName, MISD.Core.ClusterConnection clusterConnection)
        {
            throw new NotImplementedException("This method is accessible for clusters only.");
        }

        #endregion

        #region private methods to read Ram data
        /// <summary>
        /// Gets the capacity of each memory module by WMI and sums them up.
        /// </summary>
        /// <returns>Indicator name, absolute size of memory modules in MBytes and datatype.</returns>
        private Tuple<string, object, DataType> GetSize()
        {
            Int64 absolutesize = 0;
            ManagementObjectSearcher ramSearcher = new ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory");
            foreach (ManagementObject obj in ramSearcher.Get())
            {
                absolutesize += Convert.ToInt64(obj["Capacity"]);
            }
            // To KBytes
            absolutesize /= 1024;

            // To MBytes
            absolutesize /= 1024;
            return new Tuple<string, object, DataType>(indicators[0].IndicatorName, Convert.ToInt32(absolutesize), DataType.Int);
        }

        /// <summary>
        /// Gets the absolute memory capacity, and the amount of available memory and caluclates the percentage of used capacity.
        /// Information comes from WMI.
        /// </summary>
        /// <returns>Indicator name, percentage of RAM memory in use and datatype.</returns>
        private Tuple<string, object, DataType> GetLoad()
        {
            // get absolute capacity
            Double absoluteSize = 0;
            ManagementObjectSearcher ramCapacitySearcher = new ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory");
            foreach (ManagementObject obj in ramCapacitySearcher.Get())
            {
                absoluteSize += Convert.ToInt64(obj["Capacity"]);
            }
            // To KBytes
            absoluteSize /= 1024;

            // To MBytes
            absoluteSize /= 1024;

            Double available = 0;
            ManagementObjectSearcher ramAvailableSearcher = new ManagementObjectSearcher("SELECT AvailableMBytes FROM Win32_PerfFormattedData_PerfOS_Memory");
            foreach (ManagementObject obj in ramAvailableSearcher.Get())
            {
                available = Convert.ToInt32(obj["AvailableMBytes"]);
            }

            byte loadPercentage = Convert.ToByte((absoluteSize - available) / absoluteSize * 100);

            return new Tuple<string, object, DataType>(indicators[1].IndicatorName, loadPercentage, DataType.Byte);
        }
        #endregion

        #region private methods to read swap data

        /// <summary>
        /// Gets the maximal size of the Swap files in MBytes. Information comes from WMI.
        /// </summary>
        /// <returns>Indicator name, maximal size of swap in MBytes and datatype.</returns>
        private Tuple<string, object, DataType> GetSwapSize()
        {
            int size = 0;
            ManagementObjectSearcher swapSearcher = new ManagementObjectSearcher("SELECT SizeStoredInPagingFiles FROM Win32_OperatingSystem");
            foreach (ManagementObject obj in swapSearcher.Get())
            {
                size = Convert.ToInt32(obj["SizeStoredInPagingFiles"]);
            }
            // To MBytes
            size /= 1024;

            return new Tuple<string, object, DataType>(indicators[2].IndicatorName, size, DataType.Int);
        }

        /// <summary>
        /// Gets the maximal swap size, the free capacity and calculates the percentage of used swap.
        /// </summary>
        /// <returns>Indicator name, percentage of swap in use, datatype.</returns>
        private Tuple<string, object, DataType> GetSwapLoad()
        {
            Double size = 0;
            ManagementObjectSearcher swapSearcher = new ManagementObjectSearcher("SELECT SizeStoredInPagingFiles FROM Win32_OperatingSystem");
            foreach (ManagementObject obj in swapSearcher.Get())
            {
                size = Convert.ToInt32(obj["SizeStoredInPagingFiles"]);
            }
            // To MBytes
            size /= 1024;

            Double available = 1;
            ManagementObjectSearcher freeSwapSearcher = new ManagementObjectSearcher("SELECT FreeSpaceInPagingFiles FROM Win32_OperatingSystem");
            foreach (ManagementObject obj in freeSwapSearcher.Get())
            {
                available = Convert.ToInt32(obj["FreeSpaceInPagingFiles"]);
            }
            // To MBytes
            available /= 1024;

            byte load = Convert.ToByte((size - available) / size * 100);

            return new Tuple<string, object, DataType>(indicators[3].IndicatorName, Convert.ToByte(load), DataType.Byte);
        }

        #endregion


        public Platform TargetPlatform
        {
            get { return Platform.Windows; }
        }
    }
}

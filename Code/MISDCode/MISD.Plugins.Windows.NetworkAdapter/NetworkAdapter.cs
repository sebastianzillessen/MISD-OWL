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
using System.Reflection;
using System.Management;
using System.Net;
using System.Diagnostics;
using System.ComponentModel.Composition;

namespace MISD.Plugins.Windows.NetworkAdapter
{
    /// <summary>
    /// This plugin gets data about the network adapter system.
    /// All indicators are hold in a list of tuples with their default settings.
    /// </summary>
    [Export(typeof(IPlugin))]
    public class NetworkAdapter:IPlugin
    {

        private List<Tuple<PerformanceCounter, PerformanceCounter>> performanceCounters;

        #region private common information
        private static string pluginName = ((AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title;

        private List<IndicatorSettings> indicators = new List<IndicatorSettings>
		{
			new IndicatorSettings(
				pluginName,						// Pluginname
				"NumberOfAdapters",				// Indicatornname
				"",								// WorkstationDomainName
				".",							// FilterStatement
				new TimeSpan (24, 0, 0),		// UpdateInterval
				new TimeSpan (365, 0, 0, 0),	// StorageDuration
				new TimeSpan (24, 0, 0),		// MappingDuration
				DataType.Byte,					// DataType
				"",								// Metric Warning
				""),							// Metric Critical
			
			new IndicatorSettings(
				pluginName,						
				"NamePerAdapter",							
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
				"IPPerAdapter",				
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
				"MACPerAdapter",				
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
				"UpPerAdapter",				
				"",								
				".",								
				new TimeSpan (0, 0, 300),		
				new TimeSpan (31, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.String,				
				"",		
				""),
            new IndicatorSettings(
				pluginName,						
				"DownPerAdapter",				
				"",								
				".",								
				new TimeSpan (0, 0, 300),		
				new TimeSpan (31, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.String,				
				"",		
				""),
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
        /// Initializes a new instance of the <see cref="NetworkAdapter.NetworkAdapter"/> class.
		/// Fills the indicatorDictionary.
		/// </summary>
        public NetworkAdapter()
		{
			// Fill the indicatorDictionary
            indicatorDictionary.Add(indicators[0].IndicatorName, GetNumberOfAdapters);
            indicatorDictionary.Add(indicators[1].IndicatorName, GetNamePerAdapter);
            indicatorDictionary.Add(indicators[2].IndicatorName, GetIPPerAdapter);
            indicatorDictionary.Add(indicators[3].IndicatorName, GetMACPerAdapter);
            indicatorDictionary.Add(indicators[4].IndicatorName, GetUpPerAdapter);
            indicatorDictionary.Add(indicators[5].IndicatorName, GetDownPerAdapter);

            // setup performance counters
            performanceCounters = new List<Tuple<PerformanceCounter, PerformanceCounter>>();
            PerformanceCounterCategory performanceCounterCategory = new PerformanceCounterCategory("Network Interface");
            string[] instances = performanceCounterCategory.GetInstanceNames();
            foreach (string instance in instances)
            {
                // first item: SENT, second item: RECEIVED Bytes/sec
                performanceCounters.Add(new Tuple<PerformanceCounter,PerformanceCounter>(
                    new PerformanceCounter("Network Interface", "Bytes Sent/sec", instance),
                    new PerformanceCounter("Network Interface", "Bytes Received/sec", instance)));
            }
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
        /// Gets the number of installed physical network adapters.
        /// </summary>
        /// <returns>Indicator name, number of adapters, DataType of object</returns>
        private Tuple<string, object, DataType> GetNumberOfAdapters()
        {
            int count = 0;
            ManagementObjectSearcher processorSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter");
            foreach (ManagementObject obj in processorSearcher.Get())
            {
                if (obj["PhysicalAdapter"] != null)
                {
                    try
                    {
                        if(Convert.ToBoolean(obj["PhysicalAdapter"])){
                            count++;
                        }
                    }catch{}
                }
            }
            return new Tuple<string, object, DataType>(indicators[0].IndicatorName, count, DataType.Byte);
        }

        /// <summary>
        /// Gets the names of all installed physical network adapters.
        /// </summary>
        /// <returns>Indicator name, names of network adapters, DataType of object</returns>
        private Tuple<string, object, DataType> GetNamePerAdapter()
        {
            string name = ";";
            ManagementObjectSearcher processorSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter");
            foreach (ManagementObject obj in processorSearcher.Get())
            {
                if (obj["PhysicalAdapter"] != null && obj["Name"] != null)
                {
                    try
                    {
                        if (Convert.ToBoolean(obj["PhysicalAdapter"]))
                        {
                            name += obj["Name"].ToString() + ";";
                        }
                    }
                    catch { }
                }
            }
            return new Tuple<string, object, DataType>(indicators[1].IndicatorName, name.Trim(Convert.ToChar(";")), DataType.String);
        }

        /// <summary>
        /// Gets the local IP V4 adresses of the machine.
        /// </summary>
        /// <returns>Indicator name, IP-Adresses, DataType of object</returns>
        private Tuple<string, object, DataType> GetIPPerAdapter()
        {
            string ipPerAdapter = ";";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipPerAdapter += ip.ToString() + ";";
                }
            }
            return new Tuple<string, object, DataType>(indicators[2].IndicatorName, ipPerAdapter.Trim(Convert.ToChar(";")), DataType.String);
        }

        /// <summary>
        /// Gets the MAC Adresses of the physical adaptery.
        /// </summary>
        /// <returns>Indicator name, MAC Adresses, DataType of object</returns>
        private Tuple<string, object, DataType> GetMACPerAdapter()
        {
            string macPerAdapter = ";";
            ManagementObjectSearcher processorSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter");
            foreach (ManagementObject obj in processorSearcher.Get())
            {
                if (obj["PhysicalAdapter"] != null && obj["MACAddress"] != null)
                {
                    try
                    {
                        if (Convert.ToBoolean(obj["PhysicalAdapter"]))
                        {
                            macPerAdapter += obj["MACAddress"].ToString() + ";";
                        }
                    }catch{}
                }
            }

            return new Tuple<string, object, DataType>(indicators[3].IndicatorName, macPerAdapter.Trim(Convert.ToChar(";")), DataType.String);
        }

        /// <summary>
        /// Gets the sent bytes per second of every network interface.
        /// </summary>
        /// <returns>Indicator name, bytes sent, DataType of object</returns>
        private Tuple<string, object, DataType> GetUpPerAdapter()
        {

            string upPerAdapter = ";";

            foreach (Tuple<PerformanceCounter, PerformanceCounter> perfCounter in performanceCounters)
            {
                upPerAdapter += perfCounter.Item1.NextValue().ToString() + ";";
            }

            return new Tuple<string, object, DataType>(indicators[4].IndicatorName, upPerAdapter.Trim(Convert.ToChar(";")), DataType.String);
        }

        /// <summary>
        /// Gets the received bytes per second of every network interface.
        /// </summary>
        /// <returns>Indicator name, bytes received, DataType of object</returns>
        private Tuple<string, object, DataType> GetDownPerAdapter()
        {
            string downPerAdapter = ";";
            foreach (Tuple<PerformanceCounter, PerformanceCounter> perfCounter in performanceCounters)
            {
                downPerAdapter += perfCounter.Item2.NextValue().ToString() + ";";
            }

            return new Tuple<string, object, DataType>(indicators[5].IndicatorName, downPerAdapter.Trim(Convert.ToChar(";")), DataType.String);
        }
        #endregion

        public Platform TargetPlatform
        {
            get { return Platform.Windows; }
        }
    }
}

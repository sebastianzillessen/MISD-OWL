/*
* Copyright 2012 
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
using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Management;
using System.Reflection;

namespace MISD.Plugins.Windows.CPU
{
    /// <summary>
    /// This plugin gets data about the processor.
    /// All indicators are hold in a list of tuples with their default settings.
    /// </summary>
    [Export(typeof(IPlugin))]
    public class CPU : IPlugin
    {
        #region private common information
        private static string pluginName = ((AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title;

        private List<IndicatorSettings> indicators = new List<IndicatorSettings>
		{
			new IndicatorSettings(
				pluginName,						// Pluginname
				"ProcessorName",				// Indicatornname
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
				"Load",							
				"",								
				".",								
				new TimeSpan (0, 0, 30),		
				new TimeSpan (31, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.Byte,				
				"^(9[1-9]|100)$",				
				""),							
				
			new IndicatorSettings(
				pluginName,						
				"Temperature",				
				"",								
				".",								
				new TimeSpan (0, 0, 30),		
				new TimeSpan (31, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.Byte,				
				"^(6[5-9]|7[0-4])$",		
				"^(7[5-9]|[89][0-9]|[1-9][0-9]{2,8})$"),
				
			new IndicatorSettings(
				pluginName,						
				"NumberOfCores",				
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
				"LoadPerCore",				
				"",								
				".",								
				new TimeSpan (0, 0, 30),		
				new TimeSpan (31, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.String,				
				"^(9[1-9]|100)$",
				""),
				
			new IndicatorSettings(
				pluginName,						
				"TemperaturePerCore",				
				"",								
				".",								
				new TimeSpan (0, 0, 30),		
				new TimeSpan (31, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.String,				
				"^(6[5-9]|7[0-4])$",		
				"^(7[5-9]|[89][0-9]|[1-9][0-9]{2,8})$"),
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
		public CPU ()
		{
			// Fill the indicatorDictionary
			indicatorDictionary.Add(indicators [0].IndicatorName, GetProcessorName);
			indicatorDictionary.Add(indicators [1].IndicatorName, GetLoad);
			indicatorDictionary.Add(indicators [2].IndicatorName, GetTemperature);
			indicatorDictionary.Add(indicators [3].IndicatorName, GetNumberOfCores);
			indicatorDictionary.Add(indicators [4].IndicatorName, GetLoadPerCore);
			indicatorDictionary.Add(indicators [5].IndicatorName, GetTemperaturePerCore);
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
        /// This gets the name of the processor through WMI.
        /// </summary>
        /// <returns>Indicator name, name of the processor, DataType of object</returns>
        private Tuple<string, object, DataType> GetProcessorName()
        {
            string processorName = "";
            ManagementObjectSearcher processorSearcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
            foreach (ManagementObject obj in processorSearcher.Get())
            {
                processorName = obj["Name"].ToString();
            }
            return new Tuple<string, object, DataType>(indicators[0].IndicatorName, processorName, DataType.String);
        }

        /// <summary>
        /// This gets the percentage of the overall load through WMI query.
        /// </summary>
        /// <returns>Indicator name, percentage of overall load, DataType of object</returns>
        private Tuple<string, object, DataType> GetLoad()
        {
            int load = 0;
            ManagementObjectSearcher processorSearcher = new ManagementObjectSearcher("SELECT LoadPercentage FROM Win32_Processor");
            foreach (ManagementObject obj in processorSearcher.Get())
            {
                load = Convert.ToInt32(obj["LoadPercentage"]);
            }
            return new Tuple<string, object, DataType>(indicators[1].IndicatorName, Convert.ToByte(load), DataType.Byte);
        }

        /// <summary>
        /// Gets the maximum of all core temperatures, acquired through the openhardwaremonitor library.
        /// </summary>
        /// <returns>Indicator name, maximum of all core temperatures, DataType of object</returns>
        private Tuple<string, object, DataType> GetTemperature()
        {
            int max = -1;
            Computer comp = new Computer();
            comp.CPUEnabled = true;
            comp.Open();

            foreach (ISensor sens in comp.Hardware[0].Sensors)
            {
                if (sens.SensorType == SensorType.Temperature && Convert.ToInt32(sens.Value) > max)
                {
                    max = Convert.ToInt32(sens.Value);
                }
            }
            comp.Close();
            return new Tuple<string, object, DataType>(indicators[2].IndicatorName, Convert.ToByte(max), DataType.Byte);
        }

        /// <summary>
        /// Acquires core temperatures by using the openhardwaremonitor library
        /// </summary>
        /// <returns>Indicator name, a string of core temperatures separated by ';', DataType of object</returns>
        private Tuple<string, object, DataType> GetNumberOfCores()
        {
            int cores = 1;
            ManagementObjectSearcher processorSearcher = new ManagementObjectSearcher("SELECT NumberOfCores FROM Win32_Processor");
            foreach (ManagementObject obj in processorSearcher.Get())
            {
                cores = Convert.ToInt32(obj["NumberOfCores"]);
            }
            return new Tuple<string, object, DataType>(indicators[3].IndicatorName, cores, DataType.Int);
        }

        /// <summary>
        /// Gets the load per core through WMI.
        /// </summary>
        /// <returns>Indicator name, a string of load per core separated by ';', DataType of object</returns>
        private Tuple<string, object, DataType> GetLoadPerCore()
        {
            int cores = 0;
            string result = ";";
            ManagementObjectSearcher processorSearcher =
                new ManagementObjectSearcher("SELECT PercentProcessorTime FROM Win32_PerfFormattedData_PerfOS_Processor");

            foreach (ManagementObject obj in processorSearcher.Get())
            {
                // wmi returns one additional object containing the average load, we don't need this -> Count - 1
                if (cores < processorSearcher.Get().Count - 1)
                {
                    result += obj["PercentProcessorTime"].ToString() + ";";
                    cores++;
                }
            }
            return new Tuple<string, object, DataType>(indicators[4].IndicatorName, result.Trim(Convert.ToChar(";")), DataType.String);
        }

        /// <summary>
        /// Gets the core temperatures from the openhardwaremonitor library.
        /// </summary>
        /// <returns>Indicator name, a string of temperature per core separated by ';', DataType of object</returns>
        private Tuple<string, object, DataType> GetTemperaturePerCore()
        {
            // enable CPU only
            string result = ";";
            Computer comp = new Computer();
            comp.CPUEnabled = true;
            comp.Open();

            foreach (ISensor sens in comp.Hardware[0].Sensors)
            {
                if (sens.SensorType == SensorType.Temperature)
                {
                    result += sens.Value.ToString() + ";";
                }
            }
            comp.Close();
            // cut last number, as it is the average of all cores
            result = result.Trim(';');
            int lastSemi = result.LastIndexOf(';');
            result = result.Substring(0, lastSemi + 1);
            return new Tuple<string, object, DataType>(indicators[5].IndicatorName, result.Trim(Convert.ToChar(";")), DataType.String);
        }

        #endregion


        public Platform TargetPlatform
        {
            get { return Platform.Windows; }
        }
    }
}

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

namespace MISD.Plugins.Windows.Storage
{
    /// <summary>
    /// This plugin gets data about the ram and the swap.
    /// All indicators are hold in a list with their default settings.
    /// </summary>
    [Export(typeof(IPlugin))]
    public class Storage : IPlugin
    {
        #region private common information
        private static string pluginName = ((AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title;

        private List<IndicatorSettings> indicators = new List<IndicatorSettings>
		{
			new IndicatorSettings(
				pluginName,						// Pluginname
				"Capacity",						// Indicatornname
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
				new TimeSpan (0, 0, 300),		
				new TimeSpan (31, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.Byte,				
				"^(81|82|83|84|85|86|87|88|89|90|91|92|93|94|95)$",				
				"^(96|97|98|99|100)$"),
				
			new IndicatorSettings(
				pluginName,						
				"NumberOfDrives",				
				"",								
				".",								
				new TimeSpan (24, 0, 30),		
				new TimeSpan (365, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.Byte,
				"",		
				""),
				
			new IndicatorSettings(
				pluginName,						
				"CapacityPerDrive",				
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
				"LoadPerDrive",				
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
        /// Initializes a new instance of the <see cref="Storage.Storage"/> class.
        /// Fills the indicatorDictionary.
        /// </summary>
        public Storage()
        {
            // Fill the indicatorDictionary
            indicatorDictionary.Add(indicators[0].IndicatorName, GetCapacity);
            indicatorDictionary.Add(indicators[1].IndicatorName, GetLoad);
            indicatorDictionary.Add(indicators[2].IndicatorName, GetNumberOfDrives);
            indicatorDictionary.Add(indicators[3].IndicatorName, GetCapacityPerDrive);
            indicatorDictionary.Add(indicators[4].IndicatorName, GetLoadPerDrive);
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

        #region private methods to read storage data

        private Tuple<string, object, DataType> GetLoadPerDrive()
        {
            string result = ";";
            ManagementObjectSearcher driveSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk");

            foreach (ManagementObject obj in driveSearcher.Get())
            {
                if (Convert.ToInt32(obj["MediaType"]) == 12 && obj["Size"] != null && obj["FreeSpace"] != null)
                {
                    Double size = Convert.ToInt64(obj["Size"]);
                    Double freeSpace = Convert.ToInt64(obj["FreeSpace"]);
                    int load = Convert.ToInt32((size - freeSpace) / size * 100);
                    result += load.ToString() + ";";
                }
            }
            return new Tuple<string, object, DataType>(indicators[4].IndicatorName, result.Trim(Convert.ToChar(";")), DataType.String);
        }

        private Tuple<string, object, DataType> GetCapacityPerDrive()
        {
            string result = ";";
            ManagementObjectSearcher driveSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk");

            foreach (ManagementObject obj in driveSearcher.Get())
            {
                if (Convert.ToInt32(obj["MediaType"]) == 12 && obj["Size"] != null)
                {
                    Int64 size = Convert.ToInt64(obj["Size"]);
                    // To KBytes
                    size /= 1024;

                    // To MBytes
                    size /= 1024;
                    result += size.ToString() + ";";
                }
            }
            return new Tuple<string, object, DataType>(indicators[3].IndicatorName, result.Trim(Convert.ToChar(";")), DataType.String);
        }

        private Tuple<string, object, DataType> GetNumberOfDrives()
        {
            byte driveCount = 0;
            ManagementObjectSearcher driveSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk");
            foreach (ManagementObject obj in driveSearcher.Get())
            {
                // 11 removable drives, 12 fixed hard disk media
                if (Convert.ToInt32(obj["MediaType"]) == 12)
                {
                    driveCount++;
                }
            }
            return new Tuple<string, object, DataType>(indicators[2].IndicatorName, driveCount, DataType.Byte);
        }

        private Tuple<string, object, DataType> GetLoad()
        {
            // get overall capacity
            Double absoluteCapacity = 0;
            ManagementObjectSearcher driveSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk");
            foreach (ManagementObject obj in driveSearcher.Get())
            {
                // 11 removable drives, 12 fixed hard disk media
                if (Convert.ToInt32(obj["MediaType"]) == 12 && obj["Size"] != null)
                {
                    absoluteCapacity += Convert.ToInt64(obj["Size"]);
                }
            }

            // get overall free space
            Double absoluteFreeSpace = 0;
            foreach (ManagementObject obj in driveSearcher.Get())
            {
                // 11 removable drives, 12 fixed hard disk media
                if (Convert.ToInt32(obj["MediaType"]) == 12 && obj["FreeSpace"] != null)
                {
                    absoluteFreeSpace += Convert.ToInt64(obj["FreeSpace"]);
                }
            }
            byte load = Convert.ToByte((absoluteCapacity - absoluteFreeSpace) / absoluteCapacity * 100);
            return new Tuple<string, object, DataType>(indicators[1].IndicatorName, load, DataType.Byte);
        }

        private Tuple<string, object, DataType> GetCapacity()
        {
            Int64 absoluteCapacity = 0;
            ManagementObjectSearcher driveSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk");
            foreach (ManagementObject obj in driveSearcher.Get())
            {
                // 11 removable drives, 12 fixed hard disk media
                if (Convert.ToInt32(obj["MediaType"]) == 12 && obj["Size"] != null)
                {
                    absoluteCapacity += Convert.ToInt64(obj["Size"]);
                }
            }
            // To KBytes
            absoluteCapacity /= 1024;

            // To MBytes
            absoluteCapacity /= 1024;
            return new Tuple<string, object, DataType>(indicators[0].IndicatorName, Convert.ToInt32(absoluteCapacity), DataType.Int);
        }

        #endregion

        public Platform TargetPlatform
        {
            get { return Platform.Windows; }
        }

    }
}

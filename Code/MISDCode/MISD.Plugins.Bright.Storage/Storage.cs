using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MISD.Core;
using MISD.Server;
using System.ComponentModel.Composition;
using MISD.Server.Cluster;
using System.Reflection;
using System.Globalization;
using MISD.Server.Manager;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace MISD.Plugins.Bright.Storage
{
    [Export(typeof(IPlugin))]
    public class Storage : IPlugin
    {
        private MISD.Server.Cluster.BrightClusterConnection clusterConnection = null;

        #region Indicators and platform

        /// <summary>
        /// Name of the plugin.
        /// </summary>
        private static string pluginName = ((AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title;

        /// <summary>
        /// Available indicators.
        /// </summary>
        private List<IndicatorSettings> indicators = new List<IndicatorSettings>
		{
			new IndicatorSettings(
				pluginName,						// Pluginname
				"Capacity",						// Indicatornname
				"",								// WorkstationDomainName
				"",								// FilterStatement
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
				"",								
				new TimeSpan (0, 2, 0),		
				new TimeSpan (31, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.Byte,				
				"",				
				""),							
				
			new IndicatorSettings(
				pluginName,						
				"NumberOfDrives",				
				"",								
				"",								
				new TimeSpan (24, 0, 0),		
				new TimeSpan (365, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.Byte,				
				"",		
				""),
				
			new IndicatorSettings(
				pluginName,						
				"CapacityPerDrive",				
				"",								
				"",								
				new TimeSpan (24, 0, 0),		
				new TimeSpan (365, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.String,				
				"",		
				""),

            // Not available
            /*
            new IndicatorSettings(
				pluginName,						
				"LoadPerDrive",				
				"",								
				"",								
				new TimeSpan (0, 0, 30),		
				new TimeSpan (365, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.String,				
				"",		
				""),
            */
		};

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

        /// <summary>
        /// Gets the target platform of this IPlugin.
        /// </summary>
        public Platform TargetPlatform
        {
            get
            {
                return Platform.Bright;
            }
        }

        #endregion

        #region IPlugin acquire data methods

        public List<Tuple<string, object, DataType>> AcquireData()
        {
            throw new NotImplementedException("This Method is not available for Clusters.");
        }

        public List<Tuple<string, object, DataType>> AcquireData(string monitoredSystemName)
        {
            if (clusterConnection == null)
            {
                throw new NotImplementedException("No ClusterConnection object is given.");
            }
            else
            {
                return AcquireData(monitoredSystemName, clusterConnection);
            }
        }

        public List<Tuple<string, object, DataType>> AcquireData(string monitoredSystemName, ClusterConnection clusterConnection)
        {
            List<string> indicatorNames = new List<string>();

            foreach (IndicatorSettings indicator in indicators)
            {
                indicatorNames.Add(indicator.IndicatorName);
            }
            return AcquireData(indicatorNames, monitoredSystemName, clusterConnection);
        }

        public List<Tuple<string, object, DataType>> AcquireData(List<string> indicatorName)
        {
            throw new NotImplementedException("No monitored System is given. This method call cannot be invoked on a Cluster.");
        }

        public List<Tuple<string, object, DataType>> AcquireData(List<string> indicatorName, string monitoredSystemName)
        {
            if (clusterConnection == null)
            {
                throw new NotImplementedException("No ClusterConnection object is given.");
            }
            else
            {
                return AcquireData(indicatorName, monitoredSystemName, clusterConnection);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public List<Tuple<string, object, DataType>> AcquireData(List<string> indicatorNames, string monitoredSystemName, ClusterConnection clusterConnection)
        {
            if (clusterConnection.GetType() == typeof(BrightClusterConnection))
            {
                this.clusterConnection = (BrightClusterConnection)clusterConnection;
            }
            else
            {
                throw new ArgumentOutOfRangeException("ClusterConnection is no valid BrightClusterConnecction Object! Method skipped");
            }
            CheckClusterConnection();

            // Skip method if no real monSysName is given.
            if (monitoredSystemName == null || monitoredSystemName.Length == 0)
            {
                throw new ArgumentOutOfRangeException("MonitoredSystemName is not valid for this cluster");
            }

            List<Tuple<string, object, DataType>> result = new List<Tuple<string, object, DataType>>();
            foreach (string indicator in indicatorNames)
            {
                try
                {
                    if (indicator.Equals(indicators[0].IndicatorName))
                    {
                        try
                        {
                            result.Add(new Tuple<string, object, DataType>(indicators[0].IndicatorName, GetTotalCapacity(monitoredSystemName), indicators[0].DataType));
                        }
                        catch (ArgumentNullException)
                        {
                            // Do nothing. The first error that occurs is going to be logged in the plugin.
                        }
                        catch (Exception e)
                        {
                            Logger.Instance.WriteEntry("BrightCluster_Storage: Unknown Exception in " + indicator + " on Node " + monitoredSystemName + ": " + e.Message, LogType.Exception);
                        }
                    }
                    else if (indicator.Equals(indicators[1].IndicatorName))
                    {
                        try
                        {
                            result.Add(new Tuple<string, object, DataType>(indicators[1].IndicatorName, GetTotalLoad(monitoredSystemName), indicators[1].DataType));
                        }
                        catch (ArgumentNullException)
                        {
                            // Do nothing. The first error that occurs is going to be logged in the plugin.
                        }
                        catch (Exception e)
                        {
                            Logger.Instance.WriteEntry("BrightCluster_Storage: Unknown Exception in " + indicator + " on Node " + monitoredSystemName + ": " + e.Message, LogType.Exception);
                        }
                    }
                    else if (indicator.Equals(indicators[2].IndicatorName))
                    {
                        try
                        {
                            result.Add(new Tuple<string, object, DataType>(indicators[2].IndicatorName, GetNumberOfDrives(monitoredSystemName), indicators[2].DataType));
                        }
                        catch (ArgumentNullException)
                        {
                            // Do nothing. The first error that occurs is going to be logged in the plugin.
                        }
                        catch (Exception e)
                        {
                            Logger.Instance.WriteEntry("BrightCluster_Storage: Unknown Exception in " + indicator + " on Node " + monitoredSystemName + ": " + e.Message, LogType.Exception);
                        }
                    }
                    else if (indicator.Equals(indicators[3].IndicatorName))
                    {
                        try
                        {
                            result.Add(new Tuple<string, object, DataType>(indicators[3].IndicatorName, GetCapacityPerDrive(monitoredSystemName), indicators[3].DataType));
                        }
                        catch (ArgumentNullException)
                        {
                            // Do nothing. The first error that occurs is going to be logged in the plugin.
                        }
                        catch (Exception e)
                        {
                            Logger.Instance.WriteEntry("BrightCluster_Storage: Unknown Exception in " + indicator + " on Node " + monitoredSystemName + ": " + e.Message, LogType.Exception);
                        }
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception e)
                {
                    Logger.Instance.WriteEntry("Error during collection of BrightCluster data. Occured when checking for " + indicator + ". Exception was: " + e.ToString(), LogType.Exception);
                }
            }
            return result;
        }

        #endregion

        #region Internal methods

        private void CheckClusterConnection()
        {
            if (clusterConnection == null)
            {
                throw new NotImplementedException("No ClusterConnection object is given.");
            }
        }

        private int GetTotalCapacity(string monitoredSystemName)
        {
            double CapacityBytes = Convert.ToInt64(clusterConnection.GetSysinfo(monitoredSystemName, "Total Disk Space").Split(' ')[0]);
            return (int) Math.Round(CapacityBytes / (1024.0 * 1024.0));
        }

        private byte GetTotalLoad(string monitoredSystemName)
        {
            // Information: "/local" values are higher
            double FreeCapacity = Convert.ToDouble(clusterConnection.GetLatestMetricData(monitoredSystemName, "FreeSpace:/").Split(' ')[0], new CultureInfo("en-US"));
            double UsedCapacity = Convert.ToDouble(clusterConnection.GetLatestMetricData(monitoredSystemName, "UsedSpace:/").Split(' ')[0], new CultureInfo("en-US"));
            double TotalCapacity = FreeCapacity + UsedCapacity;

            byte Load = Convert.ToByte(Math.Round((UsedCapacity / TotalCapacity) * 100));
            return Load;
        }

        private byte GetNumberOfDrives(string monitoredSystemName)
        {
            byte NumberOfDrives = Convert.ToByte(clusterConnection.GetSysinfo(monitoredSystemName, "Number of Disks").Split(' ')[0]);
            return NumberOfDrives;
        }

        private string GetCapacityPerDrive(string monitoredSystemName)
        {
            List<string> Capacity = new List<string>();

            // support for up to 26 drives
            string[] Alphabet = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };

            int NumberOfDrives = GetNumberOfDrives(monitoredSystemName);

            for (int i = 0; i < NumberOfDrives; i++)
            {
                string Name = clusterConnection.GetSysinfo(monitoredSystemName, "Disk sd" + Alphabet[i]);
                Regex r = new Regex(@".*\((?<bytes>\d*).*\).*");

                MatchCollection oMatchCollection = r.Matches(Name);
                // normally here should only be one match.
                foreach (Match oMatch in oMatchCollection)
                {
                    try
                    {
                        double CurrentCapacity = Convert.ToDouble(oMatch.Groups["bytes"].Value) / 1024 / 1024;
                        int CurrentCapacityInt = Convert.ToInt32(Math.Round(CurrentCapacity));
                        Capacity.Add(CurrentCapacityInt.ToString());
                        break; // use only first matchCollection
                    }
                    catch
                    {
                        //Do Nothing
                    }
                }
            }

            string Result = "";
            foreach (string Current in Capacity)
            {
                Result += Current + ";";
            }

            // remove last ";"
            if (Result.Length > 1)
            {
                Result = Result.Substring(0, Result.Length - 1);
                return Result;
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}

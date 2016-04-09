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

namespace MISD.Plugins.Bright.RAM
{
    [Export(typeof(IPlugin))]
    public class RAM : IPlugin
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
				"Size",							// Indicatornname
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
				"^9[1-8]$",				
				"^(99|100)$"),							
				
			new IndicatorSettings(
				pluginName,						
				"SwapSize",				
				"",								
				"",								
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
				"",								
				new TimeSpan (0, 2, 0),		
				new TimeSpan (31, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.Byte,				
				"^9[1-5]$",		
				"^(9[6-9]|100)$"),
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
                            result.Add(new Tuple<string, object, DataType>(indicators[0].IndicatorName, GetMemSizeInMB(monitoredSystemName), indicators[0].DataType));
                        }
                        catch (ArgumentNullException)
                        {
                            // Do nothing. The first error that occurs is going to be logged in the plugin.
                        }
                        catch (Exception e)
                        {
                            Logger.Instance.WriteEntry("BrightCluster_RAM: Unknown Exception in " + indicator + " on Node " + monitoredSystemName + ": " + e.Message, LogType.Exception);
                        }

                    }
                    else if (indicator.Equals(indicators[1].IndicatorName))
                    {
                        try
                        {
                            result.Add(new Tuple<string, object, DataType>(indicators[1].IndicatorName, GetLoadInPercent(monitoredSystemName), indicators[1].DataType));
                        }
                        catch (ArgumentNullException)
                        {
                            // Do nothing. The first error that occurs is going to be logged in the plugin.
                        }
                        catch (Exception e)
                        {
                            Logger.Instance.WriteEntry("BrightCluster_RAM: Unknown Exception in " + indicator + " on Node " + monitoredSystemName + ": " + e.Message, LogType.Exception);
                        }
                    }
                    else if (indicator.Equals(indicators[2].IndicatorName))
                    {
                        try
                        {
                            result.Add(new Tuple<string, object, DataType>(indicators[2].IndicatorName, GetSwapInMB(monitoredSystemName), indicators[2].DataType));
                        }
                        catch (ArgumentNullException)
                        {
                            // Do nothing. The first error that occurs is going to be logged in the plugin.
                        }
                        catch (Exception e)
                        {
                            Logger.Instance.WriteEntry("BrightCluster_RAM: Unknown Exception in " + indicator + " on Node " + monitoredSystemName + ": " + e.Message, LogType.Exception);
                        }
                    }
                    else if (indicator.Equals(indicators[3].IndicatorName))
                    {
                        try
                        {
                            result.Add(new Tuple<string, object, DataType>(indicators[3].IndicatorName, GetSwapLoadInPercent(monitoredSystemName), indicators[3].DataType));
                        }
                        catch (ArgumentNullException)
                        {
                            // Do nothing. The first error that occurs is going to be logged in the plugin.
                        }
                        catch (Exception e)
                        {
                            Logger.Instance.WriteEntry("BrightCluster_RAM: Unknown Exception in " + indicator + " on Node " + monitoredSystemName + ": " + e.Message, LogType.Exception);
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

        private byte GetSwapLoadInPercent(string monitoredSystemName)
        {
            double swap = GetSwapInMB(monitoredSystemName);
            double current = GetSwapLoadInMB(monitoredSystemName);

            return Convert.ToByte(Math.Round((current / swap) * 100));
        }

        private byte GetLoadInPercent(string monitoredSystemName)
        {
            double ram = GetMemSizeInMB(monitoredSystemName);
            double current = GetLoadInMB(monitoredSystemName);

            return Convert.ToByte(Math.Round((current / ram) * 100));
        }

        private int GetSwapLoadInMB(string monitoredSystemName)
        {
            double SwapUsedBytes = Convert.ToDouble(clusterConnection.GetLatestMetricData(monitoredSystemName, "SwapUsed").Split(' ')[0], new CultureInfo("en-US"));
            return (int)Math.Round(SwapUsedBytes / (1024.0 * 1024.0));
        }

        private int GetSwapInMB(string monitoredSystemName)
        {
            double SwapSize = Convert.ToDouble(clusterConnection.GetSysinfo(monitoredSystemName, "Swap Memory").Split(' ')[0], new CultureInfo("en-US"));
            return (int)Math.Round(SwapSize / (1024.0 * 1024.0));
        }

        private int GetLoadInMB(string monitoredSystemName)
        {
            double UsedBytes = Convert.ToDouble(clusterConnection.GetLatestMetricData(monitoredSystemName, "MemoryUsed").Split(' ')[0], new CultureInfo("en-US"));
            return (int)Math.Round(UsedBytes / (1024.0 * 1024.0));
        }

        private int GetMemSizeInMB(string monitoredSystemName)
        {
            double MemSize = Convert.ToDouble(clusterConnection.GetSysinfo(monitoredSystemName, "Total Memory").Split(' ')[0], new CultureInfo("en-US"));
            return (int)Math.Round(MemSize / (1024.0 * 1024.0));
        }

        private Tuple<string, object, DataType> GetNumberOfCores(string monitoredSystemName)
        {
            CheckClusterConnection();
            return new Tuple<string, object, DataType>(indicators[2].IndicatorName, Convert.ToInt32(clusterConnection.GetSysinfo(monitoredSystemName, "Number of Cores")), indicators[2].DataType);
        }

        #endregion
    }
}

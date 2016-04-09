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

namespace MISD.Plugins.Bright.Events
{
    [Export(typeof(IPlugin))]
    public class Events : IPlugin
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
				"Event",						// Indicatornname
				"",								// WorkstationDomainName
				"",								// FilterStatement
				new TimeSpan (0, 0, 30),		// UpdateInterval
				new TimeSpan (365, 0, 0, 0),	// StorageDuration
				new TimeSpan (24, 0, 0),		// MappingDuration
				DataType.String,				// DataType
				"",								// Metric Warning
				""),							// Metric Critical
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
                            result.Add(new Tuple<string, object, DataType>(indicators[0].IndicatorName, GetSwapLoadInMB(monitoredSystemName), indicators[0].DataType));
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
                            result.Add(new Tuple<string, object, DataType>(indicators[1].IndicatorName, GetSwapLoadInMB(monitoredSystemName), indicators[1].DataType));
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
                            result.Add(new Tuple<string, object, DataType>(indicators[2].IndicatorName, GetSwapLoadInMB(monitoredSystemName), indicators[2].DataType));
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
                            result.Add(new Tuple<string, object, DataType>(indicators[3].IndicatorName, GetSwapLoadInMB(monitoredSystemName), indicators[3].DataType));
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
                    else if (indicator.Equals(indicators[4].IndicatorName))
                    {
                        try
                        {
                            result.Add(new Tuple<string, object, DataType>(indicators[3].IndicatorName, GetSwapLoadInMB(monitoredSystemName), indicators[3].DataType));
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

        private int GetSwapLoadInMB(string monitoredSystemName)
        {
            double SwapUsedBytes = Convert.ToDouble(clusterConnection.GetLatestMetricData(monitoredSystemName, "SwapUsed").Split(' ')[0], new CultureInfo("en-US"));
            return (int)Math.Round(SwapUsedBytes / (1024.0 * 1024.0));

        }

        #endregion
    }
}

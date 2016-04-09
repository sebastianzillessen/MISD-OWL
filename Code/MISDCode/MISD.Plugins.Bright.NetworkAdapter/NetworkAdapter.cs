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

namespace MISD.Plugins.Bright.NetworkAdapter
{
    [Export(typeof(IPlugin))]
    public class NetworkAdapter : IPlugin
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
				"NumberOfAdapters",				// Indicatornname
				"",								// WorkstationDomainName
				".",							// FilterStatement
				new TimeSpan (24, 0, 0),	    // UpdateInterval
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
            /* not supported
            new IndicatorSettings(
				pluginName,						
				"UpPerAdapter",				
				"",								
				".",								
				new TimeSpan (0, 0, 30),		
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
				new TimeSpan (0, 0, 30),		
				new TimeSpan (31, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.String,				
				"",		
				""),*/
		};

        /// <summary>
        /// The delegate to call the data-acquire-methods.
        /// </summary>
        private delegate Tuple<string, object, DataType> indicator_delegate(string monitoredSystemName);

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
        }

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


            foreach (string indicatorName in indicatorNames)
            {
                try
                {
                    result.Add(indicatorDictionary[indicatorName].Invoke(monitoredSystemName));
                }
                catch (KeyNotFoundException)
                {
                    Logger.Instance.WriteEntry("Network-Adapter: [ERROR] Could not find indicator name " + indicatorName + ".", LogType.Warning);
                }
                catch (ArgumentNullException)
                {
                    // Do nothing. The first error that occurs is going to be logged in the plugin.
                }
                catch (Exception e)
                {
                    Logger.Instance.WriteEntry("BrightCluster_Network: Unknown Exception in " + indicatorName + " on Node " + monitoredSystemName + ": " + e.Message, LogType.Exception);
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

        private Tuple<string, object, DataType> GetNumberOfAdapters(string monitoredSystemName)
        {
            //0
            return new Tuple<string, object, DataType>(indicators[0].IndicatorName, 1, indicators[0].DataType);
        }

        private Tuple<string, object, DataType> GetNamePerAdapter(string monitoredSystemName)
        {
            // 1
            String networkCard = ((BrightClusterConnection)clusterConnection).GetSysinfo(monitoredSystemName, "Interconnect");
            return new Tuple<string, object, DataType>(indicators[1].IndicatorName, networkCard, indicators[1].DataType);
        }

        private Tuple<string, object, DataType> GetIPPerAdapter(string monitoredSystemName)
        {
            // 2
            string devices = ((BrightClusterShell)clusterConnection.GetConnection()).RunCommands(new List<string> { "device list" });
            string[] devicelist = devices.ToString().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (String d in devicelist)
            {
                try
                {
                    string[] lineElements = d.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (lineElements.Count() > 4 && monitoredSystemName.Equals(lineElements[1]))
                    {
                        return new Tuple<string, object, DataType>(indicators[2].IndicatorName, lineElements[4].Trim(), indicators[2].DataType);
                    }
                }
                catch (Exception e)
                {
                    Logger.Instance.WriteEntry("BrightCluster: [Error] parsing result (IP-Adress): " + e.ToString(), LogType.Warning);
                }
            }
            Logger.Instance.WriteEntry("BrightCluster: [ERROR] could not find IP-Adress for node " + monitoredSystemName, LogType.Warning);
            return null;
        }


        private Tuple<string, object, DataType> GetMACPerAdapter(string monitoredSystemName)
        {
            //3
            string devices = ((BrightClusterShell)clusterConnection.GetConnection()).RunCommands(new List<string> { "device list" });
            string[] devicelist = devices.ToString().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (String d in devicelist)
            {
                try
                {
                    string[] lineElements = d.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (lineElements.Count() > 3 && monitoredSystemName.Equals(lineElements[1]))
                    {
                        return new Tuple<string, object, DataType>(indicators[3].IndicatorName, lineElements[2].Trim(), indicators[3].DataType);
                    }
                }
                catch (Exception e)
                {
                    Logger.Instance.WriteEntry("BrightCluster: [Error] parsing result (MAC): " + e.ToString(), LogType.Warning);
                }
            }
            Logger.Instance.WriteEntry("BrightCluster: [ERROR] could not find Mac for node " + monitoredSystemName, LogType.Warning);
            return null;

        }

        #endregion
    }
}
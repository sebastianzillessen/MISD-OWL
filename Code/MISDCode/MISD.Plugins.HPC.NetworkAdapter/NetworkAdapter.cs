using System;
using System.Collections.Generic;
using MISD.Core;
using System.ComponentModel.Composition;
using MISD.Server.Cluster;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Management.Automation;
using System.Collections.ObjectModel;

namespace MISD.Plugins.HPC.NetworkAdapter
{
    [Export(typeof(IPlugin))]
    public class NetworkAdapter : IPlugin
    {
        private MISD.Server.Cluster.HpcClusterConnection clusterConnection = null;

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
				"")
		};

        #endregion

        #region Constructor

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
                return Platform.HPC;
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
            if (clusterConnection.GetType() == typeof(HpcClusterConnection))
            {
                this.clusterConnection = (HpcClusterConnection)clusterConnection;
            }
            else
            {
                throw new ArgumentOutOfRangeException("ClusterConnection is no valid HpcClusterConnecction Object! Method skipped");
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
                            result.Add(new Tuple<string, object, DataType>(indicators[0].IndicatorName, GetNumberOfAdapters(monitoredSystemName), indicators[0].DataType));
                        }
                        catch (ArgumentNullException)
                        {
                            // Do nothing. The first error that occurs is going to be logged in the plugin.
                        }
                        catch (Exception e)
                        {
                            Logger.Instance.WriteEntry("HPC_Storage: Unknown Exception in " + indicator + " on Node " + monitoredSystemName + ": " + e.Message, LogType.Exception);
                        }
                    }
                    else if (indicator.Equals(indicators[1].IndicatorName))
                    {
                        try
                        {
                            result.Add(new Tuple<string, object, DataType>(indicators[1].IndicatorName, GetNamePerAdapter(monitoredSystemName), indicators[1].DataType));
                        }
                        catch (ArgumentNullException)
                        {
                            // Do nothing. The first error that occurs is going to be logged in the plugin.
                        }
                        catch (Exception e)
                        {
                            Logger.Instance.WriteEntry("HPC_Storage: Unknown Exception in " + indicator + " on Node " + monitoredSystemName + ": " + e.Message, LogType.Exception);
                        }
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception e)
                {
                    Logger.Instance.WriteEntry("Error during collection of HPC data. Occured when checking for " + indicator + ". Exception was: " + e.ToString(), LogType.Exception);
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


        private byte GetNumberOfAdapters(string monitoredSystemName)
        {
            string nodeName = monitoredSystemName.Split('.')[0];


            try
            {
                var pipelineObject = this.clusterConnection.getPipeline();

                pipelineObject.pipeline.Commands.AddScript("Add-PSSnapin Microsoft.Hpc");
                pipelineObject.pipeline.Commands.AddScript("Get-HpcMetricValue -Name HPCNetwork -NodeName " + monitoredSystemName);

                Collection<PSObject> results = pipelineObject.pipeline.Invoke();

                this.clusterConnection.freePipeline(pipelineObject);

                if (results != null || results.Count != 0)
                {
                    return Convert.ToByte(results.Count);
                }
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("HPC: Failed to acquire the number of network-adapters. Error: " + e.StackTrace, LogType.Debug);
            }

            return 0;
        }


        private string GetNamePerAdapter(string monitoredSystemName)
        {
            String networkCards = "";

            string nodeName = monitoredSystemName.Split('.')[0];

            try
            {
                var pipelineObject = this.clusterConnection.getPipeline();

                pipelineObject.pipeline.Commands.AddScript("Add-PSSnapin Microsoft.Hpc");
                pipelineObject.pipeline.Commands.AddScript("Get-HpcMetricValue -Name HPCNetwork -NodeName " + monitoredSystemName);

                Collection<PSObject> results = pipelineObject.pipeline.Invoke();

                this.clusterConnection.freePipeline(pipelineObject);


                if (results != null || results.Count != 0)
                {
                    foreach (PSObject obj in results)
                    {
                        networkCards += obj.Properties["Counter"].Value.ToString();
                        networkCards += "; ";
                    }


                    return networkCards;

                }
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("HPC: Failed to acquire name per network-adapter. Error: " + e.StackTrace, LogType.Debug);
            }

            return "-";
        }
        #endregion
    }
}
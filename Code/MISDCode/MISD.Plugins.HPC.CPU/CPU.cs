using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MISD.Core;
using System.ComponentModel.Composition;
using Microsoft.Hpc.Scheduler;
using System.Reflection;
using MISD.Server.Manager;
using MISD.Server.Cluster;
using System.Globalization;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Runtime.CompilerServices;

namespace MISD.Plugins.HPC.CPU
{
    [Export(typeof(IPlugin))]
    public class CPU : IPlugin
    {
        private MISD.Server.Cluster.HpcClusterConnection
            clusterConnection = null;

        #region private common information
        private static string pluginName = ((AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title;

        private List<IndicatorSettings> indicators = new List<IndicatorSettings>
		{
			new IndicatorSettings(
				pluginName,						
				"Load",							
				"",								
				"",								
				new TimeSpan (0, 10, 0),		
				new TimeSpan (365, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.Byte,				
				"^(9[1-9]|100)$",				
				""),	

			
			new IndicatorSettings(
				pluginName,						
				"NumberOfCores",				
				"",								
				"",								
				new TimeSpan (24, 0, 0),		
				new TimeSpan (365, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.Int,				
				"",		
				"")
		};
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

        #region acquire data public access
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
                throw new ArgumentException("ClusterConnection is no valid HpcClusterConnection Object! Method skipped");
            }
            checkClusterConnection();
            // skip method if no real monSysName is given.
            if (monitoredSystemName == null || monitoredSystemName.Length == 0)
            {
                throw new ArgumentException("MonitoredSystemName is not valid for this cluster");
            }

            List<Tuple<string, object, DataType>> result = new List<Tuple<string, object, DataType>>();

            foreach (string indicator in indicatorNames)
            {
                try
                {
                    if (indicator.Equals(indicators[1].IndicatorName))
                    {
                        try
                        {
                            result.Add(new Tuple<string, object, DataType>(indicators[1].IndicatorName, GetNumberOfCores(monitoredSystemName), indicators[1].DataType));
                        }
                        catch (Exception e)
                        {
                            Logger.Instance.WriteEntry("Failed to acquire the number of cores from \"" + monitoredSystemName + "\". Error: " + e.ToString(), LogType.Exception);
                        }

                    }

                    else if (indicator.Equals(indicators[0].IndicatorName))
                    {
                        try
                        {
                            result.Add(new Tuple<string, object, DataType>(indicators[0].IndicatorName, GetLoad(monitoredSystemName), indicators[0].DataType));
                        }
                        catch (Exception e)
                        {
                            Logger.Instance.WriteEntry("Failed to acquire the cpu-load of \"" + monitoredSystemName + "\"Error: " + e.ToString(), LogType.Exception);
                        }
                    }
                    /*
                    else if (indicator.Equals(indicators[2].IndicatorName))
                    {
                        result.Add(new Tuple<string, object, DataType>(indicators[2].IndicatorName, GetProcessorName(monitoredSystemName), indicators[2].DataType));
                    }
                    */
                    else
                    {
                        throw new ArgumentOutOfRangeException();
                    }

                }
                catch (System.FormatException f)
                {
                    Logger.Instance.WriteEntry("Error during formatation of HPC data. Occured when checking for " + indicator + ".", LogType.Exception);
                }
            }
            return result;
        }
        #endregion

        private void checkClusterConnection()
        {
            if (clusterConnection == null)
            {
                throw new NotImplementedException("No ClusterConnection object is given.");
            }
        }

        #region reading Data

        private ISchedulerNode GetSchedulerNode(string monitoredSystemName)
        {
            string nodeName = monitoredSystemName.Split('.')[0];
            checkClusterConnection();
            IScheduler scheduler = (IScheduler)(((HpcClusterConnection)clusterConnection.CopyConnection()).GetConnection());
            ISchedulerNode requestedNode = null;

            foreach (ISchedulerNode node in scheduler.GetNodeList(null, null))
            {
                if (node.Name.Equals(nodeName))
                {
                    requestedNode = node;
                }
            }
            return requestedNode;
        }


        private int GetNumberOfCores(string monitoredSystemName)
        {
            int result = GetSchedulerNode(monitoredSystemName).NumberOfCores;
            return result;
        }


        private int GetLoad(string monitoredSystemName)
        {
            string nodeName = monitoredSystemName.Split('.')[0];
            string result = "-";

            try
            {
                var pipelineObject = this.clusterConnection.getPipeline();

                pipelineObject.pipeline.Commands.AddScript("Add-PSSnapin Microsoft.Hpc");
                pipelineObject.pipeline.Commands.AddScript("Get-HpcMetricValue -Name HpcCpuUsage -NodeName " + monitoredSystemName);

                Collection<PSObject> results = pipelineObject.pipeline.Invoke();

                this.clusterConnection.freePipeline(pipelineObject);

                if (results != null || results.Count != 0)
                {
                    result = results[0].Properties["Value"].Value.ToString();
                    double resultDouble = Convert.ToDouble(result);
                    resultDouble = Math.Round(resultDouble);

                    return Convert.ToInt32(resultDouble);
                }
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("Failed to acquire HPC-data. NodeName: " + monitoredSystemName + " Indicator: CPU-Load" + " Value: " + result + e.StackTrace, LogType.Debug);
            }

            return 0;
        }


        #endregion

    }
}

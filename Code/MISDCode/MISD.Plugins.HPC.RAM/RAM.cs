using MISD.Core;
using MISD.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;
using MISD.Server.Manager;
using MISD.Server.Cluster;
using Microsoft.Hpc.Scheduler;
using System.Globalization;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Runtime.CompilerServices;

namespace MISD.Plugins.HPC.RAM
{
    [Export(typeof(IPlugin))]
    public class RAM : IPlugin
    {

        private MISD.Server.Cluster.HpcClusterConnection
             clusterConnection = null;

        #region private common information
        private static string pluginName = ((AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title;

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
                throw new ArgumentOutOfRangeException("ClusterConnection is no valid HpcClusterConnection Object! Method skipped");
            }
            checkClusterConnection();

            // skip method if no real monSysName is given.
            if (monitoredSystemName == null || monitoredSystemName.Length == 0)
            {
                throw new ArgumentOutOfRangeException("MonitoredSystemName is not valid for this cluster");
            }

            List<Tuple<string, object, DataType>> result = new List<Tuple<string, object, DataType>>();
            foreach (string indicator in indicatorNames)
            {
                if (indicator.Equals(indicators[0].IndicatorName))
                {
                    try
                    {
                        result.Add(new Tuple<string, object, DataType>(indicators[0].IndicatorName, getMemSizeInMB(monitoredSystemName), indicators[0].DataType));
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.WriteEntry("Error while retrieving data from the HPC cluster.\n\nThe node \"" + monitoredSystemName + "\" is unreachable. Error in MemSizeInMB: " + e.Message, LogType.Info);
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
            return result;
        }

        # endregion

        # region reading data

        private ISchedulerNode GetNode(string monitoredSystemName)
        {
            string nodeName = monitoredSystemName.Split('.')[0];
            checkClusterConnection();
            IScheduler scheduler = (IScheduler)clusterConnection.GetConnection();
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


        private int getMemSizeInMB(string monitoredSystemName)
        {
            try
            {
                double MemSize = Convert.ToDouble(GetNode(monitoredSystemName).MemorySize, new CultureInfo("en-US"));

                return (int)Math.Round(MemSize);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("HPC: Failed to acquire memory-size. Error: " + e.Data, LogType.Debug);
            }
            return 0;
        }
        #endregion


        private void checkClusterConnection()
        {
            if (clusterConnection == null)
            {
                throw new NotImplementedException("No ClusterConnection object is given.");
            }
        }
    }
}

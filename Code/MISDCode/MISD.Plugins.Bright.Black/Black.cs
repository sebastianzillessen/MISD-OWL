using System.Collections.Generic;
using System.Linq;
using System.Text;
using MISD.Core;
using System;
using System.ComponentModel.Composition;
using System.Reflection;

namespace MISD.Plugins.Bright.Black
{
    [Export(typeof(IPlugin))]
    public class Black : IPlugin
    {
        #region private common information
        private static string pluginName = ((AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title;

        private List<IndicatorSettings> indicators = new List<IndicatorSettings>
		{
			new IndicatorSettings(
                pluginName,						// Pluginname
				"Static",					    // Indicatornname
				"",								// WorkstationDomainName
				"",								// FilterStatement
				new TimeSpan (0, 0, 10),		// UpdateInterval
				new TimeSpan (365, 0, 0, 0),	// StorageDuration
				new TimeSpan (24, 0, 0),		// MappingDuration
				DataType.Int,					// DataType
				".",							// Metric Warning
				".")                            // Metric Critical
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
        #endregion

        #region public methods for data acquisation - not implemented

        public List<Tuple<string, object, DataType>> AcquireData()
        {
            List<Tuple<string, object, DataType>> result = new List<Tuple<string, object, DataType>>();
            Tuple<string, object, DataType> entry = new Tuple<string, object, DataType>("Static", (int)42, DataType.Int);
            result.Add(entry);

            return result;
        }

        public List<Tuple<string, object, DataType>> AcquireData(List<string> indicatorName)
        {
            return AcquireData();
        }

        public List<Tuple<string, object, DataType>> AcquireData(string monitoredSystemName)
        {
            return AcquireData();
        }

        public List<Tuple<string, object, DataType>> AcquireData(List<string> indicatorName, string monitoredSystemName)
        {
            return AcquireData();
        }

        #endregion

        #region public methods for data acquisation

        public List<Tuple<string, object, DataType>> AcquireData(string monitoredSystemName, ClusterConnection clusterConnection)
        {
            return AcquireData();
        }

        public List<Tuple<string, object, DataType>> AcquireData(List<string> indicatorName, string monitoredSystemName, ClusterConnection clusterConnection)
        {
            return AcquireData();
        }

        #endregion

        Platform IPlugin.TargetPlatform
        {
            get
            {
                return Platform.Bright;
            }
        }

       
    }
}

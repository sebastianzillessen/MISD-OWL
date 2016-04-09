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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using MISD.Core;
using System.Diagnostics;
using System.ComponentModel.Composition;

namespace MISD.Plugins.Windows.Events
{
    [Export(typeof(IPlugin))]
    public class Events : IPlugin
    {

        private List<EventLogEntry> newEntries;

        #region private common information
        private static string pluginName = ((AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title;

        private List<IndicatorSettings> indicators = new List<IndicatorSettings>
		{
			new IndicatorSettings(
				pluginName,						// Pluginname
				"Event",						// Indicatornname
				"",								// WorkstationDomainName
				".",							// FilterStatement
				new TimeSpan (0, 0, 60),		// UpdateInterval
				new TimeSpan (182, 0, 0, 0),	// StorageDuration
				new TimeSpan (24, 0, 0),		// MappingDuration
				DataType.String,				// DataType
				"",								// Metric Warning
				"") 							// Metric Critical
		};
        /// <summary>
        /// The delegate to call the data-acquire-methods.
        /// </summary>
        private delegate List<Tuple<string, object, DataType>> indicator_delegate();

        /// <summary>
        /// The indicator dictionary.
        /// </summary>
        private Dictionary<string, indicator_delegate> indicatorDictionary = new Dictionary<string, indicator_delegate>();
        #endregion

        #region Constructor
		/// <summary>
        /// Initializes a new instance of the <see cref="Events.Events"/> class.
		/// Fills the indicatorDictionary.
		/// </summary>
		public Events ()
		{
			// Fill the indicatorDictionary
			indicatorDictionary.Add(indicators [0].IndicatorName, GetEvent);

            
            // register eventhandler
            EventLog log = new EventLog();
            if (EventLog.SourceExists("System"))
            {
                newEntries = new List<EventLogEntry>();
                log.Source = "System";
                log.EntryWritten += new EntryWrittenEventHandler(log_EntryWritten);
                log.EnableRaisingEvents = true;
            }

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
                    result.AddRange(GetEvent());
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

        #region private methods to read Event data

        private List<Tuple<string, object, DataType>> GetEvent()
        {
            List<Tuple<string, object, DataType>> resultList = new List<Tuple<string, object, DataType>>();

            foreach (EventLogEntry entry in newEntries)
            {
                resultList.Add(new Tuple<string, object, DataType>(indicators[0].IndicatorName,
                    entry.TimeWritten.ToString() + " [" + entry.EntryType.ToString() + "] " + entry.Source + " " + entry.Message,
                    DataType.String));
            }
            newEntries.Clear();
            if (resultList.Count == 0)
            {
                resultList.Add(new Tuple<string,object,DataType>(indicators[0].IndicatorName,
                    "-",
                    DataType.String));
                return resultList;
            }
            else
            {
                return resultList;
            }
        }
        #endregion

        public Platform TargetPlatform
        {
            get { return Platform.Windows; }
        }

        public void log_EntryWritten(Object sender, EntryWrittenEventArgs e)
        {
            newEntries.Add(e.Entry);
        }
    }
}

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
using System.Text;
using System.Reflection;
using System.ComponentModel.Composition;

using MISD.Core;
using System.IO;

namespace Events
{
	[Export(typeof(IPlugin))]
	public class Events : IPlugin
	{
		#region private common information
		private static string pluginName = ((AssemblyTitleAttribute) Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title;
		
		private List<IndicatorSettings> indicators = new List<IndicatorSettings>
		{
			new IndicatorSettings(
				pluginName,						// Pluginname
				"Event",						// Indicatornname
				"",								// WorkstationDomainName
				".",							// FilterStatement
				new TimeSpan (24, 0, 0),		// UpdateInterval (24,0,0)
				new TimeSpan (365, 0, 0, 0),	// StorageDuration
				new TimeSpan (24, 0, 0),		// MappingDuration
				DataType.String,				// DataType
				"",								// Metric Warning
				"")								// Metric Critical
			
		};		

		/// <summary>
		/// The delegate to call the data-acquire-methods.
		/// </summary>
		private delegate IEnumerable<Tuple<string, object, DataType>> indicator_delegate();   

		/// <summary>
		/// The indicator dictionary.
		/// </summary>
		private Dictionary<string, indicator_delegate> indicatorDictionary = new Dictionary<string, indicator_delegate>();
		
		
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="CPU.Cpu"/> class.
		/// Fills the indicatorDictionary.
		/// </summary>
		public Events ()
		{
			// Fill the indicatorDictionary
			indicatorDictionary.Add(indicators [0].IndicatorName, GetEvents);
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
		
		/// <summary>
        /// Gets the target platform of this IPlugin.
        /// </summary>
        public Platform TargetPlatform
		{ 
			get
			{
            	return Platform.Linux;
        	}
		}
		#endregion
		
		#region public methods for data acquisation
		/// <summary>
		/// Acquires all data that can be retrieved from this plugin .
		/// </summary>
		/// <returns>A list containing tuples of: IndicatorName | IndicatorValue | DataType.</returns>
		public List<Tuple<string, object, DataType>> AcquireData ()
		{
			List<string> indicatorNames = new List<string> ();
			
			foreach (IndicatorSettings indicator in indicators) {
				indicatorNames.Add (indicator.IndicatorName);
			}
			
			return AcquireData (indicatorNames);
		}
		
		/// <summary>
		/// Acquires the data of the specified plugin values .
		/// </summary>
		/// <param name =" indicatorNames "> The names of the indicators that shall b retrieved .</param>
		/// <returns>A list containing tuples of: Indicatorname | IndicatorValue | DataType.</returns>
		public List<Tuple<string, object, DataType>> AcquireData (List<string> indicatorNames)
		{
			List<Tuple<string, object, DataType>> result = new List<Tuple<string, object, DataType>> ();
		
			try {
				foreach (string indicatorName in indicatorNames) {
					result.AddRange(indicatorDictionary [indicatorName].Invoke ());
				}
			} catch (KeyNotFoundException) {
				throw new ArgumentOutOfRangeException ();
			}
			
			return result;
		}


		
		/// <summary>
        /// Acquires all data that can be retrieved from this plugin.
        /// </summary>
        /// <returns>
        /// A list containing tuples of: IndicatorName | IndicatorValue | DataType.
        /// </returns>
        public List<Tuple<string, Object, DataType>> AcquireData(string monitoredSystemName)
		{
			throw new NotImplementedException ("This method is accessible for clusters only.");
		}
		
		/// <summary>
        /// Acquires all data that can be retrieved from this plugin.
        /// </summary>
        /// <returns>
        /// A list containing tuples of: IndicatorName | IndicatorValue | DataType.
        /// </returns>
        public List<Tuple<string, Object, DataType>> AcquireData(string monitoredSystemName, ClusterConnection clusterConnection)
		{
			throw new NotImplementedException ("This method is accessible for clusters only.");
		}
		
		/// <summary>
        /// Acquires the data of the specified plugin values.
        /// </summary>
        /// <param name="indicatorName">The names of the indicators that shall be retrieved.</param>
        /// <returns>
        /// A list containing tuples of: Indicatorname | IndicatorValue | DataType.
        /// </returns>
        public List<Tuple<string, Object, DataType>> AcquireData(List<string> indicatorName, string monitoredSystemName)
		{
			throw new NotImplementedException ("This method is accessible for clusters only.");
		}
		
		 /// <summary>
        /// Acquires the data of the specified plugin values.
        /// </summary>
        /// <param name="indicatorName">The names of the indicators that shall be retrieved.</param>
        /// <returns>
        /// A list containing tuples of: Indicatorname | IndicatorValue | DataType.
        /// </returns>
        public List<Tuple<string, Object, DataType>> AcquireData(List<string> indicatorName, string monitoredSystemName, ClusterConnection clusterConnection)
		{
			throw new NotImplementedException ("This method is accessible for clusters only.");
		}
		#endregion
		
		
		
		#region tupel builder

		public List<Tuple<string, object, DataType>> GetEvents ()
		{
			List<Tuple<string, object, DataType>> res = new List<Tuple<string, object, DataType>>();
			// read all events
			foreach (String event_line in LastLogs()){
				res.Add(new Tuple<string, object, DataType>(indicators[0].IndicatorName, event_line, indicators[0].DataType));
			}
			
			return res;
		}
		#endregion
		
		#region private aquire methods
		
		private String lastLine = null;
		
		/// <summary>
		/// Lists all Logs in the syslog since the last log was recorded. 
		/// If the file does not contain the last log, then it is returning 
		/// the whole file trying to get as much information as possible.
		/// </summary>
		/// <returns>
		/// A list of syslogs as string
		/// </returns>
		public List<String> LastLogs ()
		{
			string filePath = @"/var/log/syslog";
			List<String> res = new List<String> ();
			if (File.Exists (filePath)) {
				StreamReader sr;
				try {
					FileStream fs = new FileStream (filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					sr = new StreamReader (fs);
					string x = sr.ReadToEnd ();
					foreach (String s in x.Split('\n')) {
						if (s.Trim ().Length > 0) {
							res.Add (s);
							if (s != null && s.Equals (lastLine)) {
								res.RemoveRange (0, res.Count);
							}
						}
					}
					lastLine = res [res.Count - 1];
				} catch (Exception e) {
					Console.WriteLine (e);
				} finally {
					sr.Close ();
				}
			}
			return res;
		}
		
		#endregion

	}
}
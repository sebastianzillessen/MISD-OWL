
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
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Management;
using System.Globalization;
using System.Reflection;
using System.ComponentModel.Composition;

using MISD.Core;

namespace OS
{
	[Export(typeof(IPlugin))]
	public class Os : IPlugin
	{
		#region private common information		
		private static string pluginName = ((AssemblyTitleAttribute) Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title;
		
		private List<IndicatorSettings> indicators = new List<IndicatorSettings>
		{
			new IndicatorSettings(
				pluginName,						// Pluginname
				"Name",							// Indicatornname
				"",								// WorkstationDomainName
				".",							// FilterStatement
				new TimeSpan (24, 0, 0),		// UpdateInterval
				new TimeSpan (365, 0, 0, 0),	// StorageDuration
				new TimeSpan (24, 0, 0),		// MappingDuration
				DataType.String,				// DataType
				"",								// Metric Warning
				""),							// Metric Critical
			
			new IndicatorSettings(
				pluginName,						
				"Version",							
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
				"Uptime",				
				"",								
				".",								
				new TimeSpan (1, 0, 0),		
				new TimeSpan (365, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.String,				
				"",		
				"")
		};		

		/// <summary>
		/// The delegate to call the data-acquire-methods.
		/// </summary>
		private delegate Tuple<string, object, DataType> indicator_delegate();   

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
		public Os ()
		{
			// Fill the indicatorDictionary
			indicatorDictionary.Add(indicators [0].IndicatorName, getNameAsTuple);
			indicatorDictionary.Add(indicators [1].IndicatorName, getVersionAsTuple);
			indicatorDictionary.Add(indicators [2].IndicatorName, getUptimeAsTuple);
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
		/// <returns>A list containing tuples of: IndicatorName IndicatorValue DataType.</returns>
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
					result.Add (indicatorDictionary [indicatorName].Invoke ());
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
		
		#region private methods to read the needed data
		public Tuple<string, object, DataType> getNameAsTuple ()
		{
			return new Tuple<string, object, DataType> (indicators [0].IndicatorName, getName(), indicators [0].DataType);
		}

		public Tuple<string, object, DataType> getVersionAsTuple ()
		{
			return new Tuple<string, object, DataType> (indicators [1].IndicatorName, getVersion(), indicators [1].DataType);
		}
		
		public Tuple<string, object, DataType> getUptimeAsTuple ()
		{
			return new Tuple<string, object, DataType> (indicators [2].IndicatorName, getUptime(), indicators [2].DataType);
		}

		private string getName ()
		{
			Regex r = new Regex(".*NAME=\"(?<mem>[0-9.a-zA-Z,\\s]*)\".*");
			string version = evaluateRegex("cat","/etc/os-release",r);
			return version;
		}

		private string  getVersion ()
		{
			Regex r = new Regex(".*VERSION=\"(?<mem>[0-9.a-zA-Z,\\s]*)\".*");
			string version = evaluateRegex("cat","/etc/os-release",r);
			return version;
		}

		private string  getUptime ()
		{
			string up = evaluateRegex("uptime","",@".*up\s*(?<mem>[0-9:]*),.*");
			return TimeSpan.Parse(up).ToString();
		}
		
		private string evaluateRegex(string command, string attributes, string regex) {
			return evaluateRegex(command, attributes, new Regex(regex));
		}
		
		private string evaluateRegex(string command, string attributes, Regex regex) {
			// Prepare the process for starting the program "cat /proc/meminfo"
			ProcessStartInfo ps = new ProcessStartInfo (command, attributes);
			ps.UseShellExecute = false;
			ps.RedirectStandardOutput = true;
			
			string filteredOutput;

			// starts the process
			using (Process p = Process.Start (ps)) {
				
				string output= p.StandardOutput.ReadToEnd();
				p.WaitForExit();
				//Console.WriteLine(output);
				Match t = regex.Match(output);
				
				filteredOutput = t.Groups ["mem"].Value;
			}
			return filteredOutput;
		}
		
		#endregion
	}
}

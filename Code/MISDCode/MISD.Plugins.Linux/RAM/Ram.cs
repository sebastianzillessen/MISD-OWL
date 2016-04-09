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

namespace RAM
{
	[Export(typeof(IPlugin))]
	public class Ram : IPlugin
	{
		#region private common information		
		private static string pluginName = ((AssemblyTitleAttribute) Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title;
		
		private List<IndicatorSettings> indicators = new List<IndicatorSettings>
		{
			new IndicatorSettings(
				pluginName,						// Pluginname
				"Size",							// Indicatornname
				"",								// WorkstationDomainName
				".",								// FilterStatement
				new TimeSpan (25, 0, 0),		// UpdateInterval
				new TimeSpan (365, 0, 0, 0),	// StorageDuration
				new TimeSpan (24, 0, 0),		// MappingDuration
				DataType.Int,					// DataType
				"",								// Metric Warning
				""),							// Metric Critical
			
			new IndicatorSettings(
				pluginName,						
				"Load",							
				"",								
				".",								
				new TimeSpan (0, 0, 30),		
				new TimeSpan (365, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.Byte,				
				"^9[1-8]$",				
				"^(99|100)$"),							
				
			new IndicatorSettings(
				pluginName,						
				"SwapSize",				
				"",								
				".",								
				new TimeSpan (25, 0, 0),		
				new TimeSpan (365, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.Int,				
				"",		
				""),
				
			new IndicatorSettings(
				pluginName,						
				"SwapLoad",				
				"",								
				".",								
				new TimeSpan (0, 0, 30),		
				new TimeSpan (365, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.Byte,				
				"^9[1-5]$",		
				"^(9[6-9]|100)$"),
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
		public Ram ()
		{
			// Fill the indicatorDictionary
			indicatorDictionary.Add(indicators [0].IndicatorName, getMemSizeInMBAsTuple);
			indicatorDictionary.Add(indicators [1].IndicatorName, getLoadInPercentAsTuple);
			indicatorDictionary.Add(indicators [2].IndicatorName, getSwapInMBAsTuple);
			indicatorDictionary.Add(indicators [3].IndicatorName, getSwapLoadInPercentAsTuple);
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

		private byte getLoadInPercent ()
		{
			float size = getMemSizeInMB();
			float current = getLoadInMB();
			return (byte)Math.Round((current/size)*100);
		}

		private Tuple<string, object, DataType> getLoadInPercentAsTuple ()
		{
			return new Tuple<string, object, DataType> (indicators [1].IndicatorName, getLoadInPercent(), indicators [1].DataType);
		}

		private byte getSwapLoadInPercent ()
		{
			float size = getSwapInMB();
			float current = getSwapLoadInMB();
			return (byte)Math.Round((current/size)*100);
		}

		private Tuple<string, object, DataType> getSwapLoadInPercentAsTuple ()
		{
			return new Tuple<string, object, DataType> (indicators [3].IndicatorName, getSwapLoadInPercent(), indicators [3].DataType);
		}
		
		/// <summary>
		/// Evaluates the mem regex to get the ram sizes (converted from KB to MB)
		/// </summary>
		/// <returns>
		/// The mem regex.
		/// </returns>
		/// <param name='regex'>
		/// Regex.
		/// </param>
		private int evaluateMemRegex(string regex) {
			// Prepare the process for starting the program "cat /proc/meminfo"
			ProcessStartInfo ps = new ProcessStartInfo ("cat", "/proc/meminfo");
			ps.UseShellExecute = false;
			ps.RedirectStandardOutput = true;
			
			string filteredOutput;

			// starts the process
			using (Process p = Process.Start (ps)) {
				string output = p.StandardOutput.ReadToEnd ();
				Match t = Regex.Match (output, regex);
				p.WaitForExit ();
				filteredOutput = t.Groups ["mem"].Value;
			}
			try{
				return (int)(Int64.Parse (filteredOutput)/1024);
			}
			catch(Exception e){
				Console.WriteLine("FEHLER BEIM KONVERTIEREN");
				Console.WriteLine ("#"+filteredOutput+"#");
				throw e;
			}
		}
		
		/// <summary>
		/// Gets the total memory size in MB.
		/// </summary>
		/// <returns>
		/// The mem size in M.
		/// </returns>
		private int getMemSizeInMB(){
			return evaluateMemRegex(@".*MemTotal:\s*(?<mem>[0-9]*)\skB.*"); 
		}

		private Tuple<string, object, DataType> getMemSizeInMBAsTuple(){
			return new Tuple<string, object, DataType> (indicators [0].IndicatorName, getMemSizeInMB(), indicators [0].DataType);
		}
		
		/// <summary>
		/// Gets the load of the Ram in MB.
		/// </summary>
		/// <returns>
		/// The load in M.
		/// </returns>
		private int getLoadInMB(){
			int available = getMemSizeInMB();
			int free = evaluateMemRegex(@".*MemFree:\s*(?<mem>[0-9]*)\skB.*");
			int cached = evaluateMemRegex(@".*Cached:\s*(?<mem>[0-9]*)\skB.*");
			return available - free - cached;

		}
		
		/// <summary>
		/// Gets the swap load (used Swap) in MB
		/// </summary>
		/// <returns>
		/// The swap load in MB
		/// </returns>
		private int getSwapLoadInMB(){
			int available = getSwapInMB();
			int free = evaluateMemRegex(@".*SwapFree:\s*(?<mem>[0-9]*)\skB.*");
			return available - free;
		}
		
		/// <summary>
		/// Gets the swap in MB.
		/// </summary>
		/// <returns>
		/// The swap in M.
		/// </returns>
		private int getSwapInMB(){
			return evaluateMemRegex(@".*SwapTotal:\s*(?<mem>[0-9]*)\skB.*");
		}

		private Tuple<string, object, DataType> getSwapInMBAsTuple(){
			return new Tuple<string, object, DataType> (indicators [2].IndicatorName, getSwapInMB(), indicators [2].DataType);
		}
		#endregion
	}
}

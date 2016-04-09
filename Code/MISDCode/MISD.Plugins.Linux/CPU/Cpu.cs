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

namespace CPU
{
	[Export(typeof(IPlugin))]
	public class Cpu : IPlugin
	{
		#region private common information
		private static string pluginName = ((AssemblyTitleAttribute) Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title;
		
		private List<IndicatorSettings> indicators = new List<IndicatorSettings>
		{
			new IndicatorSettings(
				pluginName,						// Pluginname
				"ProcessorName",				// Indicatornname
				"",								// WorkstationDomainName
				".",								// FilterStatement
				new TimeSpan (24, 0, 0),		// UpdateInterval
				new TimeSpan (365, 0, 0, 0),	// StorageDuration
				new TimeSpan (24, 0, 0),		// MappingDuration
				DataType.String,				// DataType
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
				"^(9[1-9]|100)$",				
				""),							
				
			new IndicatorSettings(
				pluginName,						
				"Temperature",				
				"",								
				".",								
				new TimeSpan (0, 0, 30),		
				new TimeSpan (365, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.Byte,				
				"^(6[5-9]|7[0-4])$",		
				"^(7[5-9]|[89][0-9]|[1-9][0-9]{2,8})$"),
				
			new IndicatorSettings(
				pluginName,						
				"NumberOfCores",				
				"",								
				".",								
				new TimeSpan (24, 0, 0),		
				new TimeSpan (365, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.Int,				
				"",		
				""),
				
			new IndicatorSettings(
				pluginName,						
				"LoadPerCore",				
				"",								
				".",								
				new TimeSpan (0, 0, 30),		
				new TimeSpan (365, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.String,
				"(^|;)(9[1-9]|100)($|;)",
				""),
				
			new IndicatorSettings(
				pluginName,						
				"TemperaturePerCore",				
				"",								
				".",								
				new TimeSpan (0, 0, 30),		
				new TimeSpan (365, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.String,				
				"(^|;)(6[5-9]|7[0-4])($|;)",		
				"(^|;)(7[5-9]|[89][0-9]|[1-9][0-9]{2,8})($|;)"),
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
		public Cpu ()
		{
			// Fill the indicatorDictionary
			indicatorDictionary.Add(indicators [0].IndicatorName, GetProcessorName);
			indicatorDictionary.Add(indicators [1].IndicatorName, GetLoad);
			indicatorDictionary.Add(indicators [2].IndicatorName, GetTemperature);
			indicatorDictionary.Add(indicators [3].IndicatorName, GetNumberOfCores);
			indicatorDictionary.Add(indicators [4].IndicatorName, GetLoadPerCore);
			indicatorDictionary.Add(indicators [5].IndicatorName, GetTemperaturePerCore);
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
		
		#region private methods for every indicator
		private Tuple<string, object, DataType> GetProcessorName ()
		{
			try {
				// Prepare the process for starting the program "cat /proc/cpuinfo"
				ProcessStartInfo ps = new ProcessStartInfo ("cat", "/proc/cpuinfo");
				ps.UseShellExecute = false;
				ps.RedirectStandardOutput = true;
				string output;
				// starts the process
				using (Process p = Process.Start (ps)) {
					output = p.StandardOutput.ReadToEnd ();
					string regEx = @".*model name\s+:\s(?<cpuname>.+)$.*"; 
					Match t = Regex.Match (output, regEx, RegexOptions.Multiline       );
					p.WaitForExit ();
					output = t.Groups ["cpuname"].Value;
				}
				return new Tuple<string, object, DataType> (indicators [0].IndicatorName, output, indicators [0].DataType);
			} catch (Exception e) {
				// Error in CPU PLUGIN: Name not readable!
				throw new Exception (e.Message);
			}
		}

		/// <summary>
		/// Gets the total load of the cpu.
		/// </summary>
		/// <returns>
		/// The load as tuple: String (indicator name) | object (value) | DataType.
		/// </returns>
		private Tuple<string, object, DataType> GetLoad ()
		{
			try {
				// Use PerformanceCounter to ask the CPU load
				PerformanceCounter CPUCounter = new PerformanceCounter ();
				CPUCounter.CategoryName = "Processor"; 				// [für später] RAM CategoryName = "Memory"
				CPUCounter.CounterName = "% Processor Time";
				CPUCounter.InstanceName = "_Total";
				CPUCounter.NextValue ();
				System.Threading.Thread.Sleep (1000);
				byte val = (byte)Math.Round ((double)CPUCounter.NextValue ());
		
				return new Tuple<string, object, DataType> (indicators [1].IndicatorName, val, indicators [1].DataType);
			} catch (Exception e) {
				// Error in CPU PLUGIN: Load not readable!
				throw new Exception (e.Message);
			}
		}
		
		private Tuple<string, object, DataType> GetTemperature ()
		{		
			Tuple<string, object, DataType> temperaturePerCore = GetTemperaturePerCore();
			
			if (temperaturePerCore.Item2 == null || temperaturePerCore.Item2.ToString() == ""){
				return new Tuple<string, object, DataType> (indicators [2].IndicatorName, null, indicators [2].DataType);
			}
			string [] results = temperaturePerCore.Item2.ToString ().Split (';');
			float avg = 0.0f;
			// we are counting the objects explicit to prevent wrong averages if one number couldn't be transformed.
			int count = 0;
			if (results.Length == 0){
				return new Tuple<string, object, DataType> (indicators [2].IndicatorName, null, indicators [2].DataType);
			}
			// culture info needed to transform strings like [0..9]*.[0..9]* to a double
			CultureInfo en = new CultureInfo ("en-US");
			
			foreach (string s in results) {
				if (s != ""){
					//skip emtpy values
					try {
					
						float r = (float)Convert.ToDouble (s, en);
						avg += r;
						count++;
					} catch (InvalidCastException) {
					}
				}
			}
			if (count > 0 && avg > 0){
			avg = avg / (count * 1.0f);
			return new Tuple<string, object, DataType> (indicators [2].IndicatorName, (Byte)Math.Round(avg), indicators [2].DataType);
			}
			else {
				return null;
			}
		}
		
		private Tuple<string, object, DataType> GetNumberOfCores ()
		{
			try {
				// Prepare the process for starting the program "cat /proc/cpuinfo"
				ProcessStartInfo ps = new ProcessStartInfo ("cat", "/proc/cpuinfo");
				ps.UseShellExecute = false;
				ps.RedirectStandardOutput = true;
				
				string filteredOutput;
 
				// starts the process
				using (Process p = Process.Start (ps)) {
					string output = p.StandardOutput.ReadToEnd ();
					string regEx = @".*cpu cores\s+:\s(?<temp>[0-9]*)\s.*"; 

					Match t = Regex.Match (output, regEx);
					p.WaitForExit ();
					filteredOutput = t.Groups ["temp"].Value;
				}
				
				int result = Int16.Parse (filteredOutput);
				return new Tuple<string, object, DataType> (indicators [3].IndicatorName, result, indicators [3].DataType);
				
			} catch (Exception e) {
				// Error in CPU PLUGIN: NumberOfCores not readable!
				throw new Exception (e.Message);
			}
		}
		
		/// <summary>
		/// Gets the load per core.
		/// </summary>
		/// <returns>
		/// The load per core as Tuple: string (indicator name) | object (string of cpu loads, seperated with ";") | DataType.
		/// </returns>
		private Tuple<string, object, DataType> GetLoadPerCore ()
		{			
			try {
				// Use PerformanceCounter to ask the CPU load
				PerformanceCounter CPUCounter = new PerformanceCounter ();
				CPUCounter.CategoryName = "Processor";
				CPUCounter.CounterName = "% Processor Time";
				
				int numberOfCores = (int)GetNumberOfCores ().Item2;
				string result = "";
				
				for (int i=0; i<numberOfCores; i++) {
					// The InstanceName for every core in the appropriate number.
					CPUCounter.InstanceName = i.ToString ();
					CPUCounter.NextValue ();
					System.Threading.Thread.Sleep (1000);
					if (i < numberOfCores - 1)
						result += (int)Math.Round ((double)CPUCounter.NextValue ()) + ";";
					else
						result += (int)Math.Round ((double)CPUCounter.NextValue ());
				}
	
				return new Tuple<string, object, DataType> (indicators [4].IndicatorName, result, indicators [4].DataType);
			} catch (Exception e) {
				// Error in CPU PLUGIN: Load per Core not readable!
				throw new Exception (e.Message);
			}
		}
		
		private Tuple<string, object, DataType> GetTemperaturePerCore ()
		{
			try {
				string output_object = "";
				ProcessStartInfo ps = new ProcessStartInfo ("sensors", "");
				ps.UseShellExecute = false;
 
				// we need to redirect the standard output so we read it
				// internally in out program
				ps.RedirectStandardOutput = true;
 
				// starts the process
				using (Process p = Process.Start (ps)) {
					// we read the output to a string
					string output = p.StandardOutput.ReadToEnd ();
				
					Regex oRegex = new Regex (@"Core \d+\:\s+\+(?<temp>[^\(]*)°C.*"); 
  
					MatchCollection oMatchCollection = oRegex.Matches (output); 
  
					foreach (Match oMatch in oMatchCollection) { 
						if (output_object.Length > 0) {
							output_object += ";";
						}
						output_object += oMatch.Groups ["temp"]; 
					} 
					p.WaitForExit ();
				}
				if (output_object==null || output_object.Length == 0 || output_object == ";"){
					return new Tuple<string, object, DataType> (indicators [5].IndicatorName, null, indicators [5].DataType);
				}else{
					return new Tuple<string, object, DataType> (indicators [5].IndicatorName, output_object, indicators [5].DataType);
				}
			} catch (Exception e) {
				// Error in CPU PLUGIN: Temperatur not readable!
				Console.WriteLine (e.ToString());
				throw new Exception (e.Message);
			}
		}
		#endregion
	}
}
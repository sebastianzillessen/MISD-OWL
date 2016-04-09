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
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using MISD.Core;
using System.ComponentModel.Composition;
using System.IO;

namespace GPU
{
	[Export(typeof(IPlugin))]
	public class Gpu : IPlugin
	{
		#region private common information
		private static string pluginName = ((AssemblyTitleAttribute) Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title;
		
		private List<IndicatorSettings> indicators = new List<IndicatorSettings>
		{
			new IndicatorSettings(
				pluginName,						// Pluginname
				"NumberOfDevices",				// Indicatornname
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
				"NamePerDevice",							
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
		
			foreach (string indicator in indicatorNames) {
				
				if (indicator.Equals (indicators [0].IndicatorName)) {
					result.Add (GetNumberOfDevices ());
				} else if (indicator.Equals (indicators [1].IndicatorName)) {
					result.Add (GetNamePerDevice ());
				} else {
					throw new ArgumentOutOfRangeException ();
				}
				
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

		public Tuple<string, object, DataType> GetNumberOfDevices ()
		{
			
			return new Tuple<string, object, DataType> (indicators [0].IndicatorName, (Byte)GetVGAController().Count, indicators [0].DataType);
		}
		

		public Tuple<string, object, DataType> GetNamePerDevice ()
		{
			string val = "";
			foreach(string s in GetVGAController()){
				val+=s+",";
			}
			return new Tuple<string, object, DataType> (indicators [1].IndicatorName, val, indicators [1].DataType);
		}
		#endregion
		
		#region private aquisition
		public List<string> GetVGAController(){
			try {
				// This is the code for the base process
	            Process myProcess = new Process();
	            // Start a new instance of this program but specify the 'spawned' version.
	            ProcessStartInfo myProcessStartInfo = new ProcessStartInfo("lspci", "-mm");
	            myProcessStartInfo.UseShellExecute = false;
	            myProcessStartInfo.RedirectStandardOutput = true;
	            myProcess.StartInfo = myProcessStartInfo;
	            myProcess.Start();
	            StreamReader myStreamReader = myProcess.StandardOutput;
	            List<String> l = new List<String>();
				string myString = myStreamReader.ReadLine();
				while (myString != null){
					if (myString.Contains("VGA"))
						l.Add (myString.Split('"')[5]);
					myString = myStreamReader.ReadLine();
				}
	            
	            myProcess.WaitForExit();
	            myProcess.Close();
				return l;
			} catch (Exception e) {
				// Error in CPU PLUGIN: Name not readable!
				throw new Exception (e.Message);
			}	
		}
		#endregion
	}
}
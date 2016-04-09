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
using System.IO;

namespace NetworkAdapter
{
	[Export(typeof(IPlugin))]
	public class NetworkAdapter : IPlugin
	{
		#region private common information
		private static string pluginName = ((AssemblyTitleAttribute) Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title;
		
		private Dictionary<string,Tuple<DateTime,Int64>> lastUp = new Dictionary<string,Tuple<DateTime,Int64>>();
		private Dictionary<string,Tuple<DateTime,Int64>> lastDown = new Dictionary<string,Tuple<DateTime,Int64>>();
		
		private List<IndicatorSettings> indicators = new List<IndicatorSettings>
		{
			new IndicatorSettings(
				pluginName,						// Pluginname
				"NumberOfAdapters",				// Indicatornname
				"",								// WorkstationDomainName
				".",								// FilterStatement
				new TimeSpan (24, 0, 0),		// UpdateInterval
				new TimeSpan (365, 0, 0, 0),	// StorageDuration
				new TimeSpan (24, 0, 0),		// MappingDuration
				DataType.Byte,				// DataType
				"",								// Metric Warning
				""),							// Metric Critical
			
			new IndicatorSettings(
				pluginName,						
				"NamePerAdapter",							
				"",								
				".",								
				new TimeSpan (0, 0, 30),		
				new TimeSpan (365, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.String,				
				"",				
				""),							
				
			new IndicatorSettings(
				pluginName,						
				"IPPerAdapter",				
				"",								
				".",								
				new TimeSpan (0, 0, 30),		
				new TimeSpan (365, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.String,				
				"",		
				""),
				
			new IndicatorSettings(
				pluginName,						
				"MACPerAdapter",				
				"",								
				".",								
				new TimeSpan (24, 0, 0),		
				new TimeSpan (365, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.String	,				
				"",		
				""),
				
			new IndicatorSettings(
				pluginName,						
				"UpPerAdapter",				
				"",								
				".",								
				new TimeSpan (0, 0, 30),		
				new TimeSpan (365, 0, 0, 0),	
				new TimeSpan (24, 0, 0),		
				DataType.String,
				"",
				""),
				
			new IndicatorSettings(
				pluginName,						
				"DownPerAdapter",				
				"",								
				".",								
				new TimeSpan (0, 0, 30),		
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
		public NetworkAdapter ()
		{
			// Fill the indicatorDictionary
			indicatorDictionary.Add(indicators [0].IndicatorName, GetNumberOfAdapters);
			indicatorDictionary.Add(indicators [1].IndicatorName, GetNamePerAdapter);
			indicatorDictionary.Add(indicators [2].IndicatorName, GetIPPerAdapter);
			indicatorDictionary.Add(indicators [3].IndicatorName, GetMACPerAdapter);
			indicatorDictionary.Add(indicators [4].IndicatorName, GetUpPerAdapter);
			indicatorDictionary.Add(indicators [5].IndicatorName, GetDownPerAdapter);
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
		
		private string parse(List<string> list){
			String r = "";
			foreach(String s in list){
				r+=s+";";
			}
			return r;
		}

		public Tuple<string, Object, DataType> GetNumberOfAdapters ()
		{
			return new Tuple<string, Object, DataType>(indicators[0].IndicatorName, (byte)GetNetworkInterfaces().Count, indicators[0].DataType); 
		}

		public Tuple<string, Object, DataType> GetNamePerAdapter ()
		{
			return new Tuple<string, Object, DataType>(indicators[1].IndicatorName, parse (GetNetworkInterfaces()), indicators[1].DataType); 
		}

		public Tuple<string, Object, DataType> GetIPPerAdapter ()
		{
			string[] ipPrefixes = {"inet Adresse:","inet addr:"};
			string result = "";
			foreach(string dev in GetNetworkInterfaces()){
				foreach(string line in getCommand("ifconfig",dev)){
					var l = line.Trim();
					foreach(string pref in ipPrefixes){
						if (l.StartsWith(pref)){
							l= l.Remove(0,pref.Length);
							l=l.Split(' ')[0];
							result+=l+";";
						}
					}
				}
			}
			return new Tuple<string, Object, DataType>(indicators[2].IndicatorName, result, indicators[2].DataType); 
		}

		public Tuple<string, Object, DataType> GetMACPerAdapter ()
		{
			string[] macPrefixes = {"HWaddr ","Hardware Adresse "};
			string result = "";
			foreach(string dev in GetNetworkInterfaces()){
				foreach(string line in getCommand("ifconfig",dev)){
					var l = line.Trim();
					foreach(string pref in macPrefixes){
						int pos = l.IndexOf(pref);
						if (pos > -1){
							l= l.Remove(0,pos+pref.Length);
							l=l.Split(' ')[0];
							result+=l+";";
						}
					}
				}
			}
			return new Tuple<string, Object, DataType>(indicators[3].IndicatorName, result, indicators[3].DataType); 
		}

		public Tuple<string, Object, DataType> GetUpPerAdapter ()
		{
			string r = "";
			foreach(string dev in GetNetworkInterfaces()){
				r+=(int)GetCurrentUpload(dev)+";";
			}
			return new Tuple<string, Object, DataType>(indicators[4].IndicatorName, r, indicators[4].DataType); 
		}

		public Tuple<string, Object, DataType> GetDownPerAdapter ()
		{
			string r = "";
			foreach(string dev in GetNetworkInterfaces()){
				r+=(int)GetCurrentUpload(dev)+";";
			}
			return new Tuple<string, Object, DataType>(indicators[5].IndicatorName, r, indicators[5].DataType); 
		}
		#endregion
		
		#region private area
		public float GetCurrentDownload(string dev){
			string[] r={"RX-Bytes:","RX bytes:"};
			Int64 bytes =  GetRxTx(dev,r); 
			if (bytes == -1){
				return -1;
			}
			Tuple<DateTime,Int64> item;
			if (lastDown.ContainsKey(dev)){
				if (!(lastDown.TryGetValue(dev,out item))){
					return -1;
				}
			}
			else {
				item = new Tuple<DateTime, long>(DateTime.Now,0);
			}
			
			//calculate throughput
			var diffBytes = bytes-item.Item2;
			TimeSpan span = DateTime.Now.Subtract(item.Item1);
			
			
			// store the item in the list
			item = new Tuple<DateTime, long>(DateTime.Now,bytes);
			lastDown.Remove(dev);
			lastDown.Add(dev,item);
			// return
			return (float)(diffBytes/span.TotalSeconds);
		}
		
		public float GetCurrentUpload(string dev){
			string[] r={"TX-Bytes:","TX bytes:"};
			Int64 bytes =  GetRxTx(dev,r);
			if (bytes == -1){
				return -1;
			}
			Tuple<DateTime,Int64> item;
			if (lastUp.ContainsKey(dev)){
				if (!(lastUp.TryGetValue(dev,out item))){
					return -1;
				}
			}
			else {
				item = new Tuple<DateTime, long>(DateTime.Now,0);
			}
			
			//calculate throughput
			var diffBytes = bytes-item.Item2;
			TimeSpan span = DateTime.Now.Subtract(item.Item1);
			
			
			// store the item in the list
			item = new Tuple<DateTime, long>(DateTime.Now,bytes);
			lastUp.Remove(dev);
			lastUp.Add(dev,item);
			// return
			return (float)(diffBytes/span.TotalSeconds);
			
		}
		private Int64 GetRxTx(string dev, string[]prefixes){
			foreach(string line in getCommand("ifconfig",dev)){
				var l = line.Trim();
				foreach(string pref in prefixes){
					int pos = l.IndexOf(pref);
					if (pos > -1){
						l= l.Remove(0,pos+pref.Length);
						l=l.Split(' ')[0];
						try{
							return Int64.Parse(l);
						}
						catch{
							return -1;
						}
					}
				}
			}
			return -1;
		}
				
		
		public List<string> GetNetworkInterfaces(){
			List<string> InterfaceList = new List<string>();
			try{
				PerformanceCounterCategory net = new PerformanceCounterCategory("Network Interface");
				foreach(string name in net.GetInstanceNames()){
					if (!name.Equals("lo"))
						InterfaceList.Add(name);
				}
			}
			catch(Exception e){
				Logger.Instance.WriteWorkstationEntry("Network Interfaces couldn't be getted", LogType.Warning);
			}
			return InterfaceList;
		}
		
		public List<String> getCommand(string command, string attribute)
		{
			List<String> l = new List<String>();
			try {
				// This is the code for the base process
	            Process myProcess = new Process();
	            // Start a new instance of this program but specify the 'spawned' version.
	            ProcessStartInfo myProcessStartInfo = new ProcessStartInfo(command, attribute);
	            myProcessStartInfo.UseShellExecute = false;
	            myProcessStartInfo.RedirectStandardOutput = true;
	            myProcess.StartInfo = myProcessStartInfo;
	            myProcess.Start();
	            StreamReader myStreamReader = myProcess.StandardOutput;
	            string myString = myStreamReader.ReadLine();
				while (myString != null){
					l.Add (myString);
					myString = myStreamReader.ReadLine();
				}
	            
	            myProcess.WaitForExit();
	            myProcess.Close();
				
			} catch (Exception e) {
				
			}	
			return l;
		}
		#endregion
	}
}
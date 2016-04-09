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
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

using MISD.Core;

namespace MISD.Workstation.Linux
{
    /// <summary>
    /// This class is provides the connection to the MISD Server.
    /// More precisely it's the connection to the WorkstationWebService.
    /// So with this class you can call the methods of the WorkstationWebService indirectly.
    /// </summary>
    public class ServerConnection
    {
		#region Singleton
		/// <summary>
		/// The singleton instance of ServerConnection.
		/// </summary>
        private static volatile ServerConnection instance;

		/// <summary>
		/// The sync root for locking.
		/// </summary>
		private static object syncRoot = new Object();

        /// <summary>
        /// Property to access the class.
        /// </summary>
        public static ServerConnection Instance
        {
            get
            {
                if (instance == null)
                {
					lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ServerConnection();
                    }
                }
                return instance;
            }
        }
		#endregion

        private WSWebService.WorkstationWebServiceClient webService;

        private ServerConnection()
        {
            // Build the connection to the WorkstationWebService
            webService = new WSWebService.WorkstationWebServiceClient("BasicHttpBinding_IWorkstationWebService");
        }

		/// <summary>
		/// Gets the web service adress.
		/// </summary>
		/// <returns>
		/// The web service uri if available, otherwise null.
		/// </returns>
		public System.Uri GetWebServiceURI ()
		{
			var clientSection = (ClientSection)ConfigurationManager.GetSection ("system.serviceModel/client");
			ChannelEndpointElementCollection endpointCollection = (ChannelEndpointElementCollection)clientSection.ElementInformation.Properties [string.Empty].Value;
			if (endpointCollection.Count >= 1) {
				return endpointCollection[0].Address;
			} else {
				return null;
			}
		}

        public WSWebService.WorkstationWebServiceClient GetWorkstationWebService ()
		{
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler (unhandledException);
			try {
				if (webService == null) {
					new ServerConnection ();		
				}
				return webService;
			} catch (System.ServiceModel.EndpointNotFoundException) {
				WorkstationLogger.Instance.WriteLog("Unable to create WebService object.", MISD.Core.LogType.Exception, false);
				return null;
			}
        }

		private void unhandledException (object sender, UnhandledExceptionEventArgs e)
		{
			WorkstationLogger.Instance.WriteLog("[Unhandled Exception] " + e.ExceptionObject.ToString(), MISD.Core.LogType.Exception, false);
		}

        public WSWebService.WorkstationWebServiceClient RestoreConnection()
        {
            // Restore the connection.
            webService = new WSWebService.WorkstationWebServiceClient("BasicHttpBinding_IWorkstationWebService");
			return webService;
        }
		
		public string GetWorkstationName()
		{
			// Acquire the FQDN of the workstation.
			string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
			string hostName = Dns.GetHostName();
			string fqdn = "";
			if (!hostName.Contains(domainName))
				fqdn = hostName + "." + domainName;
			else
				fqdn = hostName;
		
			return fqdn;
		}
		
		/// <summary>
		/// Determines whether the workstation is connected to the web service.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the workstation is connected to the WebService; otherwise, <c>false</c>.
		/// </returns>
		public bool IsConnected()
		{
			try {
		        WebRequest request = WebRequest.Create(ServerConnection.Instance.GetWebServiceURI());
		        request.GetResponse();
				return true;
		    }
		    catch (Exception)
		    {
				return false;
		    }
		}

		/// <summary>
		/// Gets the MAC address.
		/// </summary>
		/// <returns>
		/// The most low-ordered MAC address. Null if no MAC-Address is readable.
		/// </returns>
		public string GetMACAddress ()
		{
			string stringToReturn = null;
			List<string> macAddressesAsString = new List<string>();

			try {
				// Prepare the process for starting the program "ifconfig"
				ProcessStartInfo ps = new ProcessStartInfo ("ifconfig");
				ps.UseShellExecute = false;
				ps.RedirectStandardOutput = true;
 
				// starts the process
				using (Process p = Process.Start (ps)) {
					string output = p.StandardOutput.ReadToEnd ();
					Regex oRegex = new Regex(@".*(?<temp>[0-9|a-f|A-F]{2}:[0-9|a-f|A-F]{2}:[0-9|a-f|A-F]{2}:[0-9|a-f|A-F]{2}:[0-9|a-f|A-F]{2}:[0-9|a-f|A-F]{2}).*");

					// Get the various MAC addresses
					MatchCollection oMatchCollection = oRegex.Matches (output);
					foreach (Match oMatch in oMatchCollection) {
						macAddressesAsString.Add(oMatch.Groups ["temp"].ToString());
					} 
					p.WaitForExit ();					
				}

				// Find the MAC address with the lowester order.
				long minValue = long.MaxValue;
				foreach (string macAddressWithDoublePoint in macAddressesAsString)
				{
					// Calculate the int value
					string macAddress = macAddressWithDoublePoint.Replace(":", null);
					long value = Convert.ToInt64(macAddress, 16);

					// Compare the current value with the previous minimal value
					if (value < minValue)
					{
						minValue = value;
						stringToReturn = macAddressWithDoublePoint;
					}
				}

				return stringToReturn;
				
			} catch (Exception e) {
				WorkstationLogger.Instance.WriteLog(e.Message, LogType.Exception, true);
				return null;
			}
		}

		#region WorkstationWebService methods
		/// <summary>
        /// logs the an event sent from a workstation.
        /// </summary>
        /// <param name="message">event description</param>
        /// <param name="type">event type</param>
        /// <returns></returns>
		public void WriteLog(string message, LogType type)
		{
			try
			{
				GetWorkstationWebService ().WriteLog(message, type);
			}
			catch (Exception)
			{
				WorkstationLogger.Instance.WriteLog ("Unable to call WriteLog() at server.", MISD.Core.LogType.Exception, false);
			}
		}
		
		public bool SignIn ()
		{
			while (true) {
				try {
					return GetWorkstationWebService ().SignIn (ServerConnection.Instance.GetWorkstationName(), ServerConnection.Instance.GetMACAddress(), (byte) MISD.Core.Platform.Linux);
				}
				catch (System.ServiceModel.EndpointNotFoundException)
				{
					WorkstationLogger.Instance.WriteLog ("Unable to call SignIn() at server.", MISD.Core.LogType.Exception, false);
					Thread.Sleep(5000);
				}
				catch (Exception e)
				{
					WorkstationLogger.Instance.WriteLog (e.Message, MISD.Core.LogType.Exception, true);
					Thread.Sleep(5000);
				}
			}
		}
		
		public bool SignOut()
		{
			try
			{
				return GetWorkstationWebService ().SignOut(ServerConnection.Instance.GetMACAddress());
			}
			catch (Exception)
			{
				WorkstationLogger.Instance.WriteLog ("Unable to call SignOut() at server.", MISD.Core.LogType.Exception, false);
				return false;
			}
		}

		public MISD.Core.PluginMetadata[] GetPluginList ()
		{
			try
			{
				return GetWorkstationWebService ().GetPluginList(ServerConnection.Instance.GetMACAddress());
			}
			catch (Exception)
			{
				WorkstationLogger.Instance.WriteLog ("Unable to call GetPluginList() at server.", MISD.Core.LogType.Exception, false);
				return new Core.PluginMetadata[0];
			}
		}

		public TimeSpan GetMainUpdateInterval (TimeSpan oldMainUpdateIntervall)
		{
			try
			{
				return GetWorkstationWebService ().GetMainUpdateInterval();
			}
			catch (Exception)
			{
				WorkstationLogger.Instance.WriteLog ("Unable to call GetMainUpdateInterval() at server.", MISD.Core.LogType.Exception, false);
				return oldMainUpdateIntervall;
			}
		}

		public Core.PluginFile[] DownloadPlugins (string[] pluginNames)
		{
			try
			{
				return GetWorkstationWebService ().DownloadPlugins(ServerConnection.Instance.GetMACAddress(), pluginNames);
			}
			catch (Exception)
			{
				WorkstationLogger.Instance.WriteLog ("Unable to call DownloadPlugins() at server.", MISD.Core.LogType.Exception, false);
				return null;
			}
		}

		public bool UploadIndicatorValue (string pluginName, string indicatorValueName, object value, MISD.Core.DataType valueDataType, DateTime acquiredTimestamp)
		{
			try
			{
				return GetWorkstationWebService ().UploadIndicatorValue(ServerConnection.Instance.GetMACAddress (), pluginName, indicatorValueName, value, valueDataType, acquiredTimestamp);
			}
			catch (Exception)
			{
				WorkstationLogger.Instance.WriteLog ("Unable to call UploadIndicatorValue() at server.", MISD.Core.LogType.Exception, false);
				return false;
			}
		}

		public string GetFilter (string pluginName, string indicatorName, string oldFilter)
		{
			try
			{
				return GetWorkstationWebService ().GetFilter(ServerConnection.Instance.GetMACAddress (), pluginName, indicatorName);
			}
			catch (Exception)
			{
				WorkstationLogger.Instance.WriteLog ("Unable to call GetFilter() at server.", MISD.Core.LogType.Exception, false);
				return oldFilter;
			}
		}

		public long GetUpdateInterval(string pluginName, string indicatorName, long oldUpdateInterval)
		{
			try
			{
				long returnValue =  GetWorkstationWebService ().GetUpdateInterval(ServerConnection.Instance.GetMACAddress (), pluginName, indicatorName);
				if (returnValue <= 0) 
				{
					return oldUpdateInterval;
				}
				else
				{
					return returnValue;
				}
			}
			catch (Exception)
			{
				WorkstationLogger.Instance.WriteLog ("Unable to call GetUpdateInterval() at server.", MISD.Core.LogType.Exception, false);
				return oldUpdateInterval;
			}

		}
    	#endregion
	}
}
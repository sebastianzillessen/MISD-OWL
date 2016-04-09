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
using MISD.Core;
using System.Threading;
using System.Management;

namespace MISD.Workstation.Windows
{
    /// <summary>
    /// This class is provides the connection to the MISD Server.
    /// More precisely it's the connection to the WorkstationWebService.
    /// So with this class you can call the methods of the WorkstationWebService indirectly.
    /// </summary>
    public class ServerConnection
    {
        private static WSWebService.WorkstationWebServiceClient webService;
        private static string macAdress = null;
        private static string workstationName = null;

        public static WSWebService.WorkstationWebServiceClient GetWorkstationWebService()
        {
            if (webService == null)
            {
                // Build the connection to the WorkstationWebService
                webService = new WSWebService.WorkstationWebServiceClient();
            }
            return webService;
        }

        public static void RestoreConnection()
        {
            // Restore the connection.
            webService = new WSWebService.WorkstationWebServiceClient();
        }

        public static string GetWorkstationName()
        {
            if (workstationName == null)
            {
                // Acquire the FQDN of the workstation.
                string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
                string hostName = Dns.GetHostName();
                if (!hostName.Contains(domainName))
                {
                    workstationName = hostName + "." + domainName;
                }
                else
                {
                    workstationName = hostName;
                }
            }
            return workstationName;
        }

        /// <summary>
        /// Gets the MAC Adress of a physical unit.
        /// </summary>
        /// <returns></returns>
        public static string GetMacAdress()
        {
            if (macAdress == null)
            {
                List<string> physicalAdapters = new List<string>();
                ManagementObjectSearcher adapterSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration");

                // Collect every physical MAC Adress
                foreach (ManagementObject obj in adapterSearcher.Get())
                {
                    if (obj["IPEnabled"] != null && obj["MACAddress"] != null)
                    {
                        try
                        {
                            if (Convert.ToBoolean(obj["IPEnabled"]))
                            {
                                physicalAdapters.Add(obj["MACAddress"].ToString());
                            }
                        }
                        catch
                        {
                            // value not convertable?
                        }
                    }
                }

                // Select the "smallest"
                long minValue = long.MaxValue;
                foreach (string macAddressWithDoublePoint in physicalAdapters)
                {
                    // Calculate the int value
                    string macAddress = macAddressWithDoublePoint.Replace(":", null);
                    long value = Convert.ToInt64(macAddress, 16);

                    // Compare the current value with the previous minimal value
                    if (value < minValue)
                    {
                        minValue = value;
                        macAdress = macAddressWithDoublePoint;
                    }
                }
            }
            return macAdress;

        }

        #region WorkstationWebService methods
        /// <summary>
        /// logs the an event sent from a workstation.
        /// </summary>
        /// <param name="message">event description</param>
        /// <param name="type">event type</param>
        /// <returns></returns>
        public static void WriteLog(string message, LogType type)
        {
            try
            {
                GetWorkstationWebService().WriteLog(message, type);
            }
            catch (Exception e)
            {
                WorkstationLogger.WriteLog("Couldn't write log to remote system.");
            }
        }

        public static bool SignIn(byte operatingSystem)
        {
            while (true)
            {
                try
                {
                    return GetWorkstationWebService().SignIn(ServerConnection.GetWorkstationName(), ServerConnection.GetMacAdress(), operatingSystem);
                }
                catch (Exception)
                {
                    WorkstationLogger.WriteLog("Unable to call SignIn() at server.");
                    Thread.Sleep(5000);
                }
            }
        }

        public static bool SignOut()
        {
            try
            {
                return GetWorkstationWebService().SignOut(ServerConnection.GetMacAdress());
            }
            catch (Exception)
            {
                WorkstationLogger.WriteLog("Unable to call SignOut() at server.");
                return false;
            }
        }

        public static Core.PluginMetadata[] GetPluginList()
        {
            try
            {
                return GetWorkstationWebService().GetPluginList(ServerConnection.GetMacAdress());
            }
            catch (Exception)
            {
                WorkstationLogger.WriteLog("Unable to call GetPluginList() at server.");
                return new Core.PluginMetadata[0];
            }
        }

        public static TimeSpan GetMainUpdateInterval(TimeSpan oldMainUpdateIntervall)
        {
            try
            {
                return GetWorkstationWebService().GetMainUpdateInterval();
            }
            catch (Exception)
            {
                WorkstationLogger.WriteLog("Unable to call GetMainUpdateInterval() at server.");
                return oldMainUpdateIntervall;
            }
        }

        public static Core.PluginFile[] DownloadPlugins(string[] pluginNames)
        {
            try
            {
                return GetWorkstationWebService().DownloadPlugins(ServerConnection.GetMacAdress(), pluginNames);
            }
            catch (Exception)
            {
                WorkstationLogger.WriteLog("Unable to call DownloadPlugins() at server.");
                return null;
            }
        }

        public static bool UploadIndicatorValues(string pluginName, Tuple<string, object, DataType, DateTime>[] indicatorValues)
        {
            try
            {
                return GetWorkstationWebService().UploadIndicatorValues(ServerConnection.GetMacAdress(), pluginName, indicatorValues);
            }
            catch (Exception)
            {
                WorkstationLogger.WriteLog("Unable to call UploadIndicatorValue() at server.");
                return false;
            }
        }


        public static Tuple<string, string>[] GetFilters(string pluginName, Tuple<string, string>[] oldFilters)
        {
            try
            {
                return GetWorkstationWebService().GetFilters(ServerConnection.GetMacAdress(), pluginName);
            }
            catch (Exception)
            {
                WorkstationLogger.WriteLog("Unable to call GetFilter() at server.");
                return oldFilters;
            }
        }

        public static Tuple<string, long?>[] GetUpdateIntervals(string pluginName, Tuple<string, long?>[] oldUpdateIntervals)
        {
            try
            {
                return GetWorkstationWebService().GetUpdateIntervals(ServerConnection.GetMacAdress(), pluginName);
            }
            catch (Exception)
            {
                WorkstationLogger.WriteLog("Unable to call GetUpdateInterval() at server.");
                return oldUpdateIntervals;
            }
        }

        #endregion

    }
}
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
using System.Threading;
using Renci.SshNet;
using MISD.Core;
using System.Runtime.CompilerServices;

namespace MISD.Server.Cluster
{
    public class BrightClusterConnection : ClusterConnection
    {
        #region Attributes
        /// <summary>
        /// Stores the Url of the head-node
        /// </summary>
        public string url { get; private set; }
        /// <summary>
        /// Stores the username of the headnode
        /// </summary>
        public string username { get; private set; }
        /// <summary>
        /// stores the password of the headnode
        /// </summary>
        public string password { get; private set; }

        private List<string> notifiedAlerts = new List<string>();

        /// <summary>
        /// the suffix url, which is used to generate a node name.
        /// </summary>
        private string suffix_url = "";

        // Track whether Dispose has been called.
        private bool disposed = false;

        #endregion

        #region ClusterConnection Methods
        public override void Init(string url, string username, string password)
        {
            var urlParts = url.Split('.');
            suffix_url = "." + String.Join(".", urlParts.Skip(1).ToArray());

            // set variables
            this.url = url;
            this.username = username;
            this.password = password;

            // just testing
            //if (suffix_url != ".visus.uni-stutttgart.de")
            //{
            //    throw new IndexOutOfRangeException("suffix ist falsch!");
            //}
            this.ConnectionObject = new BrightClusterShell(url, username, password);
        }

        public override ClusterConnection CopyConnection()
        {
            if (this.ConnectionObject == null)
            {
                Logger.Instance.WriteEntry("BrightClusterConnection_CopyConnection(): Connection was null and cannot be copied", LogType.Warning);
                throw new Exception("BrightClusterConnection_CopyConnection: You didn't call INIT before accessing the Cluster connection. Do this before any calls!");
            }
            ClusterConnection c = new BrightClusterConnection();
            c.Init(this.url, this.username, this.password);
            return c;
        }

        public override List<WorkstationInfo> GetNodes()
        {
            if (this.ConnectionObject == null)
            {
                Logger.Instance.WriteEntry("BrightClusterConnection_CopyConnection(): Connection was null and cannot be copied", LogType.Warning);
                throw new Exception("BrightClusterConnection_GetNodes: You didn't call INIT before accessing the Cluster connection.");
            }

            List<Core.WorkstationInfo> nodes = new List<WorkstationInfo>();
            string devices = ((BrightClusterShell)this.ConnectionObject).RunCommands(new List<string> { "device list" });
            string devicesHealth = ((BrightClusterShell)this.ConnectionObject).RunCommands(new List<string> { "device status" });

            string[] devicelist = devices.ToString().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            string[] devicesHealthList = devicesHealth.ToString().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string current in devicelist)
            {
                if (current.Contains("PhysicalNode") || current.Contains("MasterNode"))
                {
                    // columns: type, name, mac, desc, ip, unknown, unknown, unknown
                    string[] details = current.ToString().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    WorkstationInfo currentNode = new WorkstationInfo();
                    currentNode.CurrentOS = MISD.Core.Platform.Bright.ToString();
                    currentNode.Name = details.ElementAt(1);
                    currentNode.FQDN = BuildFQDN(currentNode.Name);
                    currentNode.IsAvailable = false;

                    foreach (string currentLine in devicesHealthList)
                    {
                        if (currentLine.Contains(details.ElementAt(1)) && currentLine.Contains("[   UP   ]"))
                        {
                            currentNode.IsAvailable = true;
                        }
                    }
                    currentNode.MacAddress = details.ElementAt(2);
                    currentNode.State = MISD.Core.MappingState.OK;

                    nodes.Add(currentNode);
                }
            }
            return nodes;
        }

        /// <summary>
        /// Disposes this Connetion
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (!this.disposed && this.ConnectionObject != null)
            {
                try
                {
                    ((BrightClusterShell)this.ConnectionObject).Dispose();
                    this.disposed = true;
                }
                finally
                {
                    // Call Dispose on your base class.
                    base.Dispose(disposing);
                }
            }
        }
        #endregion

        /// <summary>
        /// Generates a FQDN Name from a given node-name for this bright cluster by adding the cluster suffix to the host name.
        /// buildFQDN and getHostname are inverse to each other: buildFQDN(getHostname(x))=x
        /// </summary>
        /// <param name="host">The brightCluster host name.</param>
        /// <returns>The fqdn for this cluster name.</returns>
        public string BuildFQDN(string host)
        {
            return host + suffix_url;
        }

        /// <summary>
        /// Returns the name of a cluster node by removing the cluster suffix from the given fqdn.
        /// buildFQDN and getHostname are inverse to each other: buildFQDN(getHostname(x))=x
        /// </summary>
        /// <param name="fqdn">The fqdn name of the node.</param>
        /// <returns>The node name.</returns>
        public string GetHostname(string fqdn)
        {
            var hostname = fqdn.Replace(suffix_url, "");
            return hostname;
        }

        /// <summary>
        /// Gets the line of the latest metricdata output of a node specified by nodename. Parameter says which line should be used.
        /// only the part after the first " " is returned.
        /// </summary>
        /// <param name="fqdn">The cluster node fqdn.</param>
        /// <param name="parameter">The metric to get.</param>
        /// <returns></returns>
        public string GetLatestMetricData(string fqdn, string parameter)
        {
            return GetResult(parameter, GetHostname(fqdn), "latestmetricdata");
        }

        /// <summary>
        /// Gets the line of the static sysdtem information output of a node specified by nodename. Parameter says which line should be used.
        /// only the part after the first " " is returned.
        /// </summary>
        /// <param name="fqdn">The cluster node name.</param>
        /// <param name="parameter">The metric to get.</param>
        /// <returns></returns>
        public string GetSysinfo(string fqdn, string parameter)
        {
            return GetResult(parameter, GetHostname(fqdn), "sysinfo");
        }

        /// <summary>
        /// calls the RunCommands from the shell and searches the correct attribute.  
        /// </summary>
        /// <param name="parameter">the attribute to search in</param>
        /// <param name="hostname">the hostname which should be called.</param>
        /// <param name="call">The call method (sysinfo or latestmetricdata)</param>
        /// <returns>
        /// - If the attribute isn't found for this hostname, then a ArgumentNullException is raised.
        /// - If the shell isn't defined, a Exception is raised.
        /// - If everything is right, returns the result of the parameter as string
        /// </returns>
        private string GetResult(string parameter, string hostname, string call)
        {
            string r = "";
            if (this.ConnectionObject == null)
            {
                Logger.Instance.WriteEntry("BrightClusterConnection_CopyConnection(): Connection was null and cannot be copied", LogType.Warning);
                throw new Exception("BrightClusterConnection_GetResult: You didn't call INIT before accessing the Cluster connection.");
            }
            List<string> commands = new List<string> { "device", "use " + hostname, call };
            string cmd_result = ((BrightClusterShell)this.ConnectionObject).RunCommands(commands);
            if (cmd_result != null && !cmd_result.Trim().Equals(""))
            {
                string[] resultLines = cmd_result.ToString().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                // find beginning and end
                for (int i = 0; i < resultLines.Length; i++)
                {
                    if (resultLines.ElementAt(i).Length > 0 && resultLines.ElementAt(i).StartsWith(parameter))
                    {
                        r = resultLines.ElementAt(i).Substring(parameter.Length).Trim();
                        // check if next line starts with many empty characters.
                        i++;
                        if (i < resultLines.Length && resultLines.ElementAt(i).StartsWith("    "))
                        {
                            r += " "+resultLines.ElementAt(i).Trim();
                        }
                        break;
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(r) || r.Equals("no data"))
            {
                // log only the first time if this error occurs
                if (!notifiedAlerts.Contains(hostname + "_" + parameter))
                {
                    MISD.Core.Logger.Instance.WriteEntry("BrightClusterConnection_GetResult(" + call + "): No entry with name "
                        + parameter + " found for " + hostname + ". Returning null.", LogType.Warning);
                    if (notifiedAlerts.Count > 500)
                        notifiedAlerts.Clear();
                    notifiedAlerts.Add(hostname + "_" + parameter);
                }
                throw new ArgumentNullException("BrightClusterConnection_GetResult: Element " + parameter + " NOT found for " + hostname + ".");
            }
            else
            {
                return r;
            }

        }
    }
}
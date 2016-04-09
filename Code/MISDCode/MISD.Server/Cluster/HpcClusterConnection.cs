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

namespace MISD.Server.Cluster
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MISD.Core;
    using Microsoft.Hpc.Scheduler;
    using System.Net.Sockets;
    using System.Management.Automation.Runspaces;
    using System.Security;
    using System.Management.Automation;
    using System.Management.Automation.Remoting;
    using System.Threading;
    using Microsoft.Hpc.Scheduler.Properties;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides methods for HPC-Cluster connection and basic information retrieval.
    /// </summary>
    public class HpcClusterConnection : ClusterConnection
    {
        // this list is used to dispose and disconnect all HpcClusterConnectionObjects on shutdown
        public static List<HpcClusterConnection> HpcClusterConnectionObjects = new List<HpcClusterConnection>();


        private String url;
        private String username;
        private String password;

        private int nextRunspaceForPipeline = 0;

        private static readonly int amountOfRunspaces = 5;

        private Runspace[] runspaces = new Runspace[amountOfRunspaces];

        private Semaphore[] pipelineSemaphores = new Semaphore[amountOfRunspaces];
        private Semaphore[] refreshingRunspaceSemaphore = new Semaphore[amountOfRunspaces];

        private Semaphore getPipelineSemaphore = new Semaphore(1, 100);

        private WSManConnectionInfo connectionInfo = null;

        // Track whether Dispose has been called.
        private bool disposed = false;


        /// <summary>
        /// Initializes the HPC PowerShell and the scheduler connection.
        /// The username and password are used for the PowerShell-connection,
        /// the scheduler uses the server-credentials
        /// </summary>
        /// <param name="url">URL of the HPCs headnode.</param>
        /// <param name="username">Username for the PowerShell-connection.</param>
        /// <param name="password">Password for the PowerShell-connection</param>
        public override void Init(string url, string username, string password)
        {
            this.url = url;
            this.username = username;
            this.password = password;
            this.ConnectionObject = new Scheduler();

            HpcClusterConnection.HpcClusterConnectionObjects.Add(this);

            //format url for scheduler
            string schedulerUrl = url.Substring(7, url.Length - 7);
            schedulerUrl = schedulerUrl.Split(':')[0];

            //connect scheduler
            try
            {
                ((IScheduler)this.ConnectionObject).Connect(schedulerUrl);
            }
            catch (Exception e)
            {
                var messageEx = new StringBuilder();
                messageEx.Append("HpcClusterConnection: ");
                messageEx.Append("Could not connect scheduler to headnode \"" + schedulerUrl + "\"\n");
                messageEx.Append(e.StackTrace);
                MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);
            }

            //connect to remote PowerShell
            SecureString securePassword = new SecureString();
            password.ToCharArray().ToList().ForEach(c => securePassword.AppendChar(c));

            PSCredential credential = new PSCredential(username, securePassword);
            this.connectionInfo = new WSManConnectionInfo(new Uri(url), "", credential);

            //init runspaces and semaphores
            for (int i = 0; i < amountOfRunspaces; i++)
            {
                runspaces[i] = RunspaceFactory.CreateRunspace();
                pipelineSemaphores[i] = new Semaphore(1, 100);
                refreshingRunspaceSemaphore[i] = new Semaphore(1, 100);
            }

            for (int i = 0; i < amountOfRunspaces; i++)
            {
                try
                {
                    while (!refreshRunspace(i))
                    {
                        Logger.Instance.WriteEntry("Failed to connect to runspace #" + i + ". Retrying in 5 seconds.", LogType.Info);
                        Thread.Sleep(5000);
                    }
                }
                catch (Exception e)
                {
                    var messageEx2 = new StringBuilder();
                    messageEx2.Append("HpcClusterConnection: ");
                    messageEx2.Append("Initializing Runspaces " + i + " failed!");
                    messageEx2.Append(e.Message);
                    MISD.Core.Logger.Instance.WriteEntry(messageEx2.ToString(), LogType.Exception);
                }
            }
        }

        /// <summary>
        /// Reconnects the runspaces with the given number.
        /// </summary>
        /// <param name="runspaceNo">The number of the runspace that needs to be reconnected.</param>
        private Boolean refreshRunspace(int runspaceNo)
        {
            var runspacesConnected = false;

            if (!refreshingRunspaceSemaphore[runspaceNo].WaitOne(5000))
            {
                Logger.Instance.WriteEntry("Time out: semaphore #1", LogType.Info);
            }

            pipelineSemaphores[runspaceNo] = new Semaphore(1, 100);

            try
            {
                runspaces[runspaceNo].Close();
            }
            catch (InvalidRunspaceStateException e)
            { 
            }

            runspaces[runspaceNo] = RunspaceFactory.CreateRunspace(this.connectionInfo);

            var messageEx1 = new StringBuilder();
            messageEx1.Append("HpcClusterConnection: ");
            messageEx1.Append("Refreshed Runspace " + runspaceNo + ".");
            MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Info);

            try
            {
                runspaces[runspaceNo].Open();
                runspacesConnected = true;
            }
            catch (Exception e)
            {
                var messageEx = new StringBuilder();
                messageEx.Append("HpcClusterConnection: ");
                messageEx.Append("Could not connect runspace (" + runspaceNo + ") to headnode \"" + url + "\"\n");
                messageEx.Append(e.StackTrace);
                MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);
            }
            finally
            {
                refreshingRunspaceSemaphore[runspaceNo].Release();
            }
            return runspacesConnected;
        }

        public override ClusterConnection CopyConnection()
        {
            return this;
        }


        /// <summary>
        /// Creates a new Pipeline on one of the runspaces.
        /// </summary>
        /// <returns>New pipeline-object</returns>
        public HpcPipelineObject getPipeline()
        {
            getPipelineSemaphore.WaitOne();

            nextRunspaceForPipeline++;

            if (nextRunspaceForPipeline > amountOfRunspaces - 1)
            {
                nextRunspaceForPipeline = 0;
            }

            try
            {
                if (!this.pipelineSemaphores[nextRunspaceForPipeline].WaitOne(200))
                {
                    getPipelineSemaphore.Release();
                    return getPipeline();
                }


                if (this.runspaces[nextRunspaceForPipeline].RunspaceStateInfo.State != RunspaceState.Opened)
                {
                    Logger.Instance.WriteEntry("Runspace #" + nextRunspaceForPipeline + " is not in the opened state. Refreshing..", LogType.Info);

                    try
                    {
                        this.runspaces[nextRunspaceForPipeline].Close();
                    }
                    catch (Exception)
                    {
                        //No Error handling neccessary
                    }

                    this.refreshRunspace(nextRunspaceForPipeline);
                    getPipelineSemaphore.Release();

                    Thread.Sleep(1000);

                    return getPipeline();                  
                }

                var pipeline = this.runspaces[nextRunspaceForPipeline].CreatePipeline();
                getPipelineSemaphore.Release();

                HpcPipelineObject newPipelineObject = new HpcPipelineObject(nextRunspaceForPipeline, pipeline);

                return newPipelineObject;
            }
            catch (Exception e)
            {
                var messageEx = new StringBuilder();
                messageEx.Append("HpcClusterConnection: ");
                messageEx.Append("Runspace no. (" + nextRunspaceForPipeline + ") seems to be corrupted. Reinitialising...");
                messageEx.Append(e.ToString());
                Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Warning);        

                refreshRunspace(nextRunspaceForPipeline);

                return getPipeline();
            }
        }

        /// <summary>
        /// Closes and disposes Pipeline. Releases semaphore.
        /// </summary>
        /// <param name="pipelineObject"></param>
        public void freePipeline(HpcPipelineObject pipelineObject)
        {
            try
            {
                int pipelineNumber = pipelineObject.pipelineNumber;

                pipelineObject.Dispose();

                pipelineSemaphores[pipelineNumber].Release();
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("Failed to free pipeline", LogType.Exception);
            }
        }

        /// <summary>
        /// Acquires all available information of each cluster-node.
        /// </summary>
        /// <returns>List of WorkstationInfos conataining the node-information.</returns>
        public override List<WorkstationInfo> GetNodes()
        {
            List<Core.WorkstationInfo> nodes = new List<WorkstationInfo>();

            string utilityAddress = url.Substring(7, url.Length - 7);
            utilityAddress = utilityAddress.Split(':')[0];
            utilityAddress = utilityAddress.Split('.')[0];


            HpcPipelineObject pipelineObject = this.getPipeline();
            Dictionary<string, string> nodeInformation = HpcUtility.getSubnetInformation(utilityAddress, this);
            this.freePipeline(pipelineObject);


            Dictionary<string, NodeState> hpcNodesAvailability = new Dictionary<string, NodeState>();
            foreach (ISchedulerNode node in ((IScheduler)this.CopyConnection().GetConnection()).GetNodeList(null, null))
            {
                hpcNodesAvailability.Add(node.Name, node.State);
            }

            foreach (KeyValuePair<String, String> entry in nodeInformation)
            {
                if (hpcNodesAvailability.ContainsKey(entry.Key))
                {

                    WorkstationInfo workstationInfo = new WorkstationInfo();

                    workstationInfo.CurrentOS = (MISD.Core.Platform.HPC).ToString();
                    workstationInfo.Name = entry.Key;
                    workstationInfo.FQDN = entry.Key + ".visus.uni-stuttgart.de";
                    workstationInfo.MacAddress = entry.Value;

                    NodeState availability;
                    hpcNodesAvailability.TryGetValue(entry.Key, out availability);
                    if (availability.ToString().Equals("Online"))
                    {
                        workstationInfo.IsAvailable = true;
                    }
                    else
                    {
                        workstationInfo.IsAvailable = false;
                    }

                    workstationInfo.State = MISD.Core.MappingState.OK;

                    nodes.Add(workstationInfo);
                }
            }
            return nodes;
        }

        /// <summary>
        /// Disposes this Connetion
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                try
                {
                    ((Scheduler)this.ConnectionObject).Dispose();

                    for (int i = 0; i < this.runspaces.Length; i++)
                    {
                        try
                        {
                            runspaces[i].Close();
                            runspaces[i].Dispose();
                        }
                        catch (Exception)
                        {
                            Logger.Instance.WriteEntry("HpcClusterConnection: Failed to dispose runspace no. " + i
                                             + ". An eventual restart may exceed the runspace limit.", LogType.Exception);
                        }
                    }
                    this.disposed = true;
                }
                finally
                {
                    // Call Dispose on your base class.
                    base.Dispose(disposing);
                }
            }
        }
    }
}
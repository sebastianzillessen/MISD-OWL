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
using MISD.Core;
using MISD.Server.Manager;

namespace MISD.Server.Services
{
    /// <summary>
    /// This WebService provides functionality for the client.
    /// </summary>
    public class ClientWebService : IClientWebService
    {
        #region Plugin Management

        public List<PluginMetadata> GetPluginList(string plattform)
        {
            return PluginManager.Instance.GetPluginList(PlatformHelper.ParsePlatform(plattform));
        }

        public List<PluginFile> DownloadPlugins(List<string> pluginNames)
        {
            return PluginManager.Instance.DownloadPlugins(Platform.Visualization, pluginNames);
        }

        #endregion

        #region Filter, Metriken, Mapping und Aktualisierungsintervalle

        public List<IndicatorSettings> GetIndicatorSetting(string monitoredSystemMAC)
        {
            return MISD.Server.Manager.WorkstationManager.Instance.GetIndicatorSetting(monitoredSystemMAC);
        }

        public bool SetIndicatorSetting(List<IndicatorSettings> settings)
        {
            return MISD.Server.Manager.WorkstationManager.Instance.SetIndicatorSetting(settings);
        }
        #endregion

        #region MonitoredSystem

        public bool ResetMapping(List<Tuple<string, DateTime>> macList)
        {
            return ValueManager.Instance.ResetMapping(macList);
        }

        public List<string> ActivateMaintenanceMode(List<Tuple<string, DateTime>> monitoredSystemMACAddresses)
        {
            return MISD.Server.Manager.WorkstationManager.Instance.ActivateMaintenanceMode(monitoredSystemMACAddresses);
        }

        public List<string> DeactivateMaintenanceMode(List<Tuple<string, DateTime>> monitoredSystemMACAddresses)
        {
            return MISD.Server.Manager.WorkstationManager.Instance.DeactivateMaintenanceMode(monitoredSystemMACAddresses);
        }

        public bool MoveMonitoredSystem(List<Tuple<string, int, DateTime>> monitoredSystems)
        {
            return MISD.Server.Manager.WorkstationManager.Instance.MoveMonitoredSystem(monitoredSystems);
        }

        public bool ChangeWorkstationName(string mac, string newName, DateTime updateTime)
        {
            return WorkstationManager.Instance.ChangeWorkstationName(mac, newName, updateTime);
        }

        #endregion

        #region GUI-Configuration

        public List<Layout> GetUIConfigurationList()
        {
            return Server.Manager.UIConfigManager.Instance.GetUIConfigurationList();
        }

        public Layout AddUIConfiguration(string name, string userName, byte[] previewImageAsBase64, object data, DateTime Date)
        {
            return MISD.Server.Manager.UIConfigManager.Instance.AddUIConfiguration(name, userName, previewImageAsBase64, data, Date);
        }

        public bool RemoveUIConfiguration(int id)
        {
            return MISD.Server.Manager.UIConfigManager.Instance.RemoveUIConfiguration(id);
        }

        public Layout UpdateUIConfiguration(int configurationID, string name, string userName, byte[] previewImageAsBase64, object data, DateTime Date)
        {
            return MISD.Server.Manager.UIConfigManager.Instance.UpdateUIConfiguration(configurationID, name, userName, previewImageAsBase64, data, Date);
        }

        #endregion

        #region Ignore List

        public List<string> AddMonitoredSystemsToIgnoreList(List<Tuple<string, DateTime>> monitoredSystemMACAddresses)
        {
            return MISD.Server.Manager.WorkstationManager.Instance.AddWorkstationsToIgnoreList(monitoredSystemMACAddresses);
        }

        public List<string> RemoveMonitoredSystemsFromIgnoreList(List<Tuple<string, DateTime>> monitoredSystemMACAddresses)
        {
            return MISD.Server.Manager.WorkstationManager.Instance.RemoveWorkstationsFromIgnoreList(monitoredSystemMACAddresses);
        }

        public List<Tuple<string, string>> GetIgnoredMonitoredSystems()
        {
            return MISD.Server.Manager.WorkstationManager.Instance.GetIgnoredMonitoredSystems();
        }

        #endregion

        #region Email Berichte und Warnungen

        public int? AddEMail(string emailAdress, string userName)
        {
            return MISD.Server.Email.Mailer.Instance.AddEMail(emailAdress, userName);
        }

        public bool RemoveEMail(int userID)
        {
            return MISD.Server.Email.Mailer.Instance.RemoveEMail(userID);
        }

        public bool AddMailObserver(int userID, List<string> mac)
        {
            return MISD.Server.Email.Mailer.Instance.AddMailObserver(userID, mac);
        }

        public bool ChangeEmail(int userID, string userNameNew, string mailAdressNew)
        {
            return MISD.Server.Email.Mailer.Instance.ChangeEmail(userID, userNameNew, mailAdressNew);
        }

        public bool RemoveMailObserver(int userID, List<string> monitoredSystemMACs)
        {
            return MISD.Server.Email.Mailer.Instance.RemoveMailObserver(userID, monitoredSystemMACs);
        }

        public bool AddDailyMail(int userID)
        {
            return MISD.Server.Email.Mailer.Instance.AddDailyMail(userID);
        }

        public bool DeleteDailyMail(int userID)
        {
            return MISD.Server.Email.Mailer.Instance.DeleteDailyMail(userID);
        }

        public List<Tuple<int, string, string, bool>> GetAllMailData()
        {
            return MISD.Server.Email.Mailer.Instance.GetAllMailData();
        }

        public List<WorkstationInfo> GetObserver(int userID)
        {
            return MISD.Server.Email.Mailer.Instance.GetObserver(userID);
        }

        #endregion

        #region Organisationseinheiten

        public bool ChangeOUName(int ouID, string newName, DateTime updateTime)
        {
            return MISD.Server.Manager.OUManager.Instance.ChangeOUName(ouID, newName, updateTime);
        }

        public bool DeleteOU(int ouID)
        {
            return MISD.Server.Manager.OUManager.Instance.DeleteOU(ouID);
        }

        public int AddOU(string name, int? fatherOU, DateTime updateTime)
        {
            return MISD.Server.Manager.OUManager.Instance.AddOU(name, fatherOU, updateTime);
        }

        public bool AssignToOU(string monitoredSystemMAC, int newOUID)
        {
            return MISD.Server.Manager.OUManager.Instance.AssignToOU(monitoredSystemMAC, newOUID);
        }

        public List<Tuple<int, string, string, int?, DateTime?>> GetAllOUs()
        {
            return MISD.Server.Manager.OUManager.Instance.GetAllOUs();
        }

        public bool ChangeParent(int ouID, int? ouIDParent, DateTime updateTime)
        {
            return OUManager.Instance.ChangeParent(ouID, ouIDParent, updateTime);
        }

        #endregion

        #region Aktuelle Kenngrößen

        /// <summary>
        /// Create's a List of all platform typse
        /// </summary>
        /// <returns>List of strings wiht all plattforms</returns>
        public List<string> GetAllPlatformTyps()
        {
            try
            {
                return Enum.GetValues(typeof(Platform)).Cast<Platform>().Select(q => q.ToString()).ToList();
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("ClientWebService_GetAllPlatformTypes: Can't create type list. " + e.StackTrace, LogType.Exception);
                return null;
            }
        }

        public List<int> GetMonitoredSystemIDs()
        {
            return MISD.Server.Manager.ValueManager.Instance.GetWorkstationIDs();
        }

        public List<string> GetWorkstationMACs()
        {
            return ValueManager.Instance.GetWorkstationMACs();
        }

        public List<WorkstationInfo> GetMonitoredSystemInfo(List<Tuple<int, TimeSpan>> monitoredSystemIDsWithResettime)
        {
            return ValueManager.Instance.GetWorkstationInfo(monitoredSystemIDsWithResettime);
        }

        public List<Tuple<string, string, string, string, MappingState, DateTime>> GetLatestMonitoredSystemData(List<string> macList)
        {
            return ValueManager.Instance.GetLatestMonitoredSystemData(macList);
        }

        public List<Tuple<string, string, string, string, MappingState, DateTime>> GetPluginData(List<Tuple<string, string>> macAndPluginName)
        {
            return ValueManager.Instance.GetPluginData(macAndPluginName, null);
        }

        public List<Tuple<string, string, string, string, MappingState, DateTime>> GetCompletePluginDataList(List<string> macList, int? numberOfIndicators)
        {
            return ValueManager.Instance.GetCompletePluginData(macList, numberOfIndicators);
        }

        #endregion

        #region Alte Kenngrößenwerte

        public List<Tuple<string, string, string, string, MappingState, DateTime>> GetData(List<Tuple<string, string, string, DateTime?, DateTime?, int?>> macAndProperties)
        {
            return MISD.Server.Manager.ValueManager.Instance.GetData(macAndProperties);
        }

        #endregion

        #region Cluster verwalten

        /// <summary>
        /// Creates a list of all supported cluster types
        /// </summary>
        /// <returns></returns>
        public List<string> GetClusterTyps()
        {
            var result = new List<string>();
            try
            {
                result = (from q in Enum.GetValues(typeof(Platform)).Cast<Platform>()
                          where q != Platform.Linux &
                                q != Platform.Windows &
                                q != Platform.Server &
                                q != Platform.Visualization
                         select q.ToString()).ToList();
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("ClientWebService_GetClusterTyps: Can't create cluster types list. " + e.StackTrace, LogType.Exception);
            }

            return result;

        }

        /// <summary>
        /// Adds a new cluster to the misd owl system
        /// </summary>
        /// <param name="headnodeAddress">The name for the cluster as well as for the organisational unit for it's nodes.</param>
        /// <param name="username">The adress of the headnode of the clustermanager.</param>
        /// <param name="password">True if it's a HPC Cluster, false if it's a Bright Cluster.</param>
        /// <param name="database">database url for indicator datas</param>
        /// <param name="platform">Platform of the cluster</param>
        /// <returns>Cluster credential ID</returns>
        public int AddCluster(string headnodeAddress, string username, string password, string platform)
        {
            using (var dataContext = MISD.Server.Database.DataContextFactory.CreateDataContext())
            {
                try
                {
                    MISD.Server.Manager.ClusterManager cl = new ClusterManager();

                    Platform clusterPlatform = (Platform)Enum.Parse(typeof(Platform), platform);

                    var cluster = new Database.ClusterCredential()
                    {
                        HeadNodeUrl = headnodeAddress,
                        Username = username,
                        Password = password,
                        Platform = (byte) clusterPlatform
                    };
                    dataContext.ClusterCredential.InsertOnSubmit(cluster);
                    dataContext.SubmitChanges();

                    cl.Initialize(cluster);

                    return cluster.ID;
                }
                catch (Exception e)
                {
                    //logging exception
                    var messageEx = new StringBuilder();
                    messageEx.Append("ClientWebService_AddCluster: ");
                    messageEx.Append("Can't add new Cluster " + headnodeAddress + ". " + e.ToString());
                    MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);
                    return -1;
                }
            }
        }

        /// <summary>
        /// Changes the cluster credentials of a cluster
        /// </summary>
        /// <param name="id">ID of the cluster credentials</param>
        /// <param name="data">A tuple containing headnode url | username | password | DB url | platform</param>
        /// <returns></returns>
        public bool ChangeCluster(int id, Tuple<string, string, string, string>data)
        {
            try
            {
                using (var dataContext = Database.DataContextFactory.CreateDataContext())
                {
                    var cluster = (from q in dataContext.ClusterCredential
                                   where q.ID == id
                                   select q).FirstOrDefault();

                    ClusterManager clusterm = null;
                    foreach (var current in MetaClusterManager.Instance.clusterManagers)
                    {
                        if (cluster != null && current.ID.Equals(cluster.ID))
                        {
                            clusterm = current;
                        }
                    }

                    if (clusterm != null)
                    {
                        MISD.Server.Manager.MetaClusterManager.Instance.RemoveCluster(clusterm);
                    }

                    Platform clusterPlatform = (Platform)Enum.Parse(typeof(Platform), data.Item4);

                    if (cluster != null)
                    {
                        cluster.HeadNodeUrl = data.Item1;
                        cluster.Username = data.Item2;
                        cluster.Password = data.Item3;
                        cluster.Platform = (byte)clusterPlatform;
                        dataContext.SubmitChanges();

                        MISD.Server.Manager.ClusterManager cl = new ClusterManager();
                        var cluster2 = (from q in dataContext.ClusterCredential
                                        where q.ID == id
                                        select q).FirstOrDefault();
                        cl.Initialize(cluster2);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("ClientWebService_ChangeCluster: Can't change cluster: " + e.StackTrace, LogType.Exception);

                return false; 
            }
        }

        public bool DeleteCluster(int clusterID)
        {
            using (var dataContext = MISD.Server.Database.DataContextFactory.CreateDataContext())
            {
                try
                {
                    var cl = (from ms in dataContext.ClusterCredential
                              where ms.ID == clusterID
                              select ms).FirstOrDefault();

                    if (cl != null)
                    {
                        ClusterManager clusterm = null;
                        foreach (var current in MetaClusterManager.Instance.clusterManagers)
                        {
                            if (current.ID.Equals(cl.ID))
                            {
                                clusterm = current;
                            }
                        }

                        if (clusterm != null)
                        {
                            MISD.Server.Manager.MetaClusterManager.Instance.RemoveCluster(clusterm);
                        }

                        dataContext.ClusterCredential.DeleteOnSubmit(cl);
                        dataContext.SubmitChanges();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception e)
                {
                    //logging exception
                    var messageEx = new StringBuilder();
                    messageEx.Append("ClientWebService_DeleteCluster: ");
                    messageEx.Append("Can't delete  Cluster" + clusterID + ". " + e.ToString());
                    MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the cluster credentials
        /// </summary>
        /// <returns>A list containing tuples of ID | headnode url | username | password | DB url | platform </returns>
        public List<Tuple<int, string, string, string, string>> GetClusters()
        {

            try
            {
                using (var dataContext = Database.DataContextFactory.CreateReadOnlyDataContext())
                {
                    var c = (from q in dataContext.ClusterCredential
                                    select new Tuple<int, string, string, string, Platform>
                                        (q.ID, q.HeadNodeUrl, q.Username, q.Password, (Platform)q.Platform)).ToList();

                    var clusters = (from q in c
                                    select new Tuple<int, string, string, string, string>
                                        (q.Item1, q.Item2, q.Item3, q.Item4, q.Item5.ToString())).ToList();
                    return clusters;
                }
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("ClientWebService_GetClusters: Can't load cluster credentials: " + e.StackTrace, LogType.Exception);
                return null;
            }
        }
        
        #endregion
    }
}


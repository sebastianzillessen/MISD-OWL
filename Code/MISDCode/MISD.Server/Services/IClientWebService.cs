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
using System.ServiceModel;
using System.Runtime.Serialization;
using MISD.Core;

namespace MISD.Server.Services
{
    /// <summary>
    /// Defines the methods that are exposed via the Client Web Service.
    /// </summary>
    [ServiceContract]
    public interface IClientWebService
    {
        #region Plugin Management

        /// <summary>
        /// Gets a list of all plugins, that are currently available on the server.
        /// </summary>
        /// <returns>The metadata of all plugins that are available on the server.</returns>
        [OperationContract]
        List<PluginMetadata> GetPluginList(string plattform);

        /// <summary>
        /// Downloads the plugins that match the given names.
        /// </summary>
        /// <remarks>
        /// This method gets called only by workstations.
        /// </remarks>
        /// <param name="monitoredSystemMAC">The MAC of the workstation that wants to download the plugins.</param>
        /// <param name="pluginNames">The names of the plugins that shall be downloaded.</param>
        /// <returns>The plugin files that are specific for the given workstation.</returns>
        [OperationContract]
        List<PluginFile> DownloadPlugins(List<string> pluginNames);

        #endregion

        #region Filter, Metriken, Mapping und Aktualisierungsintervalle

        /// <summary>
        /// Gets all indicator-settings by plugin for the given workstation.
        /// </summary>
        /// <param name="monitoredSystemMAC">The MAC of the workstation to get the settings of.</param>
        /// <returns>PluginName | IndicatorSetting</returns>
        [OperationContract]
        List<IndicatorSettings> GetIndicatorSetting(string monitoredSystemMAC);

        /// <summary>
        /// Updates the given indicator-settings.
        /// </summary>
        /// <param name="settings">The new settings.</param>
        /// <returns>True, if the update was successfull, false otherwise.</returns>
        [OperationContract]
        bool SetIndicatorSetting(List<IndicatorSettings> settings);

        #endregion

        #region MonitoredSystem

        /// <summary>
        /// Resets the mapping of a workstation
        /// </summary>
        /// <param name="macList">Tuple: mac | updateTime</param>
        /// <returns></returns>
        [OperationContract]
        bool ResetMapping(List<Tuple<string, DateTime>> macList);

        /// <summary>
        /// Activates the maintenance mode for several workstations.
        /// </summary>
        /// <param name="monitoredSystemMACAddresses">Tuple: mac | updateTime</param>
        /// <returns></returns>
        [OperationContract]
        List<String> ActivateMaintenanceMode(List<Tuple<string, DateTime>> monitoredSystemMACAddresses);

        /// <summary>
        /// Deactivates the maintenance mode for several workstations.
        /// </summary>
        /// <param name="monitoredSystemMACAddresses">Tuple: mac | updateTime</param>
        /// <returns>A list containing the names of the workstations that have been put out of maintenance mode successfully.</returns>
        [OperationContract]
        List<String> DeactivateMaintenanceMode(List<Tuple<string, DateTime>> monitoredSystemMACAddresses);

        /// <summary>
        /// Changes the ouIDs of the given monitoredSytems.
        /// </summary>
        /// <param name="monitoredSystems">List of tuples: MAC | ouID | updateTime </param>
        /// <returns>boolean</returns>
        [OperationContract]
        bool MoveMonitoredSystem(List<Tuple<string, int, DateTime>> monitoredSystems);

        /// <summary>
        /// Changes the name of a monitored
        /// </summary>
        /// <param name="mac"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        [OperationContract]
        bool ChangeWorkstationName(string mac, string newName, DateTime updateTime);

        #endregion

        #region GUI-Configuration

        /// <summary>
        /// Gets a list containing all UI configurations , that are available on the
        /// server.
        /// </summary>
        /// <returns>A list containing the UI configurations.</returns>
        [OperationContract]
        List<Layout> GetUIConfigurationList();

        /// <summary>
        /// Adds a UI configuration of a certain user to the server 's UI
        /// configurations list.
        /// </summary>
        /// <param name="name">The name of the UI configuration.</param>
        /// <param name="userName">The bane of the user.</param>
        /// <param name="previewImageAsBase64">A preview image as base64 - String.</param>
        /// <param name="data">The main window view model.</param>
        /// <returns>The server version of the UI configuration.</returns>
        [OperationContract]
        Layout AddUIConfiguration(string name, string userName, byte[] previewImageAsBase64, object data, DateTime Date);

        /// <summary>
        /// Removes a UI configuration from the server.
        /// </summary>
        /// <param name="id">The ID of the UI configuration.
        /// <returns>A value that indicates , whether the UI configuration has been removed successfully , or not</returns>
        [OperationContract]
        bool RemoveUIConfiguration(int id);

        /// <summary>
        /// Updates a UI configuration with the specified values .
        /// Remarks : Parameters that are null will not be changed in the database .
        /// </summary>
        /// <param name="configurationID">The ID of the UI configuration that is about to be updated.</param>
        /// <param name="name">The new name of the UI configuration.</param>
        /// <param name="userName">The name of the UI configuration.</param>
        /// <param name="previewImageAsBase64">The new preview image of the UI configuration.</param>
        /// <param name="date">The new UI configuration data.</param>
        /// <returns>The server version of the UI configuation.</returns>
        [OperationContract]
        Layout UpdateUIConfiguration(int configurationID, string name, string userName, byte[] previewImageAsBase64, object data, DateTime Date);

        #endregion

        #region Ignore List

        /// <summary>
        /// Adds several workstations to the ignore list.
        /// </summary>
        /// <param name="monitoredSystemMACAddresses">List of tuples: MAC | updateTime</param>
        /// <returns>A list containing the names of the workstations that have been added to the ignore list successfully.</returns>
        [OperationContract]
        List<String> AddMonitoredSystemsToIgnoreList(List<Tuple<string, DateTime>> monitoredSystemMACAddresses);

        /// <summary>
        /// Removes several workstations from the ignore list .
        /// </summary>
        /// <param name="monitoredSystemMACAddresses">List of tuples: MAC | updateTime</param>
        /// <returns>A list containing the names of the workstations that have been removed from the ignore list successfully.</returns>
        [OperationContract]
        List<String> RemoveMonitoredSystemsFromIgnoreList(List<Tuple<string, DateTime>> monitoredSystemMACAddresses);

        /// <summary>
        /// Gets all workstations , that are currently ignored by the server.
        /// </summary>
        /// <returns>A list containing a tuple of: mac | name</returns>
        [OperationContract]
        List<Tuple<string, string>> GetIgnoredMonitoredSystems();

        #endregion

        #region Email Berichte und Warnungen

        /// <summary>
        /// Adds a email - adress to the adress - list.
        /// </summary>
        /// <param name="emailAdress">A string containing a email - adress.</param>
        /// <param name="userName">A string containing the full name of the user.</param>
        /// <returns>true if the execution was done without errors .</returns>
        [OperationContract]
        int? AddEMail(string emailAdress, string userName);

        /// <summary>
        /// Remove a email - adress form the system
        /// </summary>
        /// <param name="emailAdress">A string containing a email - adress.</param>
        /// <param name="userName">A string containing the full name of the user.</param>
        /// <returns>true if the execution was done without errors .</returns>
        [OperationContract]
        bool RemoveEMail(int userID);

        /// <summary>
        /// Add a email - adress to the observer - list of several workstations .
        /// </summary>
        /// <param name="emailAdress">A string containing a email - adress</param>
        /// <param name="mac">A list containing the MACs of the monitored systems.</param>
        /// <returns>true if the execution was done without errors .</returns>
        [OperationContract]
        bool AddMailObserver(int userID, List<string> mac);

        /// <summary>
        /// Remove a email - adress from the observer - list of several workstations .
        /// </summary>
        /// <param name="emailAdress">A string containing a email - adress</param>
        /// <param name="monitoredSystemMACs">A list containing the MACs of the monitored systems.</param>
        /// <returns>true if the execution was done without errors .</returns>
        [OperationContract]
        bool RemoveMailObserver(int userID, List<string> monitoredSystemMACs);

        /// <summary>
        /// Add a email - adress to the daily - mail list .
        /// </summary>
        /// <param name="mailID">A int containing a email ID</param>
        /// <returns>true if the execution was done without errors .</returns>
        [OperationContract]
        bool AddDailyMail(int userID);

        /// <summary>
        /// Remove a email - adress from the daily - mail list .
        /// </summary>
        /// <param name="emailAdress">A string containing a email - adress</param>
        /// <returns>true if the execution was done without errors .</returns>
        [OperationContract]
        bool DeleteDailyMail(int userID);

        /// <summary>
        /// Gets all email user datas
        /// </summary>
        /// <returns>List of Tuple with data: ID | username | user mail adress | daily mail </returns>
        [OperationContract]
        List<Tuple<int, string, string, bool>> GetAllMailData();

        /// <summary>
        /// Retuns all observer of an monitored system
        /// </summary>
        /// <param name="userID">User ID of the email adress</param>
        /// <returns>List of Tuples with user name | email adress</returns>
        [OperationContract]
        List<WorkstationInfo> GetObserver(int userID);

        /// <summary>
        /// Changes the mail adress and the username of a given mail adress
        /// </summary>
        /// <param name="userID">User ID of the email adress</param>
        /// <param name="userNameNew"></param>
        /// <param name="mailAdressNew"></param>
        /// <returns>true if the execution was done without errors.</returns>
        [OperationContract]
        bool ChangeEmail(int userID, string userNameNew, string mailAdressNew);

        #endregion

        #region Organisationseinheiten

        /// <summary>
        /// Changes the displayed name of a orginisational unit .
        /// </summary>
        /// <param name="ouID">ID of the orginisational unit .</param>
        /// <param name="newName">The new name of the orginisational unit .</param>
        /// <returns>true if the execution was done without errors .</returns>
        [OperationContract]
        bool ChangeOUName(int ouID, string newName, DateTime updateTime);

        /// <summary>
        /// Delete a given orginisational unit and adds the containing workstations to the ignore - list .
        /// </summary>
        /// <param name="ouID">ID of the orginisational unit .</param>
        /// <returns>true if the execution was done without errors .</returns>
        [OperationContract]
        bool DeleteOU(int ouID);

        /// <summary>
        /// Adds a new orginisational unit .
        /// </summary>
        /// <param name="name">String containing the name of the new orginisational unit .</param>
        /// <param name="fatherOU">The orginisational unit which contains the new orginisational unit . NULL for a initial orginisational unit .</param>
        /// <returns>ID of the new orginisational unit .</returns>
        [OperationContract]
        int AddOU(string name, int? fatherOU, DateTime updateTime);

        /// <summary>
        /// Assigns a workstation to a new cluster .
        /// </summary>
        /// <param name="monitoredSystemMAC">The MAC of the workstation that is assigned to a new organiational unit.</param>
        /// <param name="newOUID">ID of the new organisational unit.</param>
        /// <returns>True if the execution was done withour errors.</returns>
        [OperationContract]
        bool AssignToOU(string monitoredSystemMAC, int newOUID);

        /// <summary>
        /// Gets all stored ous at the server.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<Tuple<int, string, string, int?, DateTime?>> GetAllOUs();

        /// <summary>
        /// Changes the parent ou of a given ou.
        /// </summary>
        /// <param name="ouID"></param>
        /// <param name="ouIDParent"></param>
        /// <returns></returns>
        [OperationContract]
        bool ChangeParent(int ouID, int? ouIDParent, DateTime updateTime);

        #endregion

        #region Aktuelle Kenngrößen

        /// <summary>
        /// Create's a List of all platform typse
        /// </summary>
        /// <returns>List of strings wiht all plattforms</returns>
        [OperationContract]
        List<string> GetAllPlatformTyps();

        /// <summary>
        /// Gets the IDs of all workstations that are known to the server .
        /// </summary>
        /// <returns>A list of workstation names .</returns>
        [OperationContract]
        List<int> GetMonitoredSystemIDs();

        /// <summary>
        /// Gets the IDs of all workstations that are known to the server .
        /// </summary>
        /// <returns>A list of workstation names .</returns>
        [OperationContract]
        List<string> GetWorkstationMACs();

        /// <summary>
        /// Gets infomation about workstations , containing the workstation 's names and states .
        /// This method can be used to retrieve Level 1 data .
        /// </summary>
        /// <param name="monitoredSystemMACAddresses">The IDs of the workstations and the time of the last reset.</param>
        /// <returns> A list containing the workstation infos .</returns>
        [OperationContract]
        List<WorkstationInfo> GetMonitoredSystemInfo(List<Tuple<int, TimeSpan>> monitoredSystemIDsWithResetTime);

        /// <summary>
        /// Gets the latest indicator data for each indicator.
        /// </summary>
        /// <param name="macs">A list containing the macs of the monitored systems</param>
        /// <returns>List of: MAC | Pluginname | Indicatorname | Value | Mapping | Timestamp</returns>
        [OperationContract]
        List<Tuple<string, string, string, string, MappingState, DateTime>> GetLatestMonitoredSystemData(List<string> macList);

        /// <summary>
        /// Gets the complete data for serveral plugins of certain workstations. 
        /// This method can be used to retrieve Level 2 data .
        /// </summary>
        /// <param name="macAndPluginName"> A list containing tuples of:  MAC | PluginName</param>
        /// <returns>A list containing tuples of: MAC | PluginName | IndicatorName | Value | Mapping | Time</returns>
        [OperationContract]
        List<Tuple<string, string, string, string, MappingState, DateTime>> GetPluginData(List<Tuple<string, string>> macAndPluginName);

        /// <summary>
        /// Gets the data for all plugins of the given workstations .
        /// This method can be used to retrieve Level 3 data .
        /// </summary>
        /// <param name="mac">A list containing tuples of:  MAC | | Maximum numer of results? per inticator</param>
        /// <returns>A list containing tuples of: MAC | PluginName | IndicatorName | Value | Mapping | Time</returns>
        [OperationContract]
        List<Tuple<string, string, string, string, MappingState, DateTime>> GetCompletePluginDataList(List<string> macList, int? numberOfIndicators);

        #endregion

        #region Alte Kenngrößenwerte

        /// <summary>
        /// Gets the complete data for serveral plugins of certain workstations and a certain timespan.
        /// </summary>
        /// <param name="macAndProperties"> A list containing tuples of:  MAC | PluginName | IndicatorName | LowerBound? | UpperBound? | Maximum numer of results?</param>
        /// <returns>A list containing tuples of: MAC | PluginName | IndicatorName | Value | Mapping | Time</returns>
        [OperationContract]
        List<Tuple<string, string, string, string, MappingState, DateTime>> GetData(List<Tuple<string, string, string, DateTime?, DateTime?, int?>> macAndProperties);

        #endregion

        #region Cluster verwalten
        /// <summary>
        /// Creates a list of all supported cluster types
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<string> GetClusterTyps();

        /// <summary>
        /// Adds a new cluster to the misd owl system
        /// </summary>
        /// <param name="headnodeAddress">The name for the cluster as well as for the organisational unit for it's nodes.</param>
        /// <param name="username">The adress of the headnode of the clustermanager.</param>
        /// <param name="password">True if it's a HPC Cluster, false if it's a Bright Cluster.</param>
        /// <param name="database">database url for indicator datas</param>
        /// <param name="platform">Platform of the cluster</param>
        /// <returns>Cluster credential ID</returns>
        [OperationContract]
        int AddCluster(string headnodeAddress, string username, string password, string platform);

        /// <summary>
        /// Changes the cluster credentials of a cluster
        /// </summary>
        /// <param name="id">ID of the cluster credentials</param>
        /// <param name="data">A tuple containing headnode url | username | password |  platform</param>
        /// <returns></returns>
        [OperationContract]
        bool ChangeCluster(int id, Tuple<string, string, string, string> data);

        /// <summary>
        /// Removes a cluster from the system .
        /// </summary>
        /// <param name="clusterID">The ID of the cluster to be removed.</param>
        /// <returns>True if the execution was done withour errors.</returns>
        [OperationContract]
        bool DeleteCluster(int clusterID);

        /// <summary>
        /// Gets the cluster credentials
        /// </summary>
        /// <returns>A list containing tuples of ID | headnode url | username | password | platform </returns>
        [OperationContract]
        List<Tuple<int, string, string, string, string>> GetClusters();

        #endregion
    }
}
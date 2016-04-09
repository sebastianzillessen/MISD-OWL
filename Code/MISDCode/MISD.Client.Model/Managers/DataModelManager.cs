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
 * MISD-OWL is distributed without any warranty, witmlhout even the
 * implied warranty of merchantability or fitness for a particular purpose.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MISD.Client.Managers;
using MISD.Core;
using MISD.Client.Properties;

namespace MISD.Client.Model.Managers
{
    /// <summary>
    /// This class coordinates the transfer of the DataModel to the Powerwall clients.
    /// </summary>
    public class DataModelManager
    {
        #region Singleton

        private static volatile DataModelManager instance;
        private static object syncRoot = new Object();

        /// <summary>
        /// Initializes a new instance of the DataModel class.
        /// </summary>
        private DataModelManager()
        {
            //Note: If Config.Class.IsPowerwall == true, the LayoutManager will send the received DataModelChangeCommands to the DataModelManager.
        }

        /// <summary>
        /// Gets the singleton instance of this class.
        /// </summary>
        public static DataModelManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new DataModelManager();
                    }
                }

                return instance;
            }
        }
        #endregion

        #region Fields

        private WorkerThread sendDataModelToPowerwallThread;

        #endregion

        #region Methods

        private void SendDataModelLoop()
        {
            if (ConfigClass.IsOperator && LayoutManager.Instance.TcpConnection != null)
            {
                DataModelChangeCommand command = new DataModelChangeCommand(DataModel.Instance.Elements);
                LayoutManager.Instance.TcpConnection.Send(command);
            }

            Thread.Sleep(Settings.Default.ClientUpdateIntervall);
        }

        /// <summary>
        /// Starts the transfer of the DataModel to the powerwall clients.
        /// </summary>
        public void StartPowerwallSync()
        {
            if (ConfigClass.IsOperator)
            {
                try
                {
                    // Start sending of DataModel
                    this.sendDataModelToPowerwallThread = ThreadManager.CreateWorkerThread("SendDataModelToPowerwallThread", this.SendDataModelLoop, true);
                }
                catch (Exception e)
                {
                    ClientLogger.Instance.WriteEntry("Unexpected exception occured", e, LogType.Exception);
                }
            }
        }

        /// <summary>
        /// Stops the transfer of the DataModel to the powerwall clients.
        /// </summary>
        public void StopPowerwallSync()
        {
            if (ConfigClass.IsOperator)
            {
                this.sendDataModelToPowerwallThread.Stop();
            }
        }

        /// <summary>
        /// This method is called by the LayoutManager, if the tcp connection received a DataModelChangeCommand.
        /// </summary>
        /// <param name="data"></param>
        public void TcpConnection_newDataReceived(object data)
        {
            if (data.GetType() == typeof(DataModelChangeCommand))
            {
                DataModelChangeCommand command = (DataModelChangeCommand)data;
                if (command.CommandType == DataModelCommand.UPDATE_ELEMENTS)
                {
                    this.UpdateDataModelElements(command.GetElements());

                    // Now show the ui, if it is shown grayed.
                    DataModel.Instance.ShowUI = true;
                }
            }
        }

        /// <summary>
        /// Serializes the gven parameter.
        /// </summary>
        /// <param name="elements"></param>
        /// <returns>string</returns>
        public string SerializeBase64(ExtendedObservableCollection<TileableElement> elements)
        {
            MemoryStream ws = new MemoryStream();
            try
            {
                // Serialize to a base 64 string
                byte[] bytes;
                long length = 0;
                BinaryFormatter sf = new BinaryFormatter();
                sf.Serialize(ws, elements);
                length = ws.Length;
                bytes = ws.GetBuffer();
                string encodedData = bytes.Length + ":" + Convert.ToBase64String(bytes, 0, bytes.Length, Base64FormattingOptions.None);
                sf = null;
                return encodedData;
            }
            catch (Exception e)
            {
                ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
                return null;
            }
            finally
            {
                ws.Flush();
                ws.Close();
                ws.Dispose();
            }
        }

        /// <summary>
        /// Serializes the gven parameter.
        /// </summary>
        /// <param name="elements"></param>
        /// <returns>ExtendedObservableCollection<TileableElement></returns>
        public ExtendedObservableCollection<TileableElement> DeserializeBase64(string s)
        {
            // We need to know the exact length of the string - Base64 can sometimes pad us by a byte or two
            int p = s.IndexOf(':');
            int length = Convert.ToInt32(s.Substring(0, p));

            // Extract data from the base 64 string!
            byte[] memorydata = Convert.FromBase64String(s.Substring(p + 1));
            MemoryStream rs = new MemoryStream(memorydata, 0, length);
            try
            {
                BinaryFormatter sf = new BinaryFormatter();
                object o = sf.Deserialize(rs);
                return o as ExtendedObservableCollection<TileableElement>;
            }
            catch (Exception e)
            {
                ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
                return null;
            }
            finally
            {
                rs.Flush();
                rs.Close();
                rs.Dispose();
            }
        }

        /// <summary>
        /// Updates the DataMode.Elements.
        /// </summary>
        /// <param name="newElements">ExtendedObservableCollection<TileableElement></param>
        private void UpdateDataModelElements(ExtendedObservableCollection<TileableElement> newElements)
        {
            #region OUs
            // Add and update new OUs.
            foreach (OrganizationalUnit newOU in newElements.GetOrganizationalUnits())
            {
                bool ouFound = false;

                foreach (OrganizationalUnit oldOU in DataModel.Instance.Elements.GetOrganizationalUnits())
                {
                    if (newOU.ID == oldOU.ID)
                    {
                        ouFound = true;

                        oldOU.Name = newOU.Name;
                        oldOU.ParentID = newOU.ParentID;
                        oldOU.LastUpdate = newOU.LastUpdate;
                        oldOU.FQDN = newOU.FQDN;
                        oldOU.SortingProperty = newOU.SortingProperty;

                        break;
                    }
                }

                if (!ouFound)
                {
                    new OrganizationalUnit(newOU.ID, newOU.Name, newOU.FQDN, newOU.ParentID, newOU.Elements, newOU.LastUpdate);
                }
            }

            // Remove old Ous.
            foreach (OrganizationalUnit oldOU in DataModel.Instance.Elements.GetOrganizationalUnits())
            {
                bool ouFound = false;

                foreach (OrganizationalUnit newOU in newElements.GetOrganizationalUnits())
                {
                    if (newOU.ID == oldOU.ID)
                    {
                        ouFound = true;
                        break;
                    }
                }

                if (!ouFound)
                {
                    if (oldOU.ParentID == null)
                    {
                        DataModel.Instance.Elements.RemoveOnUI(oldOU);
                    }
                    else
                    {
                        OrganizationalUnit parentOU = DataModel.Instance.GetOu((int)oldOU.ParentID);
                        parentOU.Elements.RemoveOnUI(oldOU);
                    }
                }
            }
            #endregion

            #region MS
            // Add and update new MonitoredSystems.
            foreach (MonitoredSystem newMS in newElements.GetMonitoredSystems())
            {
                bool msFound = false;

                foreach (MonitoredSystem oldMS in DataModel.Instance.Elements.GetMonitoredSystems())
                {
                    if (oldMS.ID == newMS.ID)
                    {
                        msFound = true;

                        oldMS.LastUpdate = newMS.LastUpdate;
                        oldMS.MAC = newMS.MAC;
                        oldMS.Name = newMS.Name;
                        oldMS.OuID = newMS.OuID;
                        oldMS.ResetDate = newMS.ResetDate;
                        oldMS.State = newMS.State;
                        oldMS.Sequence = newMS.Sequence;
                        oldMS.SortingProperty = newMS.SortingProperty;
                        oldMS.CurrentPlatform = newMS.CurrentPlatform;
                        oldMS.FQDN = newMS.FQDN;
                        oldMS.IsAvailable = newMS.IsAvailable;
                        oldMS.CustomUIValuesLoaded = newMS.CustomUIValuesLoaded;

                        #region Plugins
                        //Add and update new Plugins.
                        foreach (Plugin newPlugin in newMS.Plugins)
                        {
                            bool pluginFound = false;

                            foreach (Plugin oldPlugin in oldMS.Plugins)
                            {
                                if (oldPlugin.Name == newPlugin.Name)
                                {
                                    pluginFound = true;

                                    oldPlugin.MainValue = newPlugin.MainValue;
                                    oldPlugin.Product = newPlugin.Product;
                                    oldPlugin.Platform = newPlugin.Platform;
                                    oldPlugin.Version = newPlugin.Version;
                                    oldPlugin.CurrentMapping = newPlugin.CurrentMapping;

                                    #region Indicators
                                    // Add and update new Indicators
                                    foreach (Indicator newIndi in newPlugin.Indicators)
                                    {
                                        bool indiFound = false;

                                        foreach (Indicator oldIndi in oldPlugin.Indicators)
                                        {
                                            if (oldIndi.Name == newIndi.Name)
                                            {
                                                indiFound = true;

                                                oldIndi.CurrentValue = newIndi.CurrentValue;
                                                oldIndi.DataType = newIndi.DataType;
                                                oldIndi.FilterStatement = newIndi.FilterStatement;
                                                oldIndi.IndicatorMapping = newIndi.IndicatorMapping;
                                                oldIndi.MappingDuration = newIndi.MappingDuration;
                                                oldIndi.MonitoredSystemMAC = newIndi.MonitoredSystemMAC;
                                                oldIndi.PluginName = newIndi.PluginName;
                                                oldIndi.StatementCritical = newIndi.StatementCritical;
                                                oldIndi.StatementWarning = newIndi.StatementWarning;
                                                oldIndi.StorageDuration = newIndi.StorageDuration;
                                                oldIndi.UpdateInterval = newIndi.UpdateInterval;

                                                oldIndi.IndicatorValues = newIndi.IndicatorValues;

                                                break;
                                            }
                                        }

                                        if (!indiFound)
                                        {
                                            oldPlugin.Indicators.AddOnUI(newIndi);
                                        }
                                    }

                                    // Remove old indicators
                                    var indisToRemove = new List<Indicator>();
                                    foreach (Indicator oldIndi in oldPlugin.Indicators)
                                    {
                                        bool indiFound = false;

                                        foreach (Indicator newIndi in newPlugin.Indicators)
                                        {
                                            if (newIndi.Name == oldIndi.Name)
                                            {
                                                indiFound = true;
                                                break;
                                            }
                                        }

                                        if (!indiFound)
                                        {
                                            indisToRemove.Add(oldIndi);
                                        }
                                    }
                                    foreach (Indicator indiToRemove in indisToRemove)
                                    {
                                        oldPlugin.Indicators.RemoveOnUI(indiToRemove);
                                    }
                                    #endregion

                                    break;
                                }
                            }

                            if (!pluginFound)
                            {
                                oldMS.Plugins.AddOnUI(newPlugin);
                            }
                        }

                        //Remove old plugins.
                        var pluginsToRemove = new List<Plugin>();
                        foreach (Plugin oldPlugin in oldMS.Plugins)
                        {
                            bool pluginFound = false;

                            foreach (Plugin newPlugin in newMS.Plugins)
                            {
                                if (newPlugin.Name == oldPlugin.Name)
                                {
                                    pluginFound = true;
                                    break;
                                }
                            }

                            if (!pluginFound)
                            {
                                pluginsToRemove.Add(oldPlugin);
                            }
                        }
                        foreach (Plugin pluginToRemove in pluginsToRemove)
                        {
                            oldMS.Plugins.RemoveOnUI(pluginToRemove);
                        }
                        #endregion

                        break;
                    }
                }

                if (!msFound)
                {
                    new MonitoredSystem(newMS.Name, newMS.ID, newMS.MAC, newMS.FQDN, newMS.State, newMS.ResetDate, newMS.Plugins, newMS.CurrentPlatform, newMS.IsAvailable, newMS.OuID, newMS.LastUpdate);
                }
            }

            // Remove old MonitoredSystems.
            foreach (MonitoredSystem oldMS in DataModel.Instance.Elements.GetMonitoredSystems())
            {
                bool msFound = false;

                foreach (MonitoredSystem newMS in newElements.GetMonitoredSystems())
                {
                    if (newMS.ID == oldMS.ID)
                    {
                        msFound = true;
                        break;
                    }
                }

                if (!msFound)
                {
                    OrganizationalUnit parentOU = DataModel.Instance.GetOu(oldMS.OuID);
                    parentOU.Elements.RemoveOnUI(oldMS);
                }
            }

            #endregion
        }

        #endregion
    }
}
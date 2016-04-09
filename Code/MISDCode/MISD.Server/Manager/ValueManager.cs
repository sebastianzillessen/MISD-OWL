﻿/*
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
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using MISD.Core;
using MISD.Server.Properties;
using System.Text;
using MISD.Server.Database;
using MISD.Server.Email;
using System.Runtime.CompilerServices;
using System.Data.Linq;

namespace MISD.Server.Manager
{
    public class ValueManager
    {
        #region Singleton

        private static volatile ValueManager instance;
        private static object syncRoot = new Object();

        /// <summary>
        /// Property to access the class.
        /// </summary>
        public static ValueManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ValueManager();
                    }
                }

                return instance;
            }
        }

        #endregion

        #region Fields

        CacheManager<String, Tuple<string, string, string, string, MappingState, DateTime>> cachedValues = new CacheManager<string, Tuple<string, string, string, string, MappingState, DateTime>>() { Size = 10000 };

        #endregion

        #region Caching

        /// <summary>
        /// Mac, Pluginname, Indicatorname, value, MappingState, Date
        /// </summary>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void CacheIndicator(Tuple<string, string, string, string, MappingState, DateTime> value)
        {
            try
            {
                string key = String.Join("|", new string[] { value.Item1, value.Item2, value.Item3 });
                cachedValues.Add(key, value);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("FAILURE IN CacheIndicator: " + e, LogType.Exception);
            }
        }

        public Tuple<string, string, string, string, MappingState, DateTime> GetCachedIndicator(string Mac, string PluginName, string Indicator)
        {
            string key = String.Join("|", new string[] { Mac, PluginName, Indicator });
            return cachedValues.Get(key);
        }

        #endregion

        #region Constructors

        private ValueManager()
        {
        }

        #endregion

        #region Methods

        public List<int> GetWorkstationIDs()
        {
            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {
                try
                {
                    //gets PluginMetadata and converts into DataHolder.PluginMetadata
                    var workstations = (from p in dataContext.MonitoredSystem
                                        where !p.IsIgnored
                                        select p.ID).ToList();
                    return workstations;
                }
                catch (Exception e)
                {
                    //logging exception
                    var messageEx = new StringBuilder();
                    messageEx.Append("ValueManager_GetWorkstationIDs: Can't create List of WorkstationNames. " + e.ToString());
                    MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);

                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the IDs of all workstations that are known to the server .
        /// </summary>
        /// <returns>A list of workstation names .</returns>
        public List<string> GetWorkstationMACs()
        {
            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {
                try
                {
                    //gets PluginMetadata and converts into DataHolder.PluginMetadata
                    var workstations = (from p in dataContext.MonitoredSystem
                                        where !p.IsIgnored
                                        select p.MacAddress).ToList();
                    return workstations;
                }
                catch (Exception e)
                {
                    //logging exception
                    var messageEx = new StringBuilder();
                    messageEx.Append("ValueManager_GetWorkstationMACS: Can't create List of WorkstationNames" + ". " + e.ToString());
                    MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);

                    return null;
                }
            }
        }

        public List<WorkstationInfo> GetWorkstationInfo(List<Tuple<int, TimeSpan>> workstationsWithReset)
        {
            List<WorkstationInfo> returns = new List<WorkstationInfo>();

            using (var dataContext = DataContextFactory.CreateDataContext())
            {
                dataContext.Log = Console.Out;
                try
                {
                    foreach (Tuple<int, TimeSpan> current in workstationsWithReset)
                    {
                        //update mapping state
                        UpdateMonitoredSystemMappingState(current.Item1);

                        var monitoredSystem = PrecompiledQueries.GetMonitoredSystemByID(dataContext, current.Item1);

                        WorkstationInfo workstationInfo;

                        if (monitoredSystem.LastUpdate != null)
                        {
                            //create result
                            workstationInfo = new WorkstationInfo()
                            {
                                Name = monitoredSystem.Name,
                                ID = monitoredSystem.ID,
                                FQDN = monitoredSystem.FQDN,
                                OuID = monitoredSystem.OrganizationalUnitID,
                                IsAvailable = monitoredSystem.IsAvailable,
                                CurrentOS = ((MISD.Core.Platform)monitoredSystem.OperatingSystem).ToString(),
                                MacAddress = monitoredSystem.MacAddress,
                                State = monitoredSystem.Status.HasValue ? (MappingState)monitoredSystem.Status : MappingState.OK,
                                LastUpdate = new DateTime((long)monitoredSystem.LastUpdate)
                            };
                        }
                        else
                        {
                            //create result
                            workstationInfo = new WorkstationInfo()
                            {
                                Name = monitoredSystem.Name,
                                ID = monitoredSystem.ID,
                                FQDN = monitoredSystem.FQDN,
                                OuID = monitoredSystem.OrganizationalUnitID,
                                IsAvailable = monitoredSystem.IsAvailable,
                                CurrentOS = ((MISD.Core.Platform)monitoredSystem.OperatingSystem).ToString(),
                                MacAddress = monitoredSystem.MacAddress,
                                State = monitoredSystem.Status.HasValue ? (MappingState)monitoredSystem.Status : MappingState.OK,
                                LastUpdate = null
                            };
                        }

                        returns.Add(workstationInfo);
                    }
                    return returns;
                }
                catch (Exception e)
                {
                    //logging exception
                    var messageEx = new StringBuilder();
                    messageEx.Append("ValueManager_GetWorkstationInfo: Can't create List of WorkstationInfos" + ". " + e.ToString());
                    MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);

                    return null;
                }
            }
        }

        #region old methods area
        /// <summary>
        /// Gets the complete data for serveral plugins of certain workstations. 
        /// This method can be used to retrieve Level 2 data .
        /// </summary>
        /// <param name="workstationAndPluginIDs">A list containing tuples of: monitoredSystemID | List of PluginIDs .</param>
        /// <returns>A list containing tuples of: WorkstationInfo | List of Plugins ( each of which is a tuple of PluginID | List of Plugin Values ( each of which is a tuple of PluginValueID | PluginValue )).</returns>    
        [Obsolete("Use simplified version with mac adress. No further support for this method!")]
        public List<Tuple<WorkstationInfo, List<Tuple<int, List<Tuple<int, string, MappingState>>>>>> GetPluginData(List<Tuple<int, List<int>>> workstationAndPluginIDs)
        {

            List<Tuple<WorkstationInfo, List<Tuple<int, List<Tuple<int, string, MappingState>>>>>> returns = new List<Tuple<WorkstationInfo, List<Tuple<int, List<Tuple<int, string, MappingState>>>>>>();
            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {
                try
                {
                    foreach (Tuple<int, List<int>> t in workstationAndPluginIDs)
                    {
                        //update mapping state
                        UpdateMonitoredSystemMappingState(t.Item1);

                        //create result
                        var type = (from ms in dataContext.Indicator
                                    join value in dataContext.IndicatorValue on ms.ID equals value.IndicatorID
                                    let system = ms.MonitoredSystem.ID == t.Item1
                                    let PluginID = t.Item2.Contains(ms.PluginMetadata.ID)
                                    where system & PluginID
                                    select ms.ValueType).ToList();

                        foreach (byte ty in type)
                        {
                            if (ty == (byte)DataType.String)
                            {
                                #region String

                                var plugininfos = (from ms in dataContext.Indicator
                                                   join value in dataContext.IndicatorValue on ms.ID equals value.IndicatorID
                                                   let system = ms.MonitoredSystem.ID == t.Item1
                                                   let PluginID = t.Item2.Contains(ms.PluginMetadata.ID)
                                                   where system & PluginID
                                                   select new Tuple<WorkstationInfo, List<Tuple<int, List<Tuple<int, string, MappingState>>>>>(GetWorkstationInfo(new List<Tuple<int, TimeSpan>>() { new Tuple<int, TimeSpan>(ms.MonitoredSystem.ID, new TimeSpan(24, 0, 0)) }).First()
                                                   , new List<Tuple<int, List<Tuple<int, string, MappingState>>>>() { new Tuple<int, List<Tuple<int, string, MappingState>>>(
                                           ms.PluginMetadata.ID, new List<Tuple<int, string, MappingState>>(){
                                           new Tuple<int, string, MappingState>(
                                               ms.ID, value.ValueString.Value , (MappingState) value.Mapping
                                               )
                                           }
                                           )
                                       })
                                                ).First();

                                #endregion

                                returns.Add(plugininfos);
                            };
                            if (ty == (byte)DataType.Float)
                            {
                                #region Float

                                var plugininfos = (from ms in dataContext.Indicator
                                                   join value in dataContext.IndicatorValue on ms.ID equals value.IndicatorID
                                                   let system = ms.MonitoredSystem.ID == t.Item1
                                                   let PluginID = t.Item2.Contains(ms.PluginMetadata.ID)
                                                   where system & PluginID
                                                   select new Tuple<WorkstationInfo, List<Tuple<int, List<Tuple<int, string, MappingState>>>>>(GetWorkstationInfo(new List<Tuple<int, TimeSpan>>() { new Tuple<int, TimeSpan>(ms.MonitoredSystem.ID, new TimeSpan(24, 0, 0)) }).First()
                                                   , new List<Tuple<int, List<Tuple<int, string, MappingState>>>>() { new Tuple<int, List<Tuple<int, string, MappingState>>>(
                                           ms.PluginMetadata.ID, new List<Tuple<int, string, MappingState>>(){
                                           new Tuple<int, string, MappingState>(
                                               ms.ID, value.ValueFloat.Value.ToString()  , (MappingState) value.Mapping
                                               )
                                           }
                                           )
                                       })
                                                ).First();

                                #endregion

                                returns.Add(plugininfos);
                            };
                            if (ty == (byte)DataType.Byte)
                            {
                                #region Byte

                                var plugininfos = (from ms in dataContext.Indicator
                                                   join value in dataContext.IndicatorValue on ms.ID equals value.IndicatorID
                                                   let system = ms.MonitoredSystem.ID == t.Item1
                                                   let PluginID = t.Item2.Contains(ms.PluginMetadata.ID)
                                                   where system & PluginID
                                                   select new Tuple<WorkstationInfo, List<Tuple<int, List<Tuple<int, string, MappingState>>>>>(GetWorkstationInfo(new List<Tuple<int, TimeSpan>>() { new Tuple<int, TimeSpan>(ms.MonitoredSystem.ID, new TimeSpan(24, 0, 0)) }).First()
                                                   , new List<Tuple<int, List<Tuple<int, string, MappingState>>>>() { new Tuple<int, List<Tuple<int, string, MappingState>>>(
                                           ms.PluginMetadata.ID, new List<Tuple<int, string, MappingState>>(){
                                           new Tuple<int, string, MappingState>(
                                               ms.ID, value.ValueByte.Value.ToString()  , (MappingState) value.Mapping
                                               )
                                           }
                                           )
                                       })
                                                ).First();

                                #endregion

                                returns.Add(plugininfos);
                            };
                            if (ty == (byte)DataType.Int)
                            {
                                #region Int

                                var plugininfos = (from ms in dataContext.Indicator
                                                   join value in dataContext.IndicatorValue on ms.ID equals value.IndicatorID
                                                   let system = ms.MonitoredSystem.ID == t.Item1
                                                   let PluginID = t.Item2.Contains(ms.PluginMetadata.ID)
                                                   where system & PluginID
                                                   select new Tuple<WorkstationInfo, List<Tuple<int, List<Tuple<int, string, MappingState>>>>>(GetWorkstationInfo(new List<Tuple<int, TimeSpan>>() { new Tuple<int, TimeSpan>(ms.MonitoredSystem.ID, new TimeSpan(24, 0, 0)) }).First()
                                                   , new List<Tuple<int, List<Tuple<int, string, MappingState>>>>() { new Tuple<int, List<Tuple<int, string, MappingState>>>(
                                           ms.PluginMetadata.ID, new List<Tuple<int, string, MappingState>>(){
                                           new Tuple<int, string, MappingState>(
                                               ms.ID, value.ValueInt.Value.ToString()  , (MappingState) value.Mapping
                                               )
                                           }
                                           )
                                       })
                                                ).First();

                                #endregion

                                returns.Add(plugininfos);
                            };
                        }
                    } return returns;
                }
                catch (Exception e)
                {
                    //logging exception
                    var messageEx = new StringBuilder();
                    messageEx.Append("Can't create List of PluginDatas" + ". " + e.ToString());
                    MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);

                    return null;
                }
            }
        }

        [Obsolete("Use simplified version with mac adress. No further support for this method!")]
        public List<Tuple<WorkstationInfo, List<Tuple<int, List<Tuple<int, string, MappingState>>>>>> GetCompletePluginData(List<int> workstations)
        {

            List<Tuple<WorkstationInfo, List<Tuple<int, List<Tuple<int, string, MappingState>>>>>> returns = new List<Tuple<WorkstationInfo, List<Tuple<int, List<Tuple<int, string, MappingState>>>>>>();
            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {
                try
                {
                    foreach (int t in workstations)
                    {
                        //update mapping state
                        UpdateMonitoredSystemMappingState(t);

                        //create result
                        var type = (from ms in dataContext.Indicator
                                    join value in dataContext.IndicatorValue on ms.ID equals value.IndicatorID
                                    where ms.MonitoredSystem.ID == t
                                    select ms.ValueType).ToList();

                        foreach (byte ty in type)
                        {
                            if (ty == (byte)DataType.String)
                            {
                                #region String

                                var plugininfos = (from ms in dataContext.Indicator
                                                   join value in dataContext.IndicatorValue on ms.ID equals value.IndicatorID
                                                   where ms.MonitoredSystem.ID == t
                                                   select new Tuple<WorkstationInfo, List<Tuple<int, List<Tuple<int, string, MappingState>>>>>(GetWorkstationInfo(new List<Tuple<int, TimeSpan>>() { new Tuple<int, TimeSpan>(ms.MonitoredSystem.ID, new TimeSpan(24, 0, 0)) }).First()
                                                   , new List<Tuple<int, List<Tuple<int, string, MappingState>>>>() { new Tuple<int, List<Tuple<int, string, MappingState>>>(
                                           ms.PluginMetadata.ID, new List<Tuple<int, string, MappingState>>(){
                                           new Tuple<int, string, MappingState>(
                                               ms.ID, value.ValueString.Value , (MappingState) value.Mapping
                                               )
                                           }
                                           )
                                       })
                                                ).First();

                                #endregion

                                returns.Add(plugininfos);
                            };
                            if (ty == (byte)DataType.Float)
                            {
                                #region Float

                                var plugininfos = (from ms in dataContext.Indicator
                                                   join value in dataContext.IndicatorValue on ms.ID equals value.IndicatorID
                                                   where ms.MonitoredSystem.ID == t
                                                   select new Tuple<WorkstationInfo, List<Tuple<int, List<Tuple<int, string, MappingState>>>>>(GetWorkstationInfo(new List<Tuple<int, TimeSpan>>() { new Tuple<int, TimeSpan>(ms.MonitoredSystem.ID, new TimeSpan(24, 0, 0)) }).First()
                                                   , new List<Tuple<int, List<Tuple<int, string, MappingState>>>>() { new Tuple<int, List<Tuple<int, string, MappingState>>>(
                                           ms.PluginMetadata.ID, new List<Tuple<int, string, MappingState>>(){
                                           new Tuple<int, string, MappingState>(
                                               ms.ID, value.ValueFloat.Value.ToString()  , (MappingState) value.Mapping
                                               )
                                           }
                                           )
                                       })
                                                ).First();

                                #endregion

                                returns.Add(plugininfos);
                            };
                            if (ty == (byte)DataType.Byte)
                            {
                                #region Byte

                                var plugininfos = (from ms in dataContext.Indicator
                                                   join value in dataContext.IndicatorValue on ms.ID equals value.IndicatorID
                                                   where ms.MonitoredSystem.ID == t
                                                   select new Tuple<WorkstationInfo, List<Tuple<int, List<Tuple<int, string, MappingState>>>>>(GetWorkstationInfo(new List<Tuple<int, TimeSpan>>() { new Tuple<int, TimeSpan>(ms.MonitoredSystem.ID, new TimeSpan(24, 0, 0)) }).First()
                                                   , new List<Tuple<int, List<Tuple<int, string, MappingState>>>>() { new Tuple<int, List<Tuple<int, string, MappingState>>>(
                                           ms.PluginMetadata.ID, new List<Tuple<int, string, MappingState>>(){
                                           new Tuple<int, string, MappingState>(
                                               ms.ID, value.ValueByte.Value.ToString()  , (MappingState) value.Mapping
                                               )
                                           }
                                           )
                                       })
                                                ).First();

                                #endregion

                                returns.Add(plugininfos);
                            };
                            if (ty == (byte)DataType.Int)
                            {
                                #region Int

                                var plugininfos = (from ms in dataContext.Indicator
                                                   join value in dataContext.IndicatorValue on ms.ID equals value.IndicatorID
                                                   where ms.MonitoredSystem.ID == t
                                                   select new Tuple<WorkstationInfo, List<Tuple<int, List<Tuple<int, string, MappingState>>>>>(GetWorkstationInfo(new List<Tuple<int, TimeSpan>>() { new Tuple<int, TimeSpan>(ms.MonitoredSystem.ID, new TimeSpan(24, 0, 0)) }).First()
                                                   , new List<Tuple<int, List<Tuple<int, string, MappingState>>>>() { new Tuple<int, List<Tuple<int, string, MappingState>>>(
                                           ms.PluginMetadata.ID, new List<Tuple<int, string, MappingState>>(){
                                           new Tuple<int, string, MappingState>(
                                               ms.ID, value.ValueInt.Value.ToString()  , (MappingState) value.Mapping
                                               )
                                           }
                                           )
                                       })
                                                ).First();

                                #endregion

                                returns.Add(plugininfos);
                            };
                        }
                    } return returns;
                }
                catch (Exception e)
                {
                    //logging exception
                    var messageEx = new StringBuilder();
                    messageEx.Append("ClientWebService_GetCompletePluginData: ");
                    messageEx.Append("Can't create List of CompletePluginData" + ". " + e.ToString());
                    MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);

                    return null;
                }
            }
        }

        [Obsolete("Use simplified version with mac adress. No further support for this method!")]
        public List<Tuple<int, List<Tuple<DateTime, string, MappingState>>>> GetData(int monitoredSystemID, int pluginID, List<Tuple<int, DateTime, DateTime, int?>> pluginValues)
        {
            //update mapping state
            UpdateMonitoredSystemMappingState(monitoredSystemID);

            //create result
            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {
                var returns = new List<Tuple<int, List<Tuple<DateTime, string, MappingState>>>>();
                try
                {

                    foreach (Tuple<int, DateTime, DateTime, int?> vals in pluginValues)
                    {
                        var indicator = vals.Item1;
                        var type = (from ind in dataContext.Indicator
                                    let a = monitoredSystemID == ind.MonitoredSystem.ID
                                    let b = pluginID == ind.PluginMetadata.ID
                                    let c = indicator == ind.ID
                                    where a && b && c
                                    select ind.ValueType).First();

                        #region Datatype byte
                        if (type == (byte)DataType.Byte)
                        {
                            var innerList = (from ind in dataContext.Indicator
                                             join value in dataContext.IndicatorValue on ind.ID equals value.IndicatorID
                                             let a = monitoredSystemID == ind.MonitoredSystem.ID
                                             let b = pluginID == ind.PluginMetadata.ID
                                             let c = indicator == ind.ID
                                             let lastTime = TimeSpan.FromTicks(vals.Item3.Ticks)
                                             let firstTime = TimeSpan.FromTicks(vals.Item2.Ticks)
                                             let x = TimeSpan.FromTicks(value.Timestamp)
                                             where a && b && c && (x >= firstTime) && (x <= lastTime)
                                             select new List<Tuple<DateTime, string, MappingState>>(){
                                new Tuple<DateTime, string, MappingState>(new DateTime(value.Timestamp),value.ValueByte.Value.ToString(), (Core.MappingState)value.Mapping)
                                }).First();
                            int i;
                            List<Tuple<DateTime, string, MappingState>> list = new List<Tuple<DateTime, string, MappingState>>();
                            if (vals.Item4 != null)
                            {
                                for (i = 0; i <= vals.Item4; i++)
                                {
                                    list.Add(innerList.ElementAt(i));
                                }
                            }
                            else
                            {
                                list = innerList;
                            };

                            returns.Add(new Tuple<int, List<Tuple<DateTime, string, MappingState>>>(indicator, list));
                        };

                        #endregion

                        #region Datatype float
                        if (type == (byte)DataType.Float)
                        {
                            var innerList = (from ind in dataContext.Indicator
                                             join value in dataContext.IndicatorValue on ind.ID equals value.IndicatorID
                                             let a = monitoredSystemID == ind.MonitoredSystem.ID
                                             let b = pluginID == ind.PluginMetadata.ID
                                             let c = indicator == ind.ID
                                             let lastTime = TimeSpan.FromTicks(vals.Item3.Ticks)
                                             let firstTime = TimeSpan.FromTicks(vals.Item2.Ticks)
                                             let x = TimeSpan.FromTicks(value.Timestamp)
                                             where a && b && c && (x >= firstTime) && (x <= lastTime)
                                             select new List<Tuple<DateTime, string, MappingState>>(){
                                new Tuple<DateTime, string, MappingState>(new DateTime(value.Timestamp),value.ValueByte.Value.ToString(), (Core.MappingState)value.Mapping)
                                }).First();

                            int i;
                            List<Tuple<DateTime, string, MappingState>> list = new List<Tuple<DateTime, string, MappingState>>();
                            if (vals.Item4 != null)
                            {
                                for (i = 0; i <= vals.Item4; i++)
                                {
                                    list.Add(innerList.ElementAt(i));
                                }
                            }
                            else
                            {
                                list = innerList;
                            };

                            returns.Add(new Tuple<int, List<Tuple<DateTime, string, MappingState>>>(indicator, list));
                        };

                        #endregion

                        #region Datatype int

                        if (type == (byte)DataType.Int)
                        {
                            var innerList = (from ind in dataContext.Indicator
                                             join value in dataContext.IndicatorValue on ind.ID equals value.IndicatorID
                                             let a = monitoredSystemID == ind.MonitoredSystem.ID
                                             let b = pluginID == ind.PluginMetadata.ID
                                             let c = indicator == ind.ID
                                             let lastTime = TimeSpan.FromTicks(vals.Item3.Ticks)
                                             let firstTime = TimeSpan.FromTicks(vals.Item2.Ticks)
                                             let x = TimeSpan.FromTicks(value.Timestamp)
                                             where a && b && c && (x >= firstTime) && (x <= lastTime)
                                             select new List<Tuple<DateTime, string, MappingState>>(){
                                new Tuple<DateTime, string, MappingState>(new DateTime(value.Timestamp),value.ValueByte.Value.ToString(), (Core.MappingState)value.Mapping)
                                }).First();

                            int i;
                            List<Tuple<DateTime, string, MappingState>> list = new List<Tuple<DateTime, string, MappingState>>();
                            if (vals.Item4 != null)
                            {
                                for (i = 0; i <= vals.Item4; i++)
                                {
                                    list.Add(innerList.ElementAt(i));
                                }
                            }
                            else
                            {
                                list = innerList;
                            };

                            returns.Add(new Tuple<int, List<Tuple<DateTime, string, MappingState>>>(indicator, list));
                        };

                        #endregion

                        #region Datatype string

                        if (type == (byte)DataType.String)
                        {
                            var innerList = (from ind in dataContext.Indicator
                                             join value in dataContext.IndicatorValue on ind.ID equals value.IndicatorID
                                             let a = monitoredSystemID == ind.MonitoredSystem.ID
                                             let b = pluginID == ind.PluginMetadata.ID
                                             let c = indicator == ind.ID
                                             let lastTime = TimeSpan.FromTicks(vals.Item3.Ticks)
                                             let firstTime = TimeSpan.FromTicks(vals.Item2.Ticks)
                                             let x = TimeSpan.FromTicks(value.Timestamp)
                                             where a && b && c && (x >= firstTime) && (x <= lastTime)
                                             select new List<Tuple<DateTime, string, MappingState>>(){
                                new Tuple<DateTime, string, MappingState>(new DateTime(value.Timestamp),value.ValueByte.Value.ToString(), (Core.MappingState)value.Mapping)
                                }).First();

                            int i;
                            List<Tuple<DateTime, string, MappingState>> list = new List<Tuple<DateTime, string, MappingState>>();
                            if (vals.Item4 != null)
                            {
                                for (i = 0; i <= vals.Item4; i++)
                                {
                                    list.Add(innerList.ElementAt(i));
                                }
                            }
                            else
                            {
                                list = innerList;
                            };

                            returns.Add(new Tuple<int, List<Tuple<DateTime, string, MappingState>>>(indicator, list));
                        };
                        #endregion
                    }
                }
                catch (Exception e)
                {
                    //logging exception
                    var messageEx = new StringBuilder();
                    messageEx.Append("ClientWebService_GetData: ");
                    messageEx.Append("Can't get Data of workstation" + monitoredSystemID + ". " + e.ToString());
                    MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);

                    return null;
                }
                return returns;
            }
        }
        #endregion

        // Result representing one indicatorvalue
        private class QueryResult
        {
            public string PluginName;
            public string Name;
            public long? Timestamp;
            public byte? Mapping;
            public byte ValueType;
            public byte? ByteValue;
            public int? IntValue;
            public float? FloatValue;
            public String StringValue;
        }

        // Valuetype of the indicator
        private enum ValueType
        {
            String,
            Float,
            Int,
            Byte
        }

        /// <summary>
        /// Gets the latest indicator data for each indicator.
        /// </summary>
        /// <param name="macs">A list containing the macs of the monitored systems</param>
        /// <returns>List of: MAC | Pluginname | Indicatorname | Value | Mapping | Timestamp</returns>
        public List<Tuple<string, string, string, string, MappingState, DateTime>> GetLatestMonitoredSystemData(List<string> macList)
        {
            // Debug
            //DateTime now = DateTime.Now;

            List<Tuple<string, string, string, string, MappingState, DateTime>> result = new List<Tuple<string, string, string, string, MappingState, DateTime>>();

            if (macList == null || macList.Count < 1)
            {
                return result;
            }

            try
            {
                using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
                {
                    foreach (var mac in macList)
                    {
                        try
                        {
                            var ID = PrecompiledQueries.GetMonitoredSystemIDByMAC(dataContext, mac);
                            var OS = PrecompiledQueries.GetMonitoredSystemPlatformByID(dataContext, ID);

                            var query = new StringBuilder();

                            query.Append("select p.Name as PluginName, i.Name, Timestamp, iv.Mapping, i.ValueType, ");
                            query.Append("vb.Value as ByteValue, vi.Value as IntValue, ");
                            query.Append("vf.Value as FloatValue, vs.Value as StringValue from IndicatorValue iv ");
                            query.Append("left join Indicator i on iv.IndicatorID = i.ID ");
                            query.Append("left join PluginMetadata p on p.ID = i.PluginMetadataID ");
                            query.Append("left join ValueByte vb on vb.ID = iv.ValueID ");
                            query.Append("left join ValueInt vi on vi.ID = iv.ValueID ");
                            query.Append("left join ValueFloat vf on vf.ID = iv.ValueID ");
                            query.Append("left join ValueString vs on vs.ID = iv.ValueID ");
                            query.Append("where iv.ID in (select MAX(ID) from IndicatorValue ");
                            query.Append("where IndicatorID in ");
                            query.Append("(select ID from Indicator where MonitoredSystemID = " + ID + " and ");
                            query.Append("PluginMetadataID in (select ID from MISD.dbo.PluginMetadata Where ([Platform] = " + OS + " or [Platform] = 4))) ");
                            query.Append("group by IndicatorID)");

                            var queryResult = dataContext.ExecuteQuery<QueryResult>(query.ToString()).ToList();

                            foreach (var item in queryResult)
                            {
                                switch ((ValueType)item.ValueType)
                                {
                                    case ValueType.Byte:
                                        result.Add(new Tuple<string, string, string, string, MappingState, DateTime>
                                            (
                                                mac,
                                                item.PluginName,
                                                item.Name,
                                                item.ByteValue.ToString(),
                                                (MappingState)item.Mapping,
                                                new DateTime((long)item.Timestamp)
                                            )
                                        );
                                        break;
                                    case ValueType.Int:
                                        result.Add(new Tuple<string, string, string, string, MappingState, DateTime>
                                            (
                                                mac,
                                                item.PluginName,
                                                item.Name,
                                                item.IntValue.ToString(),
                                                (MappingState)item.Mapping,
                                                new DateTime((long)item.Timestamp)
                                            )
                                        );
                                        break;
                                    case ValueType.Float:
                                        result.Add(new Tuple<string, string, string, string, MappingState, DateTime>
                                            (
                                                mac,
                                                item.PluginName,
                                                item.Name,
                                                item.FloatValue.ToString(),
                                                (MappingState)item.Mapping,
                                                new DateTime((long)item.Timestamp)
                                            )
                                        );
                                        break;
                                    case ValueType.String:
                                        result.Add(new Tuple<string, string, string, string, MappingState, DateTime>
                                            (
                                                mac,
                                                item.PluginName,
                                                item.Name,
                                                item.StringValue.ToString(),
                                                (MappingState)item.Mapping,
                                                new DateTime((long)item.Timestamp)
                                            )
                                        );
                                        break;
                                    default:
                                        //logging problem
                                        var messageEx = new StringBuilder();
                                        messageEx.Append("ValueManager_GetLatestMonitoredSystemData: Unknown valuetype, cannot save indicatorvalue.");
                                        MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Warning);
                                        break;
                                }
                            }

                            // cache new element
                            //if (result.Count() > 0 && result.ElementAt(result.Count - 1) != null)
                            //{
                            //    this.CacheIndicator(result.ElementAt(result.Count - 1));
                            //}
                        }
                        catch (Exception e)
                        {
                            Logger.Instance.WriteEntry("ValueManager_GetLatestMonitoredSystemData: Problem getting data for mac " + mac, LogType.Exception);
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                #region logging
                //logging exception
                var messageEx = new StringBuilder();
                messageEx.Append("ValueManager_GetLatestMonitoredSystemData: Can't create List of PluginDatas" + ". " + e.ToString() + " StackTrace: " + e.StackTrace);
                MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);
                #endregion
            }

            // Debug
            //DateTime then = DateTime.Now;
            //Logger.Instance.WriteEntry("GetLatestMonitoredSystemData for " + macList.Count + " systems needed " + Math.Round(then.Subtract(now).TotalMilliseconds) + " milliseconds", LogType.Debug);

            return result;
        }

        /// <summary>
        /// Gets the complete data for serveral plugins of certain workstations and a certain timespan.
        /// </summary>
        /// <param name="macAndProperties"> A list containing tuples of:  MAC | PluginName | IndicatorName | LowerBound? | UpperBound? | Maximum numer of results?</param>
        /// <returns>A list containing tuples of: MAC | PluginName | IndicatorName | Value | Mapping | Time</returns>
        public List<Tuple<string, string, string, string, MappingState, DateTime>> GetData(List<Tuple<string, string, string, DateTime?, DateTime?, int?>> macAndProperties)
        {
            // Debug
            //DateTime now = DateTime.Now;

            #region check arguments
            if (macAndProperties == null || macAndProperties.Count == 0)
            {
                throw new ArgumentException("ValueManager_GetData: macAndProperties list item false.", "macAndProperties");
            }
            #endregion

            List<Tuple<string, string, string, string, MappingState, DateTime>> result = new List<Tuple<string, string, string, string, MappingState, DateTime>>();

            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {
                //current:  (MAC | PluginName | IndicatorName | LowerBound? | UpperBound? | Maximum numer of results?)
                foreach (var mac in macAndProperties)
                {
                    try
                    {
                        #region check arguments
                        if (mac.Item1 == null || mac.Item1 == "")
                        {
                            throw new ArgumentException("ValueManager_GetData: macAndProperties list item false.", "macAdress (Item1)");
                        }
                        if (mac.Item2 == null || mac.Item2 == "")
                        {
                            throw new ArgumentException("ValueManager_GetData: macAndProperties list item false.", "pluginName (Item2)");
                        }
                        if (mac.Item3 == null || mac.Item3 == "")
                        {
                            throw new ArgumentException("ValueManager_GetData: macAndProperties list item false.", "indicatorName (Item3)");
                        }

                        #endregion

                        //update inidcator mapping
                        UpdateIndicatorMappingState(mac.Item1, mac.Item2, mac.Item3);

                        //set numbers of values
                        int numbersOfValues = (mac.Item6 != null && mac.Item6 > 0) ? (int)mac.Item6 : int.MaxValue;
                        long lowerTicks = (mac.Item4 != null) ? mac.Item4.Value.Ticks : 0;
                        long upperTicks = (mac.Item5 != null) ? mac.Item5.Value.Ticks : long.MaxValue;

                        // caching for single results, continuing in loop
                        //if (numbersOfValues == 1)
                        //{
                        //    Tuple<string, string, string, string, MappingState, DateTime> value = GetCachedIndicator(mac.Item1, mac.Item2, mac.Item3);
                        //    if (value != null)
                        //    {
                        //        result.Add(value);
                        //        continue;
                        //    }
                        //}

                        var ID = PrecompiledQueries.GetMonitoredSystemIDByMAC(dataContext, mac.Item1);
                        var OS = PrecompiledQueries.GetMonitoredSystemPlatformByID(dataContext, ID);

                        if (ID == null || OS == null)
                        {
                            Logger.Instance.WriteEntry("ValueManager_GetData: Could not find the ID and/or OS for the mac " + mac.Item1, LogType.Debug);
                            continue;
                        }

                        DateTime superNow = DateTime.Now;

                        var query = new StringBuilder();

                        query.Append("select p.Name as PluginName, i.Name, Timestamp, iv.Mapping, i.ValueType, ");
                        query.Append("vb.Value as ByteValue, vi.Value as IntValue, ");
                        query.Append("vf.Value as FloatValue, vs.Value as StringValue from IndicatorValue iv ");
                        query.Append("left join Indicator i on iv.IndicatorID = i.ID ");
                        query.Append("left join PluginMetadata p on p.ID = i.PluginMetadataID ");
                        query.Append("left join ValueByte vb on vb.ID = iv.ValueID ");
                        query.Append("left join ValueInt vi on vi.ID = iv.ValueID ");
                        query.Append("left join ValueFloat vf on vf.ID = iv.ValueID ");
                        query.Append("left join ValueString vs on vs.ID = iv.ValueID ");
                        query.Append("where iv.ID in (select TOP " + numbersOfValues + " ID from IndicatorValue where ");
                        query.Append("IndicatorID in (select ID from Indicator where MonitoredSystemID = " + ID + ") and ");
                        query.Append("IndicatorID in (select ID from Indicator where PluginMetadataID in ");
                        query.Append("(select ID from PluginMetadata where Name = '" + mac.Item2 + "' and ([Platform] = " + OS + ") or [Platform] = 4)) and ");
                        query.Append("IndicatorID in (select ID from Indicator where Name = '" + mac.Item3 + "') and ");
                        query.Append("Timestamp >= " + lowerTicks + " and ");
                        query.Append("Timestamp <= " + upperTicks + " order by ID desc)");

                        var queryResult = dataContext.ExecuteQuery<QueryResult>(query.ToString()).ToList();

                        foreach (var item in queryResult)
                        {
                            try
                            {
                                switch ((ValueType)item.ValueType)
                                {
                                    case ValueType.Byte:
                                        result.Add(new Tuple<string, string, string, string, MappingState, DateTime>
                                            (
                                                mac.Item1,
                                                item.PluginName,
                                                item.Name,
                                                item.ByteValue.ToString(),
                                                (MappingState)item.Mapping,
                                                new DateTime((long)item.Timestamp)
                                            )
                                        );
                                        break;
                                    case ValueType.Int:
                                        result.Add(new Tuple<string, string, string, string, MappingState, DateTime>
                                            (
                                                mac.Item1,
                                                item.PluginName,
                                                item.Name,
                                                item.IntValue.ToString(),
                                                (MappingState)item.Mapping,
                                                new DateTime((long)item.Timestamp)
                                            )
                                        );
                                        break;
                                    case ValueType.Float:
                                        result.Add(new Tuple<string, string, string, string, MappingState, DateTime>
                                            (
                                                mac.Item1,
                                                item.PluginName,
                                                item.Name,
                                                item.FloatValue.ToString(),
                                                (MappingState)item.Mapping,
                                                new DateTime((long)item.Timestamp)
                                            )
                                        );
                                        break;
                                    case ValueType.String:
                                        result.Add(new Tuple<string, string, string, string, MappingState, DateTime>
                                            (
                                                mac.Item1,
                                                item.PluginName,
                                                item.Name,
                                                item.StringValue.ToString(),
                                                (MappingState)item.Mapping,
                                                new DateTime((long)item.Timestamp)
                                            )
                                        );
                                        break;
                                    default:
                                        break;
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Instance.WriteEntry("ValueManager_GetData: Problem saving entry, " + e.ToString(), LogType.Exception);
                                continue;
                            }

                        }

                        //cache first element
                        //if (numbersOfValues == 1 && result.Count() > 0 && result.ElementAt(0) != null)
                        //{
                        //    this.CacheIndicator(result.ElementAt(0));
                        //}
                    }
                    catch (Exception e)
                    {
                        #region logging
                        //logging exception
                        var messageEx = new StringBuilder();
                        messageEx.Append("ValueManager_GetData: Can't create List of PluginDatas" + ". " + e.ToString() + " StackTrace: " + e.StackTrace);
                        MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);
                        #endregion
                    }
                }
            }

            // Debug
            //DateTime then = DateTime.Now;
            //Logger.Instance.WriteEntry("GetData for " + macAndProperties.Count + " elements needed " + Math.Round(then.Subtract(now).TotalMilliseconds) + " milliseconds", LogType.Debug);

            return result;
        }


        /// <summary>
        /// Gets the complete data for serveral plugins of certain workstations. 
        /// This method can be used to retrieve Level 2 data .
        /// </summary>
        /// <param name="macAndPluginName"> A list containing tuples of:  MAC | PluginName</param>
        /// <returns>A list containing tuples of: MAC | PluginName | IndicatorName | Value | Mapping | Time</returns>
        public List<Tuple<string, string, string, string, MappingState, DateTime>> GetPluginData(List<Tuple<string, string>> macAndPluginName, int? numberOfIndicators)
        {
            List<Tuple<string, string, string, DateTime?, DateTime?, int?>> macAndProperties = new List<Tuple<string, string, string, DateTime?, DateTime?, int?>>();

            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {

                foreach (var names in macAndPluginName)
                {

                    try
                    {
                        var indicators = (from p in dataContext.Indicator
                                          where p.MonitoredSystem.MacAddress == names.Item1
                                          where p.PluginMetadata.Name == names.Item2
                                          select p.Name).ToList();
                        foreach (var ind in indicators)
                        {
                            macAndProperties.Add(new Tuple<string, string, string, DateTime?, DateTime?, int?>(names.Item1, names.Item2, ind, null, null, numberOfIndicators));
                        }

                    }
                    catch (Exception e)
                    {
                        //logging exception
                        #region logging
                        var messageEx = new StringBuilder();
                        messageEx.Append("ValueManager_GetPluginData: Can't create List of PluginDatas" + ". " + e.ToString());
                        MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);
                        #endregion
                    }
                }

                //get data
                return GetData(macAndProperties);
            }

        }

        /// <summary>
        /// Gets the data for all plugins of the given workstations .
        /// This method can be used to retrieve Level 3 data .
        /// </summary>
        /// <param name="mac">A list containing tuples of:  MAC | Maximum numer of results? per inticator</param>
        /// <returns>A list containing tuples of: MAC | PluginName | IndicatorName | Value | Mapping | Time</returns>
        public List<Tuple<string, string, string, string, MappingState, DateTime>> GetCompletePluginData(List<string> macList, int? numberOfIndicators)
        {
            List<Tuple<string, string>> macAndPluginName = new List<Tuple<string, string>>();

            try
            {
                using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
                {
                    foreach (var mac in macList)
                    {
                        try
                        {
                            var pluginNames = (from p in dataContext.Indicator
                                               where p.MonitoredSystem.MacAddress == mac
                                               select p.PluginMetadata.Name).Distinct().ToList();

                            foreach (var pName in pluginNames)
                            {
                                macAndPluginName.Add(new Tuple<string, string>(mac, pName));
                            }

                        }
                        catch (Exception e)
                        {
                            #region logging
                            //logging exception
                            var messageEx = new StringBuilder();
                            messageEx.Append("ValueManager_GetCompletePluginData: Can't create List of PluginDatas" + ". " + e.ToString());
                            MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);
                            #endregion
                        }
                    }
                }

                //get data
                return GetPluginData(macAndPluginName, numberOfIndicators);
            }
            catch (Exception e)
            {
                #region logging
                //logging exception
                var messageEx = new StringBuilder();
                messageEx.Append("ValueManager_GetCompletePluginData: Can't create List of PluginDatas" + ". " + e.ToString());
                MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);
                #endregion
                return null;
            }
        }

        /// <summary>
        /// Rests the mapping of a workstation
        /// </summary>
        /// <param name="macList">List of MACs</param>
        /// <returns></returns>
        public bool ResetMapping(List<Tuple<string, DateTime>> macList)
        {
            try
            {
                using (var dataContext = DataContextFactory.CreateDataContext())
                {
                    bool returnValue = true;
                    foreach (Tuple<string, DateTime> tuple in macList)
                    {
                        #region reset monitored system
                        var ms = (from q in dataContext.MonitoredSystem
                                  where q.MacAddress == tuple.Item1
                                  select q).FirstOrDefault();

                        if (ms != null && (ms.LastUpdate == null || ms.LastUpdate < tuple.Item2.Ticks))
                        {
                            ms.LastUpdate = tuple.Item2.Ticks;
                            ms.CriticalEnd = null;
                            ms.WarningEnd = null;
                            ms.Status = (byte)MappingState.OK;
                        }
                        else
                        {
                            returnValue = false;
                            Logger.Instance.WriteEntry("ValueManager_ResetMapping: Can't reset mapping. The update time is too late or the ms doesn't exist.", LogType.Warning);
                        }
                        #endregion

                        #region reset indicator Mapping
                        var indicators = (from q in dataContext.Indicator
                                          where q.MonitoredSystemID == ms.ID
                                          select q).ToList();

                        foreach (var ind in indicators)
                        {
                            ind.Status = (byte)MappingState.OK;
                            ind.WarningEnd = null;
                            ind.CriticalEnd = null;
                            dataContext.SubmitChanges();
                        }
                        #endregion
                    }
                    dataContext.SubmitChanges();
                    return returnValue;
                }
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("ValueManager_ResetMapping: Can't reset mapping. " + e.StackTrace, LogType.Exception);
                return false;
            }
        }

        /// <summary>
        /// Updates the mapping of a plugin
        /// </summary>
        /// <param name="pluginID">ID of the plugin to update</param>
        public void UpdateIndicatorMappingState(int indcatorID)
        {
            try
            {
                using (var dataContext = DataContextFactory.CreateDataContext())
                {
                    MappingState newState = MappingState.Critical;
                    var indicator = (from q in dataContext.Indicator
                                     where q.ID == indcatorID
                                     select q).FirstOrDefault();

                    if (indicator != null)
                    {
                        //critical state
                        if (indicator.CriticalEnd != null)
                        {
                            if (indicator.CriticalEnd < DateTime.Now.Ticks)
                            {
                                newState = MappingState.Warning;
                            }
                        }
                        else
                        {
                            newState = MappingState.Warning;
                        }


                        // Warning state
                        if (indicator.WarningEnd != null)
                        {
                            if (indicator.WarningEnd < DateTime.Now.Ticks)
                            {
                                newState = MappingState.OK;
                            }
                        }
                        else
                        {
                            newState = MappingState.OK;
                        }

                        indicator.Status = (byte)newState;
                        dataContext.SubmitChanges();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("ValueManager_UpdatePluginMappingState: Can't update pulugin mapping state. " + e, LogType.Exception);
            }
        }

        public void UpdateIndicatorMappingState(string mac, string pluginName, string indicatorName)
        {
            try
            {
                using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
                {
                    var msID = PrecompiledQueries.GetMonitoredSystemIDByMAC(dataContext, mac);
                    var msOS = PrecompiledQueries.GetMonitoredSystemPlatformByID(dataContext, msID);

                    var id = (from q in dataContext.Indicator
                              let ms = q.MonitoredSystemID == msID
                              let plugin = q.PluginMetadata.Name == pluginName
                              let indicator = q.Name == indicatorName
                              let platform = q.PluginMetadata.Platform == msOS
                              where ms && plugin && indicator && platform
                              select q.ID).FirstOrDefault();
                    UpdateIndicatorMappingState(id);
                }
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("ValueManager_UpdateIndicatorMappingState[mac, pluginname, indicatorname]: Can't update mapping states. " + e.StackTrace, LogType.Exception);
            }
        }

        public void UpdateIndicatorMappingState(string mac)
        {
            using (var dataContext = DataContextFactory.CreateDataContext())
            {
                var idList = (from q in dataContext.Indicator
                              where q.MonitoredSystem.MacAddress == mac
                              select q.ID).ToList();
                idList.ForEach(q => UpdateIndicatorMappingState(q));
            }
        }

        /// <summary>
        /// Updates the mapping state of a monitored system, if the mapping duration of this monitored system is out of date
        /// </summary>
        /// <param name="monitoredSystemID">MAC Adress of the monitored system to update</param>
        public void UpdateMonitoredSystemMappingState(string monitoredSystemMac)
        {
            try
            {
                using (var dataContext = DataContextFactory.CreateDataContext())
                {
                    var monitoredSystem = PrecompiledQueries.GetMonitoredSystemByMAC(dataContext, monitoredSystemMac);
                    MappingState newState = MappingState.Critical;

                    // Critical state
                    if (monitoredSystem.CriticalEnd != null)
                    {
                        if (monitoredSystem.CriticalEnd < DateTime.Now.Ticks)
                        {
                            newState = MappingState.Warning;
                        }
                    }
                    else
                    {
                        newState = MappingState.Warning;
                    }


                    // Warning state
                    if (monitoredSystem.WarningEnd != null)
                    {
                        if (monitoredSystem.WarningEnd < DateTime.Now.Ticks)
                        {
                            newState = MappingState.OK;
                        }
                    }
                    else
                    {
                        newState = MappingState.OK;
                    }


                    // Maintenance state
                    if (PrecompiledQueries.GetMonitoredSystemMaintenanceModeByID(dataContext, monitoredSystem.ID))
                    {
                        newState = MappingState.Maintenance;
                    }

                    monitoredSystem.Status = (byte)newState;
                    dataContext.SubmitChanges();
                }
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("ValueManager_UpdateMappingState: Can't update the mapping of workstation (MAC: " + monitoredSystemMac + "). " + e.StackTrace, LogType.Exception);
            }
        }

        /// <summary>
        /// Updates the mapping state of a monitored system, if the mapping duration of this monitored system is out of date
        /// </summary>
        /// <param name="monitoredSystemMac">ID of the monitored system to update</param>
        public void UpdateMonitoredSystemMappingState(int monitoredSystemID)
        {
            try
            {
                using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
                {
                    var monitoredSystem = PrecompiledQueries.GetMonitoredSystemByID(dataContext, monitoredSystemID);

                    if (monitoredSystem.MacAddress != null)
                    {
                        UpdateMonitoredSystemMappingState(monitoredSystem.MacAddress);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("ValueManager_UpdateMappingState: Can't update the mapping of workstation " + monitoredSystemID + ". " + e.StackTrace, LogType.Exception);
            }
        }

        #endregion
    }
}
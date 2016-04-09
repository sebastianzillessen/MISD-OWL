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
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using MISD.Core;
using MISD.Server.Performancetest.Properties;
using System.Text;
using MISD.Server.Performancetest.Database;
using System.Runtime.CompilerServices;
using System.Data.Linq;

namespace MISD.Server.Performancetest.Manager
{
    public class ValueManagerOld
    {
        #region Singleton

        private static volatile ValueManagerOld instance;
        private static object syncRoot = new Object();

        /// <summary>
        /// Property to access the class.
        /// </summary>
        public static ValueManagerOld Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ValueManagerOld();
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
                Console.WriteLine("FAILURE IN CahceIndicator: " + e, LogType.Exception);
            }
        }

        public Tuple<string, string, string, string, MappingState, DateTime> GetCachedIndicator(string Mac, string PluginName, string Indicator)
        {
            string key = String.Join("|", new string[] { Mac, PluginName, Indicator });
            return cachedValues.Get(key);
        }

        #endregion

        #region Constructors

        private ValueManagerOld()
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
                    messageEx.Append("Can't create List of WorkstationNames" + ". " + e.ToString());
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

        public Func<MISDDataContext, int, long, MappingState> GetMonitoredSystemStateByID = CompiledQuery.Compile<MISDDataContext, int, long, MappingState>((dataContext, monitoredSystemID, workstationReset) => (MappingState)dataContext.Indicator.Where(p => p.MonitoredSystemID == monitoredSystemID).Select(p => new { p.ID, p.MappingDuration }).Join(dataContext.IndicatorValue.Where(z => z.Mapping > 0).Select(p => new { p.IndicatorID, p.Timestamp, p.Mapping }), p => p.ID, q => q.IndicatorID, (p, q) => new { q.Timestamp, p.MappingDuration, q.Mapping }).Where(p => p.Timestamp > Math.Max(DateTime.Now.Ticks - workstationReset, DateTime.Now.Ticks - p.MappingDuration)).Select(p => p.Mapping).Max());

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
                        UpdateMappingState(current.Item1);

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
                    messageEx.Append("Can't create List of WorkstationInfos" + ". " + e.ToString());
                    MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);

                    return null;
                }
            }
        }

        // timestamp, Mapping, Value
        private class ValueResult
        {
            public long Timestamp;
            public byte Mapping;
            public string Value;
        }

        /// <summary>
        /// Gets the complete data for serveral plugins of certain workstations and a certain timespan.
        /// </summary>
        /// <param name="macAndProperties"> A list containing tuples of:  MAC | PluginName | IndicatorName | LowerBound? | UpperBound? | Maximum numer of results?</param>
        /// <returns>A list containing tuples of: MAC | PluginName | IndicatorName | Value | Mapping | Time</returns>
        public List<Tuple<string, string, string, string, MappingState, DateTime>> GetDataOLDAPPROACH(List<Tuple<string, string, string, DateTime?, DateTime?, int?>> macAndProperties)
        {

            #region check arguments
            if (macAndProperties == null || macAndProperties.Count == 0)
            {
                throw new ArgumentException("macAndProperties list item false.", "macAndProperties");
            }
            #endregion

            List<Tuple<string, string, string, string, MappingState, DateTime>> result = new List<Tuple<string, string, string, string, MappingState, DateTime>>();


            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {

                //current:  (MAC | PluginName | IndicatorName | LowerBound? | UpperBound? | Maximum numer of results?)
                foreach (var c in macAndProperties)
                {
                    try
                    {
                        // TODO Arno entfernen
                        //break;

                        #region check arguments
                        if (c.Item1 == null || c.Item1 == "")
                        {
                            throw new ArgumentException("macAndProperties list item false.", "macAdress (Item1)");
                        }
                        if (c.Item2 == null || c.Item2 == "")
                        {
                            throw new ArgumentException("macAndProperties list item false.", "pluginName (Item2)");
                        }
                        if (c.Item3 == null || c.Item3 == "")
                        {
                            throw new ArgumentException("macAndProperties list item false.", "IndicatorName (Item3)");
                        }

                        #endregion

                        //update mapping state
                        UpdateMappingState(c.Item1);

                        //set numbers of values
                        int numbersOfValues = (c.Item6 != null && c.Item6 > 0) ? (int)c.Item6 : int.MaxValue;
                        long lowerTicks = (c.Item4 != null) ? c.Item4.Value.Ticks : 0;
                        long upperTicks = (c.Item5 != null) ? c.Item5.Value.Ticks : long.MaxValue;

                        #region caching for single results, continuing in loop
                        if (numbersOfValues == 1)
                        {
                            Tuple<string, string, string, string, MappingState, DateTime> value = GetCachedIndicator(c.Item1, c.Item2, c.Item3);
                            if (value != null)
                            {
                                result.Add(value);
                                continue;
                            }
                        }
                        #endregion

                        //get valueX type
                        var type = (from q in dataContext.Indicator
                                    where q.Name == c.Item3
                                    select q.ValueType).FirstOrDefault();

                        if (!(type == (byte)Core.DataType.Byte || type == (byte)Core.DataType.Float || type == (byte)Core.DataType.Int || type == (byte)Core.DataType.String))
                        {
                            continue;
                        }

                        //get valueX, create sub-results and add them to the result
                        #region byte
                        if (type == (byte)Core.DataType.Byte)
                        {
                            var byteResult = PrecompiledQueries.GetByteValues(dataContext, c.Item1, c.Item2, c.Item3, lowerTicks, upperTicks, numbersOfValues);
                            result.AddRange(byteResult);
                        }
                        #endregion

                        #region int
                        if (type == (byte)DataType.Int)
                        {
                            var intResult = PrecompiledQueries.GetIntValues(dataContext, c.Item1, c.Item2, c.Item3, lowerTicks, upperTicks, numbersOfValues);
                            result.AddRange(intResult);
                        }
                        #endregion

                        #region float
                        if (type == (byte)DataType.Float)
                        {
                            var floatResult = PrecompiledQueries.GetFloatValues(dataContext, c.Item1, c.Item2, c.Item3, lowerTicks, upperTicks, numbersOfValues);
                            result.AddRange(floatResult);
                        }
                        #endregion

                        #region string
                        if (type == (byte)DataType.String)
                        {
                            var stringResult = PrecompiledQueries.GetStringValues(dataContext, c.Item1, c.Item2, c.Item3, lowerTicks, upperTicks, numbersOfValues);
                            result.AddRange(stringResult);
                        }
                        #endregion

                        //cache first element
                        if (numbersOfValues == 1 && result.ElementAt(0) != null)
                        {
                            this.CacheIndicator(result.ElementAt(0));
                        }
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

            //TODO remove this log
            Logger.Instance.WriteEntry("ValueManager_GetData: Data createtd and returned: " + result.Count + " elements for " + macAndProperties.Count() + " mac/plugin/indicator combis starting with " + macAndProperties.First().Item1 + " and Plugin " + macAndProperties.First().Item2, LogType.Debug);

            return result;
        }

        /// <summary>
        /// Gets the complete data for serveral plugins of certain workstations and a certain timespan.
        /// </summary>
        /// <param name="macAndProperties"> A list containing tuples of:  MAC | PluginName | IndicatorName | LowerBound? | UpperBound? | Maximum numer of results?</param>
        /// <returns>A list containing tuples of: MAC | PluginName | IndicatorName | Value | Mapping | Time</returns>
        public List<Tuple<string, string, string, string, MappingState, DateTime>> GetData(List<Tuple<string, string, string, DateTime?, DateTime?, int?>> macAndProperties)
        {
            #region check arguments
            if (macAndProperties == null || macAndProperties.Count == 0)
            {
                throw new ArgumentException("macAndProperties list item false.", "macAndProperties");
            }
            #endregion

            List<Tuple<string, string, string, string, MappingState, DateTime>> result = new List<Tuple<string, string, string, string, MappingState, DateTime>>();

            try
            {
                using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
                {

                    //current:  (MAC | PluginName | IndicatorName | LowerBound? | UpperBound? | Maximum numer of results?)
                    foreach (var c in macAndProperties)
                    {
                        // TODO Arno entfernen
                        break;

                        #region check arguments
                        if (c.Item1 == null || c.Item1 == "")
                        {
                            throw new ArgumentException("macAndProperties list item false.", "macAdress (Item1)");
                        }
                        if (c.Item2 == null || c.Item2 == "")
                        {
                            throw new ArgumentException("macAndProperties list item false.", "pluginName (Item2)");
                        }
                        if (c.Item3 == null || c.Item3 == "")
                        {
                            throw new ArgumentException("macAndProperties list item false.", "IndicatorName (Item3)");
                        }

                        #endregion

                        //update mapping state
                        UpdateMappingState(c.Item1);

                        //set numbers of values
                        int numbersOfValues = (c.Item6 != null) ? (int)c.Item6 : int.MaxValue;
                        long lowerTicks = (c.Item4 != null) ? c.Item4.Value.Ticks : 0;
                        long upperTicks = (c.Item5 != null) ? c.Item5.Value.Ticks : long.MaxValue;

                        //get valueX type
                        var type = (from q in dataContext.Indicator
                                    where q.Name == c.Item3
                                    select (Core.DataType)q.ValueType).First();


                        //get valueX, create sub-results and add them to the result
                        string tableName = "";
                        #region byte
                        if (type == Core.DataType.Byte)
                        {

                            tableName = "ValueByte";

                        }
                        #endregion

                        #region int
                        if (type == DataType.Int)
                        {
                            tableName = "ValueInt";
                        }
                        #endregion

                        #region float
                        if (type == DataType.Float)
                        {
                            tableName = "ValueFloat";
                        }
                        #endregion

                        #region string
                        if (type == DataType.String)
                        {
                            tableName = "ValueString";
                        }
                        #endregion

                        if (tableName != "")
                        {
                            #region query
                            var query = new StringBuilder();
                            query.Append("SELECT TOP {5} A.Timestamp, A.Mapping, V.Value FROM (SELECT ValueID,Timestamp,Mapping FROM MISD.dbo.IndicatorValue IV where IV.Timestamp>={3}  AND IV.Timestamp<={4} AND IV.IndicatorID IN ");
                            query.Append("(SELECT IndicatorID FROM ");
                            query.Append("(Select ID AS IndicatorID,MonitoredSystemID from ( ");
                            query.Append("Select ID AS PM_ID FROM MISD.dbo.PluginMetadata PM ");
                            query.Append("WHERE PM.Name = '{1}' ) PM ");
                            query.Append("JOIN MISD.dbo.Indicator I ON I.PluginMetadataID = PM.PM_ID WHERE I.Name = '{2}') IP ");
                            query.Append("JOIN MISD.dbo.MonitoredSystem MS ON IP.MonitoredSystemID = MS.ID WHERE MS.MacAddress = '{0}' ");
                            query.Append(") ");
                            query.Append(") A JOIN MISD.dbo.{6} V on V.ID = A.ValueID ");
                            query.Append("ORDER BY A.Timestamp");
                            #endregion
                            var tempResult = dataContext.ExecuteQuery<ValueResult>(
                                query.ToString(),
                                c.Item1,
                                c.Item2,
                                c.Item3,
                                lowerTicks,
                                upperTicks,
                                numbersOfValues,
                                tableName).ToList();
                            Logger.Instance.WriteEntry(tempResult.ToString(), LogType.Exception);
                            foreach (ValueResult b in tempResult)
                            {
                                Tuple<string, string, string, string, MappingState, DateTime> t = new Tuple<string, string, string, string, MappingState, DateTime>
                                    (
                                    c.Item1,
                                    c.Item2,
                                    c.Item3,
                                    b.Value.ToString(),
                                    (MappingState)b.Mapping,
                                    new DateTime(b.Timestamp)
                                    );
                                result.Add(t);
                            }
                        }
                    }

                }
            }
            catch (ArgumentException argumentException)
            {
                throw argumentException;
            }
            catch (Exception e)
            {
                #region logging
                //logging exception
                var messageEx = new StringBuilder();
                messageEx.Append("ValueManager_GetData: Can't create List of PluginDatas" + ". " + e.ToString());
                MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);

                //TODO remove this log
                Logger.Instance.WriteEntry("ValueManager_GetData: error. " + e.StackTrace, LogType.Debug);
                #endregion
                return null;
            }

            //TODO remove this log
            Logger.Instance.WriteEntry("ValueManager_GetData: Data createtd and returned: " + result.Count + " for " + macAndProperties.First().Item1 + macAndProperties.First().Item2, LogType.Debug);

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
                        messageEx.Append("ValueManager_GetPluginData: Can't create List of PluginDatas" + ". " + e.ToString() + "| " + e.StackTrace);
                        MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);
                        #endregion
                    }
                }

                //get data
                return GetDataOLDAPPROACH(macAndProperties);
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
                    foreach (Tuple<string, DateTime> tuple in macList)
                    {
                        bool returnValue = true;

                        var ms = (from q in dataContext.MonitoredSystem
                                  where q.MacAddress == tuple.Item1
                                  select q).First();

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
                    }
                    dataContext.SubmitChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("ValueManager_ResetMapping: Can't reset mapping. " + e.StackTrace, LogType.Exception);
                return false;
            }
        }

        /// <summary>
        /// Updates the mapping state of a monitored system, if the mapping duration of this monitored system is out of date
        /// </summary>
        /// <param name="monitoredSystemID">MAC Adress of the monitored system to update</param>
        public void UpdateMappingState(string monitoredSystemMac)
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
                    else { newState = MappingState.Warning; }


                    // Warning state
                    if (monitoredSystem.WarningEnd != null)
                    {
                        if (monitoredSystem.WarningEnd < DateTime.Now.Ticks)
                        {
                            newState = MappingState.OK;
                        }
                    }
                    else
                    { newState = MappingState.OK; }


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
        public void UpdateMappingState(int monitoredSystemID)
        {
            try
            {
                using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
                {
                    var monitoredSystem = PrecompiledQueries.GetMonitoredSystemByID(dataContext, monitoredSystemID);

                    if (monitoredSystem.MacAddress != null)
                    {
                        UpdateMappingState(monitoredSystem.MacAddress);
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

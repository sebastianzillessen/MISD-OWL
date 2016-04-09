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
using System.Data.Linq;
using System.Linq;
using System.Collections.Generic;
using MISD.Core;

namespace MISD.Server.Performancetest.Database
{
    /// <summary>
    /// This class contains some useful precompiled queries.
    /// </summary>
    public static class PrecompiledQueries
    {
        /// <summary>
        /// This precompiled query returns the ID of a monitored system while taking the monitored system's FQDN as input.
        /// </summary>
        public static Func<MISDDataContext, string, int> GetMonitoredSystemIDByMAC = CompiledQuery.Compile<MISDDataContext, string, int>((dataContext, MAC) => dataContext.MonitoredSystem.Where(p => p.MacAddress == MAC).Select(p => p.ID).Single());

        /// <summary>
        /// This precompiled query returns the name of a monitored system while taking the monitored system's ID as input.
        /// </summary>
        public static Func<MISDDataContext, int, string> GetMonitoredSystemNameByID = CompiledQuery.Compile<MISDDataContext, int, string>((dataContext, ID) => dataContext.MonitoredSystem.Where(p => p.ID == ID).Select(p => p.Name).Single());

        /// <summary>
        /// This precompiled query returns the ID of a monitored system while taking the monitored system's FQDN as input.
        /// </summary>
        public static Func<MISDDataContext, string, int> GetPluginMetadataIDByName = CompiledQuery.Compile<MISDDataContext, string, int>((dataContext, pluginName) => dataContext.PluginMetadata.Where(p => p.Name == pluginName).FirstOrDefault().ID);

        /// <summary>
        /// This precompiled query returns a monitored system while taking the monitored system's ID as input.
        /// </summary>
        public static Func<MISDDataContext, int, MonitoredSystem> GetMonitoredSystemByID = CompiledQuery.Compile<MISDDataContext, int, MonitoredSystem>((dataContext, ID) => dataContext.MonitoredSystem.Where(p => p.ID == ID).FirstOrDefault());

        /// <summary>
        /// This precompiled query returns a monitored system while taking the monitored system's ID as input.
        /// </summary>
        public static Func<MISDDataContext, string, MonitoredSystem> GetMonitoredSystemByMAC = CompiledQuery.Compile<MISDDataContext, string, MonitoredSystem>((dataContext, mac) => dataContext.MonitoredSystem.Where(p => p.MacAddress == mac).FirstOrDefault());

        /// <summary>
        /// This precompiled query returns a monitored system's operating system while taking the monitored system's ID as input.
        /// </summary>
        public static Func<MISDDataContext, int, byte> GetMonitoredSystemPlatformByID = CompiledQuery.Compile<MISDDataContext, int, byte>((dataContext, ID) => dataContext.MonitoredSystem.Where(p => p.ID == ID).Single().OperatingSystem);

        public static Func<MISDDataContext, int, bool> GetMonitoredSystemMaintenanceModeByID = CompiledQuery.Compile<MISDDataContext, int, bool>((dataContext, ID) => dataContext.Maintenance.Where(p => p.MonitoredSystemID == ID).Where(p => p.Beginning != null).Where(p => p.End == null).Count() > 0);

        /// <summary>
        /// This precompiled query returns a plugin metadata operating system while taking a monitored system's ID and a platform as input.
        /// </summary>
        public static Func<MISDDataContext, int, byte, IEnumerable<Core.PluginMetadata>> GetCorePluginMetadataByMonitoredSystemIDAndPlatform = CompiledQuery.Compile<MISDDataContext, int, byte, IEnumerable<Core.PluginMetadata>>((dataContext, monitoredSystemID, platform) => dataContext.PluginMetadata.Where(p => p.Platform == platform).Select(p => new Core.PluginMetadata()
        {
            Name = p.Name,
            Description = p.Description,
            FileName = p.FileName,
            Company = p.Company,
            Version = (p.Version == null) ? new Version(0, 1) : new Version(p.Version),
            Indicators = dataContext.Indicator.Where(q => (q.PluginMetadataID == p.ID) && (q.MonitoredSystemID == monitoredSystemID)).Select(r => new Core.IndicatorSettings()
            {
                PluginName = p.Name,
                IndicatorName = r.Name,
                MonitoredSystemMAC = r.MonitoredSystem.MacAddress,
                FilterStatement = r.FilterStatement,
                UpdateInterval = (r.UpdateInterval == null) ? new TimeSpan(1, 0, 0) : new TimeSpan((long)r.UpdateInterval),
                StorageDuration = (r.StorageDuration == null) ? new TimeSpan(31, 0, 0, 0) : new TimeSpan((long)r.StorageDuration),
                MappingDuration = (r.MappingDuration == null) ? new TimeSpan(24, 0, 0) : new TimeSpan((long)r.MappingDuration)
            }).ToList()
        }));

        /// <summary>
        /// This precompiled query returns a plugin metadata for visualization plugins.
        /// </summary>
        public static Func<MISDDataContext, IEnumerable<Core.PluginMetadata>> GetCorePluginMetadataForVisualisation = CompiledQuery.Compile<MISDDataContext, IEnumerable<Core.PluginMetadata>>((dataContext) => dataContext.PluginMetadata.Where(p => p.Platform == 5).Select(p => new Core.PluginMetadata()
        {
            Name = p.Name,
            Description = p.Description,
            FileName = p.FileName,
            Company = p.Company,
            Version = (p.Version == null) ? new Version(0, 1) : new Version(p.Version),
            Indicators = dataContext.Indicator.Where(q => (q.PluginMetadataID == p.ID)).Select(r => new Core.IndicatorSettings()
            {
                PluginName = p.Name,
                IndicatorName = r.Name,
                //MonitoredSystemMAC = r.MonitoredSystem.MacAddress,
                //FilterStatement = r.FilterStatement,
                UpdateInterval = (r.UpdateInterval == null) ? new TimeSpan(1, 0, 0) : new TimeSpan((long)r.UpdateInterval),
                StorageDuration = (r.StorageDuration == null) ? new TimeSpan(31, 0, 0, 0) : new TimeSpan((long)r.StorageDuration),
                MappingDuration = (r.MappingDuration == null) ? new TimeSpan(24, 0, 0) : new TimeSpan((long)r.MappingDuration)
            }).ToList()
        }));

        /// <summary>
        /// This precompiled query returns a plugin metadata for visualization plugins.
        /// </summary>
        //public static Func<MISDDataContext, int, byte> GetMonitoredSystemStateByID = 
        //    CompiledQuery.Compile<MISDDataContext, int, byte>((dataContext) => dataContext.Indicator.Join();

        //(MappingState)((from p in dataContext.Indicator
        //                                                  join value in dataContext.IndicatorValue on p.ID equals value.IndicatorID
        //                                                  let system = p.MonitoredSystem.ID == current.Item1
        //                                                  let mappingDuration = p.MappingDuration == null ? 0 : p.MappingDuration
        //                                                  let deadLineTicks = DateTime.Now.Ticks - mappingDuration
        //                                                  let resetLineTicks = current.Item2
        //                                                  let indicatorValueTicks = value.Timestamp
        //                                                  where system && (indicatorValueTicks >= deadLineTicks) && (indicatorValueTicks >= resetLineTicks.Ticks)
        //                                                  group value by value.Mapping into q
        //                                                  from r in q
        //                                                  select r.Mapping).Max());

        /// <summary>
        /// Returns the byte values of given monitored system
        /// </summary>
        /// <param name="dataContext">DataContext of the database.</param>
        /// <param name="mac">MAC adress of the monitored system.</param>
        /// <param name="pluginName">Plugin name</param>
        /// <param name="indicatorName">Indicator name</param>
        /// <param name="indicatorID">The indicator ID</param>
        /// <param name="lowerTicks">Lower time bound of the values as ticks</param>
        /// <param name="upperTicks">Upper time bound of the values as ticks</param>
        /// <param name="numberOfResults">Number of results per indicator</param>
        /// <returns>A list containing tuples of: MAC | PluginName | IndicatorName | Value | Mapping | Time</returns>
        public static Func<MISDDataContext, string, string, string, int, long, long, int, IEnumerable<Tuple<string, string, string, string, MappingState, DateTime>>>
            GetByteValuesFast = CompiledQuery.Compile<MISDDataContext, string, string, string, int, long, long, int, IEnumerable<Tuple<string, string, string, string, MappingState, DateTime>>>
            ((dataContext, mac, pluginName, indicatorName, indicatorID, lowerTicks, upperTicks, numberOfResults) =>
                dataContext.IndicatorValue.
                Where(q => q.IndicatorID == indicatorID).
                Where(q => q.Timestamp >= lowerTicks).
                Where(q => q.Timestamp <= upperTicks).
                OrderByDescending(q => q.Timestamp).
                Take(numberOfResults).
                Select(q => new Tuple<string, string, string, string, MappingState, DateTime>
                    (mac, pluginName, indicatorName, q.ValueByte.Value.ToString(), MappingState.OK, new DateTime(q.Timestamp))).
                ToList());

        /// <summary>
        /// Returns the int values of given monitored system
        /// </summary>
        /// <param name="dataContext">DataContext of the database.</param>
        /// <param name="mac">MAC adress of the monitored system.</param>
        /// <param name="pluginName">Plugin name</param>
        /// <param name="indicatorName">Indicator name</param>
        /// <param name="indicatorID">The indicator ID</param>
        /// <param name="lowerTicks">Lower time bound of the values as ticks</param>
        /// <param name="upperTicks">Upper time bound of the values as ticks</param>
        /// <param name="numberOfResults">Number of results per indicator</param>
        /// <returns>A list containing tuples of: MAC | PluginName | IndicatorName | Value | Mapping | Time</returns>
        public static Func<MISDDataContext, string, string, string, int, long, long, int, IEnumerable<Tuple<string, string, string, string, MappingState, DateTime>>>
            GetIntValuesFast = CompiledQuery.Compile<MISDDataContext, string, string, string, int, long, long, int, IEnumerable<Tuple<string, string, string, string, MappingState, DateTime>>>
            ((dataContext, mac, pluginName, indicatorName, indicatorID, lowerTicks, upperTicks, numberOfResults) =>
                dataContext.IndicatorValue.
                Where(q => q.IndicatorID == indicatorID).
                Where(q => q.Timestamp >= lowerTicks).
                Where(q => q.Timestamp <= upperTicks).
                OrderByDescending(q => q.Timestamp).
                Take(numberOfResults).
                Select(q => new Tuple<string, string, string, string, MappingState, DateTime>
                    (mac, pluginName, indicatorName, q.ValueInt.Value.ToString(), MappingState.OK, new DateTime(q.Timestamp))).
                ToList());

        /// <summary>
        /// Returns the float values of given monitored system
        /// </summary>
        /// <param name="dataContext">DataContext of the database.</param>
        /// <param name="mac">MAC adress of the monitored system.</param>
        /// <param name="pluginName">Plugin name</param>
        /// <param name="indicatorName">Indicator name</param>
        /// <param name="indicatorID">The indicator ID</param>
        /// <param name="lowerTicks">Lower time bound of the values as ticks</param>
        /// <param name="upperTicks">Upper time bound of the values as ticks</param>
        /// <param name="numberOfResults">Number of results per indicator</param>
        /// <returns>A list containing tuples of: MAC | PluginName | IndicatorName | Value | Mapping | Time</returns>
        public static Func<MISDDataContext, string, string, string, int, long, long, int, IEnumerable<Tuple<string, string, string, string, MappingState, DateTime>>>
            GetFloatValuesFast = CompiledQuery.Compile<MISDDataContext, string, string, string, int, long, long, int, IEnumerable<Tuple<string, string, string, string, MappingState, DateTime>>>
            ((dataContext, mac, pluginName, indicatorName, indicatorID, lowerTicks, upperTicks, numberOfResults) =>
                dataContext.IndicatorValue.
                Where(q => q.IndicatorID == indicatorID).
                Where(q => q.Timestamp >= lowerTicks).
                Where(q => q.Timestamp <= upperTicks).
                OrderByDescending(q => q.Timestamp).
                Take(numberOfResults).
                Select(q => new Tuple<string, string, string, string, MappingState, DateTime>
                    (mac, pluginName, indicatorName, q.ValueFloat.Value.ToString(), MappingState.OK, new DateTime(q.Timestamp))).
                ToList());

        /// <summary>
        /// Returns the string values of given monitored system
        /// </summary>
        /// <param name="dataContext">DataContext of the database.</param>
        /// <param name="mac">MAC adress of the monitored system.</param>
        /// <param name="pluginName">Plugin name</param>
        /// <param name="indicatorName">Indicator name</param>
        /// <param name="indicatorID">The indicator ID</param>
        /// <param name="lowerTicks">Lower time bound of the values as ticks</param>
        /// <param name="upperTicks">Upper time bound of the values as ticks</param>
        /// <param name="numberOfResults">Number of results per indicator</param>
        /// <returns>A list containing tuples of: MAC | PluginName | IndicatorName | Value | Mapping | Time</returns>
        public static Func<MISDDataContext, string, string, string, int, long, long, int, IEnumerable<Tuple<string, string, string, string, MappingState, DateTime>>>
            GetStringValuesFast = CompiledQuery.Compile<MISDDataContext, string, string, string, int, long, long, int, IEnumerable<Tuple<string, string, string, string, MappingState, DateTime>>>
            ((dataContext, mac, pluginName, indicatorName, indicatorID, lowerTicks, upperTicks, numberOfResults) =>
                dataContext.IndicatorValue.
                Where(q => q.IndicatorID == indicatorID).
                Where(q => q.Timestamp >= lowerTicks).
                Where(q => q.Timestamp <= upperTicks).
                OrderByDescending(q => q.Timestamp).
                Take(numberOfResults).
                Select(q => new Tuple<string, string, string, string, MappingState, DateTime>
                    (mac, pluginName, indicatorName, q.ValueString.Value.ToString(), MappingState.OK, new DateTime(q.Timestamp))).
                ToList());

        /// <summary>
        /// Returns the byte values of given monitored system
        /// </summary>
        /// <param name="dataContext">DataContext of the database.</param>
        /// <param name="mac">MAC adress of the monitored system.</param>
        /// <param name="pluginName">Plugin name</param>
        /// <param name="indicatorName">Indicator name</param>
        /// <param name="lowerTicks">Lower time bound of the values as ticks</param>
        /// <param name="upperTicks">Upper time bound of the values as ticks</param>
        /// <param name="numberOfResults">Number of results per indicator</param>
        /// <returns>A list containing tuples of: MAC | PluginName | IndicatorName | Value | Mapping | Time</returns>
        public static Func<MISDDataContext, string, string, string, long, long, int, IEnumerable<Tuple<string, string, string, string, MappingState, DateTime>>> GetByteValues = CompiledQuery.Compile<MISDDataContext, string, string, string, long, long, int, IEnumerable<Tuple<string, string, string, string, MappingState, DateTime>>>((dataContext, mac, pluginName, indicatorName, lowerTicks, upperTicks, numberOfResults) =>
                dataContext.Indicator.
                Where(q => q.PluginMetadata.Name == pluginName).
                Where(q => q.Name == indicatorName).
                Where(q => q.MonitoredSystem.MacAddress == mac).
                Join(dataContext.IndicatorValue, p => p.ID, s => s.IndicatorID, (p, s) => new { IndicatorValue = s }).
                Where(s => s.IndicatorValue.Timestamp >= lowerTicks).
                Where(s => s.IndicatorValue.Timestamp <= upperTicks).
                Take(numberOfResults).
                Select(s => new Tuple<string, string, string, string, MappingState, DateTime>(mac, pluginName, indicatorName, s.IndicatorValue.ValueByte.Value.ToString(), MappingState.OK, new DateTime(s.IndicatorValue.Timestamp))).
                ToList());

        /// <summary>
        /// Returns the integer values of given monitored system
        /// </summary>
        /// <param name="dataContext">DataContext of the database.</param>
        /// <param name="mac">MAC adress of the monitored system.</param>
        /// <param name="pluginName">Plugin name</param>
        /// <param name="indicatorName">Indicator name</param>
        /// <param name="lowerTicks">Lower time bound of the values as ticks</param>
        /// <param name="upperTicks">Upper time bound of the values as ticks</param>
        /// <param name="numberOfResults">Number of results per indicator</param>
        /// <returns>A list containing tuples of: MAC | PluginName | IndicatorName | Value | Mapping | Time</returns>
        public static Func<MISDDataContext, string, string, string, long, long, int, IEnumerable<Tuple<string, string, string, string, MappingState, DateTime>>> GetIntValues = CompiledQuery.Compile<MISDDataContext, string, string, string, long, long, int, IEnumerable<Tuple<string, string, string, string, MappingState, DateTime>>>((dataContext, mac, pluginName, indicatorName, lowerTicks, upperTicks, numberOfResults) =>
                dataContext.Indicator.
                Where(q => q.PluginMetadata.Name == pluginName).
                Where(q => q.Name == indicatorName).
                Where(q => q.MonitoredSystem.MacAddress == mac).
                Join(dataContext.IndicatorValue, p => p.ID, s => s.IndicatorID, (p, s) => new { IndicatorValue = s }).
                Where(s => s.IndicatorValue.Timestamp >= lowerTicks).
                Where(s => s.IndicatorValue.Timestamp <= upperTicks).
                Take(numberOfResults).
                Select(s => new Tuple<string, string, string, string, MappingState, DateTime>(mac, pluginName, indicatorName, s.IndicatorValue.ValueInt.Value.ToString(), MappingState.OK, new DateTime(s.IndicatorValue.Timestamp))).
                ToList());

        /// <summary>
        /// Returns the float values of given monitored system
        /// </summary>
        /// <param name="dataContext">DataContext of the database.</param>
        /// <param name="mac">MAC adress of the monitored system.</param>
        /// <param name="pluginName">Plugin name</param>
        /// <param name="indicatorName">Indicator name</param>
        /// <param name="lowerTicks">Lower time bound of the values as ticks</param>
        /// <param name="upperTicks">Upper time bound of the values as ticks</param>
        /// <param name="numberOfResults">Number of results per indicator</param>
        /// <returns>A list containing tuples of: MAC | PluginName | IndicatorName | Value | Mapping | Time</returns>
        public static Func<MISDDataContext, string, string, string, long, long, int, IEnumerable<Tuple<string, string, string, string, MappingState, DateTime>>> GetFloatValues = CompiledQuery.Compile<MISDDataContext, string, string, string, long, long, int, IEnumerable<Tuple<string, string, string, string, MappingState, DateTime>>>((dataContext, mac, pluginName, indicatorName, lowerTicks, upperTicks, numberOfResults) =>
                dataContext.Indicator.
                Where(q => q.PluginMetadata.Name == pluginName).
                Where(q => q.Name == indicatorName).
                Where(q => q.MonitoredSystem.MacAddress == mac).
                Join(dataContext.IndicatorValue, p => p.ID, s => s.IndicatorID, (p, s) => new { IndicatorValue = s }).
                Where(s => s.IndicatorValue.Timestamp >= lowerTicks).
                Where(s => s.IndicatorValue.Timestamp <= upperTicks).
                Take(numberOfResults).
                Select(s => new Tuple<string, string, string, string, MappingState, DateTime>(mac, pluginName, indicatorName, s.IndicatorValue.ValueFloat.Value.ToString(), MappingState.OK, new DateTime(s.IndicatorValue.Timestamp))).
                ToList());

        /// <summary>
        /// Returns the string values of given monitored system
        /// </summary>
        /// <param name="dataContext">DataContext of the database.</param>
        /// <param name="mac">MAC adress of the monitored system.</param>
        /// <param name="pluginName">Plugin name</param>
        /// <param name="indicatorName">Indicator name</param>
        /// <param name="lowerTicks">Lower time bound of the values as ticks</param>
        /// <param name="upperTicks">Upper time bound of the values as ticks</param>
        /// <param name="numberOfResults">Number of results per indicator</param>
        /// <returns>A list containing tuples of: MAC | PluginName | IndicatorName | Value | Mapping | Time</returns>
        public static Func<MISDDataContext, string, string, string, long, long, int, IEnumerable<Tuple<string, string, string, string, MappingState, DateTime>>> GetStringValues = CompiledQuery.Compile<MISDDataContext, string, string, string, long, long, int, IEnumerable<Tuple<string, string, string, string, MappingState, DateTime>>>((dataContext, mac, pluginName, indicatorName, lowerTicks, upperTicks, numberOfResults) =>
                dataContext.Indicator.
                Where(q => q.PluginMetadata.Name == pluginName).
                Where(q => q.Name == indicatorName).
                Where(q => q.MonitoredSystem.MacAddress == mac).
                Join(dataContext.IndicatorValue, p => p.ID, s => s.IndicatorID, (p, s) => new { IndicatorValue = s }).
                Where(s => s.IndicatorValue.Timestamp >= lowerTicks).
                Where(s => s.IndicatorValue.Timestamp <= upperTicks).
                Take(numberOfResults).
                Select(s => new Tuple<string, string, string, string, MappingState, DateTime>(mac, pluginName, indicatorName, s.IndicatorValue.ValueString.Value, MappingState.OK, new DateTime(s.IndicatorValue.Timestamp))).
                ToList());
    }
}

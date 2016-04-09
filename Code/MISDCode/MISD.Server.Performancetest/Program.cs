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
using System.Threading.Tasks;
using MISD.Server.Performancetest.Database;
using MISD.Server.Performancetest.Manager;
using MISD.Core;
using System.Data.Linq;
using System.Data.SqlClient;

namespace MISD.Server.Performancetest
{
    class Program
    {
        private class ValueResultByte
        {
            public long Timestamp;
            public byte Mapping;
            public byte Value;
        }

        private class ValueResultInt
        {
            public long Timestamp;
            public byte Mapping;
            public int Value;
        }

        private class ValueResultFloat
        {
            public long Timestamp;
            public byte Mapping;
            public float Value;
        }

        private class ValueResultString
        {
            public long Timestamp;
            public byte Mapping;
            public string Value;
        }

        private class ValueResult
        {
            public Object name;
            public Object indicatorname;
            public Object timestamp;
            public Object valuetype;
            public Object bytevalue;
            public Object floatvalue;
            public Object intvalue;
            public Object stringvalue;
        }

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

        /// <summary>
        /// The different data types for indicator values
        /// </summary>
        private enum ValueType
        {
            String,
            Float,
            Int,
            Byte
        }

        #region raw approach methods

        /// <summary>
        /// Reads input from the console to determine whether to query one system or all systems.
        /// </summary>
        /// <param name="whereClause">On successful return, will be set to the appropriate WHERE
        /// clause to insert into the query</param>
        /// <returns>true for success, false for invalid input</returns>
        static bool ReadInput(out string whereClause)
        {
            whereClause = "";

            Console.WriteLine("Current indicator values for which monitored system ID?");
            Console.Write("(Press Enter to query all monitored systems): ");

            string input = Console.ReadLine();
            if (input != "")
            {
                int systemId;
                try
                {
                    systemId = int.Parse(input);
                }
                catch (FormatException)
                {
                    // Invalid input
                    return false;
                }

                // Consider only indicator values referring to this particular system.
                whereClause = "where IndicatorID in (select ID from Indicator " +
                    "where MonitoredSystemID = " + systemId + ") ";
            }

            return true;
        }

        /// <summary>
        /// Prints the current indicator values from a query result.
        /// </summary>
        /// <param name="reader">The data reader resulting from the query</param>
        /// <returns>The total number of indicator values displayed</returns>
        static int PrintResults(SqlDataReader reader)
        {
            string lastMachineName = null;
            int values = 0;
            while (reader.Read())
            {
                // Print the machine name only once per machine
                string machineName = (string)reader[0];
                if (machineName != lastMachineName)
                {
                    Console.WriteLine();
                    Console.WriteLine(machineName);
                    Console.WriteLine(new String('=', 40));
                    lastMachineName = machineName;
                }

                // Indent the indicator name
                Console.Write("    " + (string)reader[1] + ": ");

                // Print the value of the correct type
                switch ((ValueType)(byte)reader[3])
                {
                    case ValueType.String:
                        Console.Write((string)reader[7]);
                        break;

                    case ValueType.Float:
                        Console.Write((double)reader[5]);
                        break;

                    case ValueType.Int:
                        Console.Write((int)reader[6]);
                        break;

                    case ValueType.Byte:
                        Console.Write((byte)reader[4]);
                        break;
                }

                // Add the timestamp of this value
                Console.WriteLine(" (" + new DateTime((long)reader[2]) + ")");

                values++;
            }

            return values;
        }

        #endregion

        static void Main(string[] args)
        {
            Console.WriteLine("<-<-<-<-<PERFORMANCETEST>->->->->");

            var macs = ValueManagerOld.Instance.GetWorkstationMACs();

            List<string> macList = new List<string>();
            for (int i = 0; i < macs.Count; i++)
            {
                if (i % 20 == 0)
                {
                    macList.Add(macs.ElementAt(i));
                    Console.WriteLine("Added " + i + ". system " + macs.ElementAt(i));
                }
            }

            #region new optimal approach

            DateTime now3 = DateTime.Now;

            Console.WriteLine("Starting NEW OPTIMAL APPROACH");

            #region manual query

            for (int j = 0; j < 1; j++)
            {
                List<Tuple<string, string, string, string, MappingState, DateTime>> result2 = new List<Tuple<string, string, string, string, MappingState, DateTime>>();

                using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
                {
                    foreach (var mac in macList)
                    {
                        var ID = PrecompiledQueries.GetMonitoredSystemIDByMAC(dataContext, mac);
                        var OS = PrecompiledQueries.GetMonitoredSystemPlatformByID(dataContext, ID);

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
                        query.Append("where iv.ID in (select MAX(ID) from IndicatorValue ");
                        query.Append("where IndicatorID in ");
                        query.Append("(select ID from Indicator where MonitoredSystemID = " + ID + " and ");
                        query.Append("PluginMetadataID in (select ID from MISD.dbo.PluginMetadata Where [Platform] = " + OS + ")) ");
                        query.Append("group by IndicatorID)");

                        var result = dataContext.ExecuteQuery<QueryResult>(query.ToString()).ToList();

                        foreach (var item in result)
                        {
                            switch ((ValueType)item.ValueType)
                            {
                                case ValueType.Byte:
                                    result2.Add(new Tuple<string, string, string, string, MappingState, DateTime>
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
                                    result2.Add(new Tuple<string, string, string, string, MappingState, DateTime>
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
                                    result2.Add(new Tuple<string, string, string, string, MappingState, DateTime>
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
                                    result2.Add(new Tuple<string, string, string, string, MappingState, DateTime>
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
                                    break;
                            }
                        }
                        DateTime superThen = DateTime.Now;
                        Console.WriteLine(Math.Round(superThen.Subtract(superNow).TotalMilliseconds) + " milliseconds for id " + mac + " needed ");
                    }
                }
                Console.WriteLine("Count: " + result2.Count());
                //foreach (var entry in result2)
                //{
                //    Console.WriteLine(entry.Item1 + " " + entry.Item2 + " " + entry.Item3 + " " + entry.Item4);
                //}
            }

            #endregion

            Console.WriteLine("Finished");

            DateTime then3 = DateTime.Now;
            TimeSpan difference3 = then3.Subtract(now3);

            Console.WriteLine("Needed time RAW: " + difference3.TotalSeconds);

            #endregion

            #region new different approach

            //DateTime now = DateTime.Now;

            //Console.WriteLine("Starting NEW DIFFERENT METHOD");

            //for (int j = 0; j < 1; j++)
            //{
            //    var values = ValueManager.Instance.GetLatestMonitoredSystemData(macList);
            //    Console.WriteLine("Count: " + values.Count());
            //    foreach (var entry in values)
            //    {
            //        Console.WriteLine(entry.Item2 + " " + entry.Item3 + " " + entry.Item4);
            //    }
            //}

            //Console.WriteLine("Finished");

            //DateTime then = DateTime.Now;
            //TimeSpan difference = then.Subtract(now);

            //Console.WriteLine("Needed time NEW DIFFERENT METHOD: " + difference.TotalSeconds);

            #endregion

            #region new approach

            //DateTime now0 = DateTime.Now;

            //Console.WriteLine("Starting NEW");

            //for (int j = 0; j < 1; j++)
            //{
            //    var values = ValueManager.Instance.GetCompletePluginData(macList, 1);
            //    Console.WriteLine("Count: " + values.Count());
            //    //foreach (var entry in values)
            //    //{
            //    //    Console.WriteLine(entry.Item2 + " " + entry.Item3 + " " + entry.Item4);
            //    //}
            //}

            //Console.WriteLine("Finished");

            //DateTime then0 = DateTime.Now;
            //TimeSpan difference0 = then0.Subtract(now0);

            //Console.WriteLine("Needed time NEW: " + difference0.TotalSeconds);

            #endregion

            #region raw approach

            DateTime now1 = DateTime.Now;

            //Console.WriteLine("Starting RAW APPROACH");

            //for (int j = 0; j < 1; j++)
            //{
            //    // Replace these with a valid user name and password to access the database.
            //    string userName = "stupro";
            //    string password = "C0mplex";

            //    if (userName == "" || password == "")
            //    {
            //        Console.WriteLine("Please set database user name and password in the code first.");
            //        return;
            //    }

            //    string whereClause = "";
            //    //if (!ReadInput(out whereClause))
            //    //    return;

            //    Console.WriteLine("Retrieving values...");

            //    // Measure the time from before opening the connection to after retrieving the result set.
            //    DateTime startTime = DateTime.Now;

            //    SqlConnection connection = new SqlConnection(
            //        "Data Source=acid.visus.uni-stuttgart.de;Initial Catalog=MISD;" +
            //        "Persist Security Info=True;User ID=" + userName + ";Password=" + password);
            //    connection.Open();

            //    // Build one complex query to retrieve all the required information. This queries the most
            //    // recently added indicator values for all indicators for all systems (or just a single
            //    // system if whereClause has been set accordingly). It also resolves any names and
            //    // translates any value ids.
            //    SqlCommand command = new SqlCommand(
            //        "select m.Name, i.Name, Timestamp, i.ValueType, " +
            //        "vb.Value as ByteValue, vf.Value as FloatValue, vi.Value as IntValue, " +
            //        "vs.Value as StringValue from IndicatorValue v " +
            //        "left join Indicator i on v.IndicatorID = i.ID " +
            //        "left join MonitoredSystem m on m.ID = i.MonitoredSystemID " +
            //        "left join ValueByte vb on vb.ID = v.ValueID " +
            //        "left join ValueFloat vf on vf.ID = v.ValueID " +
            //        "left join ValueInt vi on vi.ID = v.ValueID " +
            //        "left join ValueString vs on vs.ID = v.ValueID " +
            //        "where v.ID in (select MAX(ID) from IndicatorValue " +
            //        //whereClause +
            //        "group by IndicatorID) " +
            //        "order by m.Name, i.Name",
            //        connection);

            //    SqlDataReader reader = command.ExecuteReader();
            //    TimeSpan queryTime = DateTime.Now - startTime;

            //    // Print and count the results.
            //    int values = PrintResults(reader);

            //    reader.Dispose();
            //    command.Dispose();
            //    connection.Dispose();

            //    // Print the statistics.
            //    Console.WriteLine();
            //    Console.WriteLine("Read a total of " + values + " values in " + queryTime);
            //}

            //Console.WriteLine("Finished");

            //DateTime then1 = DateTime.Now;
            //TimeSpan difference1 = then1.Subtract(now1);

            //Console.WriteLine("Needed time RAW APPROACH: " + difference1.TotalSeconds);

            #endregion

            #region raw approach custom

            //DateTime now2 = DateTime.Now;

            //Console.WriteLine("Starting RAW APPROACH");

            //for (int j = 0; j < 1; j++)
            //{
            //    var whereClause = "where IndicatorID in (select ID from Indicator " +
            //    "where MonitoredSystemID = 1)";

            //    whereClause = "";

            //    using (DataContext context = DataContextFactory.CreateReadOnlyDataContext())
            //    {
            //        var result = context.ExecuteQuery<ValueResult>(
            //            "select m.Name, i.Name as IndicatorName, Timestamp, i.ValueType, " +
            //            "vb.Value as ByteValue, vf.Value as FloatValue, vi.Value as IntValue, " +
            //            "vs.Value as StringValue from IndicatorValue v " +
            //            "left join Indicator i on v.IndicatorID = i.ID " +
            //            "left join MonitoredSystem m on m.ID = i.MonitoredSystemID " +
            //            "left join ValueByte vb on vb.ID = v.ValueID " +
            //            "left join ValueFloat vf on vf.ID = v.ValueID " +
            //            "left join ValueInt vi on vi.ID = v.ValueID " +
            //            "left join ValueString vs on vs.ID = v.ValueID " +
            //            "where v.ID in (select MAX(ID) from IndicatorValue " +
            //            whereClause +
            //            "group by IndicatorID) " +
            //            "order by m.Name, i.Name"
            //            ).ToList();

            //        Console.WriteLine("Count: " + result.Count());

            //        //foreach (var item in result)
            //        //{
            //        //    Console.WriteLine("name: " + item.name + " indicator: " + item.indicatorname + "time: " + item.timestamp + " type: " + item.valuetype);
            //        //    Console.WriteLine("      " + item.stringvalue + " / " + item.floatvalue + " / " + item.intvalue + " / " + item.bytevalue);
            //        //}
            //    }
            //}

            //Console.WriteLine("Finished");

            //DateTime then2 = DateTime.Now;
            //TimeSpan difference2 = then2.Subtract(now2);

            //Console.WriteLine("Needed time RAW APPROACH: " + difference2.TotalSeconds);

            #endregion

            #region old approach

            //DateTime now1 = DateTime.Now;

            //Console.WriteLine("Starting OLD");

            //for (int j = 0; j < 1; j++)
            //{
            //    var values = ValueManagerOld.Instance.GetCompletePluginData(macList, 1);
            //    Console.WriteLine("Count: " + values.Count());
            //    //foreach (var entry in values)
            //    //{
            //    //    Console.WriteLine(entry.Item2 + " " + entry.Item3 + " " + entry.Item4);
            //    //}
            //}

            //Console.WriteLine("Finished");

            //DateTime then1 = DateTime.Now;
            //TimeSpan difference1 = then1.Subtract(now1);

            //Console.WriteLine("Needed time OLD: " + difference1.TotalSeconds);

            #endregion

            #region precompiled

            //DateTime now2 = DateTime.Now;

            //Console.WriteLine("Starting PRECOMPILED");

            //#region manual query

            //for (int j = 0; j < 1; j++)
            //{
            //    List<Tuple<string, string, string, string, MappingState, DateTime>> result = new List<Tuple<string, string, string, string, MappingState, DateTime>>();

            //    using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            //    {
            //        foreach (var mac in macList)
            //        {
            //            for (int i = 0; i < 1; i++)
            //            {
            //                var pluginNames = (from p in dataContext.Indicator
            //                                   where p.MonitoredSystem.MacAddress == mac
            //                                   select p.PluginMetadata.Name).Distinct().ToList();

            //                foreach (var pName in pluginNames)
            //                {
            //                    var indicators = (from q in dataContext.Indicator
            //                                      where q.MonitoredSystem.MacAddress == mac
            //                                      where q.PluginMetadata.Name == pName
            //                                      select new Tuple<string, byte, int>(q.Name, q.ValueType, q.ID)).ToList();

            //                    foreach (var iName in indicators)
            //                    {
            //                        if (iName.Item2 == (byte)Core.DataType.Byte)
            //                        {
            //                            var byteResult = PrecompiledQueries.GetByteValuesFast(dataContext, mac, pName, iName.Item1, iName.Item3, 0, long.MaxValue, 1);
            //                            result.AddRange(byteResult);
            //                        }

            //                        if (iName.Item2 == (byte)DataType.Int)
            //                        {
            //                            var intResult = PrecompiledQueries.GetIntValuesFast(dataContext, mac, pName, iName.Item1, iName.Item3, 0, long.MaxValue, 1);
            //                            result.AddRange(intResult);
            //                        }

            //                        if (iName.Item2 == (byte)DataType.Float)
            //                        {
            //                            var floatResult = PrecompiledQueries.GetFloatValuesFast(dataContext, mac, pName, iName.Item1, iName.Item3, 0, long.MaxValue, 1);
            //                            result.AddRange(floatResult);
            //                        }

            //                        if (iName.Item2 == (byte)DataType.String)
            //                        {
            //                            var stringResult = PrecompiledQueries.GetStringValuesFast(dataContext, mac, pName, iName.Item1, iName.Item3, 0, long.MaxValue, 1);
            //                            result.AddRange(stringResult);
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    Console.WriteLine("Count: " + result.Count());
            //    foreach (var entry in result)
            //    {
            //        Console.WriteLine(entry.Item2 + " " + entry.Item3 + " " + entry.Item4);
            //    }
            //}

            //#endregion

            //Console.WriteLine("Finished");

            //DateTime then2 = DateTime.Now;
            //TimeSpan difference2 = then2.Subtract(now2);

            //Console.WriteLine("Needed time PRECOMPILED: " + difference2.TotalSeconds);

            #endregion

            #region raw sql

            //DateTime now3 = DateTime.Now;

            //Console.WriteLine("Starting RAW");

            //#region manual query

            //for (int j = 0; j < 1; j++)
            //{
            //    List<Tuple<string, string, string, string, MappingState, DateTime>> result2 = new List<Tuple<string, string, string, string, MappingState, DateTime>>();

            //    using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            //    {
            //        foreach (var mac in macList)
            //        {
            //            for (int i = 0; i < 1; i++)
            //            {
            //                //DateTime now = DateTime.Now;

            //                var pluginNames = (from p in dataContext.Indicator
            //                                   where p.MonitoredSystem.MacAddress == mac
            //                                   select new Tuple<string, int>(p.PluginMetadata.Name, p.PluginMetadata.ID)).Distinct().ToList();

            //                //DateTime then = DateTime.Now;
            //                //Console.WriteLine(Math.Round(then.Subtract(now).TotalMilliseconds) + " milliseconds for query mac " + mac + " needed ");

            //                foreach (var plugin in pluginNames)
            //                {
            //                    //DateTime superNow = DateTime.Now;

            //                    var indicators = (from p in dataContext.Indicator
            //                                      where p.MonitoredSystem.MacAddress == mac
            //                                      where p.PluginMetadataID == plugin.Item2
            //                                      select new Tuple<string, byte, int>(p.Name, p.ValueType, p.ID)).ToList();

            //                    //DateTime superThen = DateTime.Now;
            //                    //Console.WriteLine(Math.Round(superThen.Subtract(superNow).TotalMilliseconds) + " milliseconds for query plugin " + plugin.Item1 + " needed ");

            //                    //foreach (var indi in indicators)
            //                    //{
            //                    //    Console.WriteLine("indicator: " + indi);
            //                    //}

            //                    foreach (var iName in indicators)
            //                    {
            //                        DateTime superNow2 = DateTime.Now;

            //                        if (iName.Item2 == (byte)Core.DataType.Byte)
            //                        {
            //                            var query = new StringBuilder();

            //                            //"select m.Name, i.Name as IndicatorName, Timestamp, i.ValueType, " +
            //                            //"vb.Value as ByteValue, vf.Value as FloatValue, vi.Value as IntValue, " +
            //                            //"vs.Value as StringValue from IndicatorValue v " +
            //                            //"left join Indicator i on v.IndicatorID = i.ID " +
            //                            //"left join MonitoredSystem m on m.ID = i.MonitoredSystemID " +
            //                            //"left join ValueByte vb on vb.ID = v.ValueID " +
            //                            //"left join ValueFloat vf on vf.ID = v.ValueID " +
            //                            //"left join ValueInt vi on vi.ID = v.ValueID " +
            //                            //"left join ValueString vs on vs.ID = v.ValueID " +
            //                            //"where v.ID in (select MAX(ID) from IndicatorValue " +
            //                            ////whereClause +
            //                            //"group by IndicatorID) " +
            //                            //"order by m.Name, i.Name"

            //                            query.Append("select Timestamp, iv.Mapping, v.Value from IndicatorValue iv ");
            //                            query.Append("left join Indicator i on iv.IndicatorID = i.ID ");
            //                            query.Append("left join ValueByte v on v.ID = iv.ValueID ");
            //                            query.Append("where iv.ID in (select TOP 1 ID from IndicatorValue ");
            //                            query.Append("where IndicatorID = " + iName.Item3 + " order by ID desc)");

            //                            //query.Append("SELECT IV.Timestamp, IV.Mapping, V.Value FROM ");
            //                            //query.Append("((SELECT MAX(ID) FROM MISD.dbo.IndicatorValue IV where ");
            //                            //query.Append("IV.IndicatorID=" + iName.Item3);
            //                            //query.Append(") A JOIN MISD.dbo.ValueByte V on V.ID = A.ValueID) ");
            //                            //query.Append("ORDER BY A.Timestamp DESC");

            //                            var tempResult = dataContext.ExecuteQuery<ValueResultByte>(query.ToString()).ToList();
            //                            foreach (var b in tempResult)
            //                            {
            //                                Tuple<string, string, string, string, MappingState, DateTime> t = new Tuple<string, string, string, string, MappingState, DateTime>
            //                                (mac,
            //                                plugin.Item1,
            //                                iName.Item1,
            //                                b.Value.ToString(),
            //                                (MappingState)b.Mapping,
            //                                new DateTime(b.Timestamp));
            //                                result2.Add(t);
            //                            }
            //                        }

            //                        if (iName.Item2 == (byte)DataType.Int)
            //                        {
            //                            var query = new StringBuilder();

            //                            query.Append("select Timestamp, iv.Mapping, v.Value from IndicatorValue iv ");
            //                            query.Append("left join Indicator i on iv.IndicatorID = i.ID ");
            //                            query.Append("left join ValueInt v on v.ID = iv.ValueID ");
            //                            query.Append("where iv.ID in (select TOP 1 ID from IndicatorValue ");
            //                            query.Append("where IndicatorID = " + iName.Item3 + " order by ID desc)");

            //                            //query.Append("SELECT IV.Timestamp, IV.Mapping, V.Value FROM ");
            //                            //query.Append("((SELECT MAX(ID) FROM MISD.dbo.IndicatorValue IV where ");
            //                            //query.Append("IV.IndicatorID=" + iName.Item3);
            //                            //query.Append(") A JOIN MISD.dbo.ValueInt V on V.ID = A.ValueID) ");
            //                            //query.Append("ORDER BY A.Timestamp DESC");

            //                            var tempResult = dataContext.ExecuteQuery<ValueResultInt>(query.ToString()).ToList();
            //                            foreach (var b in tempResult)
            //                            {
            //                                Tuple<string, string, string, string, MappingState, DateTime> t = new Tuple<string, string, string, string, MappingState, DateTime>
            //                                (mac,
            //                                plugin.Item1,
            //                                iName.Item1,
            //                                b.Value.ToString(),
            //                                (MappingState)b.Mapping,
            //                                new DateTime(b.Timestamp));
            //                                result2.Add(t);
            //                            }
            //                        }

            //                        if (iName.Item2 == (byte)DataType.Float)
            //                        {
            //                            var query = new StringBuilder();

            //                            query.Append("select Timestamp, iv.Mapping, v.Value from IndicatorValue iv ");
            //                            query.Append("left join Indicator i on iv.IndicatorID = i.ID ");
            //                            query.Append("left join ValueFloat v on v.ID = iv.ValueID ");
            //                            query.Append("where iv.ID in (select TOP 1 ID from IndicatorValue ");
            //                            query.Append("where IndicatorID = " + iName.Item3 + " order by ID desc)");

            //                            //query.Append("SELECT IV.Timestamp, IV.Mapping, V.Value FROM ");
            //                            //query.Append("((SELECT MAX(ID) FROM MISD.dbo.IndicatorValue IV where ");
            //                            //query.Append("IV.IndicatorID=" + iName.Item3);
            //                            //query.Append(") A JOIN MISD.dbo.ValueFloat V on V.ID = A.ValueID) ");
            //                            //query.Append("ORDER BY A.Timestamp DESC");

            //                            var tempResult = dataContext.ExecuteQuery<ValueResultFloat>(query.ToString()).ToList();
            //                            foreach (var b in tempResult)
            //                            {
            //                                Tuple<string, string, string, string, MappingState, DateTime> t = new Tuple<string, string, string, string, MappingState, DateTime>
            //                                (mac,
            //                                plugin.Item1,
            //                                iName.Item1,
            //                                b.Value.ToString(),
            //                                (MappingState)b.Mapping,
            //                                new DateTime(b.Timestamp));
            //                                result2.Add(t);
            //                            }
            //                        }

            //                        if (iName.Item2 == (byte)DataType.String)
            //                        {
            //                            var query = new StringBuilder();

            //                            query.Append("select Timestamp, iv.Mapping, v.Value from IndicatorValue iv ");
            //                            query.Append("left join Indicator i on iv.IndicatorID = i.ID ");
            //                            query.Append("left join ValueString v on v.ID = iv.ValueID ");
            //                            query.Append("where iv.ID in (select TOP 1 ID from IndicatorValue ");
            //                            query.Append("where IndicatorID = " + iName.Item3 + " order by ID desc)");

            //                            //query.Append("SELECT IV.Timestamp, IV.Mapping, V.Value FROM ");
            //                            //query.Append("((SELECT MAX(ID) FROM MISD.dbo.IndicatorValue IV where ");
            //                            //query.Append("IV.IndicatorID=" + iName.Item3);
            //                            //query.Append(") A JOIN MISD.dbo.ValueString V on V.ID = A.ValueID) ");
            //                            //query.Append("ORDER BY A.Timestamp DESC");

            //                            var tempResult = dataContext.ExecuteQuery<ValueResultString>(query.ToString()).ToList();
            //                            foreach (var b in tempResult)
            //                            {
            //                                Tuple<string, string, string, string, MappingState, DateTime> t = new Tuple<string, string, string, string, MappingState, DateTime>
            //                                (mac,
            //                                plugin.Item1,
            //                                iName.Item1,
            //                                b.Value.ToString(),
            //                                (MappingState)b.Mapping,
            //                                new DateTime(b.Timestamp));
            //                                result2.Add(t);
            //                            }
            //                        }
            //                        DateTime superThen2 = DateTime.Now;
            //                        Console.WriteLine(Math.Round(superThen2.Subtract(superNow2).TotalMilliseconds) + " milliseconds for query indicator " + iName + " needed ");
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    Console.WriteLine("Count: " + result2.Count());
            //    //foreach (var entry in result2)
            //    //{
            //    //    Console.WriteLine(entry.Item2 + " " + entry.Item3 + " " + entry.Item4);
            //    //}
            //}

            //#endregion

            //Console.WriteLine("Finished");

            //DateTime then3 = DateTime.Now;
            //TimeSpan difference3 = then3.Subtract(now3);

            //Console.WriteLine("Needed time RAW: " + difference3.TotalSeconds);

            #endregion

            Console.WriteLine("<-<-<-<-<LEVEL L>->->->->");

            // build input
            List<Tuple<string, string, string, DateTime?, DateTime?, int?>> input = new List<Tuple<string, string, string, DateTime?, DateTime?, int?>>();
            foreach (var mac in macList)
            {
                input.Add(new Tuple<string, string, string, DateTime?, DateTime?, int?>(mac, "CPU", "Load", new DateTime(0), new DateTime(2100, 1, 1), 40));
                input.Add(new Tuple<string, string, string, DateTime?, DateTime?, int?>(mac, "CPU", "ProcessorName", new DateTime(0), new DateTime(2100, 1, 1), 40));
                input.Add(new Tuple<string, string, string, DateTime?, DateTime?, int?>(mac, "RAM", "Size", new DateTime(0), new DateTime(2100, 1, 1), 40));
                input.Add(new Tuple<string, string, string, DateTime?, DateTime?, int?>(mac, "Storage", "NamePerDrive", new DateTime(0), new DateTime(2100, 1, 1), 40));
                input.Add(new Tuple<string, string, string, DateTime?, DateTime?, int?>(mac, "Events", "Event", new DateTime(0), new DateTime(2100, 1, 1), 40));
            }

            // result
            List<Tuple<string, string, string, string, MappingState, DateTime>> queryResult = new List<Tuple<string, string, string, string, MappingState, DateTime>>();

            #region new

            Console.WriteLine("Starting Level L new");
            DateTime nowLevelL = DateTime.Now;

            #region query

            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {
                foreach (var currentValue in input)
                {
                    var ID = PrecompiledQueries.GetMonitoredSystemIDByMAC(dataContext, currentValue.Item1);
                    var OS = PrecompiledQueries.GetMonitoredSystemPlatformByID(dataContext, ID);

                    DateTime superNow = DateTime.Now;

                    var query = new StringBuilder();

                    //query.Append("select TOP " + currentValue.Item6 + " p.Name as PluginName, iv.Name, iv.Timestamp, iv.Mapping, iv.ValueType, ");
                    //query.Append("vb.Value as ByteValue, vi.Value as IntValue, ");
                    //query.Append("vf.Value as FloatValue, vs.Value as StringValue from IndiValComb iv ");
                    //query.Append("left join PluginMetadata p on p.ID = iv.PluginMetadataID ");
                    //query.Append("left join ValueByte vb on vb.ID = iv.ValueID ");
                    //query.Append("left join ValueInt vi on vi.ID = iv.ValueID ");
                    //query.Append("left join ValueFloat vf on vf.ID = iv.ValueID ");
                    //query.Append("left join ValueString vs on vs.ID = iv.ValueID ");
                    //query.Append("where iv.ID in (select ID from Indicator where MonitoredSystemID = " + ID + " and ");
                    //query.Append("PluginMetadataID in (select ID from PluginMetadata where Name = '" + currentValue.Item2 + "') and ");
                    //query.Append("Name = '" + currentValue.Item3 + "') and ");
                    //query.Append("Timestamp >= " + ((DateTime)currentValue.Item4).Ticks + " and ");
                    //query.Append("Timestamp <= " + ((DateTime)currentValue.Item5).Ticks + " ");
                    //query.Append("order by iv.Timestamp desc");

                    query.Append("select p.Name as PluginName, i.Name, Timestamp, iv.Mapping, i.ValueType, ");
                    query.Append("vb.Value as ByteValue, vi.Value as IntValue, ");
                    query.Append("vf.Value as FloatValue, vs.Value as StringValue from IndicatorValue iv ");
                    query.Append("left join Indicator i on iv.IndicatorID = i.ID ");
                    query.Append("left join PluginMetadata p on p.ID = i.PluginMetadataID ");
                    query.Append("left join ValueByte vb on vb.ID = iv.ValueID ");
                    query.Append("left join ValueInt vi on vi.ID = iv.ValueID ");
                    query.Append("left join ValueFloat vf on vf.ID = iv.ValueID ");
                    query.Append("left join ValueString vs on vs.ID = iv.ValueID ");
                    query.Append("where iv.ID in (select TOP " + currentValue.Item6 + " ID from IndicatorValue where ");
                    query.Append("IndicatorID in (select ID from Indicator where MonitoredSystemID = " + ID + ") and ");
                    query.Append("IndicatorID in (select ID from Indicator where PluginMetadataID in (select ID from PluginMetadata where Name = '" + currentValue.Item2 + "' and [Platform] = "+ OS + ")) and ");
                    query.Append("IndicatorID in (select ID from Indicator where Name = '" + currentValue.Item3 + "') and ");
                    query.Append("Timestamp >= " + ((DateTime)currentValue.Item4).Ticks + " and ");
                    query.Append("Timestamp <= " + ((DateTime)currentValue.Item5).Ticks + " order by ID desc)");

                    var result = dataContext.ExecuteQuery<QueryResult>(query.ToString()).ToList();

                    foreach (var item in result)
                    {
                        switch ((ValueType)item.ValueType)
                        {
                            case ValueType.Byte:
                                queryResult.Add(new Tuple<string, string, string, string, MappingState, DateTime>
                                    (
                                        currentValue.Item1,
                                        item.PluginName,
                                        item.Name,
                                        item.ByteValue.ToString(),
                                        (MappingState)item.Mapping,
                                        new DateTime((long)item.Timestamp)
                                    )
                                );
                                break;
                            case ValueType.Int:
                                queryResult.Add(new Tuple<string, string, string, string, MappingState, DateTime>
                                    (
                                        currentValue.Item1,
                                        item.PluginName,
                                        item.Name,
                                        item.IntValue.ToString(),
                                        (MappingState)item.Mapping,
                                        new DateTime((long)item.Timestamp)
                                    )
                                );
                                break;
                            case ValueType.Float:
                                queryResult.Add(new Tuple<string, string, string, string, MappingState, DateTime>
                                    (
                                        currentValue.Item1,
                                        item.PluginName,
                                        item.Name,
                                        item.FloatValue.ToString(),
                                        (MappingState)item.Mapping,
                                        new DateTime((long)item.Timestamp)
                                    )
                                );
                                break;
                            case ValueType.String:
                                queryResult.Add(new Tuple<string, string, string, string, MappingState, DateTime>
                                    (
                                        currentValue.Item1,
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
                }
            }

            #endregion

            Console.WriteLine("Cout: " + queryResult.Count());
            foreach (var item in queryResult)
            {
                //Console.WriteLine(item);
            }

            DateTime thenLevelL = DateTime.Now;
            Console.WriteLine("Finished Level L new");
            Console.WriteLine("Level L data needed: " + Math.Round(thenLevelL.Subtract(nowLevelL).TotalMilliseconds) + " milliseconds.");

            #endregion

            #region old

            Console.WriteLine("Starting Level L old");
            DateTime nowLevelLold = DateTime.Now;

            var queryResultOld = ValueManager.Instance.GetData(input);
            Console.WriteLine("Count: " + queryResultOld.Count());
            foreach (var item in queryResultOld)
            {
                //Console.WriteLine(item);
            }

            DateTime thenLevelLold = DateTime.Now;
            Console.WriteLine("Finished Level L old");
            Console.WriteLine("Level L data needed: " + Math.Round(thenLevelLold.Subtract(nowLevelLold).TotalMilliseconds) + " milliseconds.");

            #endregion

            Console.WriteLine("<-<-<-<-<PERFORMANCETEST>->->->->");
            Console.ReadLine();
        }
    }
}

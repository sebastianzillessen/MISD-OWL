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
using System.Data.Linq;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using MISD.Core;
using MISD.Server.Database;
using MISD.Server.Email;

namespace MISD.Server.Manager
{
    public class WorkstationManager
    {
        #region Singleton

        private static volatile WorkstationManager instance;
        private static object syncRoot = new Object();

        private static object intRoot = new Object();
        private static object byteRoot = new Object();
        private static object stringRoot = new Object();
        private static object floatRoot = new Object();

        /// <summary>
        /// Property to access the class.
        /// </summary>
        public static WorkstationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new WorkstationManager();
                    }
                }

                return instance;
            }
        }

        #endregion

        #region Caching

        // buffers to decrease interaction with database
        private CacheManager<byte, string> cacheManByte;
        private CacheManager<int, string> cacheManInt;
        private CacheManager<float, string> cacheManFloat;
        private CacheManager<string, string> cacheManString;

        #endregion

        #region Constructors

        private WorkstationManager()
        {
            cacheManByte = new CacheManager<byte, string>();
            cacheManInt = new CacheManager<int, string>();
            cacheManFloat = new CacheManager<float, string>();
            cacheManString = new CacheManager<string, string>();
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        public bool SignIn(string monitoredSystemFQDN, string monitoredSystemMAC, byte operatingSystem)
        {
            using (var dataContext = DataContextFactory.CreateDataContext())
            {
                #region check arguments

                try
                {
                    bool badArgument = true;
                    if (monitoredSystemMAC.Equals(String.Empty))
                    {
                        return false;
                    }
                    foreach (Platform p in Enum.GetValues(typeof(Platform)))
                    {
                        if ((byte)p == operatingSystem)
                        {
                            // The passed in OS param is valid
                            badArgument = false;
                            break;
                        }
                    }
                    if (badArgument)
                    {
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Logger.Instance.WriteEntry("WorkstationManager_SignIn: " + e.ToString(), LogType.Exception);
                }

                #endregion

                bool signedIn = false;
                Database.MonitoredSystem monitoredSystem = null;
                OrganizationalUnit ou;

                try
                {
                    var result = (from p in dataContext.MonitoredSystem
                                  where p.MacAddress == monitoredSystemMAC
                                  select p);

                    monitoredSystem = result.FirstOrDefault();

                    if (monitoredSystem == null)
                    {
                        //logging exception
                        var messageNew = new StringBuilder();
                        messageNew.Append("WorkstationManager_SignIn: ");
                        messageNew.Append("Workstation " + monitoredSystemMAC + " ");
                        messageNew.Append("(" + (Platform)operatingSystem + ") ");
                        messageNew.Append("is new. The workstation will be moved to OU Default!");
                        MISD.Core.Logger.Instance.WriteEntry(messageNew.ToString(), LogType.Info);

                        #region Create organisation units if necessary and get the organisation unit for the monitored system
                        //Create organisation units if necessary and get the organisation unit for the monitored system
                        var myOUs = ADManager.Instance.GetOU(monitoredSystemFQDN);

                        try
                        {
                            foreach (var unit in myOUs)
                            {
                                if (!OUManager.Instance.Exists(unit))
                                {
                                    if (unit == myOUs.First())
                                    {
                                        //create organisation unit with parent root
                                        OUManager.Instance.CreateOU(unit.Split('.').Last(), null, DateTime.Now);
                                    }
                                    else
                                    {
                                        //create organisation unit whit his parent
                                        var parent = (from p in dataContext.OrganizationalUnit
                                                      where p.FQDN == myOUs[myOUs.IndexOf(unit) - 1]
                                                      select p).FirstOrDefault();
                                        if (parent == null)
                                        {
                                            Logger.Instance.WriteEntry("WorkstationManager_SignIn: Could not create inner OU for FQDN " + myOUs.Last(), LogType.Warning);
                                        }
                                        else
                                        {
                                            OUManager.Instance.CreateOU(unit.Split('.').Last(), parent.FQDN, DateTime.Now);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            //logging exception
                            var messageEx2 = new StringBuilder();
                            messageEx2.Append("WorkstationWebService_SignIn: ");
                            messageEx2.Append("Workstation " + monitoredSystem.Name + " ");
                            messageEx2.Append("(" + monitoredSystem.OperatingSystem.ToString() + ") ");
                            messageEx2.Append("can´t sign in. Can't check for the OUs or create new OUs. " + e.ToString());
                            MISD.Core.Logger.Instance.WriteEntry(messageEx2.ToString(), LogType.Exception);
                        }

                        if (myOUs.Count != 0 && !(myOUs.Count == 1 && myOUs.First().Equals(Properties.Settings.Default.DefaultOU)))
                        {
                            //set last organisation unit in list as unit for the monitored system
                            ou = (from p in dataContext.OrganizationalUnit
                                  where p.FQDN == myOUs.Last()
                                  select p).FirstOrDefault();

                            if (ou == null)
                            {
                                Logger.Instance.WriteEntry("WorkstationManager_SignIn: Could not find inner OU for FQDN " + myOUs.Last(), LogType.Warning);
                                ou = (from p in dataContext.OrganizationalUnit
                                      where p.FQDN == Properties.Settings.Default.DefaultOU
                                      select p).FirstOrDefault();
                            }
                        }
                        else
                        {
                            //if the monitored system has no ou set to default organisation unit
                            //check default and create if necessary
                            if (!OUManager.Instance.Exists(Properties.Settings.Default.DefaultOU))
                            {
                                Logger.Instance.WriteEntry("WorkstationWebService_SignIn: Create default OU.", LogType.Info);
                                OUManager.Instance.CreateOU(Properties.Settings.Default.DefaultOU, null, DateTime.Now);
                            }

                            ou = (from p in dataContext.OrganizationalUnit
                                  where p.FQDN == Properties.Settings.Default.DefaultOU
                                  select p).FirstOrDefault();
                        }

                        #endregion

                        monitoredSystem = new Database.MonitoredSystem();
                        monitoredSystem.OrganizationalUnit = ou;
                        monitoredSystem.Name = monitoredSystemFQDN;
                        monitoredSystem.FQDN = monitoredSystemFQDN;
                        monitoredSystem.IsAvailable = true;
                        monitoredSystem.IsIgnored = false;
                        monitoredSystem.OperatingSystem = operatingSystem;
                        monitoredSystem.MacAddress = monitoredSystemMAC;

                        dataContext.MonitoredSystem.InsertOnSubmit(monitoredSystem);
                        dataContext.SubmitChanges();

                        //create indicator entries for the new workstations in the db.
                        PluginManager.Instance.UpdateDatabase();
                    }
                    else
                    {
                        if (monitoredSystem.IsIgnored)
                        {
                            MISD.Core.Logger.Instance.WriteEntry("WorkstationManager_SignIn: The monitored system " + monitoredSystem.Name + " is ignored and cannot sign in.", LogType.Debug);
                            return false;
                        }
                        if (monitoredSystem.IsAvailable)
                        {
                            //logging exception
                            var messageEx1 = new StringBuilder();
                            messageEx1.Append("WorkstationManager_SignIn: ");
                            messageEx1.Append("Workstation " + monitoredSystem.Name + " ");
                            messageEx1.Append("(" + monitoredSystem.OperatingSystem.ToString() + ") ");
                            messageEx1.Append("can´t sign in. The workstation was already signed in!");
                            MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Warning);
                        }

                        //set monitored system as online
                        monitoredSystem.IsAvailable = true;

                        bool platformChanged = false;

                        //set the current platform
                        #region logging plattfrom change
                        if (monitoredSystem.OperatingSystem != operatingSystem)
                        {
                            Logger.Instance.WriteEntry("WorkstationManager_SignIn: Operating system of workstation " + monitoredSystem.Name + " has changed to " + operatingSystem, LogType.Debug);
                            platformChanged = true;
                        }
                        #endregion
                        monitoredSystem.OperatingSystem = operatingSystem;

                        dataContext.SubmitChanges();

                        if (platformChanged)
                        {
                            //create indicator entries for the new workstations in the db.
                            PluginManager.Instance.UpdateDatabase();
                        }

                        //logging info
                        var messageOK = new StringBuilder();
                        messageOK.Append("WorkstationManager_SignIn: ");
                        messageOK.Append("Workstation " + monitoredSystem.Name + " ");
                        messageOK.Append("(" + monitoredSystem.OperatingSystem.ToString() + ") ");
                        messageOK.Append("has signed in.");
                        MISD.Core.Logger.Instance.WriteEntry(messageOK.ToString(), LogType.Info);
                    }
                    signedIn = true;
                }
                catch (Exception e)
                {
                    signedIn = false;

                    //logging exception
                    string workstationLogName;
                    string osLogName;
                    if (monitoredSystem != null)
                    {
                        workstationLogName = monitoredSystem.Name;
                        osLogName = monitoredSystem.OperatingSystem.ToString();
                    }
                    else
                    {
                        workstationLogName = "[unkown]";
                        osLogName = "system unkown";
                    }

                    var messageEx2 = new StringBuilder();
                    messageEx2.Append("WorkstationManager_SignIn: ");
                    messageEx2.Append("Workstation " + workstationLogName + " ");
                    messageEx2.Append("(" + osLogName + ") ");
                    messageEx2.Append("sign in has failed. " + e.ToString());
                    MISD.Core.Logger.Instance.WriteEntry(messageEx2.ToString(), LogType.Exception);
                }

                return signedIn;
            }
        }

        public bool SignOut(string monitoredSystemMAC)
        {
            using (var dataContext = DataContextFactory.CreateDataContext())
            {
                #region check arguments

                if (monitoredSystemMAC == "")
                {
                    return false;
                }

                #endregion

                bool signedOut = false;
                MonitoredSystem monitoredSystem = null;

                try
                {
                    monitoredSystem = (from ms in dataContext.MonitoredSystem
                                       where ms.MacAddress == monitoredSystemMAC
                                       select ms).FirstOrDefault();

                    if (monitoredSystem != null)
                    {
                        if (monitoredSystem.IsIgnored)
                        {
                            MISD.Core.Logger.Instance.WriteEntry("WorkstationManager_SignOut: The monitored system " + monitoredSystem.Name + " is ignored and cannot sign out.", LogType.Debug);
                            return false;
                        }
                        if (monitoredSystem.IsAvailable == false)
                        {
                            #region logging exception
                            //logging exception
                            var messageEx1 = new StringBuilder();
                            messageEx1.Append("WorkstationWebService_SignOut: ");
                            messageEx1.Append("Workstation" + monitoredSystem.Name + " ");
                            messageEx1.Append("(" + monitoredSystem.OperatingSystem.ToString() + ") ");
                            messageEx1.Append("can´t sign out. The workstation is already signed out!");
                            MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Warning);
                            #endregion
                        }
                        else
                        {
                            monitoredSystem.IsAvailable = false;
                            dataContext.SubmitChanges();
                            #region logging info
                            //logging info
                            var messageOK = new StringBuilder();
                            messageOK.Append("WorkstationManager_SignOut: ");
                            messageOK.Append("Workstation" + monitoredSystem.Name + " ");
                            messageOK.Append("(" + monitoredSystem.OperatingSystem.ToString() + ") ");
                            messageOK.Append("has signed out.");
                            MISD.Core.Logger.Instance.WriteEntry(messageOK.ToString(), LogType.Info);
                            #endregion
                        }
                    }
                    signedOut = true;
                }
                catch (Exception e)
                {
                    signedOut = false;

                    #region logging exception
                    //logging exception
                    string workstationLogName;
                    string osLogName;
                    if (monitoredSystem != null)
                    {
                        workstationLogName = monitoredSystem.Name;
                        osLogName = monitoredSystem.OperatingSystem.ToString();
                    }
                    else
                    {
                        workstationLogName = "[unkown]";
                        osLogName = "system unkown";
                    }

                    var messageEx2 = new StringBuilder();
                    messageEx2.Append("WorkstationManager_SignOut: ");
                    messageEx2.Append("Workstation" + workstationLogName + " ");
                    messageEx2.Append("(" + osLogName + ") ");
                    messageEx2.Append("sign out has failed. " + e.ToString());
                    MISD.Core.Logger.Instance.WriteEntry(messageEx2.ToString(), LogType.Exception);

                    #endregion
                }

                return signedOut;
            }
        }

        public Func<MISDDataContext, int, Tuple<bool, bool>> GetMonitoredSystemState = CompiledQuery.Compile<MISDDataContext, int, Tuple<bool, bool>>((dataContext, monitoredSystemID) => dataContext.MonitoredSystem.Where(p => p.ID == monitoredSystemID).Select(p => new Tuple<bool, bool>(p.IsInMaintenanceMode, p.IsIgnored)).SingleOrDefault());
        public Func<MISDDataContext, int, string, string, Indicator> GetIndicatorByMonitoredSystemIDPluginNameAndIndicatorName = CompiledQuery.Compile<MISDDataContext, int, string, string, Indicator>((dataContext, monitoredSystemID, pluginName, indicatorName) => dataContext.Indicator.Where(p => p.Name == indicatorName && p.MonitoredSystemID == monitoredSystemID && p.PluginMetadata.Name == pluginName && (p.PluginMetadata.Platform == p.MonitoredSystem.OperatingSystem || p.PluginMetadata.Platform == (byte)Platform.Server)).FirstOrDefault());

        public Func<MISDDataContext, double, ValueFloat> GetFloatByValue = CompiledQuery.Compile<MISDDataContext, double, ValueFloat>((dataContext, value) => dataContext.ValueFloat.Where(p => p.Value == value).SingleOrDefault());
        public Func<MISDDataContext, string, ValueString> GetStringByValue = CompiledQuery.Compile<MISDDataContext, string, ValueString>((dataContext, value) => dataContext.ValueString.Where(p => p.Value == value).SingleOrDefault());
        public Func<MISDDataContext, byte, ValueByte> GetByteByValue = CompiledQuery.Compile<MISDDataContext, byte, ValueByte>((dataContext, value) => dataContext.ValueByte.Where(p => p.Value == value).SingleOrDefault());
        public Func<MISDDataContext, int, ValueInt> GetIntByValue = CompiledQuery.Compile<MISDDataContext, int, ValueInt>((dataContext, value) => dataContext.ValueInt.Where(p => p.Value == value).SingleOrDefault());
        public Func<MISDDataContext, int, MISD.Server.Database.PluginMetadata> GetPluginByIndicatorID = CompiledQuery.Compile<MISDDataContext, int, MISD.Server.Database.PluginMetadata>((dataContext, indicatorID) => dataContext.Indicator.Where(p => p.ID == indicatorID).Select(p => p.PluginMetadata).FirstOrDefault());

        public bool UploadIndicatorValues(int monitoredSystemID, string pluginName, List<Tuple<string, Object, MISD.Core.DataType, DateTime>> indicatorValues)
        {

            bool success = false;
            try
            {
                using (var dataContext = DataContextFactory.CreateDataContext())
                {
                    // Item 1: MaintenanceMode, Item2: IsIgnored
                    var monitoredSystemState = GetMonitoredSystemState(dataContext, monitoredSystemID);
                    if (monitoredSystemState.Item1 == true || monitoredSystemState.Item2 == true)
                    {
                        //logging exception
                        var messageEx1 = new StringBuilder();
                        messageEx1.Append("WorkstationManager_UploadIndicatorValues: ");
                        messageEx1.Append("Transfer from " + monitoredSystemID + " and " + pluginName + " ignored. ");
                        messageEx1.Append("The system is ignored or in maintance status. ");
                        MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Debug);
                    }
                    else
                    {
                        foreach (var listTuple in indicatorValues)
                        {
                            if (listTuple.Item2 != null)
                            {
                                var indicator = GetIndicatorByMonitoredSystemIDPluginNameAndIndicatorName(dataContext, monitoredSystemID, pluginName, listTuple.Item1);
                                if (indicator == null)
                                {
                                    //logging exception
                                    var message = new StringBuilder();
                                    message.Append("WorkstationManager_UploadIndicatorValues: ");
                                    message.Append("Transfer from " + monitoredSystemID + " and " + pluginName + " failed. Could not find the indicator.");
                                    MISD.Core.Logger.Instance.WriteEntry(message.ToString(), LogType.Debug);
                                    continue;
                                }

                                int valueID = 0;

                                if (listTuple.Item3 == Core.DataType.Byte)
                                {
                                    #region create byte

                                    lock (byteRoot)
                                    {
                                        string index = cacheManByte.Get(Convert.ToByte(listTuple.Item2));
                                        if (index == null)
                                        {
                                            using (var byteDataContext = DataContextFactory.CreateDataContext())
                                            {
                                                var element = GetByteByValue(byteDataContext, Convert.ToByte(listTuple.Item2));
                                                if (element == null)
                                                {
                                                    ValueByte byteValue = new ValueByte();
                                                    byteValue.Value = Convert.ToByte(listTuple.Item2);
                                                    byteDataContext.ValueByte.InsertOnSubmit(byteValue);
                                                    byteDataContext.SubmitChanges();
                                                    valueID = byteValue.ID;
                                                    cacheManByte.Add(Convert.ToByte(listTuple.Item2), valueID.ToString());
                                                }
                                                else
                                                {
                                                    valueID = element.ID;
                                                    cacheManByte.Add(Convert.ToByte(listTuple.Item2), valueID.ToString());
                                                }
                                            }
                                        }
                                        else
                                        {
                                            valueID = Convert.ToInt32(index);
                                        }
                                    }
                                    #endregion
                                }
                                else if (listTuple.Item3 == Core.DataType.Float)
                                {
                                    #region create float

                                    lock (floatRoot)
                                    {
                                        string index = cacheManFloat.Get((float) Convert.ToDouble(listTuple.Item2));
                                        if (index == null)
                                        {
                                            using (var floatDataContext = DataContextFactory.CreateDataContext())
                                            {
                                                var element = GetFloatByValue(floatDataContext, Convert.ToDouble(listTuple.Item2));

                                                if (element == null)
                                                {
                                                    ValueFloat floatValue = new ValueFloat();
                                                    floatValue.Value = Convert.ToDouble(listTuple.Item2);
                                                    floatDataContext.ValueFloat.InsertOnSubmit(floatValue);
                                                    floatDataContext.SubmitChanges();
                                                    valueID = floatValue.ID;
                                                    cacheManFloat.Add((float)Convert.ToDouble(listTuple.Item2), valueID.ToString());
                                                }
                                                else
                                                {
                                                    valueID = element.ID;
                                                    cacheManFloat.Add((float)Convert.ToDouble(listTuple.Item2), valueID.ToString());
                                                }
                                            }
                                        }
                                        else
                                        {
                                            valueID = Convert.ToInt32(index);
                                        }
                                    }
                                    #endregion
                                }
                                else if (listTuple.Item3 == Core.DataType.Int)
                                {
                                    #region create int

                                    lock (intRoot)
                                    {
                                        string index = cacheManInt.Get(Convert.ToInt32(listTuple.Item2));
                                        if (index == null)
                                        {
                                            using (var intDataContext = DataContextFactory.CreateDataContext())
                                            {

                                                var element = GetIntByValue(intDataContext, Convert.ToInt32(listTuple.Item2));

                                                if (element == null)
                                                {

                                                    ValueInt intValue = new ValueInt();
                                                    intValue.Value = Convert.ToInt32(listTuple.Item2);
                                                    intDataContext.ValueInt.InsertOnSubmit(intValue);
                                                    intDataContext.SubmitChanges();
                                                    valueID = intValue.ID;
                                                    cacheManInt.Add(Convert.ToInt32(listTuple.Item2), valueID.ToString());
                                                }
                                                else
                                                {
                                                    valueID = element.ID;
                                                    cacheManInt.Add(Convert.ToInt32(listTuple.Item2), valueID.ToString());
                                                }
                                            }
                                        }
                                        else
                                        {
                                            valueID = Convert.ToInt32(index);
                                        }
                                    }
                                    #endregion
                                }
                                else if (listTuple.Item3 == Core.DataType.String)
                                {
                                    #region create string

                                    lock (stringRoot)
                                    {
                                        string index = cacheManString.Get(listTuple.Item2.ToString());
                                        if (index == null)
                                        {
                                            using (var stringDataContext = DataContextFactory.CreateDataContext())
                                            {
                                                var element = GetStringByValue(stringDataContext, listTuple.Item2 as string);

                                                if (element == null)
                                                {
                                                    ValueString stringValue = new ValueString();
                                                    stringValue.Value = listTuple.Item2.ToString();
                                                    stringDataContext.ValueString.InsertOnSubmit(stringValue);
                                                    stringDataContext.SubmitChanges();
                                                    valueID = stringValue.ID;
                                                    cacheManString.Add(listTuple.Item2.ToString(), valueID.ToString());
                                                }
                                                else
                                                {
                                                    valueID = element.ID;
                                                    cacheManString.Add(listTuple.Item2.ToString(), valueID.ToString());
                                                }
                                            }
                                        }
                                        else
                                        {
                                            valueID = Convert.ToInt32(index);
                                        }
                                    }
                                    #endregion
                                }

                                IndicatorValue value = new IndicatorValue();
                                value.IndicatorID = indicator.ID;
                                value.Timestamp = listTuple.Item4.Ticks;
                                value.Mapping = (byte)MetricManager.Instance.GetMetricValue(indicator.MonitoredSystemID, pluginName, listTuple.Item1, listTuple.Item2.ToString());
                                value.ValueID = valueID;

                                dataContext.IndicatorValue.InsertOnSubmit(value);

                                dataContext.SubmitChanges();

                                // Caching für GetLatestData
                                //var ms = Database.PrecompiledQueries.GetMonitoredSystemByID(dataContext, monitoredSystemID);
                                //ValueManager.Instance.CacheIndicator(
                                //    new Tuple<string, string, string, string, MappingState, DateTime>
                                //        (
                                //        ms.MacAddress, pluginName, indicator.Name, listTuple.Item2.ToString(),(MappingState) value.Mapping, new DateTime( value.Timestamp)
                                //        )
                                //);


                                //set monitored system mapping
                                #region monitored system mapping
                                var monitoredSystem = PrecompiledQueries.GetMonitoredSystemByID(dataContext, monitoredSystemID);
                                if (monitoredSystem.Status != (byte)MappingState.Maintenance)
                                {
                                    if (value.Mapping == (byte)MappingState.Critical)
                                    {
                                        //set mapping state
                                        monitoredSystem.Status = (byte)MappingState.Critical;

                                        //set end time
                                        monitoredSystem.CriticalEnd = indicator.MappingDuration + DateTime.Now.Ticks;

                                        dataContext.SubmitChanges();
                                    }
                                    else if (value.Mapping == (byte)MappingState.Warning && monitoredSystem.Status != (byte)MappingState.Critical)
                                    {
                                        //set mapping state
                                        monitoredSystem.Status = (byte)MappingState.Warning;

                                        //set end time
                                        monitoredSystem.WarningEnd = indicator.MappingDuration + DateTime.Now.Ticks;

                                        dataContext.SubmitChanges();
                                    } 
                                }
                                #endregion
                                #region indicator mapping
                                if (value.Mapping == (byte)MappingState.Critical)
                                {
                                    //set mapping state
                                    indicator.Status = (byte)MappingState.Critical;

                                    //set end time
                                    indicator.CriticalEnd = indicator.MappingDuration + DateTime.Now.Ticks;

                                    dataContext.SubmitChanges();
                                }
                                else if (value.Mapping == (byte)MappingState.Warning && indicator.Status != (byte)MappingState.Critical)
                                {
                                    //set mapping state
                                    indicator.Status = (byte)MappingState.Warning;

                                    //set end time
                                    indicator.WarningEnd = indicator.MappingDuration + DateTime.Now.Ticks;

                                    dataContext.SubmitChanges();
                                }
                                #endregion

                                //send warning mails to the observing users
                                #region waring mails
                                if (value.Mapping == (byte)MappingState.Critical)
                                {
                                    var mailThread = new Thread(new ThreadStart(() =>
                                    {
                                        Mailer.Instance.SendAllWarnings(
                                            monitoredSystemID,
                                            new DateTime(value.Timestamp),
                                            pluginName,
                                            listTuple.Item1,
                                            (listTuple.Item2 == null) ? "" : listTuple.Item2.ToString());
                                    }));
                                    mailThread.Start();
                                }
                                #endregion

                                success = true;
                            }
                            else
                            {
                                success = false;
                                #region logging
                                //logging exception
                                var messageEx3 = new StringBuilder();
                                messageEx3.Append("WorkstationManager_UploadIndicatorValues: ");
                                messageEx3.Append("Transfer from " + monitoredSystemID + " and " + pluginName + " failed. Value (Object) was null.");
                                MISD.Core.Logger.Instance.WriteEntry(messageEx3.ToString(), LogType.Debug);
                                #endregion
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //logging exception
                var messageEx2 = new StringBuilder();
                messageEx2.Append("WorkstationManager_UploadIndicatorValues: ");
                messageEx2.Append("Transfer from " + monitoredSystemID + " and " + pluginName + " failed. " + e.ToString());
                MISD.Core.Logger.Instance.WriteEntry(messageEx2.ToString(), LogType.Exception);
            }
            return success;
        }

        public List<IndicatorSettings> GetIndicatorSetting(string monitoredSystemMAC)
        {
            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {
                try
                {
                    var os = (from q in dataContext.MonitoredSystem
                              where q.MacAddress == monitoredSystemMAC
                              select q.OperatingSystem).FirstOrDefault();

                    //gets PluginMetadata and converts into DataHolder.PluginMetadata
                    var settings = (from q in dataContext.Indicator
                                    where q.MonitoredSystem.MacAddress == monitoredSystemMAC
                                    where (q.PluginMetadata.Platform == os || q.PluginMetadata.Platform == 4)
                                    select new Core.IndicatorSettings()
                                    {
                                        PluginName = q.PluginMetadata.Name,
                                        IndicatorName = q.Name,
                                        MonitoredSystemMAC = monitoredSystemMAC,
                                        FilterStatement = q.FilterStatement,
                                        UpdateInterval = (q.UpdateInterval == null) ? new TimeSpan(1, 0, 0) : new TimeSpan((long)q.UpdateInterval),
                                        StorageDuration = (q.StorageDuration == null) ? new TimeSpan(31, 0, 0, 0) : new TimeSpan((long)q.StorageDuration),
                                        MappingDuration = (q.MappingDuration == null) ? new TimeSpan(24, 0, 0) : new TimeSpan((long)q.MappingDuration),
                                        DataType = (DataType)q.ValueType,
                                        MetricCritical = q.StatementCritical,
                                        MetricWarning = q.StatementWarning,
                                        IndicatorMapping = q.Status == null ? MappingState.OK : (MappingState)q.Status
                                    }).ToList();

                    return (List<Core.IndicatorSettings>)settings;
                }
                catch (Exception e)
                {
                    //logging exception
                    var messageEx = new StringBuilder();
                    messageEx.Append("WorkstationManager_GetIndicatorSetting: ");
                    messageEx.Append("Can't create list of indicator settings for " + monitoredSystemMAC + ". " + e.ToString());
                    MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);

                    return null;
                }
            }
        }

        public bool SetIndicatorSetting(List<IndicatorSettings> settings)
        {
            using (var dataContext = DataContextFactory.CreateDataContext())
            {
                bool returns = true;

                foreach (IndicatorSettings s in settings)
                {
                    try
                    {
                        var setting = (from q in dataContext.Indicator
                                       where q.MonitoredSystem.MacAddress == s.MonitoredSystemMAC
                                       where q.PluginMetadata.Name == s.PluginName
                                       where q.Name == s.IndicatorName
                                       select q).FirstOrDefault();

                        if (setting != null)
                        {
                            setting.PluginMetadata.Name = s.PluginName;
                            setting.Name = s.IndicatorName;
                            setting.MonitoredSystem.MacAddress = s.MonitoredSystemMAC;
                            setting.FilterStatement = s.FilterStatement;
                            setting.UpdateInterval = s.UpdateInterval.Ticks;
                            setting.StorageDuration = s.StorageDuration.Ticks;
                            setting.MappingDuration = s.MappingDuration.Ticks;
                            setting.ValueType = (byte)s.DataType;
                            setting.StatementCritical = s.MetricCritical;
                            setting.StatementWarning = s.MetricWarning;
                            dataContext.SubmitChanges();
                        }
                        else
                        {
                            Logger.Instance.WriteEntry("WorkstationManager_SetIndicatorSetting: Could not find the indicator to be updated (name: " + s.IndicatorName + ")", LogType.Warning);
                        }
                    }
                    catch (Exception e)
                    {
                        //logging exception
                        var messageEx1 = new StringBuilder();
                        messageEx1.Append("WorkstationManager_SetIndicatorSetting: ");
                        messageEx1.Append("Setting of" + s + " failed. " + e.ToString());
                        MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Exception);
                        returns = false;
                    }
                }
                return returns;
            }
        }

        public List<string> AddWorkstationsToIgnoreList(List<Tuple<string, DateTime>> monitoredSystemMACAddresses)
        {
            using (var dataContext = DataContextFactory.CreateDataContext())
            {
                List<string> returns = new List<string>();

                foreach (Tuple<string, DateTime> tuple in monitoredSystemMACAddresses)
                {
                    try
                    {
                        var workstation = (from p in dataContext.MonitoredSystem
                                           where p.MacAddress == tuple.Item1
                                           select p).FirstOrDefault();

                        if (workstation != null && (workstation.LastUpdate == null || workstation.LastUpdate <= tuple.Item2.Ticks))
                        {
                            workstation.LastUpdate = tuple.Item2.Ticks;

                            workstation.IsIgnored = true;

                           
                            dataContext.SubmitChanges();
                            returns.Add(tuple.Item1);

                            // 
                        }
                        else
                        {
                            var messageEx1 = new StringBuilder();
                            messageEx1.Append("WorkstationManager_AddWorkstationsToIgnoreList: ");
                            messageEx1.Append("Adding of" + tuple.Item1 + "to IgnoreList failed. MS not found or update time too late.");
                            MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Warning);
                        }


                    }
                    catch (Exception e)
                    {
                        //logging exception
                        var messageEx1 = new StringBuilder();
                        messageEx1.Append("WorkstationManager_AddWorkstationsToIgnoreList: ");
                        messageEx1.Append("Adding of" + tuple.Item1 + "to IgnoreList failed. " + e.ToString());
                        MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Exception);

                    }
                }
                return returns;
            }
        }

        public List<string> RemoveWorkstationsFromIgnoreList(List<Tuple<string, DateTime>> monitoredSystemMACAddresses)
        {
            using (var dataContext = DataContextFactory.CreateDataContext())
            {
                List<string> returns = new List<string>();

                foreach (Tuple<string, DateTime> tuple in monitoredSystemMACAddresses)
                {
                    try
                    {
                        var workstation = (from p in dataContext.MonitoredSystem
                                           where p.MacAddress == tuple.Item1
                                           select p).FirstOrDefault();

                        if (workstation != null && (workstation.LastUpdate == null || workstation.LastUpdate <= tuple.Item2.Ticks))
                        {
                            workstation.LastUpdate = tuple.Item2.Ticks;

                            workstation.IsIgnored = false;

                            dataContext.SubmitChanges();
                            returns.Add(tuple.Item1);

                            //set new parent ou
                            if (workstation.OrganizationalUnit.Name == "IgnoredOuForInternalUsageOfIgnoredSystems")
                            {
                                var ignoredou = (from p in dataContext.OrganizationalUnit
                                                 where p.Name == "FromIgnored"
                                                 select p).ToList();
                                int id;
                                if (ignoredou.Count == 0)
                                {
                                    id = OUManager.Instance.AddOU("FromIgnored", null, System.DateTime.Now);
                                }
                                else 
                                { 
                                    id = ignoredou.FirstOrDefault().ID; 
                                }

                                //MoveMonitoredSystem to FromIgnored
                                var msList = new List<Tuple<string, int, DateTime>>();
                                msList.Add(new Tuple<string, int, DateTime>(workstation.MacAddress, id, DateTime.Now));
                                WorkstationManager.Instance.MoveMonitoredSystem(msList);
                            }
                        }
                        else
                        {
                            //logging exception
                            var messageEx1 = new StringBuilder();
                            messageEx1.Append("WorkstationManager_AddWorkstationsToIgnoreList: ");
                            messageEx1.Append("Removing of" + tuple.Item1 + "from IgnoreList failed. MS not found or update time too late.");
                            MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Warning);
                        }

                    }
                    catch (Exception e)
                    {
                        //logging exception
                        var messageEx1 = new StringBuilder();
                        messageEx1.Append("WorkstationManager_AddWorkstationsToIgnoreList: ");
                        messageEx1.Append("Removing of" + tuple.Item1 + "from IgnoreList failed. " + e.ToString());
                        MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Exception);
                    }
                }
                return returns;
            }
        }

        public List<Tuple<string, string>> GetIgnoredMonitoredSystems()
        {
            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {
                try
                {
                    List<Tuple<string, string>> returnList = new List<Tuple<string, string>>();

                    // TODO Jonas bitte drüber schauen :-)
                    var ignoredMonitoredSystems = (from p in dataContext.MonitoredSystem
                                where p.IsIgnored == true
                                select p).ToList();

                    foreach (MonitoredSystem monitoredSystem in ignoredMonitoredSystems)
                    {
                        returnList.Add(new Tuple<string, string>(monitoredSystem.MacAddress, monitoredSystem.Name));
                    }

                    return returnList;
                }
                catch (Exception e)
                {
                    //logging exception
                    var messageEx = new StringBuilder();
                    messageEx.Append("WorkstationManager_GetIgnoredWorkstationNames: ");
                    messageEx.Append("Can't create list of ignored workstations. " + e.ToString());
                    MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);

                    return null;
                }
            }
        }

        public List<string> ActivateMaintenanceMode(List<Tuple<string, DateTime>> monitoredSystemMACAddresses)
        {
            using (var dataContext = DataContextFactory.CreateDataContext())
            {
                List<string> returns = new List<string>();

                foreach (Tuple<string, DateTime> tuple in monitoredSystemMACAddresses)
                {
                    try
                    {
                        /* COMMENT by Yannic: Now I need the whole monitored system. 
                        var id = PrecompiledQueries.GetMonitoredSystemIDByMAC(dataContext, tuple.Item1);*/

                        var monitoredSystem = (from p in dataContext.MonitoredSystem
                                               where p.MacAddress == tuple.Item1
                                               select p).FirstOrDefault();

                        if (monitoredSystem != null && (monitoredSystem.LastUpdate == null || monitoredSystem.LastUpdate < tuple.Item2.Ticks))
                        {
                            monitoredSystem.LastUpdate = tuple.Item2.Ticks;

                            var maintenance = new Database.Maintenance()
                            {
                                MonitoredSystemID = monitoredSystem.ID,
                                Beginning = System.DateTime.Now,
                                End = null
                            };
                            
                            dataContext.Maintenance.InsertOnSubmit(maintenance);
                            dataContext.SubmitChanges();
                            
                            returns.Add(tuple.Item1);
                        }
                        else
                        {
                            //logging
                            var messageEx1 = new StringBuilder();
                            messageEx1.Append("WorkstationManager_ActivateMaintenanceMode: ");
                            messageEx1.Append("Adding of" + tuple.Item1 + "to Maintenance failed. MS not found or update time too late.");
                            MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Warning);
                        }
                    }
                    catch (Exception e)
                    {
                        //logging exception
                        var messageEx1 = new StringBuilder();
                        messageEx1.Append("WorkstationManager_ActivateMaintenanceMode: ");
                        messageEx1.Append("Adding of" + tuple.Item1 + "to Maintenance failed. " + e.ToString());
                        MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Exception);
                    }
                }
                return returns;
            }
        }

        public List<string> DeactivateMaintenanceMode(List<Tuple<string, DateTime>> monitoredSystemMACAddresses)
        {
            using (var dataContext = DataContextFactory.CreateDataContext())
            {
                var returns = new List<string>();
                foreach (Tuple<string, DateTime> tuple in monitoredSystemMACAddresses)
                {

                    var workstation = (from p in dataContext.Maintenance
                                       let id = p.MonitoredSystem.MacAddress == tuple.Item1
                                       let start = p.Beginning != null
                                       let stop = p.End == null
                                       where id & start & stop
                                       select p).FirstOrDefault();

                    var monitoredSystem = (from p in dataContext.MonitoredSystem
                                           where p.MacAddress == tuple.Item1
                                           select p).FirstOrDefault();

                    if (workstation != null && monitoredSystem != null && (monitoredSystem.LastUpdate == null || monitoredSystem.LastUpdate < tuple.Item2.Ticks))
                    {
                        monitoredSystem.LastUpdate = tuple.Item2.Ticks;
                        workstation.End = System.DateTime.Now;
                        try
                        {
                            dataContext.SubmitChanges();
                            returns.Add(tuple.Item1);
                        }
                        catch (Exception e)
                        {
                            //logging exception
                            var messageEx1 = new StringBuilder();
                            messageEx1.Append("WorkstationManager_DeactivateMaintenanceMode: ");
                            messageEx1.Append("Removing of" + tuple.Item1 + "from Maintenance failed. " + e.ToString());
                            MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Exception);
                        }
                    }
                    else
                    {
                        //logging exception
                        var messageEx1 = new StringBuilder();
                        messageEx1.Append("WorkstationManager_DeactivateMaintenanceMode: ");
                        messageEx1.Append("Removing of" + tuple.Item1 + "from Maintenance failed. MS not found or update time too late.");
                        MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Warning);
                    }
                    
                }
                return returns;
            }
        }

        public bool MoveMonitoredSystem(List<Tuple<string, int, DateTime>> monitoredSystems)
        {
            using (var dataContext = DataContextFactory.CreateDataContext())
            {
                bool returnValue = true;
                foreach (var tuple in monitoredSystems)
                {
                    var workstation = (from p in dataContext.MonitoredSystem
                                       where p.MacAddress == tuple.Item1
                                       select p).FirstOrDefault();

                    if (workstation != null && (workstation.LastUpdate == null || workstation.LastUpdate < tuple.Item3.Ticks))
                    {
                        try
                        {
                            workstation.LastUpdate = tuple.Item3.Ticks;
                            workstation.OrganizationalUnitID = tuple.Item2;
                            dataContext.SubmitChanges();
                        }
                        catch (Exception e)
                        {
                            returnValue = false;

                            //logging exception
                            var messageEx1 = new StringBuilder();
                            messageEx1.Append("WorkstationManager_MoveMonitoredSystem: ");
                            messageEx1.Append("Moving of monitored System with MAC=" + tuple.Item1 + " failed. " + e.ToString());
                            MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Exception);
                        }
                    }
                    else
                    {
                        //logging exception
                        var messageEx1 = new StringBuilder();
                        messageEx1.Append("WorkstationManager_MoveMonitoredSystem: ");
                        messageEx1.Append("Moving of monitored System with MAC=" + tuple.Item1 + " failed. MS not found or update time too late.");
                        MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Warning);
                    }
                }
                return returnValue;
            }
        }

        public bool ChangeWorkstationName(string mac, string newName, DateTime updateTime)
        {
            if (newName == String.Empty)
            {
                return false;
            }
            try
            {
                using (var dataContext = DataContextFactory.CreateDataContext())
                {
                    var ms = PrecompiledQueries.GetMonitoredSystemByMAC(dataContext, mac);

                    if (ms != null && (ms.LastUpdate == null || ms.LastUpdate < updateTime.Ticks))
                    {
                        ms.Name = newName;
                        ms.LastUpdate = updateTime.Ticks;
                        dataContext.SubmitChanges();
                        return true; 
                    }
                    else
                    {
                        //logging exception
                        var messageEx1 = new StringBuilder();
                        messageEx1.Append("WorkstationManager_ChangeWorkstationName: ");
                        messageEx1.Append("Name of ms" + mac + "to" + newName + " failed. " + "UpdateTime to late or MS not found.");
                        MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Warning);
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("WorkstationManager_ChangeWorkstationName: Can't change name" + e.StackTrace, LogType.Exception);
                return false;
            }
        }

        #endregion
    }
}

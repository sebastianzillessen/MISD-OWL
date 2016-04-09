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
using MISD.Server.Database;
using MISD.Server.Manager;
using MISD.Core;

namespace MISD.Server.Manager
{
    public class OUManager
    {
        #region Singleton

        private static volatile OUManager instance;
        private static object syncRoot = new Object();

        /// <summary>
        /// Property to access the class.
        /// </summary>
        public static OUManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new OUManager();
                    }
                }

                return instance;
            }
        }

        #endregion

        #region Properties

        private string rootName;
        public string RootName
        {
            get
            {
                return rootName;
            }
        }

        #endregion

        #region Constructors

        private OUManager()
        {
            //set root name
            rootName = "System";

            #region check root organisation unit
            using (var dataContext = DataContextFactory.CreateDataContext())
            {
                //create organisation unit root if necessary
                var root = (from ou in dataContext.OrganizationalUnit
                            let name = ou.Name == "System"
                            let parent = ou.Parent == null
                            where name & parent
                            select ou).ToList();
                if (root.Count == 0)
                {
                    //create root
                    Database.OrganizationalUnit ouRoot = new OrganizationalUnit()
                    {
                        Name = "System",
                        FQDN = "System",
                        Parent = null
                    };

                    dataContext.OrganizationalUnit.InsertOnSubmit(ouRoot);
                    dataContext.SubmitChanges();

                    var messageInfo1 = new StringBuilder();
                    messageInfo1.Append("WorkstationWebservice_WorkstationWebservice: ");
                    messageInfo1.Append("Organisation unit root created.");
                    MISD.Core.Logger.Instance.WriteEntry(messageInfo1.ToString(), LogType.Info);
                }
                else if (root.Count > 1)
                {
                    //logging warning. There are two roots
                    var messageW1 = new StringBuilder();
                    messageW1.Append("WorkstationWebservice_WorkstationWebservice: ");
                    messageW1.Append("Two organisation unit root in the database.");
                    MISD.Core.Logger.Instance.WriteEntry(messageW1.ToString(), LogType.Warning);
                }

                //logging. Webservice start.
                var messageInfo2 = new StringBuilder();
                messageInfo2.Append("Workstation webservice started.");
                MISD.Core.Logger.Instance.WriteEntry(messageInfo2.ToString(), LogType.Info);

            }
            #endregion
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create a new organisation unit, if the given parent exist.
        /// </summary>
        /// <param name="name">Name of the new organisation unit</param>
        /// <param name="parentFQDN">FQDN of the parent of this organisation unit</param>
        public int CreateOU(string name, string parentFQDN, DateTime updateTime)
        {
            using (var dataContext = DataContextFactory.CreateDataContext())
            {
                try
                {
                    var newOU = new OrganizationalUnit();

                    newOU.Name = name;
                    newOU.LastUpdate = updateTime.Ticks;

                    if (parentFQDN == null || parentFQDN.Length < 1)
                    {
                        newOU.Parent = null;
                        newOU.LastUpdate = null;
                        newOU.FQDN = newOU.Name;
                    }
                    else
                    {
                        var existParent = (from p in dataContext.OrganizationalUnit
                                           let parent = p.FQDN == parentFQDN
                                           where parent
                                           orderby p.Parent ascending
                                           select p).FirstOrDefault();

                        if (existParent == null)
                        {
                            newOU.Parent = null;
                            newOU.LastUpdate = null;
                            newOU.FQDN = newOU.Name;
                        }
                        else
                        {
                            newOU.FQDN = existParent.FQDN + "." + name;
                            newOU.Parent = existParent.ID;
                        }
                    }

                    dataContext.OrganizationalUnit.InsertOnSubmit(newOU);
                    dataContext.SubmitChanges();

                    return newOU.ID;
                }
                catch (Exception e)
                {
                    //parentFQDN isn't in the db
                    var messageEx1 = new StringBuilder();
                    messageEx1.Append("OUManager_CreateOU: ");
                    messageEx1.Append("creating OU" + name + " with parent " + parentFQDN + " failed. " + e.ToString());
                    MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Exception);

                    // exception is used in the Client-Webservice to return that this OU isn't available
                    throw new InvalidOperationException("Create OU. The parent FQDN dosn't exist.");
                }
            }
        }

        public int AddOU(string name, int? fatherOU, DateTime updateTime)
        {
            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {
                try
                {
                    if (fatherOU != null)
                    {
                        var existParent = (from p in dataContext.OrganizationalUnit
                                           let parent = p.ID == fatherOU
                                           where parent
                                           select p).First();

                        //create OU
                        return this.CreateOU(name, existParent.FQDN, updateTime);
                    }
                    else
                    {
                        //create OU
                        return this.CreateOU(name, null, updateTime);
                    }
                }
                catch (Exception e)
                {
                    //logging exception
                    var messageEx1 = new StringBuilder();
                    messageEx1.Append("ClientWebService_AddOU: ");
                    messageEx1.Append("Adding of OU" + name + " failed. " + e.ToString());
                    MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Exception);
                    return -1;
                }
            }
        }

        /// <summary>
        /// Check if a given organisation unit exist.
        /// </summary>
        /// <param name="fqdn">FQDN of the organisation unit</param>
        /// <returns></returns>
        public bool Exists(string fqdn)
        {
            try
            {
                using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
                {
                    return ((from p in dataContext.OrganizationalUnit
                             where p.FQDN == fqdn
                             select p).Count() >= 1);
                }
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("OUManager_Exists: FQDN: " + fqdn + " couldnt be aquired! " + e.ToString(), LogType.Warning);
                return false;
            }
        }

        public bool ChangeOUName(int ouID, string newName, DateTime updateTime)
        {
            using (var dataContext = DataContextFactory.CreateDataContext())
            {
                var ou = (from p in dataContext.OrganizationalUnit
                          where p.ID == ouID
                          select p).FirstOrDefault();


                if (ou != null && (ou.LastUpdate == null || ou.LastUpdate < updateTime.Ticks))
                {
                    ou.Name = newName;
                    ou.LastUpdate = updateTime.Ticks;
                    try
                    {
                        dataContext.SubmitChanges();
                        return true;
                    }
                    catch (Exception e)
                    {
                        //logging exception
                        var messageEx1 = new StringBuilder();
                        messageEx1.Append("ClientWebService_ChangeOUName: ");
                        messageEx1.Append("Name of OU" + ouID + "to" + newName + " failed. " + e.ToString());
                        MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Exception);
                        return false;
                    }
                }
                else
                {
                    //logging exception
                    var messageEx1 = new StringBuilder();
                    messageEx1.Append("ClientWebService_ChangeOUName: ");
                    messageEx1.Append("Name of OU" + ouID + "to" + newName + " failed. " + "UpdateTime to late or OU not found.");
                    MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Warning);
                    return false;
                }
            }
        }

        public bool DeleteOU(int ouID)
        {
            using (var dataContext = DataContextFactory.CreateDataContext())
            {

                var ou = (from p in dataContext.OrganizationalUnit
                          where p.ID == ouID
                          select p).FirstOrDefault();

                if (ou == null)
                {
                    Logger.Instance.WriteEntry("OUManager_DeleteOU: Cannot find OU to be deleted.", LogType.Warning);
                    return false;
                }
                var children = (from q in dataContext.OrganizationalUnit
                                where q.Parent == ouID
                                select q.ID).ToList();
                foreach (int c in children)
                {
                    this.DeleteOU(c);
                }


                List<Tuple<string, DateTime>> monitoredSystemMACAddresses = new List<Tuple<string, DateTime>>();
                var ws = (from p in dataContext.MonitoredSystem
                          where p.OrganizationalUnit.ID == ouID
                          select p).ToList(); //new Tuple<string, long?> (p.MacAddress, p.LastUpdate)).ToList();

                foreach (var ms in ws)
                {

                    monitoredSystemMACAddresses.Add(new Tuple<string, DateTime>(ms.MacAddress, new DateTime((long)ms.LastUpdate)));

                }

                MISD.Server.Manager.WorkstationManager.Instance.AddWorkstationsToIgnoreList(monitoredSystemMACAddresses);
                using (var dataContext2 = DataContextFactory.CreateDataContext())
                {
                    int id;
                    ws = (from p in dataContext2.MonitoredSystem
                          where p.OrganizationalUnit.ID == ouID
                          select p).ToList();
                    foreach (var ms in ws)
                    {
                        var ignoredou = (from p in dataContext2.OrganizationalUnit
                                         where p.Name == "IgnoredOuForInternalUsageOfIgnoredSystems"
                                         select p).ToList();
                        if (ignoredou.Count == 0)
                        {
                            id = this.AddOU("IgnoredOuForInternalUsageOfIgnoredSystems", null, System.DateTime.Now);
                        }
                        else { id = ignoredou.FirstOrDefault().ID; }


                        ms.OrganizationalUnitID = id;
                        ms.OrganizationalUnit = (from p in dataContext2.OrganizationalUnit
                                                 where p.ID == id
                                                 select p).ToList().FirstOrDefault();
                        try
                        {
                            dataContext2.SubmitChanges();
                        }
                        catch (Exception e)
                        {
                            //logging exception
                            var messageEx1 = new StringBuilder();
                            messageEx1.Append("ClientWebService_DeleteOU: ");
                            messageEx1.Append("Deletion of  References from" + ouID + " failed. " + e.ToString());
                            MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Exception);
                        }


                    }
                }

            }
            using (var dataContext3 = DataContextFactory.CreateDataContext())
            {
                var ou = (from p in dataContext3.OrganizationalUnit
                          where p.ID == ouID
                          select p).FirstOrDefault();
                dataContext3.OrganizationalUnit.DeleteOnSubmit(ou);

                try
                {

                    dataContext3.SubmitChanges();
                    return true;
                }
                catch (Exception e)
                {
                    //logging exception
                    var messageEx1 = new StringBuilder();
                    messageEx1.Append("ClientWebService_DeleteOU: ");
                    messageEx1.Append("Deletion of OU" + ouID + " failed. " + e.ToString());
                    MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Exception);
                    return false;
                }
            }

        }

        /* COMMENT by Yannic: This method is never used because there is a method MoveMonitoredSystemo*/
        public bool AssignToOU(string monitoredSystemMAC, int newOUID)
        {
            using (var dataContext = DataContextFactory.CreateDataContext())
            {
                try
                {
                    var workstation = (from p in dataContext.MonitoredSystem
                                       where p.MacAddress == monitoredSystemMAC
                                       select p).FirstOrDefault();

                    if (workstation != null)
                    {
                        workstation.OrganizationalUnit.ID = newOUID;


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
                    messageEx.Append("ClientWebService_AssignToOU: ");
                    messageEx.Append("Can't assign" + monitoredSystemMAC + "to OU" + newOUID + ". " + e.ToString());
                    MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);
                    return false;
                }
            }
        }

        public List<Tuple<int, string, string, int?, DateTime?>> GetAllOUs()
        {
            using (var dataContext = DataContextFactory.CreateDataContext())
            {
                try
                {
                    List<Tuple<int, string, string, int?, DateTime?>> returnList = new List<Tuple<int, string, string, int?, DateTime?>>();

                    var organisationalUnits = (from p in dataContext.OrganizationalUnit
                                               where p.Name != "IgnoredOuForInternalUsageOfIgnoredSystems"
                                               select p).ToList();
                    foreach (var ou in organisationalUnits)
                    {
                        if (ou.LastUpdate != null)
                        {
                            returnList.Add(new Tuple<int, string, string, int?, DateTime?>(ou.ID, ou.Name, ou.FQDN, ou.Parent, new DateTime((long)ou.LastUpdate)));
                        }
                        else
                        {
                            returnList.Add(new Tuple<int, string, string, int?, DateTime?>(ou.ID, ou.Name, ou.FQDN, ou.Parent, null));
                        }
                    }

                    return returnList;
                }
                catch (Exception e)
                {
                    //logging exception
                    var messageEx = new StringBuilder();
                    messageEx.Append("ClientWebService_GetAllOUs: ");
                    messageEx.Append("Can't select all OUs. " + e.ToString());
                    MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);
                    return null;
                }
            }
        }

        /// <summary>
        /// Changes the parent ou of a given ou.
        /// </summary>
        /// <param name="ouID"></param>
        /// <param name="ouIDParent"></param>
        /// <returns></returns>
        public bool ChangeParent(int ouID, int? ouIDParent, DateTime updateTime)
        {
            try
            {
                using (var dataContext = DataContextFactory.CreateDataContext())
                {
                    var ou = (from q in dataContext.OrganizationalUnit
                              where q.ID == ouID
                              select q).FirstOrDefault();

                    if (ou != null && (ou.LastUpdate == null || ou.LastUpdate < updateTime.Ticks))
                    {
                        ou.LastUpdate = updateTime.Ticks;

                        if (ou != null && ouIDParent != null)
                        {
                            var parentOU = (from p in dataContext.OrganizationalUnit
                                            where p.ID == ouIDParent
                                            select p.ID).ToList();

                            if (parentOU.Count == 1)
                            {
                                ou.Parent = ouIDParent;
                                dataContext.SubmitChanges();
                            }
                            else
                            {
                                throw new ArgumentException("Parent OU deosn't exists");
                            }
                        }
                        else
                        {
                            ou.Parent = null;
                            dataContext.SubmitChanges();
                        }
                    }
                    else if (ou == null)
                    {
                        Logger.Instance.WriteEntry("OUManager_ChangeParent: Can't find the OU to be changed.", LogType.Warning);
                        return false;
                    }
                    else
                    {
                        Logger.Instance.WriteEntry("OUManager_ChangeParent: Can't change parent OU. The UpdateTime is to late.", LogType.Warning);
                        return false;
                    }


                }
                return true;
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("OUManager_ChangeParent: Can't change parent OU. " + e.StackTrace + " " + e.Message + " " + e, LogType.Exception);
                return false;
            }
        }

        #endregion
    }
}

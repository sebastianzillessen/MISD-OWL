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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace MISD.Server.Manager
{
    public class UIConfigManager
    {
        #region Singleton

        private static volatile UIConfigManager instance;
        private static object syncRoot = new Object();

        /// <summary>
        /// Property to access the class.
        /// </summary>
        public static UIConfigManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new UIConfigManager();
                    }
                }

                return instance;
            }
        }

        #endregion

        #region Constructors

        private UIConfigManager()
        {

        }

        #endregion

        # region Methods

        private class LayoutResult
        {
            public int ID;
            public string Name;
            public string UserName;
            public DateTime Date;
            public byte[] PreviewImage;
            public string Data;
        }

        private int GetObjectSize(object TestObject)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            try
            {
                byte[] Array;
                bf.Serialize(ms, TestObject);
                Array = ms.ToArray();
                return Array.Length;
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("UIConfigManager_GetObjectSize_Unexpected exception occured: " + e, LogType.Exception);
                return -1;
            }
            finally
            {
                ms.Flush();
                ms.Close();
                ms.Dispose();
            }
        }

        public List<MISD.Core.Layout> GetUIConfigurationList()
        {
            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
                try
                {
                    //List<MISD.Core.Layout> result = new List<MISD.Core.Layout>();

                    //var query = new StringBuilder();

                    //query.Append("select l.ID, l.Name, u.Name as UserName, Date, PreviewImage, Data from MISD.dbo.Layout l join MISD.dbo.[User] u on l.UserID = u.ID");

                    //var queryResult = dataContext.ExecuteQuery<LayoutResult>(query.ToString()).ToList();

                    //foreach (var item in queryResult)
                    //{
                    //    result.Add(new MISD.Core.Layout() { ID = item.ID, Name = item.Name, UserName = item.UserName, Date = item.Date, Data = item.Data, PreviewImage = item.PreviewImage });
                    //    Logger.Instance.WriteEntry("Data size: " + this.GetObjectSize(item.Data), LogType.Warning);
                    //    Logger.Instance.WriteEntry("PreviewImage size: " + this.GetObjectSize(item.PreviewImage), LogType.Warning);
                    //}

                    var uiConfigurations = (from p in dataContext.Layout
                                            join u in dataContext.User on p.UserID equals u.ID
                                            select new Core.Layout()
                                            {
                                                ID = p.ID,
                                                Name = p.Name,
                                                UserName = u.Name,
                                                Date = p.Date,
                                                PreviewImage = p.PreviewImage.ToArray(),
                                                Data = p.Data
                                            }).ToList();

                    return uiConfigurations;

                    //return result;
                }
                catch (Exception e)
                {
                    //logging exception
                    var messageEx = new StringBuilder();
                    messageEx.Append("ClientWebService_GetUIConfigurationList: ");
                    messageEx.Append("Can't create UIConfigurationList" + ". " + e.ToString());
                    MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);

                    return null;
                }
        }

        public MISD.Core.Layout AddUIConfiguration(string name, string userName, byte[] previewImageAsBase64, object data, DateTime Date)
        {
            using (var dataContext = DataContextFactory.CreateDataContext())
            {
                try
                {
                    MISD.Server.Database.User user = new MISD.Server.Database.User();

                    var hasuser = (from p in dataContext.User
                                   where p.Name == userName
                                   select p).ToList();
                    if (hasuser.Count == 0)
                    {
                        user.Name = userName;
                        dataContext.User.InsertOnSubmit(user);
                        dataContext.SubmitChanges();
                    }
                    MISD.Server.Database.Layout config = new MISD.Server.Database.Layout();
                    config.Name = name;
                    config.User = (from p in dataContext.User
                                   where p.Name == userName
                                   select p).FirstOrDefault();
                    config.PreviewImage = (System.Data.Linq.Binary)previewImageAsBase64;
                    config.Data = (string)data;
                    config.Date = Date;
                    dataContext.Layout.InsertOnSubmit(config);

                    dataContext.SubmitChanges();
                    var uiConfigurationx = (from p in dataContext.Layout
                                            where p.Name == name
                                            select new Core.Layout()
                                            {
                                                Name = p.Name,
                                                UserName = p.User.Name,
                                                Date = p.Date,
                                                PreviewImage = p.PreviewImage.ToArray(),
                                                Data = p.Data
                                            }).First();
                    return uiConfigurationx;

                }
                catch (Exception e)
                {
                    //logging exception
                    var messageEx1 = new StringBuilder();
                    messageEx1.Append("ClientWebService_AddUIConfiguration: ");
                    messageEx1.Append("Adding of IConfiguration " + name + " to UIConfigurations failed. " + e.ToString());
                    MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Exception);
                    return null;
                }

            }
        }

        public bool RemoveUIConfiguration(int id)
        {
            using (var dataContext = DataContextFactory.CreateDataContext())
            {

                var uiConfiguration = (from p in dataContext.Layout
                                       where p.ID == id
                                       select p).FirstOrDefault();

                if (uiConfiguration != null)
                {
                    dataContext.Layout.DeleteOnSubmit(uiConfiguration);
                    try
                    {
                        dataContext.SubmitChanges();
                        return true;
                    }
                    catch (Exception e)
                    {
                        //logging exception
                        var messageEx1 = new StringBuilder();
                        messageEx1.Append("ClientWebService_RemoveUIConfiguration: ");
                        messageEx1.Append("Removing of UIConfiguration from UIConfigurations failed. " + e.ToString());
                        MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Exception);
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public MISD.Core.Layout UpdateUIConfiguration(int configurationID, string name, string userName, byte[] previewImageAsBase64, object data, DateTime Date)
        {
            using (var dataContext = DataContextFactory.CreateDataContext())
            {
                try
                {
                    MISD.Server.Database.User user = new MISD.Server.Database.User();

                    var hasuser = (from p in dataContext.User
                                   where p.Name == userName
                                   select p).ToList();
                    if (hasuser.Count == 0)
                    {
                        user.Name = userName;
                        dataContext.User.InsertOnSubmit(user);
                        dataContext.SubmitChanges();
                    }
                    //Gets UI Configuration by ID and changes parameters of it
                    var uiConfiguration = (from p in dataContext.Layout
                                           where p.ID == configurationID
                                           select p).First();

                    uiConfiguration.Name = name;
                    uiConfiguration.User = (from p in dataContext.User
                                   where p.Name == userName
                                   select p).FirstOrDefault();
                    uiConfiguration.Date = Date;
                    uiConfiguration.PreviewImage = (System.Data.Linq.Binary)previewImageAsBase64;
                    uiConfiguration.Data = (string)data;

                    dataContext.SubmitChanges();

                    var uiConfigurationx = (from p in dataContext.Layout
                                            where p.ID == configurationID
                                            select new Core.Layout()
                                            {
                                                ID = p.ID,
                                                Name = p.Name,
                                                UserName = p.User.Name,
                                                Date = p.Date,
                                                PreviewImage = p.PreviewImage.ToArray(),
                                                Data = p.Data
                                            }).First();
                    return uiConfigurationx;
                }
                catch (Exception e)
                {
                    //logging exception
                    var messageEx = new StringBuilder();
                    messageEx.Append("ClientWebService_UpdateUIConfiguration: ");
                    messageEx.Append("Can't update UIConfiguration " + configurationID + ". " + e.ToString());
                    MISD.Core.Logger.Instance.WriteEntry(messageEx.ToString(), LogType.Exception);
                    return null;
                }
            }
        }


        #endregion
    }
}

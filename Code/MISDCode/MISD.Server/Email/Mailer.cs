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
using System.Net.Mail;

using MISD.Core.Scheduling;
using System.Net;
using System.Security;
using MISD.Server.Manager;
using MISD.Server.Email.Templates;
using MISD.Server.Email.WarningMailParser;
using MISD.Core;
using System.IO;
using System.Reflection;
using MISD.Server.Database;

namespace MISD.Server.Email
{
    public class Mailer
    {
        #region Instance
        private static Mailer instance = new Mailer();

        /// <summary>
        /// Singleton field. Use this one to get a Logger instance.
        /// </summary>
        public static Mailer Instance
        {
            get
            {
                return instance;
            }
        }

        #endregion

        #region Properties

        private MailParser parser = new MailParser();

        private List<MailAddress> dailyMails = new List<MailAddress>();

        private Dictionary<int, List<MailAddress>> warningMails = new Dictionary<int, List<MailAddress>>();

        #endregion

        #region Path
        protected string TemplatePath
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                    + Path.DirectorySeparatorChar
                    + Properties.Settings.Default.TemplatePath;
            }
        }

        #endregion

        #region Constructors

        private Mailer()
        {
            //create directory
            if (Directory.Exists(TemplatePath))
            {
                Directory.CreateDirectory(TemplatePath);
            }

            // load Daily-Mails from database
            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {
                dailyMails = (from mail in dataContext.Email
                              where mail.DailMail == true
                              select new MailAddress(mail.Address, mail.ReceiverName)).ToList<MailAddress>();
            }
        }

        #endregion

        #region Daily Mail Methods
        /// <summary>
        /// Sends all daily e-mails.
        /// </summary>
        public void SendAllDailyMails()
        {
            try
            {
                if (dailyMails != null && dailyMails.Count > 0)
                {
                    //generate email text
                    string emailText = this.GenerateDailyMail();

                    //send the mail
                    var from = new MailAddress(Properties.Settings.Default.MailAdressFrom, Properties.Settings.Default.MailFromDisplayName);
                    this.SendMail(from, dailyMails, Properties.Settings.Default.DailyMailSubject, emailText);
                }
            }
            catch (Exception e)
            {
                //logging
                StringBuilder messageEx1 = new StringBuilder();
                messageEx1.Append("Error " + e.ToString());
                Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Exception);
            }


            //logging
            Logger.Instance.WriteEntry("Daily mail transmitted.", LogType.Info);
        }

        #endregion

        #region E-Mail Warnings Methods

        /// <summary>
        /// Sends all e-mail-warnings.
        /// </summary>
        /// <param name="monitoredSystemID">The ID of the system that caused the warning.</param>
        /// <param name="criticalValueReceived">The time when the critical value was received.</param>
        /// <param name="pluginName">The plugin that aquired the value.</param>
        /// <param name="indicator">The indicator of the value.</param>
        /// <param name="criticalValue">The value that is mapped to critical.</param>
        public void SendAllWarnings(int monitoredSystemID, DateTime criticalValueReceived,
            string pluginName, string indicator, string criticalValue)
        {
            try
            {
                List<MailAddress> list;
                if (warningMails.ContainsKey(monitoredSystemID))
                {
                    warningMails.TryGetValue(monitoredSystemID, out list);
                }
                else
                {
                    using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
                    {
                        list = (from observer in dataContext.EmailObserver
                                join mail in dataContext.Email on observer.EmailID equals mail.ID
                                join ws in dataContext.MonitoredSystem on observer.MonitoredSystemID equals ws.ID
                                where ws.ID == monitoredSystemID
                                select new MailAddress(mail.Address, mail.ReceiverName)
                                          ).ToList<MailAddress>();
                        warningMails.Add(monitoredSystemID, list);
                    }
                }
                //get mail adresses from the db

                if (list != null)
                {
                    if (list.Count > 0)
                    {
                        //generate text
                        string emailText = this.GenerateWarningMail(monitoredSystemID, criticalValueReceived,
                            pluginName, indicator, criticalValue);

                        //send the mail
                        var from = new MailAddress(Properties.Settings.Default.MailAdressFrom, Properties.Settings.Default.WarningMailFromDisplayName);
                        this.SendMail(from, list, Properties.Settings.Default.WarningMailSubject, emailText); 
                    }
                }
                else
                {
                    //logging
                    StringBuilder messageEx1 = new StringBuilder();
                    messageEx1.Append("Mailer_SendAllWarnings: ");
                    messageEx1.Append("Can't send mail for system " + monitoredSystemID + "  .");
                    messageEx1.Append("The list of emails wasn't correctly loaded!");
                    Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Exception);
                }
            }
            catch (Exception e)
            {
                //logging
                StringBuilder messageEx1 = new StringBuilder();
                messageEx1.Append("Mailer_SendAllWarnings: ");
                messageEx1.Append("Can't send mail for system " + monitoredSystemID + ". " + e.ToString());
                Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Warning);
            }
        }

        #endregion

        #region Template Methods

        private string GenerateDailyMail()
        {
            long now = DateTime.UtcNow.Ticks;

            List<WorkstationInfo> maintanceList = new List<WorkstationInfo>();
            List<WorkstationInfo> criticalList = new List<WorkstationInfo>();
            List<WorkstationInfo> warningList = new List<WorkstationInfo>();

            //generate template data
            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {
                #region get maintance data

                try
                {
                    maintanceList = (from ms in dataContext.MonitoredSystem
                                     join main in dataContext.Maintenance on ms.ID equals main.MonitoredSystemID
                                     let start = main.Beginning != null
                                     let end = main.End == null
                                     where start & end
                                     group ms by ms.ID into msGroup
                                     select new WorkstationInfo()
                                     {
                                         ID = msGroup.First().ID,
                                         Name = msGroup.First().Name,
                                         FQDN = msGroup.First().FQDN,
                                         IsAvailable = (bool)msGroup.First().IsAvailable,
                                         CurrentOS = ((Platform)msGroup.First().OperatingSystem).ToString(),
                                         State = (MappingState)MappingState.Maintenance
                                     }).ToList<WorkstationInfo>();
                }
                catch (Exception)
                {
                    Logger.Instance.WriteEntry("Mailer_GenerateDailyMail: Can't load maintance list.", LogType.Warning);
                }

                #endregion

                #region get critical data

                try
                {
                    criticalList = (from ms in dataContext.MonitoredSystem
                                    join ind in dataContext.Indicator on ms.ID equals ind.MonitoredSystemID
                                    join value in dataContext.IndicatorValue on ind.ID equals value.IndicatorID
                                    let mapping = value.Mapping == (byte)MappingState.Critical
                                    let mappingDuration = (ind.MappingDuration == null) ? 864000000000 : ind.MappingDuration
                                    let deadLineTicks = now - mappingDuration
                                    let indicatorValueTicks = value.Timestamp
                                    where mapping & (indicatorValueTicks >= deadLineTicks)
                                    group ms by ms.ID into msGroup
                                    select new WorkstationInfo()
                                    {
                                        ID = msGroup.First().ID,
                                        Name = msGroup.First().Name,
                                        FQDN = msGroup.First().FQDN,
                                        IsAvailable = (bool)msGroup.First().IsAvailable,
                                        CurrentOS = ((Platform)msGroup.First().OperatingSystem).ToString(),
                                        State = (MappingState)MappingState.Maintenance
                                    }).ToList<WorkstationInfo>();
                }
                catch (Exception)
                {
                    Logger.Instance.WriteEntry("Mailer_GenerateDailyMail: Can't load critical list.", LogType.Warning);
                }

                #endregion

                #region get warning data

                try
                {
                    warningList = (from ms in dataContext.MonitoredSystem
                                   join ind in dataContext.Indicator on ms.ID equals ind.MonitoredSystemID
                                   join value in dataContext.IndicatorValue on ind.ID equals value.IndicatorID
                                   let mapping = value.Mapping == (byte)MappingState.Warning
                                   let mappingDuration = (ind.MappingDuration == null) ? 864000000000 : ind.MappingDuration
                                   let deadLineTicks = now - mappingDuration
                                   let indicatorValueTicks = value.Timestamp
                                   where mapping & (indicatorValueTicks >= deadLineTicks)
                                   group ms by ms.ID into msGroup
                                   select new WorkstationInfo()
                                   {
                                       ID = msGroup.First().ID,
                                       Name = msGroup.First().Name,
                                       FQDN = msGroup.First().FQDN,
                                       IsAvailable = (bool)msGroup.First().IsAvailable,
                                       CurrentOS = ((Platform)msGroup.First().OperatingSystem).ToString(),
                                       State = (MappingState)MappingState.Maintenance
                                   }).ToList<WorkstationInfo>();
                }
                catch (Exception)
                {
                    Logger.Instance.WriteEntry("Mailer_GenerateDailyMail: Can't load warning list.", LogType.Warning);
                }

                #endregion
            }

            var mailData = new DailyMailTemplateData()
            {
                maintanceWorkstations = (maintanceList == null) ? new List<WorkstationInfo>() : maintanceList,
                criticalWorkstations = (criticalList == null) ? new List<WorkstationInfo>() : criticalList,
                waringWorkstations = (warningList == null) ? new List<WorkstationInfo>() : warningList
            };

            //generate email text
            DailyMailTemplate page = new DailyMailTemplate(mailData);
            string emailText = page.TransformText();

            return emailText;

        }

        /// <summary>
        /// Loads the HTML-template for the warning mail and generates the mail
        /// Template tags: [%WSName%], [%Date%], [%PluginName%], [%Indicator%], [%Value%]
        /// </summary>
        /// <returns>String containing the HTML-Email-Message</returns>
        private string GenerateWarningMail(int monitoredSystemID, DateTime criticalValueReceived,
            string pluginName, string indicator, string criticalValue)
        {
            string template = "";

            //instantiate the WarningMailParser
            parser.ClearTags();

            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {
                StreamReader reader = null;
                try
                {
                    string name = PrecompiledQueries.GetMonitoredSystemNameByID(dataContext, monitoredSystemID);
                    parser.AddTag("[%WSName%]", name);
                    parser.AddTag("[%Date%]", criticalValueReceived.ToString());
                    parser.AddTag("[%PluginName%]", pluginName);
                    parser.AddTag("[%Indicator%]", indicator);
                    parser.AddTag("[%Value%]", criticalValue);

                    //load the warning mail template
                    reader = new StreamReader(TemplatePath);

                    template = reader.ReadToEnd();
                }
                catch (Exception e)
                {
                    //logging
                    StringBuilder messageEx1 = new StringBuilder();
                    messageEx1.Append("Mailer_GenerateWarningMail: ");
                    messageEx1.Append("Can't load warning mail template. " + e.ToString());
                    Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Exception);
                }
                finally
                {
                    reader.Close();
                }

                //Parese template string
                return parser.ParseTemplateString(template);
            }
        }

        #endregion

        #region Mail Sender
        /// <summary>
        /// Send a Email
        /// </summary>
        /// <param name="from">Sender mail adress</param>
        /// <param name="to">Reminder mail adress</param>
        /// <param name="subjekt">Subjekt of the email</param>
        /// <param name="message">Message of the email</param>
        /// <returns></returns>
        public Boolean SendMail(MailAddress from, List<MailAddress> to, string subjekt, string message)
        {
            Boolean result = true;

            //create Message
            var messageToSend = new MailMessage();
            messageToSend.Body = message;
            messageToSend.BodyEncoding = System.Text.Encoding.UTF8;
            messageToSend.IsBodyHtml = true;
            messageToSend.Subject = subjekt;
            messageToSend.SubjectEncoding = System.Text.Encoding.UTF8;
            messageToSend.From = from;
            foreach (MailAddress adress in to)
            {
                messageToSend.To.Add(adress);
            }

            //create connection
            SmtpClient smtpClient = new SmtpClient()
            {
                Host = Properties.Settings.Default.MailHost,
                Port = Properties.Settings.Default.MailPort,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(
                    Properties.Settings.Default.MailAdressFrom,
                    Properties.Settings.Default.MailAdressFromPW),
                Timeout = 20000
            };

            //send mail
            try
            {
                smtpClient.Send(messageToSend);
            }
            catch (Exception e)
            {
                var ex1 = new StringBuilder();
                ex1.Append("Can't send the mail: " + messageToSend.Subject + " ");
                ex1.Append(e.ToString());
                Logger.Instance.WriteEntry(ex1.ToString(), LogType.Debug);
                result = false;
            }
            finally
            {
                //clean up
                messageToSend.Dispose();
                smtpClient.Dispose();
            }
            return result;
        }
        #endregion

        #region Mailing DB

        public int? AddEMail(string emailAdress, string userName)
        {
            using (var dataContext = DataContextFactory.CreateDataContext())
            {
                MISD.Server.Database.Email mail = new MISD.Server.Database.Email();
                mail.Address = emailAdress;
                mail.ReceiverName = userName;
                mail.DailMail = false;
                dataContext.Email.InsertOnSubmit(mail);
                try
                {
                    dataContext.SubmitChanges();
                    return mail.ID;
                }
                catch
                {

                    //logging exception
                    var messageEx1 = new StringBuilder();
                    messageEx1.Append("Mailer_AddEMail: ");
                    messageEx1.Append("Adding of emailadress " + emailAdress + " of " + userName + " to MailList failed.");
                    MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Exception);
                    return null;
                }
            }

        }

        public bool RemoveEMail(int userID)
        {
            using (var dataContext = DataContextFactory.CreateDataContext())
            {
                var mail = (from p in dataContext.Email
                            where (p.ID == userID)
                            select p).FirstOrDefault();

                if (mail == null)
                {
                    Logger.Instance.WriteEntry("Mailer_RemoveEMail: Cannot find the email to be removed.", LogType.Warning);
                    return false;
                }

                //save user to delete from local cache
                MailAddress m = new MailAddress(mail.Address, mail.ReceiverName);

                // delete on database
                dataContext.Email.DeleteOnSubmit(mail);

                // remove from local cache
                
                dailyMails.Remove(m);
                for (int i = 0; i < warningMails.Count; i++)
                {
                    warningMails.ElementAt(i).Value.Remove(m);
                }
                try
                {
                    dataContext.SubmitChanges();
                    return true;
                }
                catch
                {

                    //logging exception
                    var messageEx1 = new StringBuilder();
                    messageEx1.Append("Mailer_RemoveEMail: ");
                    messageEx1.Append("Removing of emailadress " + m.Address + " of " + m.User + " from MailList failed.");
                    MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Exception);
                    return false;
                }
            }

        }

        public bool AddMailObserver(int userID, List<string> mac)
        {
            if (mac == null)
            {
                return false;
            }
            try
            {
                using (var dataContext = DataContextFactory.CreateDataContext())
                {
                    foreach (string i in mac)
                    {
                       var msID = (from q in dataContext.MonitoredSystem
                                    where q.MacAddress == i
                                    select q.ID).FirstOrDefault();

                        EmailObserver observer = new EmailObserver()
                        {
                            EmailID = userID,
                            MonitoredSystemID = msID
                        };
                        dataContext.EmailObserver.InsertOnSubmit(observer);
                        dataContext.SubmitChanges();

                        // local caching.
                        #region caching
                        if (warningMails.ContainsKey(msID))
                        {
                            List<MailAddress> list;
                            if (warningMails.TryGetValue(msID, out list))
                            {
                                warningMails.Remove(msID);
                            }
                            else
                            {
                                list = new List<MailAddress>();
                            }
                            list.Add(new MailAddress(observer.Email.Address, observer.Email.ReceiverName));
                            warningMails.Add(msID, list);
                        }
                        #endregion 
                        
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                //logging exception
                var messageEx1 = new StringBuilder();
                messageEx1.Append("Mailer_AddMailObserver: ");
                messageEx1.Append("Adding of MailObserver failed. " + e.ToString());
                MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Exception);
                return false;
            } 
        }

        public bool RemoveMailObserver(int userID, List<string> monitoredSystemMACs)
        {
            using (var dataContext = DataContextFactory.CreateDataContext())
            {
                try
                {
                    foreach (string mac in monitoredSystemMACs)
                    {
                        var mail = (from p in dataContext.EmailObserver
                                    where (p.EmailID == userID && p.MonitoredSystem.MacAddress == mac)
                                    select p).FirstOrDefault();
                        if (mail != null)
                        {
                            dataContext.EmailObserver.DeleteOnSubmit(mail);
                            dataContext.SubmitChanges();

                            #region caching
                            for (int i = 0; i < warningMails.Count; i++)
                            {
                                warningMails.ElementAt(i).Value.Remove(new MailAddress(mail.Email.Address, mail.Email.ReceiverName));
                            }
                            #endregion
                        }
                        else
                        {
                            Logger.Instance.WriteEntry("Mailer_RemoveMailObserver: Cannot find mail to be removed", LogType.Warning);
                        }
                    }
                    return true;
                }
                catch (Exception e)
                {
                    //logging exception
                    var messageEx1 = new StringBuilder();
                    messageEx1.Append("Mailer_RemoveMailObserver: ");
                    messageEx1.Append("Removing of MailObserver failed. " + e.ToString());
                    MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Exception);
                    return false;
                }
            }
        }

        public bool AddDailyMail(int mailID)
        {
            using (var db = DataContextFactory.CreateDataContext())
            {
                Database.Email mail;
                try
                {
                    mail = (from p in db.Email
                            where p.ID == mailID
                            select p).FirstOrDefault();

                    if (mail == null)
                    {
                        Logger.Instance.WriteEntry("Mailer_AddDailyMail: Cannot find mail to be added.", LogType.Warning);
                        return false;
                    }

                    mail.DailMail = true;
                
                    db.SubmitChanges();
                    return true;
                }
                catch
                {

                    //logging exception
                    var messageEx1 = new StringBuilder();
                    messageEx1.Append("Mailer_AddDailyMail: ");
                    messageEx1.Append("Adding of emailadress " + mailID + " to DailyMail List failed.");
                    MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Exception);
                    return false;
                }
            }
        }

        public bool DeleteDailyMail(int userID)
        {
            using (var dataContext = DataContextFactory.CreateDataContext())
            {
                var mail = (from p in dataContext.Email
                            where p.ID == userID
                            select p).FirstOrDefault();

                if (mail == null)
                {
                    Logger.Instance.WriteEntry("Mailer_DeleteDailyMail: Cannot find mail to be added.", LogType.Warning);
                    return false;
                }

                mail.DailMail = false;
                try
                {
                    dataContext.SubmitChanges();
                    dailyMails.Remove(new MailAddress(mail.Address, mail.ReceiverName));
                    return true;
                }
                catch
                {

                    //logging exception
                    var messageEx1 = new StringBuilder();
                    messageEx1.Append("Mailer_DeleteDailyMail: ");
                    messageEx1.Append("Deleting of emailadress " + userID + " from DailyMail List failed.");
                    MISD.Core.Logger.Instance.WriteEntry(messageEx1.ToString(), LogType.Exception);
                    return false;
                }
            }

        }

        /// <summary>
        /// Gets all email user datas
        /// </summary>
        /// <returns>List of Tuple with data: ID | username | user mail adress | daily mail </returns>
        public List<Tuple<int, string, string, bool>> GetAllMailData()
        {
            try
            {
                List<Tuple<int, string, string, bool>> result;
                using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
                {
                    result = (from q in dataContext.Email
                              select new Tuple<int, string, string, bool>(q.ID, q.ReceiverName, q.Address, q.DailMail)).ToList();
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("Mailer_GetAllMailData: Can't create result: " + e.StackTrace, LogType.Exception);
                return null;
            }
        }

        /// <summary>
        /// Retuns all observer of an monitored system
        /// </summary>
        /// <param name="userID">ID of the users email adress</param>
        /// <returns>List of MAC adress </returns>
        public List<WorkstationInfo> GetObserver(int userID)
        {
            List<WorkstationInfo> result = new List<WorkstationInfo>();

            try
            {
                using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
                {
                    var msIDs = (from q in dataContext.EmailObserver
                                 where q.EmailID == userID
                                 select q.MonitoredSystem.ID).ToList();

                    foreach (var ms in msIDs)
                    {
                        //update mapping state
                        ValueManager.Instance.UpdateMonitoredSystemMappingState(ms);

                        var monitoredSystem = PrecompiledQueries.GetMonitoredSystemByID(dataContext, ms);

                        //create result
                        var workstationinfo = new WorkstationInfo()
                        {
                            Name = monitoredSystem.Name,
                            ID = monitoredSystem.ID,
                            FQDN = monitoredSystem.FQDN,
                            OuID = monitoredSystem.OrganizationalUnitID,
                            IsAvailable = monitoredSystem.IsAvailable,
                            CurrentOS = ((MISD.Core.Platform)monitoredSystem.OperatingSystem).ToString(),
                            MacAddress = monitoredSystem.MacAddress,
                            State = monitoredSystem.Status.HasValue ? (MappingState)monitoredSystem.Status : MappingState.OK
                        };
                        result.Add(workstationinfo);
                    }

                }

                return result;
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("Mailer_GetObserver: Can't create result: " + e.StackTrace, LogType.Exception);
                return null;
            }
        }

        /// <summary>
        /// Changes the mail adress and the username of a given mail adress
        /// </summary>
        /// <param name="mailAdressOld"></param>
        /// <param name="userNameNew"></param>
        /// <param name="mailAdressNew"></param>
        /// <returns>true if the execution was done without errors.</returns>
        public bool ChangeEmail(int  userID, string userNameNew, string mailAdressNew)
        {
            try
            {
                using (var dataContext = DataContextFactory.CreateDataContext())
                {
                    var user = (from q in dataContext.Email
                                where q.ID == userID
                                select q).FirstOrDefault();

                    if (user == null)
                    {
                        Logger.Instance.WriteEntry("Mailer_ChangeEmail: Cannot find the user.", LogType.Warning);
                        return false;
                    }

                    user.ReceiverName = userNameNew;
                    user.Address = mailAdressNew;

                    dataContext.SubmitChanges();

                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("Mailer_ChangeEmail: Can't change email: " + e.StackTrace, LogType.Exception);
                return false;
            }
        }

        #endregion
    }
}

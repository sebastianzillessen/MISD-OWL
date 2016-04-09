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


namespace MISD.DatabaseTest
{
    /// <summary>
    /// This class fills the database with test data.
    /// Run the main-Method to start the console program.
    /// </summary>
    class Program
    {
    //    private static MISD.Server.Database.MISDDataContext db = new MISD.Server.Database.MISDDataContext();

    //    private static string[] tables = new string[14]
    //    {
    //            "EmailObserver",
    //            "Email",
    //            "Maintenance",
    //            "UIConfiguration",
    //            "ValueByte",
    //            "ValueFloat",
    //            "ValueInt",
    //            "ValueString",
    //            "[User]",
    //            "IndicatorValue",
    //            "Indicator",
    //            "MonitoredSystem",
    //            "OrganizationalUnit",
    //            "PluginMetadata"
    //    };

    //    private static void Main(string[] args)
    //    {
    //        #region Delete Data
    //        // Delete all data
    //        Console.WriteLine("Delete all data...");

    //        foreach (string table in tables)
    //        {
    //            db.ExecuteCommand("DELETE FROM " + table);
    //        }
    //        #endregion

    //        #region User
    //        // Fill table 'User'
    //        Console.WriteLine("Fill table 'User'...");

    //        List<User> users = new List<User>();

    //        String[] userStrings = new String[6]
    //        { 
    //            "Yannic Noller",
    //            "Carl Coder", 
    //            "Willi Biene", 
    //            "Biene Maja",
    //            "Blinky Bill",
    //            "Nils Holgerson"
    //        };

    //        User user;
    //        foreach (String userString in userStrings)
    //        {
    //            user = new User();
    //            user.Name = userString;
    //            users.Add(user);
    //        }

    //        db.User.InsertAllOnSubmit(users);
    //        db.SubmitChanges();
    //        #endregion

    //        #region UIConfiguration
    //        // Fill table 'UIConfiguration'
    //        Console.WriteLine("Fill table 'UIConfiguration'...");

    //        List<UIConfiguration> uiConfigurations = new List<UIConfiguration>();

    //        UIConfiguration uiConfig;
    //        foreach (User userItem in users)
    //        {
    //            uiConfig = new UIConfiguration();
    //            uiConfig.Name = "UIConfig" + users.IndexOf(userItem);
    //            uiConfig.Date = DateTime.Now;
    //            uiConfig.PreviewImage = null;
    //            uiConfig.Data = "DATA";
    //            uiConfig.UserID = userItem.ID;
    //            uiConfigurations.Add(uiConfig);
    //        }

    //        db.UIConfiguration.InsertAllOnSubmit(uiConfigurations);
    //        db.SubmitChanges();
    //        #endregion

    //        #region OrganizationalUnit
    //        // Fill table 'OrganizationalUnit'
    //        Console.WriteLine("Fill table 'OrganizationalUnit'...");

    //        List<OrganizationalUnit> list = new List<OrganizationalUnit>();

    //        OrganizationalUnit visPool = new OrganizationalUnit();
    //        visPool.ID = Guid.NewGuid();
    //        visPool.Name = "VISPool";
    //        visPool.FQDN = "d75225e3-fac4-4c51-8025-be1784cf359b";
    //        visPool.Parent = null;
    //        list.Add(visPool);
    //        OrganizationalUnit brightCluster = new OrganizationalUnit();
    //        brightCluster.ID = Guid.NewGuid();
    //        brightCluster.Name = "BrightCluster";
    //        brightCluster.FQDN = "d76225e3-fac7-4c51-8025-ba1784df319b";
    //        brightCluster.Parent = null;
    //        list.Add(brightCluster);
    //        OrganizationalUnit hpcCluster = new OrganizationalUnit();
    //        hpcCluster.ID = Guid.NewGuid();
    //        hpcCluster.Name = "HPCCluster";
    //        hpcCluster.FQDN = "d79515u3-fuc7-4c51-8725-bi1784df318l";
    //        hpcCluster.Parent = null;
    //        list.Add(hpcCluster);

    //        db.OrganizationalUnit.InsertAllOnSubmit(list);
    //        db.SubmitChanges();

    //        OrganizationalUnit visusPool = new OrganizationalUnit();
    //        visusPool.ID = Guid.NewGuid();
    //        visusPool.Name = "VISUSPool";
    //        visusPool.FQDN = "z79715u3-fuc7-4c51-8725-bi1784df318l";
    //        visusPool.Parent = visPool.ID;

    //        db.OrganizationalUnit.InsertOnSubmit(visusPool);
    //        db.SubmitChanges();
    //        #endregion

    //        #region MonitoredSystem
    //        // Fill table 'MonitoredSystem'
    //        Console.WriteLine("Fill table 'MonitoredSystem'...");

    //        List<MonitoredSystem> monitoredSystems = new List<MonitoredSystem>();

    //        MonitoredSystem vis1 = new MonitoredSystem();
    //        vis1.ID = Guid.NewGuid();
    //        vis1.OrganizationalUnit = visPool;
    //        vis1.Name = "VISPoolPC1";
    //        vis1.FQDN = "e15225e1-fac4-4c51-8025-be1784cf359b";
    //        vis1.IsAvailable = false;
    //        vis1.IsIgnored = false;
    //        vis1.OperatingSystem = 0;
    //        monitoredSystems.Add(vis1);
    //        MonitoredSystem vis2 = new MonitoredSystem();
    //        vis2.ID = Guid.NewGuid();
    //        vis2.OrganizationalUnit = visPool;
    //        vis2.Name = "VISPoolPC2";
    //        vis2.FQDN = "e15225e2-fac4-4c51-8025-be1784cf359b";
    //        vis2.IsAvailable = false;
    //        vis2.IsIgnored = false;
    //        vis2.OperatingSystem = 0;
    //        monitoredSystems.Add(vis2);
    //        MonitoredSystem vis3 = new MonitoredSystem();
    //        vis3.ID = Guid.NewGuid();
    //        vis3.OrganizationalUnit = visPool;
    //        vis3.Name = "VISPoolPC3";
    //        vis3.FQDN = "e15225e3-fac4-4c51-8025-be1784cf359b";
    //        vis3.IsAvailable = false;
    //        vis3.IsIgnored = false;
    //        vis3.OperatingSystem = 0;
    //        monitoredSystems.Add(vis3);
    //        MonitoredSystem visus1 = new MonitoredSystem();
    //        visus1.ID = Guid.NewGuid();
    //        visus1.OrganizationalUnit = visusPool;
    //        visus1.Name = "VISUSPoolPC1";
    //        visus1.FQDN = "e25225e1-fac4-4c51-8025-be1784cf359b";
    //        visus1.IsAvailable = false;
    //        visus1.IsIgnored = false;
    //        visus1.OperatingSystem = 0;
    //        monitoredSystems.Add(visus1);
    //        MonitoredSystem visus2 = new MonitoredSystem();
    //        visus2.ID = Guid.NewGuid();
    //        visus2.OrganizationalUnit = visusPool;
    //        visus2.Name = "VISUSPoolPC2";
    //        visus2.FQDN = "e25225e2-fac4-4c51-8025-be1784cf359b";
    //        visus2.IsAvailable = false;
    //        visus2.IsIgnored = false;
    //        visus2.OperatingSystem = 0;
    //        monitoredSystems.Add(visus2);
    //        MonitoredSystem visus3 = new MonitoredSystem();
    //        visus3.ID = Guid.NewGuid();
    //        visus3.OrganizationalUnit = visusPool;
    //        visus3.Name = "VISUSPoolPC3";
    //        visus3.FQDN = "e25225e3-fac4-4c51-8025-be1784cf359b";
    //        visus3.IsAvailable = false;
    //        visus3.IsIgnored = false;
    //        visus3.OperatingSystem = 0;
    //        monitoredSystems.Add(visus3);
    //        MonitoredSystem bright1 = new MonitoredSystem();
    //        bright1.ID = Guid.NewGuid();
    //        bright1.OrganizationalUnit = brightCluster;
    //        bright1.Name = "BrightPC1";
    //        bright1.FQDN = "e35225e1-fac4-4c51-8025-be1784cf359b";
    //        bright1.IsAvailable = true;
    //        bright1.IsIgnored = false;
    //        bright1.OperatingSystem = 0;
    //        monitoredSystems.Add(bright1);
    //        MonitoredSystem bright2 = new MonitoredSystem();
    //        bright2.ID = Guid.NewGuid();
    //        bright2.OrganizationalUnit = brightCluster;
    //        bright2.Name = "BrightPC2";
    //        bright2.FQDN = "e35225e2-fac4-4c51-8025-be1784cf359b";
    //        bright2.IsAvailable = true;
    //        bright2.IsIgnored = false;
    //        bright2.OperatingSystem = 0;
    //        monitoredSystems.Add(bright2);
    //        MonitoredSystem bright3 = new MonitoredSystem();
    //        bright3.ID = Guid.NewGuid();
    //        bright3.OrganizationalUnit = brightCluster;
    //        bright3.Name = "BrightPC3";
    //        bright3.FQDN = "e35225e3-fac4-4c51-8025-be1784cf359b";
    //        bright3.IsAvailable = true;
    //        bright3.IsIgnored = false;
    //        bright3.OperatingSystem = 0;
    //        monitoredSystems.Add(bright3);
    //        MonitoredSystem hpc1 = new MonitoredSystem();
    //        hpc1.ID = Guid.NewGuid();
    //        hpc1.OrganizationalUnit = hpcCluster;
    //        hpc1.Name = "HPCPC1";
    //        hpc1.FQDN = "e45225e1-fac4-4c51-8025-be1784cf359b";
    //        hpc1.IsAvailable = true;
    //        hpc1.IsIgnored = false;
    //        hpc1.OperatingSystem = 0;
    //        monitoredSystems.Add(hpc1);
    //        MonitoredSystem hpc2 = new MonitoredSystem();
    //        hpc2.ID = Guid.NewGuid();
    //        hpc2.OrganizationalUnit = hpcCluster;
    //        hpc2.Name = "HPCPC2";
    //        hpc2.FQDN = "e45225e2-fac4-4c51-8025-be1784cf359b";
    //        hpc2.IsAvailable = true;
    //        hpc2.IsIgnored = false;
    //        hpc2.OperatingSystem = 0;
    //        monitoredSystems.Add(hpc2);
    //        MonitoredSystem hpc3 = new MonitoredSystem();
    //        hpc3.ID = Guid.NewGuid();
    //        hpc3.OrganizationalUnit = hpcCluster;
    //        hpc3.Name = "HPCPC3";
    //        hpc3.FQDN = "e45225e3-fac4-4c51-8025-be1784cf359b";
    //        hpc3.IsAvailable = true;
    //        hpc3.IsIgnored = false;
    //        hpc3.OperatingSystem = 0;
    //        monitoredSystems.Add(hpc3);
    //        db.MonitoredSystem.InsertAllOnSubmit(monitoredSystems);
    //        db.SubmitChanges();
    //        #endregion

    //        #region Maintenance
    //        // Fill table 'Maintenance'
    //        Console.WriteLine("Fill table 'Maintenance'...");

    //        List<Maintenance> mainList = new List<Maintenance>();

    //        Maintenance maintenance;

    //        maintenance = new Maintenance();
    //        maintenance.MonitoredSystemID = vis1.ID;
    //        maintenance.Beginning = DateTime.Parse("2012-08-03");
    //        maintenance.End = DateTime.Parse("2012-08-04");
    //        mainList.Add(maintenance);

    //        maintenance = new Maintenance();
    //        maintenance.MonitoredSystemID = vis1.ID;
    //        maintenance.Beginning = DateTime.Parse("2012-08-05");
    //        maintenance.End = DateTime.Parse("2012-08-06");
    //        mainList.Add(maintenance);

    //        maintenance = new Maintenance();
    //        maintenance.MonitoredSystemID = vis1.ID;
    //        maintenance.Beginning = DateTime.Parse("2012-08-10");
    //        maintenance.End = DateTime.Parse("2012-08-14");
    //        mainList.Add(maintenance);

    //        maintenance = new Maintenance();
    //        maintenance.MonitoredSystemID = vis2.ID;
    //        maintenance.Beginning = DateTime.Parse("2012-09-01");
    //        maintenance.End = null;
    //        mainList.Add(maintenance);

    //        db.Maintenance.InsertAllOnSubmit(mainList);
    //        db.SubmitChanges();
    //        #endregion

    //        #region Email
    //        // Fill table 'Email'
    //        Console.WriteLine("Fill table 'Email'...");

    //        List<Email> emails = new List<Email>();

    //        String[,] emailStrings = new String[7, 2]
    //        { 
    //            {"nolleryc@gmail.com", "Yannic Noller"},
    //            {"carl@coder.de", "Carl Coder"}, 
    //            {"gisela@gierig.de", "Gisela Gierig"}, 
    //            {"rainer@zufall.de", "Rainer Zufall"},
    //            {"tester@test.de", "Tester Test"},
    //            {"jane@doe.de", "Jane Doe"},
    //            {"john@doe.de", "John Doe"}
    //        };

    //        Email email;
    //        for (int i=0; i<emailStrings.GetLength(0); i++)
    //        {
    //            email = new Email();
    //            email.Address = emailStrings[i, 0];
    //            email.ReceiverName = emailStrings[i, 1];
    //            if (i%2 == 0)
    //                email.DailMail = true;
    //            else
    //                email.DailMail = false;
    //            emails.Add(email);
    //        }

    //        db.Email.InsertAllOnSubmit(emails);
    //        db.SubmitChanges();
    //        #endregion

    //        #region EmailObserver
    //        // Fill table 'EmailObserver'
    //        Console.WriteLine("Fill table 'EmailObserver'...");

    //        EmailObserver observer;
    //        List<EmailObserver> observers = new List<EmailObserver>();
            
    //        foreach (Email mail in emails)
    //        {
    //            if (emails.IndexOf(mail)%2 == 0)
    //            {
    //                observer = new EmailObserver();
    //                observer.EmailID = mail.ID;
    //                observer.MonitoredSystemID = bright1.ID;
    //                observers.Add(observer);
    //            }
    //        }

    //        db.EmailObserver.InsertAllOnSubmit(observers);
    //        db.SubmitChanges();
    //        #endregion

    //        #region PluginMetadata
    //        // Fill table 'PluginMetadata'
    //        Console.WriteLine("Fill table 'PluginMetadata'...");

    //        PluginMetadata ram = new PluginMetadata();
    //        ram.Name = "RAM";
    //        ram.Version = 1;
    //        ram.Description = "Infos über Arbeitsspeicher";
    //        ram.FileName = "ram.dll";
    //        ram.Author = "Carl Coder";

    //        PluginMetadata cpu = new PluginMetadata();
    //        cpu.Name = "CPU";
    //        cpu.Version = 3;
    //        cpu.Description = "Infos über Prozessor";
    //        cpu.FileName = "cpu.dll";
    //        cpu.Author = "Gisela Gierig";

    //        db.PluginMetadata.InsertOnSubmit(ram);
    //        db.PluginMetadata.InsertOnSubmit(cpu);
    //        db.SubmitChanges();
    //        #endregion

    //        #region Indicator, IndicatorValue, ValueX
    //        // Fill table 'Indicator' and 'IndicatorValue' and 'ValueX'
    //        Console.WriteLine("Fill table 'Indicator', 'IndicatorValue', 'ValueX'...");

    //        ValueFloat value;
    //        Indicator indicator;
    //        IndicatorValue indicatorValue;

    //        foreach (MonitoredSystem monitoredSystem in monitoredSystems)
    //        {
    //            indicator = new Indicator();
    //            indicator.Name = "StatischeGröße";
    //            indicator.PluginMetadataID = ram.ID;
    //            indicator.UpdateInterval = 1L;
    //            indicator.FilterStatement = "FilterStatement";
    //            indicator.StatementWarning = "StatementWarning";
    //            indicator.StatementCritical = "StatementCrictical";
    //            indicator.ValueType = 0;
    //            indicator.MonitoredSystemID = monitoredSystem.ID;
    //            db.Indicator.InsertOnSubmit(indicator);
    //            db.SubmitChanges();

    //            value = new ValueFloat();
    //            value.Value = 4069.0;
    //            db.ValueFloat.InsertOnSubmit(value);
    //            db.SubmitChanges();

    //            indicatorValue = new IndicatorValue();
    //            indicatorValue.IndicatorID = indicator.ID;
    //            indicatorValue.ValueID = value.ID;
    //            indicatorValue.Mapping = null;
    //            db.IndicatorValue.InsertOnSubmit(indicatorValue);
    //            db.SubmitChanges();


    //            indicator = new Indicator();
    //            indicator.Name = "Auslastung";
    //            indicator.PluginMetadataID = ram.ID;
    //            indicator.UpdateInterval = 1L;
    //            indicator.FilterStatement = "FilterStatement";
    //            indicator.StatementWarning = "StatementWarning";
    //            indicator.StatementCritical = "StatementCrictical";
    //            indicator.ValueType = 0;
    //            indicator.MonitoredSystemID = monitoredSystem.ID;
    //            db.Indicator.InsertOnSubmit(indicator);
    //            db.SubmitChanges();

    //            value = new ValueFloat();
    //            value.Value = 60;
    //            db.ValueFloat.InsertOnSubmit(value);
    //            db.SubmitChanges();

    //            indicatorValue = new IndicatorValue();
    //            indicatorValue.IndicatorID = indicator.ID;
    //            indicatorValue.ValueID = value.ID;
    //            indicatorValue.Mapping = null;
    //            db.IndicatorValue.InsertOnSubmit(indicatorValue);
    //            db.SubmitChanges();


    //            indicator = new Indicator();
    //            indicator.Name = "Taktrate";
    //            indicator.PluginMetadataID = cpu.ID;
    //            indicator.UpdateInterval = 1L;
    //            indicator.FilterStatement = "FilterStatement";
    //            indicator.StatementWarning = "StatementWarning";
    //            indicator.StatementCritical = "StatementCrictical";
    //            indicator.ValueType = 0;
    //            indicator.MonitoredSystemID = monitoredSystem.ID;
    //            db.Indicator.InsertOnSubmit(indicator);
    //            db.SubmitChanges();

    //            value = new ValueFloat();
    //            value.Value = 1024;
    //            db.ValueFloat.InsertOnSubmit(value);
    //            db.SubmitChanges();

    //            indicatorValue = new IndicatorValue();
    //            indicatorValue.IndicatorID = indicator.ID;
    //            indicatorValue.ValueID = value.ID;
    //            indicatorValue.Mapping = null;
    //            db.IndicatorValue.InsertOnSubmit(indicatorValue);
    //            db.SubmitChanges();


    //            indicator = new Indicator();
    //            indicator.Name = "Auslastung";
    //            indicator.PluginMetadataID = cpu.ID;
    //            indicator.UpdateInterval = 1L;
    //            indicator.FilterStatement = "FilterStatement";
    //            indicator.StatementWarning = "StatementWarning";
    //            indicator.StatementCritical = "StatementCrictical";
    //            indicator.ValueType = 0;
    //            indicator.MonitoredSystemID = monitoredSystem.ID;
    //            db.Indicator.InsertOnSubmit(indicator);
    //            db.SubmitChanges();

    //            value = new ValueFloat();
    //            value.Value = 50;
    //            db.ValueFloat.InsertOnSubmit(value);
    //            db.SubmitChanges();

    //            indicatorValue = new IndicatorValue();
    //            indicatorValue.IndicatorID = indicator.ID;
    //            indicatorValue.ValueID = value.ID;
    //            indicatorValue.Mapping = null;
    //            db.IndicatorValue.InsertOnSubmit(indicatorValue);
    //            db.SubmitChanges();
    //        }
    //        #endregion
    //    }
    }
}


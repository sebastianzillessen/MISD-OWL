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
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

using MISD.Client.Managers;
using MISD.Core;
using System.Windows.Threading;
using System.Windows;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MISD.Client.Model.Resources;
using MISD.Client.Model.Synchronization;
using System.Windows.Shapes;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using MISD.Client.Model.Managers;
using System.Reflection;
using MISD.Client.Properties;
using System.ComponentModel;


namespace MISD.Client.Model
{
    /// <summary>
    /// This class contains the data model of the MISD client application.
    /// </summary>
    public class DataModel : BindableBase
    {
        #region Persistence

        /// <summary>
        /// Gets or sets the path to the level definitons.
        /// </summary>
        public string LevelDefinitionsPath
        {
            get
            {
                return System.IO.Path.GetDirectoryName(Application.Current.GetType().Assembly.Location) + System.IO.Path.DirectorySeparatorChar + "leveldefinitons.xml";
            }
        }

        public async Task Save()
        {
            try
            {
                StringWriter writer = new StringWriter();

                XmlSerializer serializer = new XmlSerializer(typeof(ExtendedObservableCollection<LevelDefinition>));
                serializer.Serialize(writer, this.LevelDefinitions);

                if (!Directory.Exists(System.IO.Path.GetDirectoryName(this.LevelDefinitionsPath)))
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(this.LevelDefinitionsPath));
                }

                using (FileStream stream = new FileStream(LevelDefinitionsPath, FileMode.Create, FileAccess.Write))
                {
                    using (StreamWriter streamWriter = new StreamWriter(stream))
                    {
                        await streamWriter.WriteAsync(writer.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                ClientLogger.Instance.WriteEntry("Unable to save level definitions to \"" + this.LevelDefinitionsPath + "\".", e, LogType.Exception);
            }
        }

        public void Load()
        {
            try
            {
                this.LevelDefinitions.CollectionChanged -= LevelDefinitions_CollectionChanged;

                string fileContents = null;
                using (FileStream stream = new FileStream(LevelDefinitionsPath, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader streamReader = new StreamReader(stream))
                    {
                        fileContents = streamReader.ReadToEnd();
                    }
                }

                StringReader reader = new StringReader(fileContents);
                XmlSerializer serializer = new XmlSerializer(typeof(ExtendedObservableCollection<LevelDefinition>));

                this.LevelDefinitions = (ExtendedObservableCollection<LevelDefinition>)serializer.Deserialize(reader);

                this.LevelDefinitions.CollectionChanged += LevelDefinitions_CollectionChanged;
            }
            catch (Exception e)
            {
                ClientLogger.Instance.WriteEntry("Unable to load level definitions from \"" + this.LevelDefinitionsPath + "\".", e, LogType.Exception);

                this.LevelDefinitions = new ExtendedObservableCollection<LevelDefinition>();

                this.LevelDefinitions.AddOnUI(new LevelDefinition() { ID = Guid.Parse("fd03e7b1-014a-47a4-969a-75821357e9f4"), HasStatusBar = false, UseCustomUI = false, Rows = 0 });
                this.LevelDefinitions.AddOnUI(new LevelDefinition() { ID = Guid.Parse("f8ff6ef9-3ccb-4eb0-bd1e-7b669633e05f"), HasStatusBar = true, UseCustomUI = false, Rows = 6 });
                this.LevelDefinitions.AddOnUI(new LevelDefinition() { ID = Guid.Parse("b417be3e-e241-453b-b601-dbb4d9c99b78"), HasStatusBar = false, UseCustomUI = true, Rows = 12 });

                this.Save();

                this.LevelDefinitions.CollectionChanged += LevelDefinitions_CollectionChanged;
            }
        }

        private async void LevelDefinitions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await this.Save();
        }

        #endregion

        #region Singleton

        private static volatile DataModel instance;
        private static object syncRoot = new Object();

        /// <summary>
        /// Initializes a new instance of the DataModel class.
        /// </summary>
        private DataModel()
        {
            Initialize();

            // Create the web service client
            this.RestoreWebServiceConnection();
        }

        /// <summary>
        /// Gets the singleton instance of this class.
        /// </summary>
        public static DataModel Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new DataModel();
                    }
                }

                return instance;
            }
        }

        #endregion

        #region Fields

        private ClientWebServiceClient clientWebService;
        private ExtendedObservableCollection<TileableElement> elements;
        private ExtendedObservableCollection<LevelDefinition> levelDefinitions;
        private ExtendedObservableCollection<Layout> layouts;
        private bool isSyncing = false;
        private bool showUI = false;
        private bool initalMSSyncDone = false;
        private Tuple<int, int, int, int, int> statusBarInfo;
        private ExtendedObservableCollection<Tuple<string, string>> ignoredMonitoredSystems;
        private ExtendedObservableCollection<MailUser> mailUsers;
        private ExtendedObservableCollection<Cluster> clusters;
        private ExtendedObservableCollection<String> clusterplatforms;
        private ExtendedObservableCollection<String> platforms;

        private WorkerThread monitoredSystemSyncThread;
        private WorkerThread pluginSyncThread;
        private WorkerThread settingsSyncThread;

        private CountdownEvent initMonitoredSystemSyncFinished = new CountdownEvent(1);
        private CountdownEvent initSettingsSyncFinished = new CountdownEvent(1);

        private List<OrganizationalUnit> remainingOusToAdd = new List<OrganizationalUnit>();

        #endregion

        #region Properties

        #region interval Properties


        public TimeSpan MonitoredSystemInterval
        {
            get
            {
                return (TimeSpan)Properties.Settings.Default["MonitoredSystemInterval"];

            }
            set
            {
                if (value != null)
                {
                    Properties.Settings.Default["MonitoredSystemInterval"] = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }

        }
        public TimeSpan PluginInterval
        {
            get
            {
                return (TimeSpan)Properties.Settings.Default["PluginInterval"];

            }
            set
            {
                if (value != null)
                {
                    Properties.Settings.Default["PluginInterval"] = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }

        }
        public TimeSpan SettingsInterval
        {
            get
            {
                return (TimeSpan)Properties.Settings.Default["SettingsInterval"];

            }
            set
            {
                if (value != null)
                {
                    Properties.Settings.Default["SettingsInterval"] = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }

        }

        public TimeSpan MainInterval
        {
            get
            {
                return (TimeSpan)MISD.Client.Properties.Settings.Default["ClientUpdateIntervall"];

            }
            set
            {
                if (value != null)
                {
                    Settings.Default["ClientUpdateIntervall"] = value;
                    Settings.Default.Save();
                    OnPropertyChanged();
                }
            }

        }


        #endregion


        #region clusters properties

        public ExtendedObservableCollection<String> ClusterPlatforms
        {
            get
            {
                if (clusterplatforms == null)
                {
                    clusterplatforms = new ExtendedObservableCollection<string>();
                }
                return clusterplatforms;
            }
            set
            {
                clusterplatforms = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public ExtendedObservableCollection<String> Platforms
        {
            get
            {
                if (platforms == null)
                {
                    platforms = new ExtendedObservableCollection<string>();
                }
                return platforms;
            }
            set
            {
                platforms = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the synchronization client that connects to the client web service.
        /// </summary>
        public ClientWebServiceClient ClientWebService
        {
            get
            {
                return this.clientWebService;
            }
            set
            {
                this.clientWebService = value;
            }
        }

        // depth of deepest Tile in Tree
        public int HierachieDepth
        {
            get
            {
                int depth = 0;
                List<ExtendedObservableCollection<TileableElement>> remainingElementsToAnalyze = new List<ExtendedObservableCollection<TileableElement>>();
                List<ExtendedObservableCollection<TileableElement>> remainingElementsToAnalyze2 = new List<ExtendedObservableCollection<TileableElement>>();
                remainingElementsToAnalyze.Add(elements);
                var hasElements = false;
                while (remainingElementsToAnalyze.Count > 0)
                {
                    while (remainingElementsToAnalyze.Count > 0)
                    {
                        foreach (var ouElement in remainingElementsToAnalyze.First())
                        {
                            var monitoredSystem = ouElement as MonitoredSystem;
                            if (monitoredSystem != null)
                            {
                                hasElements = true;
                                continue;
                            }

                            var ou = ouElement as OrganizationalUnit;
                            if (ou != null)
                            {
                                remainingElementsToAnalyze2.Add(ou.Elements);
                            }

                        }

                        remainingElementsToAnalyze.RemoveAt(0);
                    }
                    if (hasElements) { depth++; }
                    hasElements = false;
                    while (remainingElementsToAnalyze2.Count > 0)
                    {
                        foreach (var ouElement in remainingElementsToAnalyze2.First())
                        {
                            var monitoredSystem = ouElement as MonitoredSystem;
                            if (monitoredSystem != null)
                            {
                                hasElements = true;
                                continue;
                            }

                            var ou = ouElement as OrganizationalUnit;
                            if (ou != null)
                            {
                                remainingElementsToAnalyze.Add(ou.Elements);
                            }

                        }
                        remainingElementsToAnalyze2.RemoveAt(0);
                    }
                    if (hasElements) { depth++; }
                    hasElements = false;
                }
                return depth;
            }
        }

        // Name of the last changed Layout
        public string LastChangedLayout
        {
            get
            {
                try
                {
                    Layout lastChanged = null;
                    if (Layouts != null && Layouts.Count > 0)
                    {
                        lastChanged = Layouts.First();
                    }

                    if (lastChanged != null)
                    {
                        foreach (Layout l in Layouts)
                        {
                            if (l.Date < lastChanged.Date)
                            {
                                lastChanged = l;
                            }
                        }
                        return lastChanged.Name;
                    }
                    else
                    {
                        return Strings.NoChangeYet;
                    }
                }
                catch { return Strings.NoChangeYet; }
            }
            set
            {
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the displayable elements (e.g. monitored systems or organizational units).
        /// </summary>
        public ExtendedObservableCollection<TileableElement> Elements
        {
            get
            {
                if (this.elements == null) this.elements = new ExtendedObservableCollection<TileableElement>();
                return this.elements;
            }
            set
            {
                this.elements = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the level definitions.
        /// </summary>
        public ExtendedObservableCollection<LevelDefinition> LevelDefinitions
        {
            get
            {
                if (this.levelDefinitions == null) this.levelDefinitions = new ExtendedObservableCollection<LevelDefinition>();
                return this.levelDefinitions;
            }
            set
            {
                this.levelDefinitions = value;
                this.OnPropertyChanged();
            }
        }


        /// <summary>
        /// Gets or sets a value that indicates whether the model is currently in sync with the server.
        /// </summary>
        public bool IsSyncing
        {
            get
            {
                return this.isSyncing;
            }
            set
            {
                this.isSyncing = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the thread that is used to synchronize the monitored systems and ous.
        /// </summary>
        private WorkerThread MonitoredSystemSyncThread
        {
            get
            {
                return this.monitoredSystemSyncThread;
            }
            set
            {
                this.monitoredSystemSyncThread = value;
            }
        }

        /// <summary>
        /// Gets or sets the thread that is used to synchronize the plugins, indicators and indicator values.
        /// </summary>
        private WorkerThread PluginSyncThread
        {
            get
            {
                return this.pluginSyncThread;
            }
            set
            {
                this.pluginSyncThread = value;
            }
        }

        /// <summary>
        /// Gets or sets the thread that is used to synchronize the layout settings.
        /// </summary>
        private WorkerThread SettingsSyncThread
        {
            get
            {
                return this.settingsSyncThread;
            }
            set
            {
                this.settingsSyncThread = value;
            }
        }

        /// <summary>
        /// Gets or sets the list of Layouts stored on the client.
        /// </summary>
        public ExtendedObservableCollection<Layout> Layouts
        {
            get
            {
                if (this.layouts == null)
                {
                    this.layouts = new ExtendedObservableCollection<Layout>();
                }
                return this.layouts;
            }
            set
            {
                this.layouts = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Sets and gets the infos for the statusbar.
        /// Item1: Number of critical mapped monitored systems.
        /// Item2: Number of warning mapped monitored systems.
        /// Item3: Number of ok mapped monitored systems.
        /// Item4: Number of maintenance mapped monitored systems.
        /// Item5: Total number of monitored systems.
        /// </summary>
        public Tuple<int, int, int, int, int> StatusBarInfo
        {
            get
            {
                if (this.statusBarInfo == null)
                {
                    this.statusBarInfo = new Tuple<int, int, int, int, int>(0, 0, 0, 0, 0);
                }
                return this.statusBarInfo;
            }
            set
            {
                this.statusBarInfo = value;
                this.OnPropertyChanged();
            }
        }
        /// <summary>
        /// Determines if the ui is running as a Powerwall or not.
        /// </summary>
        public bool IsPowerwall
        {
            get
            {
                return ConfigClass.IsPowerwall;
            }
            set
            {
                ConfigClass.IsPowerwall = value;
            }
        }

        /// <summary>
        /// The width of the powerwall
        /// </summary>
        public double PowerwallWidth
        {
            get
            {
                return ConfigClass.PowerwallWidth;
            }
        }

        /// <summary>
        /// The height of the powerwall
        /// </summary>
        public double PowerwallHeight
        {
            get
            {
                return ConfigClass.PowerwallHeight;
            }
        }

        /// <summary>
        /// Determines if the ui is running as an Operator or not.
        /// </summary>
        public bool IsOperator
        {
            get
            {
                return ConfigClass.IsOperator;
            }
            set
            {
                ConfigClass.IsOperator = value;
            }
        }

        /// <summary>
        /// Determines whether the ui show the monitored systems or a gray background.
        /// </summary>
        public bool ShowUI
        {
            get
            {
                return this.showUI;
            }
            set
            {
                this.showUI = value;
                this.OnPropertyChanged();
            }
        }


        /// <summary>
        /// Gets and sets the monitoredsystems at the ignore list.
        /// It is an Tuple: MAC | Name.
        /// </summary>
        public ExtendedObservableCollection<Tuple<string, string>> IgnoredMonitoredSystems
        {
            get
            {
                if (this.ignoredMonitoredSystems == null)
                {
                    this.ignoredMonitoredSystems = new ExtendedObservableCollection<Tuple<string, string>>();
                }
                return this.ignoredMonitoredSystems;
            }
            set
            {
                this.ignoredMonitoredSystems = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets and sets the mail users.
        /// </summary>
        public ExtendedObservableCollection<MailUser> MailUsers
        {
            get
            {
                if (this.mailUsers == null)
                {
                    this.mailUsers = new ExtendedObservableCollection<MailUser>();
                }
                return this.mailUsers;
            }
            set
            {
                this.mailUsers = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets and sets the clusters.
        /// </summary>
        public ExtendedObservableCollection<Cluster> Clusters
        {
            get
            {
                if (this.clusters == null)
                {
                    this.clusters = new ExtendedObservableCollection<Cluster>();
                }
                return this.clusters;
            }
            set
            {
                this.clusters = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region ClientWebService

        public ClientWebServiceClient RestoreWebServiceConnection()
        {
            // Restore the connection.
            this.clientWebService = new ClientWebServiceClient();

            return this.clientWebService;
        }

        #endregion

        #region Synchronization

        /// <summary>
        /// Starts the synchronization. 
        /// </summary>
        public async Task StartSynchronization()
        {
            this.IsSyncing = false;

            // Load the level definitions
            this.Load();

            #region Open WebService Connection

            // Connect to the client web service, this shouldnt' be a problem, even if the server is not reachable.
            bool openedWebService = false;

            while (!openedWebService)
            {
                try
                {
                    this.ClientWebService.Open();
                    openedWebService = true;
                }
                catch (TimeoutException e2)
                {
                    // The client received a Timeout
                    this.IsSyncing = false;
                }
                catch (CommunicationException e3)
                {
                    // The client could not connect to the client web service
                    this.IsSyncing = false;
                }
                catch (Exception e)
                {
                    ClientLogger.Instance.WriteEntry("Unexpected exception occured", e, LogType.Exception);
                }

                // If Syncronization failed, wait some time
                if (!openedWebService)
                {
                    Thread.Sleep(1000);
                }
            }

            #endregion

            #region Initial Full Sync

            bool initialSyncFinished = false;

            while (!initialSyncFinished)
            {
                try
                {
                    // Clear all storages
                    this.Elements.ClearOnUI();
                    this.Layouts.ClearOnUI();

                    // Start loading the plugins asynchronously
                    await PluginManager.Instance.InitializeAsync();

                    // InitialSync Finsihed
                    initialSyncFinished = true;
                }
                catch (EndpointNotFoundException e1)
                {
                    // The client could not connect to the client web service
                    this.IsSyncing = false;
                }
                catch (TimeoutException e2)
                {
                    // The client received a Timeout
                    this.IsSyncing = false;
                }
                catch (CommunicationException e3)
                {
                    // The client could not connect to the client web service
                    this.IsSyncing = false;
                }
                catch (Exception e)
                {
                    ClientLogger.Instance.WriteEntry("Unexpected exception occured", e, LogType.Exception);
                }

                // If Syncronization failed, wait some time
                if (!initialSyncFinished)
                {
                    Thread.Sleep(1000);
                }
            }

            #endregion

            #region Start Continuous Synchronization

            bool continousSyncFinished = false;

            // The Powerwall client should not sync continous with the server.
            if (!ConfigClass.IsPowerwall)
            {
                while (!continousSyncFinished)
                {
                    try
                    {
                        // Before the continuous synchronization starts, we should let the plugins download completely
                        //var pluginFiles = await plugins;

                        // IMPORTANT: Make sure, that the Sync Thread is not running anymore
                        // and that it has been properly released - or you might want to recycle it

                        var pl = await this.ClientWebService.GetAllPlatformTypsAsync();
                        var plats = new ExtendedObservableCollection<string>();
                        foreach (var s in pl)
                        {
                            plats.Add(s);
                        }
                        Platforms = plats;

                        continousSyncFinished = true;
                    }
                    catch (EndpointNotFoundException e1)
                    {
                        // The client could not connect to the client web service
                        this.IsSyncing = false;
                    }
                    catch (TimeoutException e2)
                    {
                        // The client received a Timeout
                        this.IsSyncing = false;
                    }
                    catch (CommunicationException e3)
                    {
                        // The client could not connect to the client web service
                        this.IsSyncing = false;
                    }
                    catch (Exception e)
                    {
                        ClientLogger.Instance.WriteEntry("Unexpected exception occured", e, LogType.Exception);
                    }

                    // If Syncronization failed, wait some time
                    if (!continousSyncFinished)
                    {
                        Thread.Sleep(1000);
                    }

                }

                if (this.SettingsSyncThread == null)
                {
                    this.SettingsSyncThread = ThreadManager.CreateWorkerThread("SettingsSyncThread", this.SettingsSyncLoop, true);
                }
                if (this.MonitoredSystemSyncThread == null)
                {
                    this.MonitoredSystemSyncThread = ThreadManager.CreateWorkerThread("MonitoredSystemSyncThread", this.MonitoredSystemSyncLoop, true);
                }
                if (this.PluginSyncThread == null)
                {
                    this.PluginSyncThread = ThreadManager.CreateWorkerThread("PluginSyncThread", this.PluginSyncLoop, true);
                }
            }

            #endregion
        }

        private volatile bool monitoredSystemSyncLoop_shouldStop = false;
        private volatile bool pluginSyncLoop_shouldStop = false;
        private volatile bool settingsSyncpLoop_shouldStop = false;

        /// <summary>
        /// Syncs the monitoredSystems and also the ignored list.
        /// </summary>
        public void MonitoredSystemSyncLoop()
        {
            try
            {
                // Waiting for the initial sync of the settings
                this.initSettingsSyncFinished.Wait();

                #region Sync OUs

                // Download Ous
                var downloadedOrganizsationlUnits = this.ClientWebService.GetAllOUs();
                // Syncing was successfull
                this.IsSyncing = true;

                if (downloadedOrganizsationlUnits != null)
                {
                    // Analyze current ous
                    IEnumerable<OrganizationalUnit> currentOUs = this.Elements.GetOrganizationalUnits();
                    List<OrganizationalUnit> ousToAdd = new List<OrganizationalUnit>();
                    List<OrganizationalUnit> ousToRemove = new List<OrganizationalUnit>();
                    foreach (Tuple<int, string, string, int?, DateTime?> tuple in downloadedOrganizsationlUnits)
                    {
                        bool ouFound = false;
                        foreach (OrganizationalUnit ou in currentOUs)
                        {
                            if (ou.ID == -1 && ou.Name == tuple.Item2 && ou.ParentID == tuple.Item4)
                            {
                                ouFound = true;
                                break;
                            }
                            else if (ou.ID == tuple.Item1)
                            {
                                ouFound = true;

                                // If last update of the ou at the server is more up to date.
                                if (tuple.Item5 != null && ou.LastUpdate < tuple.Item5)
                                {
                                    ou.Name = tuple.Item2;
                                    ou.FQDN = tuple.Item3;
                                    ou.LastUpdate = tuple.Item5;

                                    // OU position has changed
                                    if (ou.ParentID != tuple.Item4)
                                    {
                                        ou.ParentID = tuple.Item4;
                                    }
                                }

                                break;
                            }
                        }

                        if (!ouFound)
                        {
                            new OrganizationalUnit(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, new ExtendedObservableCollection<TileableElement>(), tuple.Item5);
                        }
                    }

                    // Analyze ous which are not in the new ou list.
                    foreach (OrganizationalUnit ou in currentOUs)
                    {
                        bool ouFound = false;
                        foreach (Tuple<int, string, string, int?, DateTime?> tuple in downloadedOrganizsationlUnits)
                        {
                            if (tuple.Item1 == ou.ID || ou.ID == -1)
                            {
                                ouFound = true;
                                break;
                            }

                        }
                        if (!ouFound)
                        {
                            ousToRemove.Add(ou);
                        }
                    }

                    // Remove old Ous
                    foreach (OrganizationalUnit ouToRemove in ousToRemove)
                    {
                        IEnumerable<OrganizationalUnit> allOUs = this.Elements.GetOrganizationalUnits();

                        if (ouToRemove.ParentID == null)
                        {
                            this.Elements.RemoveOnUI(ouToRemove);
                        }
                        else
                        {
                            foreach (OrganizationalUnit currentOu in allOUs)
                            {
                                if (currentOu.ID == ouToRemove.ParentID)
                                {
                                    currentOu.Elements.RemoveOnUI(ouToRemove);
                                    break;
                                }
                            }

                        }
                    }

                    //raise changed event
                    OnOrganisationUnitsChanged(new EventArgs());
                }

                #endregion

                #region Sync MonitoredSystems

                // Download the monitored Systems
                var x = ClientWebService.GetMonitoredSystemIDs();
                List<WorkstationInfo> downloadedMonitoredSystemInfos = ClientWebService.GetMonitoredSystemInfo((from p in x select new Tuple<int, TimeSpan>(p, TimeSpan.FromDays(100))).ToList());

                // Syncing was successfull
                this.IsSyncing = true;
                OrganizationalUnit def = null;
                foreach (var ou in this.Elements.GetOrganizationalUnits())
                {
                    if (ou.Name == "Default")
                    {
                        def = ou;
                    }
                }

                if (downloadedMonitoredSystemInfos != null)
                {
                    // Analyze current monitored systems
                    IEnumerable<MonitoredSystem> currentMonitoredSystems = this.Elements.GetMonitoredSystems();
                    List<MonitoredSystem> monitoredSystemsToAdd = new List<MonitoredSystem>();
                    List<MonitoredSystem> monitoredSystemsToRemove = new List<MonitoredSystem>();
                    foreach (WorkstationInfo msInfo in downloadedMonitoredSystemInfos)
                    {
                        bool monitoredSystemFound = false;
                        foreach (MonitoredSystem ms in currentMonitoredSystems)
                        {
                            if (ms.MAC == msInfo.MacAddress)
                            {
                                monitoredSystemFound = true;

                                if (msInfo.LastUpdate != null && ms.LastUpdate < msInfo.LastUpdate)
                                {
                                    // Update this monitoredSystem, but do not remove the current Plugins
                                    ms.Name = msInfo.Name;
                                    ms.CurrentPlatform = msInfo.CurrentOS;
                                    ms.ID = msInfo.ID;
                                    ms.IsAvailable = msInfo.IsAvailable;
                                    ms.State = msInfo.State;
                                    ms.LastUpdate = msInfo.LastUpdate;
                                    // Position changed.
                                    if (ms.OuID != msInfo.OuID)
                                    {
                                        ms.OuID = msInfo.OuID;
                                    }

                                }

                                break;
                            }
                        }

                        if (!monitoredSystemFound)
                        {
                            new MonitoredSystem(
                                 msInfo.Name,
                                 msInfo.ID,
                                 msInfo.MacAddress,
                                 msInfo.FQDN,
                                 msInfo.State,
                                 null,
                                 new ExtendedObservableCollection<Plugin>(),
                                 msInfo.CurrentOS,
                                 msInfo.IsAvailable,
                                 msInfo.OuID,
                                 msInfo.LastUpdate);



                        }
                    }

                    // Analyze monitored systems which are not in the downloaded list.
                    foreach (MonitoredSystem ms in currentMonitoredSystems)
                    {
                        bool monitoredSystemFound = false;
                        foreach (WorkstationInfo msInfo in downloadedMonitoredSystemInfos)
                        {
                            if (msInfo.MacAddress == ms.MAC)
                            {
                                monitoredSystemFound = true;
                                break;
                            }

                        }
                        if (!monitoredSystemFound)
                        {
                            monitoredSystemsToRemove.Add(ms);
                        }
                    }

                    // Remove monitored systems
                    foreach (MonitoredSystem msToRemove in monitoredSystemsToRemove)
                    {
                        IEnumerable<OrganizationalUnit> allOUs = this.Elements.GetOrganizationalUnits();

                        foreach (OrganizationalUnit currentOu in allOUs)
                        {
                            if (currentOu.ID == msToRemove.OuID)
                            {
                                try
                                {
                                    currentOu.Elements.RemoveOnUI(msToRemove);
                                }
                                catch (Exception e)
                                {
                                    ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
                                }
                                break;
                            }
                        }
                    }

                    //raise changed event
                    OnMonitoredSystemsChanged(new EventArgs());
                }

                #endregion

                // Update statusbar.
                this.UpdateStatusBarInfo();

                #region Update IgnoredList

                // Download
                List<Tuple<string, string>> downloadedIgnoredMonitoredSystems = this.ClientWebService.GetIgnoredMonitoredSystems();

                // Syncing was successfull
                this.IsSyncing = true;

                // Update
                if (downloadedIgnoredMonitoredSystems != null)
                {
                    var msToAdd = new List<Tuple<string, string>>();
                    var msToRemove = new List<Tuple<string, string>>();

                    foreach (Tuple<string, string> tuple in downloadedIgnoredMonitoredSystems)
                    {
                        bool msFound = false;

                        foreach (Tuple<string, string> currentMonitoredSystem in this.IgnoredMonitoredSystems)
                        {
                            if (currentMonitoredSystem.Item1 == tuple.Item1)
                            {
                                msFound = true;

                                if (currentMonitoredSystem.Item2 != tuple.Item2)
                                {
                                    msToRemove.Add(currentMonitoredSystem);
                                    msToAdd.Add(tuple);
                                }

                                break;
                            }
                        }

                        if (!msFound)
                        {
                            msToAdd.Add(tuple);
                        }
                    }

                    foreach (Tuple<string, string> currentMonitoredSystem in this.IgnoredMonitoredSystems)
                    {
                        bool msFound = false;

                        foreach (Tuple<string, string> tuple in downloadedIgnoredMonitoredSystems)
                        {
                            if (tuple.Item1 == currentMonitoredSystem.Item1)
                            {
                                msFound = true;
                                break;
                            }
                        }

                        if (!msFound)
                        {
                            msToRemove.Add(currentMonitoredSystem);
                        }
                    }

                    foreach (Tuple<string, string> ms in msToRemove)
                    {
                        this.IgnoredMonitoredSystems.RemoveOnUI(ms);
                    }

                    foreach (Tuple<string, string> ms in msToAdd)
                    {
                        this.IgnoredMonitoredSystems.AddOnUI(ms);
                    }

                }

                #endregion

                // Signal the pluginSyncLoop to start.
                if (initMonitoredSystemSyncFinished.CurrentCount > 0)
                {
                    this.initMonitoredSystemSyncFinished.Signal();
                }
            }
            catch (EndpointNotFoundException e1)
            {
                // The client could not connect to the client web service
                this.IsSyncing = false;
                //ClientLogger.Instance.WriteEntry("WebService Connection failed", e1, LogType.Exception);
            }
            catch (TimeoutException e2)
            {
                // The client received a Timeout
                this.IsSyncing = false;
                //ClientLogger.Instance.WriteEntry("Received Timeout from WebService", e2, LogType.Exception);
            }
            catch (CommunicationException e3)
            {
                // The client could not connect to the client web service
                this.IsSyncing = false;
                //ClientLogger.Instance.WriteEntry("WebService Connection failed", e3, LogType.Exception);
            }

            // Wait for the interval.
            Thread.Sleep(Properties.Settings.Default.MonitoredSystemInterval);
        }

        private bool powerwallSyncStarted = false;

        /// <summary>
        /// Syncs the pugins, indicators and indicator values.
        /// </summary>
        public void PluginSyncLoop()
        {
            try
            {
                // Wait for the initial sync of monitored systems
                this.initMonitoredSystemSyncFinished.Wait();

                #region Download current plugins

                List<Tuple<string, List<Plugin>>> currentPluginsPerstring = new List<Tuple<string, List<Plugin>>>();
                foreach (string platform in Platforms)
                {
                    if (platform != "Visualization")
                    {
                        var pluginList = ClientWebService.GetPluginList(platform);
                        var currentPlugins = (from q in pluginList
                                              select new Plugin() { Company = q.Company, Copyright = q.Copyright, Description = q.Description, FileName = q.FileName, Name = q.Name, Product = q.Product, Version = q.Version, Platform = platform }).ToList();
                        currentPluginsPerstring.Add(new Tuple<string, List<Plugin>>((string)platform, currentPlugins));
                    }
                }


                // Syncing was successfull
                this.IsSyncing = true;

                #endregion

                #region Update Plugins of each monitored System

                foreach (MonitoredSystem monitoredSystem in this.Elements.GetMonitoredSystems())
                {
                    //var monitoredSystem = ouElement as MonitoredSystem;
                    // Only monitored Systems should be updated, no OUs
                    if (monitoredSystem != null)
                    {
                        List<Plugin> pluginsToRemove = new List<Plugin>();
                        List<Plugin> pluginsToAdd = new List<Plugin>();

                        // Analyze the up-to-dateness of the local plugin
                        foreach (Tuple<string, List<Plugin>> tuple in currentPluginsPerstring)
                        {
                            if (tuple.Item1 == monitoredSystem.CurrentPlatform || tuple.Item1 == Platform.Server.ToString())
                            {
                                // Analyze which plugin must be added or replaced.
                                foreach (Plugin newPlugin in tuple.Item2)
                                {
                                    Boolean oldPluginFound = false;
                                    foreach (Plugin oldPlugin in monitoredSystem.Plugins)
                                    {
                                        if (newPlugin.Name.ToLower() == oldPlugin.Name.ToLower())
                                        {
                                            oldPluginFound = true;

                                            if (newPlugin.Version > oldPlugin.Version)
                                            {

                                                oldPlugin.Company = newPlugin.Company;
                                                oldPlugin.Copyright = newPlugin.Copyright;
                                                oldPlugin.Description = newPlugin.Description;
                                                oldPlugin.FileName = newPlugin.FileName;
                                                oldPlugin.Indicators = oldPlugin.Indicators;
                                                oldPlugin.MainValue = newPlugin.MainValue;
                                                oldPlugin.Name = newPlugin.Name;
                                                oldPlugin.Platform = newPlugin.Platform;
                                                oldPlugin.Product = newPlugin.Product;
                                                oldPlugin.Version = (Version)newPlugin.Version.Clone();

                                            }

                                            break;
                                        }
                                    }

                                    if (!oldPluginFound)
                                    {
                                        Plugin plugin = new Plugin()
                                        {
                                            Company = newPlugin.Company,
                                            Copyright = newPlugin.Copyright,
                                            Description = newPlugin.Description,
                                            FileName = newPlugin.FileName,
                                            MainValue = newPlugin.MainValue,
                                            Name = newPlugin.Name,
                                            Platform = newPlugin.Platform,
                                            Product = newPlugin.Product,
                                            Version = (Version)newPlugin.Version.Clone(),
                                        };
                                        pluginsToAdd.Add(plugin);
                                    }
                                }

                                // Analyse which plugins has to be removed
                                foreach (Plugin oldPlugin in monitoredSystem.Plugins)
                                {
                                    bool newPluginFound = false;

                                    // Check the native plugins and the plugins from the server.
                                    foreach (Tuple<string, List<Plugin>> newTuple in currentPluginsPerstring)
                                    {
                                        if (newTuple.Item1 == monitoredSystem.CurrentPlatform || newTuple.Item1 == Platform.Server.ToString())
                                        {
                                            foreach (Plugin newPlugin in newTuple.Item2)
                                            {
                                                if (newPlugin.Name == oldPlugin.Name)
                                                {
                                                    newPluginFound = true;
                                                    break;
                                                }
                                            }

                                            if (newPluginFound)
                                            {
                                                break;
                                            }
                                        }
                                    }

                                    if (!newPluginFound)
                                    {
                                        pluginsToRemove.Add(oldPlugin);
                                    }
                                }

                            }
                        }

                        // Update the plugins
                        foreach (Plugin oldPlugin in pluginsToRemove)
                        {
                            monitoredSystem.Plugins.RemoveOnUI(oldPlugin);
                        }
                        foreach (Plugin newPlugin in pluginsToAdd)
                        {
                            monitoredSystem.Plugins.AddOnUI(newPlugin);
                        }

                    }
                }

                #endregion

                #region Update current indicators

                foreach (MonitoredSystem monitoredSystem in this.Elements.GetMonitoredSystems())
                {
                    //var monitoredSystem = ouElement as MonitoredSystem;
                    // Only monitored Systems should be updated, no OUs
                    if (monitoredSystem != null)
                    {
                        // Download all indicator Settings for this monitored System.
                        List<IndicatorSettings> downloadedIndicatorSettings = this.ClientWebService.GetIndicatorSetting(monitoredSystem.MAC);

                        // Syncing was successfull
                        this.IsSyncing = true;

                        if (downloadedIndicatorSettings != null)
                        {
                            // Analyze the up-to-dateness of the local indicators
                            List<Indicator> indicatorsToAdd = new List<Indicator>();
                            List<Indicator> indicatorsToRemove = new List<Indicator>();

                            // Analyze which indicator must be added or replaced.
                            foreach (IndicatorSettings indicatorSetting in downloadedIndicatorSettings)
                            {
                                bool pluginFound = false;
                                bool foundIndicatorInPlugin = false;

                                foreach (Plugin plugin in monitoredSystem.Plugins)
                                {
                                    if (plugin.Name == indicatorSetting.PluginName)
                                    {
                                        pluginFound = true;

                                        foreach (Indicator oldIndicator in plugin.Indicators)
                                        {
                                            if (oldIndicator.Name == indicatorSetting.IndicatorName)
                                            {
                                                foundIndicatorInPlugin = true;

                                                // Replace indicator only if something changed
                                                if (oldIndicator.StatementCritical != indicatorSetting.MetricCritical ||
                                                    oldIndicator.StatementWarning != indicatorSetting.MetricWarning ||
                                                    oldIndicator.DataType != indicatorSetting.DataType ||
                                                    oldIndicator.FilterStatement != indicatorSetting.FilterStatement ||
                                                    oldIndicator.MappingDuration != indicatorSetting.MappingDuration ||
                                                    oldIndicator.StorageDuration != indicatorSetting.StorageDuration ||
                                                    oldIndicator.IndicatorMapping != indicatorSetting.IndicatorMapping ||
                                                    oldIndicator.UpdateInterval != indicatorSetting.UpdateInterval)
                                                {
                                                    oldIndicator.Name = indicatorSetting.IndicatorName;
                                                    oldIndicator.PluginName = indicatorSetting.PluginName;
                                                    oldIndicator.FilterStatement = indicatorSetting.FilterStatement;
                                                    oldIndicator.UpdateInterval = indicatorSetting.UpdateInterval;
                                                    oldIndicator.StorageDuration = indicatorSetting.StorageDuration;
                                                    oldIndicator.MappingDuration = indicatorSetting.MappingDuration;
                                                    oldIndicator.DataType = indicatorSetting.DataType;
                                                    oldIndicator.StatementCritical = indicatorSetting.MetricCritical;
                                                    oldIndicator.StatementWarning = indicatorSetting.MetricWarning;
                                                    oldIndicator.IndicatorValues = oldIndicator.IndicatorValues;
                                                    oldIndicator.MonitoredSystemMAC = indicatorSetting.MonitoredSystemMAC;
                                                    oldIndicator.IndicatorMapping = indicatorSetting.IndicatorMapping;

                                                }
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                }

                                if (pluginFound && !foundIndicatorInPlugin)
                                {
                                    // Build new indicator and save IndicatorValues
                                    Indicator newIndicator = new Indicator(
                                        indicatorSetting.IndicatorName,
                                        indicatorSetting.PluginName,
                                        indicatorSetting.FilterStatement,
                                        indicatorSetting.UpdateInterval,
                                        indicatorSetting.StorageDuration,
                                        indicatorSetting.MappingDuration,
                                        indicatorSetting.DataType,
                                        indicatorSetting.MetricWarning,
                                        indicatorSetting.MetricCritical,
                                        new ExtendedObservableCollection<IndicatorValue>(),
                                        indicatorSetting.MonitoredSystemMAC,
                                        indicatorSetting.IndicatorMapping);

                                    indicatorsToAdd.Add(newIndicator);
                                }
                            }

                            // Analyze which indicator must be removed
                            foreach (Plugin plugin in monitoredSystem.Plugins)
                            {
                                foreach (Indicator oldIndicator in plugin.Indicators)
                                {
                                    bool indicatorFound = false;

                                    foreach (IndicatorSettings newIndicatorSettings in downloadedIndicatorSettings)
                                    {
                                        if (newIndicatorSettings.PluginName == plugin.Name && newIndicatorSettings.IndicatorName == oldIndicator.Name)
                                        {
                                            indicatorFound = true;
                                            break;
                                        }
                                    }

                                    if (!indicatorFound)
                                    {
                                        indicatorsToRemove.Add(oldIndicator);
                                    }
                                }
                            }

                            // Update the indicators
                            foreach (Indicator indicator in indicatorsToRemove)
                            {
                                foreach (Plugin plugin in monitoredSystem.Plugins)
                                {
                                    if (plugin.Name == indicator.PluginName)
                                    {
                                        plugin.Indicators.RemoveOnUI(indicator);
                                        break;
                                    }
                                }
                            }
                            foreach (Indicator indicator in indicatorsToAdd)
                            {
                                foreach (Plugin plugin in monitoredSystem.Plugins)
                                {
                                    if (plugin.Name == indicator.PluginName)
                                    {
                                        plugin.Indicators.AddOnUI(indicator);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }


                #endregion

                // Now show the ui, if it is shown grayed.
                this.ShowUI = true;

                // Start the sync with the powerwall (will only works if the clent is started as operator).
                if (!powerwallSyncStarted)
                {
                    DataModelManager.Instance.StartPowerwallSync();
                    powerwallSyncStarted = true;
                }

                #region Update IndicatorValues

                // Build the List with the monitoredSystem IDs
                IEnumerable<MonitoredSystem> currentMonitoredSystems = this.Elements.GetMonitoredSystems();
                List<string> monitoredSystemMACs = new List<string>();
                List<Tuple<string, string, string, string, MappingState, DateTime>> downloadedIndicatorValues = new List<Tuple<string, string, string, string, MappingState, DateTime>>();
                var pluginDataTasks = new List<Task<List<Tuple<string, string, string, string, MappingState, DateTime>>>>();

                int i = 1;
                foreach (MonitoredSystem monitoredSystem in currentMonitoredSystems)
                {
                    monitoredSystemMACs.Add(monitoredSystem.MAC);

                    // just get data for two monitored systems for timeout reasons
                    if (monitoredSystemMACs.Count % 10 == 0)
                    {
                        var values = this.ClientWebService.GetLatestMonitoredSystemData(monitoredSystemMACs);

                        // update data model
                        foreach (MonitoredSystem ms in currentMonitoredSystems)
                        {
                            #region set indicator Values
                            foreach (Tuple<string, string, string, string, MappingState, DateTime> tuple in values)
                            {
                                if (ms.MAC == tuple.Item1)
                                {
                                    foreach (Plugin plugin in ms.Plugins)
                                    {
                                        if (plugin.Name == tuple.Item2)
                                        {
                                            foreach (Indicator indicator in plugin.Indicators)
                                            {
                                                if (indicator.Name == tuple.Item3)
                                                {
                                                    if (indicator.CurrentValue == null || tuple.Item6 > indicator.CurrentValue.Timestamp)
                                                    {
                                                        IndicatorValue newValue = new IndicatorValue(tuple.Item4, indicator.DataType, tuple.Item6, tuple.Item5);
                                                        indicator.CurrentValue = newValue;
                                                        indicator.IndicatorValues.Insert(0, newValue);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                            #region mainvalue calculation
                            if (ms != null)
                            {
                                foreach (Plugin plugin in ms.Plugins)
                                {

                                    foreach (IPluginVisualization visPlugin in PluginManager.Instance.GetLoadedPlugins())
                                    {
                                        if (visPlugin != null && visPlugin.Name.ToLower() == plugin.Name.ToLower())
                                        {
                                            try
                                            {
                                                string res = visPlugin.CalculateMainValue(plugin.Indicators);
                                                plugin.MainValue = res;
                                            }
                                            catch (Exception)
                                            {

                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                        }

                        monitoredSystemMACs.Clear();
                    }
                    i++;
                }

                if (monitoredSystemMACs.Count > 0)
                {
                    var values = this.ClientWebService.GetLatestMonitoredSystemData(monitoredSystemMACs);

                    // update data model
                    foreach (MonitoredSystem ms in currentMonitoredSystems)
                    {
                        #region set indicator Values
                        foreach (Tuple<string, string, string, string, MappingState, DateTime> tuple in values)
                        {
                            if (ms.MAC == tuple.Item1)
                            {
                                foreach (Plugin plugin in ms.Plugins)
                                {
                                    if (plugin.Name == tuple.Item2)
                                    {
                                        foreach (Indicator indicator in plugin.Indicators)
                                        {

                                            if (indicator.Name == tuple.Item3)
                                            {
                                                IndicatorValue newValue = new IndicatorValue(tuple.Item4, indicator.DataType, tuple.Item6, tuple.Item5);

                                                indicator.CurrentValue = newValue;
                                                indicator.IndicatorValues.Insert(0, newValue);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                        #region mainvalue calculation
                        if (ms != null)
                        {
                            foreach (Plugin plugin in ms.Plugins)
                            {

                                foreach (IPluginVisualization visPlugin in PluginManager.Instance.GetLoadedPlugins())
                                {
                                    if (visPlugin != null && visPlugin.Name.ToLower() == plugin.Name.ToLower())
                                    {
                                        try
                                        {
                                            string res = visPlugin.CalculateMainValue(plugin.Indicators);
                                            plugin.MainValue = res;
                                        }
                                        catch (Exception)
                                        {

                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                }

                // Syncing was successfull
                this.IsSyncing = true;

                #endregion

                #region Update mapping states
                foreach (MonitoredSystem ms in currentMonitoredSystems)
                {
                    foreach (Plugin p in ms.Plugins)
                    {
                        MappingState worstMapping = MappingState.OK;
                        foreach (Indicator indi in p.Indicators)
                        {
                            foreach (IndicatorValue value in indi.IndicatorValues)
                            {
                                if (value.MappingState > worstMapping && value.MappingState != MappingState.Maintenance)
                                {
                                    worstMapping = value.MappingState;
                                }
                            }
                        }

                        // set plugin's mapping state to worst found
                        p.CurrentMapping = worstMapping;
                    }
                }
                #endregion
            }
            catch (EndpointNotFoundException e1)
            {
                // The client could not connect to the client web service
                this.IsSyncing = false;
                //ClientLogger.Instance.WriteEntry("WebService Connection failed", e1, LogType.Exception);
            }
            catch (TimeoutException e2)
            {
                // The client received a Timeout
                this.IsSyncing = false;
                //ClientLogger.Instance.WriteEntry("Received Timeout from WebService", e2, LogType.Exception);
            }
            catch (CommunicationException e3)
            {
                // The client could not connect to the client web service
                this.IsSyncing = false;
                //ClientLogger.Instance.WriteEntry("WebService Connection failed", e3, LogType.Exception);
            }

            // Wait for the interval.
            Thread.Sleep(Properties.Settings.Default.PluginInterval);
        }

        /// <summary>
        /// Syncs the layouts and mail uers.
        /// </summary>
        public void SettingsSyncLoop()
        {
            try
            {
                #region Layouts

                // Download layouts
                var downloadedLayoutstoCast = clientWebService.GetUIConfigurationList();
                var downloadedLayouts = new List<MISD.Client.Model.Layout>();

                // Syncing was successfull
                this.IsSyncing = true;

                if (downloadedLayoutstoCast != null)
                {
                    // Analyze local layouts
                    List<Layout> layoutsToAdd = new List<Layout>();
                    List<Layout> layoutsToRemove = new List<Layout>();

                    // Analyze which layouts must be added or replaced.
                    foreach (MISD.Core.Layout layout in downloadedLayoutstoCast)
                    {
                        Layout newLayout = LayoutManager.Instance.DeserializeBase64(layout.Data) as Layout;
                        newLayout.UserName = layout.UserName;
                        newLayout.Date = layout.Date;
                        newLayout.ID = layout.ID;
                        newLayout.PreviewImage = layout.PreviewImage;
                        newLayout.Name = layout.Name;
                        newLayout.Data = layout.Data;
                        downloadedLayouts.Add(newLayout);

                        bool layoutFound = false;
                        foreach (Layout oldLayout in this.Layouts)
                        {
                            if (oldLayout.ID == newLayout.ID)
                            {
                                layoutFound = true;

                                if (oldLayout.Date < newLayout.Date)
                                {
                                    layoutsToRemove.Add(oldLayout);
                                    layoutsToAdd.Add(newLayout);
                                }

                                break;
                            }
                        }

                        if (!layoutFound)
                        {
                            layoutsToAdd.Add(newLayout);
                        }
                    }

                    // Analyze which plugin must be removed.
                    foreach (Layout oldLayout in this.Layouts)
                    {
                        bool layoutFound = false;

                        foreach (Layout newLayout in downloadedLayouts)
                        {
                            if (newLayout.ID == oldLayout.ID)
                            {
                                layoutFound = true;
                                break;
                            }
                        }

                        if (!layoutFound)
                        {
                            layoutsToRemove.Add(oldLayout);
                        }
                    }

                    // Update the layouts.
                    foreach (Layout oldLayout in layoutsToRemove)
                    {
                        this.Layouts.RemoveOnUI(oldLayout);
                    }
                    foreach (Layout newLayout in layoutsToAdd)
                    {
                        this.Layouts.AddOnUI(newLayout);
                    }
                }

                #endregion

                #region MailUsers

                // Download the MailUsers
                var downloadedMailUsers = this.ClientWebService.GetAllMailData();

                // Syncing was successfull
                this.IsSyncing = true;

                if (downloadedMailUsers != null)
                {
                    // Analyze the old MailUsers
                    List<MailUser> mailUsersToAdd = new List<MailUser>();
                    List<MailUser> mailUsersToRemove = new List<MailUser>();

                    foreach (Tuple<int, string, string, bool> tuple in downloadedMailUsers)
                    {
                        bool mailUserFound = false;

                        foreach (MailUser currentMailUser in this.MailUsers)
                        {
                            if (currentMailUser.ID == -1 && currentMailUser.Name == tuple.Item2)
                            {
                                mailUserFound = true;
                                break;
                            }
                            else if (currentMailUser.ID == tuple.Item1)
                            {
                                mailUserFound = true;

                                currentMailUser.Name = tuple.Item2;
                                currentMailUser.Email = tuple.Item3;
                                currentMailUser.DailyMail = tuple.Item4;

                                break;
                            }
                        }

                        if (!mailUserFound)
                        {
                            mailUsersToAdd.Add(new MailUser(
                                tuple.Item1,
                                tuple.Item2,
                                tuple.Item3,
                                tuple.Item4,
                                new ExtendedObservableCollection<Tuple<int, string, string, string>>()));
                        }
                    }

                    foreach (MailUser currentMailUser in this.MailUsers)
                    {
                        bool mailUserFound = false;

                        foreach (Tuple<int, string, string, bool> tuple in downloadedMailUsers)
                        {
                            if (tuple.Item1 == currentMailUser.ID || currentMailUser.ID == -1)
                            {
                                mailUserFound = true;
                                break;
                            }
                        }

                        if (!mailUserFound)
                        {
                            mailUsersToRemove.Add(currentMailUser);
                        }
                    }

                    // Update the MailUsers
                    foreach (MailUser mailUserToRemove in mailUsersToRemove)
                    {
                        this.MailUsers.RemoveOnUI(mailUserToRemove);
                    }
                    foreach (MailUser mailUserToAdd in mailUsersToAdd)
                    {
                        this.MailUsers.AddOnUI(mailUserToAdd);
                    }

                    // Update registered monitored systems
                    foreach (MailUser currentMailUser in this.MailUsers)
                    {
                        var downloadedMonitoredSystems = this.ClientWebService.GetObserver(currentMailUser.ID);
                        var msToAdd = new List<Tuple<int, string, string, string>>();
                        var msToRemove = new List<Tuple<int, string, string, string>>();

                        // Syncing was successfull
                        this.IsSyncing = true;

                        if (downloadedMonitoredSystems != null)
                        {

                            foreach (WorkstationInfo msInfo in downloadedMonitoredSystems)
                            {
                                bool msInfoFound = false;

                                foreach (Tuple<int, string, string, string> currentMonitoredSystem in currentMailUser.RegisteredMonitoredSystems)
                                {
                                    if (currentMonitoredSystem.Item1 == msInfo.ID)
                                    {
                                        msInfoFound = true;

                                        msToRemove.Add(currentMonitoredSystem);
                                        msToAdd.Add(new Tuple<int, string, string, string>(msInfo.ID, msInfo.MacAddress, msInfo.Name, msInfo.FQDN));

                                        break;
                                    }
                                }

                                if (!msInfoFound)
                                {
                                    msToAdd.Add(new Tuple<int, string, string, string>(msInfo.ID, msInfo.MacAddress, msInfo.Name, msInfo.FQDN));
                                }
                            }

                            foreach (Tuple<int, string, string, string> currentMonitoredSystem in currentMailUser.RegisteredMonitoredSystems)
                            {
                                bool msFound = false;

                                foreach (WorkstationInfo msInfo in downloadedMonitoredSystems)
                                {
                                    if (msInfo.MacAddress == currentMonitoredSystem.Item2)
                                    {
                                        msFound = true;
                                        break;
                                    }
                                }

                                if (!msFound)
                                {
                                    msToRemove.Add(currentMonitoredSystem);
                                }
                            }

                            foreach (Tuple<int, string, string, string> ms in msToRemove)
                            {
                                currentMailUser.RegisteredMonitoredSystems.Remove(ms);
                            }

                            foreach (Tuple<int, string, string, string> ms in msToAdd)
                            {
                                currentMailUser.RegisteredMonitoredSystems.Add(ms);
                            }
                        }
                    }
                }

                #endregion

                #region Clusters

                var dowloadedTypes = this.ClientWebService.GetClusterTyps();

                List<string> typesToRemove = new List<string>();
                List<string> typesToAdd = new List<string>();

                if (dowloadedTypes != null)
                {
                    // Analyze exsiting types
                    foreach (string newtype in dowloadedTypes)
                    {
                        bool typesFound = false;

                        foreach (string currentType in this.ClusterPlatforms)
                        {
                            if (currentType.Equals(newtype))
                            {
                                typesFound = true;
                                break;
                            }
                        }

                        if (!typesFound)
                        {
                            typesToAdd.Add(newtype);
                        }
                    }

                    foreach (string currentType in this.ClusterPlatforms)
                    {
                        bool typesFound = false;

                        foreach (string newtype in dowloadedTypes)
                        {
                            if (currentType.Equals(newtype))
                            {
                                typesFound = true;
                                break;
                            }

                        }
                        if (!typesFound)
                        {
                            typesToRemove.Add(currentType);
                        }
                    }

                }

                // Update DataModel.Instance.Clusters
                foreach (string type in typesToRemove)
                {
                    this.ClusterPlatforms.RemoveOnUI(type);
                }

                foreach (string type in typesToAdd)
                {
                    this.ClusterPlatforms.AddOnUI(type);
                }
                var downloadedClusters = this.ClientWebService.GetClusters();

                // Syncing was successfull
                this.IsSyncing = true;

                List<Cluster> clustersToRemove = new List<Cluster>();
                List<Cluster> clustersToAdd = new List<Cluster>();

                if (downloadedClusters != null)
                {
                    // Analyze exsiting clusters
                    foreach (Tuple<int, string, string, string, string> newCluster in downloadedClusters)
                    {
                        bool clusterFound = false;

                        foreach (Cluster currentCluster in this.Clusters)
                        {
                            if (currentCluster.ID == newCluster.Item1)
                            {
                                clusterFound = true;

                                currentCluster.HeadnodeAddress = newCluster.Item2;
                                currentCluster.UserName = newCluster.Item3;
                                currentCluster.Password = newCluster.Item4;
                                currentCluster.CurrentPlatform = newCluster.Item5;

                                break;
                            }
                        }

                        if (!clusterFound)
                        {
                            clustersToAdd.Add(new Cluster(newCluster.Item1, newCluster.Item2, newCluster.Item5, newCluster.Item3, newCluster.Item4));
                        }
                    }

                    foreach (Cluster currentCluster in this.Clusters)
                    {
                        bool clusterFound = false;

                        foreach (Tuple<int, string, string, string, string> newCluster in downloadedClusters)
                        {
                            if (newCluster.Item1 == currentCluster.ID)
                            {
                                clusterFound = true;
                                break;
                            }
                        }

                        if (!clusterFound)
                        {
                            clustersToRemove.Add(currentCluster);
                        }
                    }

                    // Update DataModel.Instance.Clusters
                    foreach (Cluster clusterToRemove in clustersToRemove)
                    {
                        this.Clusters.RemoveOnUI(clusterToRemove);
                    }

                    foreach (Cluster clusterToAdd in clustersToAdd)
                    {
                        this.Clusters.AddOnUI(clusterToAdd);
                    }
                }

                #endregion

                // Signal the MonitoredSystemSyncLoop to start.
                if (initSettingsSyncFinished.CurrentCount > 0)
                {
                    this.initSettingsSyncFinished.Signal();
                }
            }
            catch (EndpointNotFoundException e1)
            {
                // The client could not connect to the client web service
                this.IsSyncing = false;
                //ClientLogger.Instance.WriteEntry("WebService Connection failed", e1, LogType.Exception);
            }
            catch (TimeoutException e2)
            {
                // The client received a Timeout
                this.IsSyncing = false;
                //ClientLogger.Instance.WriteEntry("Received Timeout from WebService", e2, LogType.Exception);

            }
            catch (CommunicationException e3)
            {
                // The client could not connect to the client web service
                this.IsSyncing = false;
                //ClientLogger.Instance.WriteEntry("WebService Connection failed", e3, LogType.Exception);
            }

            // Wait for the interval.
            Thread.Sleep(Properties.Settings.Default.SettingsInterval);
        }

        /// <summary>
        /// Stops the synchronization. 
        /// </summary>
        public void StopSynchronization()
        {
            try
            {
                // We are no longer syncing
                this.IsSyncing = false;

                // Disconnect from the client web service
                this.ClientWebService.Close();
            }
            catch (Exception e)
            {
                ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
            }
        }

        #endregion

        #region BackSynchronization

        #region Indicator

        /// <summary>
        /// Synchronize the indicator back with the server.
        /// </summary>
        /// <param name="changedIndicator"></param>
        /// <param name="monitoredSystemMAC"></param>
        /// <param name="pluginName"></param>
        /// <returns>boolean whether it was successful</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool SyncBackIndicator(List<IndicatorSettings> changedIndicator)
        {
            if (!ConfigClass.IsPowerwall && changedIndicator != null)
            {
                while (true)
                {
                    try
                    {
                        return this.ClientWebService.SetIndicatorSetting(changedIndicator);
                    }
                    catch (Exception e)
                    {
                        ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
                        Thread.Sleep(3000);
                    }
                }
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Layout

        /// <summary>
        /// Updates the given layouts at the server.
        /// </summary>
        /// <param name="changedLayouts"> id | name | username | previewImage | data</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool SyncBackLayoutChange(List<Tuple<int, string, string, byte[], string, DateTime>> changedLayouts)
        {
            if (!ConfigClass.IsPowerwall && changedLayouts != null)
            {
                bool resultValue = true;
                bool exceptionRaised = true;

                while (exceptionRaised)
                {
                    resultValue = true;
                    exceptionRaised = false;

                    foreach (Tuple<int, string, string, byte[], string, DateTime> changedLayout in changedLayouts)
                    {
                        if (changedLayout != null && changedLayout.Item1 != -1 && changedLayout.Item2 != null && changedLayout.Item3 != null && changedLayout.Item4 != null && changedLayout.Item5 != null && changedLayout.Item6 != null)
                        {
                            try
                            {
                                var resultLayout = this.ClientWebService.UpdateUIConfiguration(
                                    changedLayout.Item1,
                                    changedLayout.Item2,
                                    changedLayout.Item3,
                                    changedLayout.Item4,
                                    changedLayout.Item5,
                                    changedLayout.Item6
                                    );
                                if (resultLayout == null)
                                {
                                    resultValue = false;
                                }
                            }
                            catch (Exception e)
                            {
                                exceptionRaised = true;

                                ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
                                Thread.Sleep(3000);

                                break;
                            }
                        }
                        else
                        {
                            resultValue = false;
                        }
                    }
                }
                return resultValue;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Adds the given layout to the server database.
        /// </summary>
        /// <param name="layoutToAdd"></param>
        /// <returns>boolean whether it was successful</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool SyncBackLayoutAdd(List<Layout> layoutsToAdd)
        {
            if (!ConfigClass.IsPowerwall && layoutsToAdd != null)
            {
                bool returnValue = true;
                bool exceptionRaised = true;

                while (exceptionRaised)
                {
                    returnValue = true;
                    exceptionRaised = false;

                    foreach (Layout layoutToAdd in layoutsToAdd)
                    {
                        if (layoutToAdd != null && layoutToAdd.Name != null && layoutToAdd.UserName != null && layoutToAdd.PreviewImage != null && layoutToAdd.Data != null && layoutToAdd.Date != null)
                        {
                            try
                            {
                                var newLayout = this.ClientWebService.AddUIConfiguration(
                                     layoutToAdd.Name,
                                     layoutToAdd.UserName,
                                     layoutToAdd.PreviewImage,
                                     layoutToAdd.Data,
                                     layoutToAdd.Date
                                     );

                                if (newLayout != null)
                                {
                                    layoutToAdd.ID = newLayout.ID;
                                }
                                else
                                {
                                    returnValue = false;
                                }
                            }
                            catch (Exception e)
                            {
                                exceptionRaised = true;

                                ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
                                Thread.Sleep(3000);

                                break;
                            }
                        }
                        else
                        {
                            returnValue = false;
                        }
                    }
                }
                return returnValue;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Removes the given layout from the server database.
        /// </summary>
        /// <param name="layoutToRemove"> ID </param>
        /// <returns>boolean whether it was successful</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool SyncBackLayoutRemove(List<int> layoutsToRemove)
        {
            if (!ConfigClass.IsPowerwall && layoutsToRemove != null)
            {
                bool returnValue = true;
                bool exceptionRaised = true;

                while (exceptionRaised)
                {
                    returnValue = true;
                    exceptionRaised = false;

                    foreach (int layoutToRemove in layoutsToRemove)
                    {
                        try
                        {
                            if (layoutToRemove != -1)
                            {
                                if (!this.ClientWebService.RemoveUIConfiguration(layoutToRemove))
                                {
                                    returnValue = false;
                                }
                            }
                        }
                        catch (Exception e)
                        {

                            exceptionRaised = true;

                            ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
                            Thread.Sleep(3000);

                            break;
                        }
                    }
                }
                return returnValue;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region IgnoreList
        /// <summary>
        /// Adds a list of monitored system macs to the ingnored list at the server.
        /// </summary>
        /// <param name="monitoredSystemMAcsToAdd"></param>
        /// <returns>boolean whether it was successful</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool SyncBackIgnoreListAdd(List<Tuple<string, DateTime>> monitoredSystemMAcsToAdd)
        {
            if (!ConfigClass.IsPowerwall && monitoredSystemMAcsToAdd != null)
            {
                while (true)
                {
                    try
                    {
                        var addedMACs = this.ClientWebService.AddMonitoredSystemsToIgnoreList(monitoredSystemMAcsToAdd);
                        if (addedMACs != null)
                        {
                            return addedMACs.Count == monitoredSystemMAcsToAdd.Count;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
                        Thread.Sleep(3000);
                    }
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Removes the given list monitored system macs from the ignore list at the server.
        /// </summary>
        /// <param name="monitoredSystemMAcsToRemove"></param>
        /// <returns>boolean whether it was successful</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool SyncBackIgnoreListRemove(List<Tuple<string, DateTime>> monitoredSystemMAcsToRemove)
        {
            if (!ConfigClass.IsPowerwall && monitoredSystemMAcsToRemove != null)
            {
                while (true)
                {
                    try
                    {
                        var removedMACS = this.ClientWebService.RemoveMonitoredSystemsFromIgnoreList(monitoredSystemMAcsToRemove);
                        if (removedMACS != null)
                        {
                            return removedMACS.Count == monitoredSystemMAcsToRemove.Count;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
                        Thread.Sleep(3000);
                    }
                }
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region OU

        /// <summary>
        /// Adds the new ous at the server.
        /// </summary>
        /// <param name="newOus"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool SyncBackOUAdd(List<OrganizationalUnit> newOus)
        {
            if (!ConfigClass.IsPowerwall && newOus != null)
            {
                bool exceptionRaised = true;
                bool returnValue = true;

                while (exceptionRaised)
                {

                    exceptionRaised = false;
                    returnValue = true;

                    foreach (OrganizationalUnit ou in newOus)
                    {
                        if (ou != null)
                        {
                            try
                            {
                                if (ou.LastUpdate == null)
                                {
                                    ou.LastUpdate = DateTime.Now;
                                }
                                if (ou.ID == -1)
                                {
                                    int newID = this.ClientWebService.AddOU(ou.Name, ou.ParentID, (DateTime)ou.LastUpdate);
                                    if (newID != -1)
                                    {
                                        ou.ID = newID;
                                    }
                                    else
                                    {
                                        returnValue = false;
                                    }
                                }
                                else
                                {
                                    returnValue = false;
                                    exceptionRaised = false;
                                }
                            }
                            catch (Exception e)
                            {
                                exceptionRaised = true;

                                ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
                                Thread.Sleep(3000);

                                break;
                            }
                        }
                        else
                        {
                            returnValue = false;
                        }
                    }
                }

                //raise changed event
                OnOrganisationUnitsChanged(new EventArgs());

                return returnValue;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Changes the name of the given ou at the server.
        /// </summary>
        /// <param name="newNamedOUs">ID | Name | updateTime</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool SyncBackOUChangedName(List<Tuple<int, string, DateTime>> newNamedOUs)
        {
            if (!ConfigClass.IsPowerwall && newNamedOUs != null)
            {
                bool returnValue = true;
                bool exceptionRaised = true;

                while (exceptionRaised)
                {
                    exceptionRaised = false;
                    returnValue = true;

                    foreach (Tuple<int, string, DateTime> ou in newNamedOUs)
                    {
                        if (ou != null)
                        {
                            try
                            {
                                if (ou.Item1 != -1)
                                {
                                    if (!this.ClientWebService.ChangeOUName(ou.Item1, ou.Item2, ou.Item3))
                                    {
                                        returnValue = false;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                exceptionRaised = true;

                                ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
                                Thread.Sleep(3000);

                                break;
                            }
                        }
                        else
                        {
                            returnValue = false;
                        }
                    }
                }

                //raise changed event
                OnOrganisationUnitsChanged(new EventArgs());

                return returnValue;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Changes the parent ID of the moved ou at the server.
        /// </summary>
        /// <param name="movedOUs"> ID | ParentID | updateTime</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool SyncBackOUMoved(List<Tuple<int, int?, DateTime>> movedOUs)
        {
            if (!ConfigClass.IsPowerwall && movedOUs != null)
            {
                bool exceptionRaised = true;
                bool returnValue = true;

                while (exceptionRaised)
                {
                    exceptionRaised = false;
                    returnValue = true;

                    foreach (Tuple<int, int?, DateTime> ou in movedOUs)
                    {
                        if (ou != null)
                        {
                            try
                            {
                                if (ou.Item1 != -1)
                                {
                                    if (!this.ClientWebService.ChangeParent(ou.Item1, ou.Item2, ou.Item3))
                                    {
                                        returnValue = false;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                exceptionRaised = true;

                                ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
                                Thread.Sleep(3000);

                                break;
                            }
                        }
                        else
                        {
                            returnValue = false;
                        }
                    }
                }

                //raise changed event
                OnOrganisationUnitsChanged(new EventArgs());

                return returnValue;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Removes the given OU at the server.
        /// </summary>
        /// <param name="removedOUs"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool SyncBackOURemoved(List<int> removedOUs)
        {
            if (!ConfigClass.IsPowerwall && removedOUs != null)
            {
                bool returnValue = true;
                bool exceptionRaised = true;

                while (exceptionRaised)
                {
                    exceptionRaised = false;
                    returnValue = true;

                    foreach (int ou in removedOUs)
                    {
                        try
                        {
                            if (ou != -1)
                            {
                                if (!this.ClientWebService.DeleteOU(ou))
                                {
                                    returnValue = false;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            exceptionRaised = true;

                            // TOOD Logging
                            Console.WriteLine(e);

                            Thread.Sleep(3000);

                            break;
                        }
                    }
                }

                //raise changed event
                OnOrganisationUnitsChanged(new EventArgs());

                return returnValue;
            }
            else
            {
                return false;
            }

        }

        #endregion

        #region MonitoredSystem

        /// <summary>
        /// Changes the ouID of the given monitoredSystems at the server.
        /// </summary>
        /// <param name="movedMonitoredSystems">MAC | ouID | updateTime</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool SyncBackMonitoredSystemMoved(List<Tuple<string, int, DateTime>> movedMonitoredSystems)
        {
            if (!ConfigClass.IsPowerwall && movedMonitoredSystems != null)
            {
                List<Tuple<string, int, DateTime>> paramList = new List<Tuple<string, int, DateTime>>();
                foreach (Tuple<string, int, DateTime> ms in movedMonitoredSystems)
                {
                    if (ms != null && ms.Item2 != -1)
                    {
                        paramList.Add(new Tuple<string, int, DateTime>(ms.Item1, ms.Item2, ms.Item3));
                    }
                }

                //raise monitoredsystems changed event
                OnMonitoredSystemsChanged(new EventArgs());

                while (true)
                {
                    try
                    {
                        return this.ClientWebService.MoveMonitoredSystem(paramList);
                    }
                    catch (Exception e)
                    {
                        ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
                        Thread.Sleep(3000);
                    }
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Sets all given monitored systems in maintenance mode at the server.
        /// </summary>
        /// <param name="maintenancedMonitoredSystems">MAC | updateTime</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool SyncBackMonitoredSystemMaintenanceActivated(List<Tuple<string, DateTime>> maintenancedMonitoredSystems)
        {
            if (!ConfigClass.IsPowerwall && maintenancedMonitoredSystems != null)
            {
                while (true)
                {
                    try
                    {
                        var result = this.ClientWebService.ActivateMaintenanceMode(maintenancedMonitoredSystems);
                        return result.Count == maintenancedMonitoredSystems.Count();
                    }
                    catch (Exception e)
                    {
                        ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
                        Thread.Sleep(3000);
                    }
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Deactivate for all given monitored systems the maintenace mode at the server.
        /// </summary>
        /// <param name="monitoredSystems">MAC | updateTime</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool SyncBackMonitoredSystemMaintenanceDeactivated(List<Tuple<string, DateTime>> monitoredSystems)
        {
            if (!ConfigClass.IsPowerwall && monitoredSystems != null)
            {
                while (true)
                {
                    try
                    {
                        var result = this.ClientWebService.DeactivateMaintenanceMode(monitoredSystems);
                        return result.Count == monitoredSystems.Count();
                    }
                    catch (Exception e)
                    {
                        ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
                        Thread.Sleep(3000);
                    }
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Updates the reset date of the given monitored systems at the server. This means that that current mapping will be updated with the current indicator value.
        /// </summary>
        /// <param name="monitoredSystems">MAC | updateTime</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool SyncBackMonitoredSystemResetDateUpdated(List<Tuple<string, DateTime>> monitoredSystems)
        {
            if (!ConfigClass.IsPowerwall && monitoredSystems != null)
            {
                while (true)
                {
                    try
                    {
                        return this.ClientWebService.ResetMapping(monitoredSystems);
                    }
                    catch (Exception e)
                    {
                        ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
                        Thread.Sleep(3000);
                    }
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Updates the name of the given monitored systems at the server.
        /// </summary>
        /// <param name="monitoredSystems">MAC | Name | UpdateTime</param>
        /// <returns>bool</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool SyncBackMonitoredSystemName(List<Tuple<string, string, DateTime>> monitoredSystemNames)
        {
            if (monitoredSystemNames != null && monitoredSystemNames.Count > 0)
            {
                bool resultValue = true;
                bool exceptionRaised = true;

                while (exceptionRaised)
                {
                    resultValue = true;
                    exceptionRaised = false;

                    foreach (Tuple<string, string, DateTime> newName in monitoredSystemNames)
                    {
                        if (newName != null && newName.Item1 != null && newName.Item1 != "" && newName.Item2 != null && newName.Item2 != "")
                        {
                            try
                            {
                                bool result = this.ClientWebService.ChangeWorkstationName(newName.Item1, newName.Item2, newName.Item3);
                                if (!result)
                                {
                                    resultValue = false;
                                }
                            }
                            catch (Exception e)
                            {
                                exceptionRaised = true;

                                ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
                                Thread.Sleep(3000);

                                break;
                            }
                        }
                        else
                        {
                            resultValue = false;
                        }
                    }
                }
                return resultValue;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region MailUser

        /// <summary>
        /// Changes the mail users at the server.
        /// </summary>
        /// <param name="changedMailUsers">ID | Name | Email</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool SyncBackMailUserChanged(List<Tuple<int, string, string>> changedMailUsers)
        {
            if (!ConfigClass.IsPowerwall && changedMailUsers != null)
            {
                bool returnValue = true;
                bool exceptionRaised = true;

                while (exceptionRaised)
                {
                    returnValue = true;
                    exceptionRaised = false;

                    foreach (Tuple<int, string, string> mailUser in changedMailUsers)
                    {
                        if (mailUser != null && mailUser.Item1 != -1)
                        {
                            try
                            {
                                if (!this.ClientWebService.ChangeEmail(mailUser.Item1, mailUser.Item2, mailUser.Item3))
                                {
                                    returnValue = false;
                                }
                            }
                            catch (Exception e)
                            {
                                exceptionRaised = true;

                                ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
                                Thread.Sleep(3000);

                                break;
                            }
                        }
                        else
                        {
                            returnValue = false;
                        }
                    }
                }

                return returnValue;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Changes the Daily Mail flag at the server.
        /// </summary>
        /// <param name="changedMailUsers">ID | DailyMail</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool SyncBackMailUserChangedDailyMail(List<Tuple<int, bool>> changedMailUsers)
        {
            if (!ConfigClass.IsPowerwall && changedMailUsers != null)
            {
                bool returnValue = true;
                bool exceptionRaised = true;

                while (exceptionRaised)
                {
                    returnValue = true;
                    exceptionRaised = false;

                    foreach (Tuple<int, bool> mailUser in changedMailUsers)
                    {
                        if (mailUser != null && mailUser.Item1 != -1)
                        {
                            try
                            {
                                if (mailUser.Item2)
                                {
                                    if (!this.ClientWebService.AddDailyMail(mailUser.Item1))
                                    {
                                        returnValue = false;
                                    }
                                }
                                else
                                {
                                    if (!this.ClientWebService.DeleteDailyMail(mailUser.Item1))
                                    {
                                        returnValue = false;
                                    }
                                }

                            }
                            catch (Exception e)
                            {
                                exceptionRaised = true;

                                ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
                                Thread.Sleep(3000);

                                break;
                            }
                        }
                        else
                        {
                            returnValue = false;
                        }
                    }
                }
                return returnValue;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Adds monitored systems to the registered Mail User at the server.
        /// </summary>
        /// <param name="mailUser"></param>
        /// <param name="monitoredSystemsToAdd">MAC</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool SyncBackMailUserAddMonitoredSystem(int mailUserID, List<string> monitoredSystemsToAdd)
        {
            if (!ConfigClass.IsPowerwall && monitoredSystemsToAdd != null && mailUserID != -1)
            {
                while (true)
                {
                    try
                    {
                        return this.ClientWebService.AddMailObserver(mailUserID, monitoredSystemsToAdd);
                    }
                    catch (Exception e)
                    {
                        ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
                        Thread.Sleep(3000);
                    }
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Adds the mail user at the server.
        /// </summary>
        /// <param name="newMailUser"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool SyncBackMailUserNew(List<MailUser> newMailUser)
        {
            if (!ConfigClass.IsPowerwall && newMailUser != null)
            {
                bool returnValue = true;
                bool exceptionRaised = true;

                while (exceptionRaised)
                {
                    returnValue = true;
                    exceptionRaised = false;

                    foreach (MailUser mailUser in newMailUser)
                    {
                        if (mailUser != null && mailUser.ID != -1)
                        {
                            try
                            {
                                int? newID = this.ClientWebService.AddEMail(mailUser.Email, mailUser.Name);
                                if (newID != null && newID != -1)
                                {
                                    mailUser.ID = (int)newID;
                                }
                                else
                                {
                                    returnValue = false;
                                }
                            }
                            catch (Exception e)
                            {
                                exceptionRaised = true;

                                ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
                                Thread.Sleep(3000);

                                break;
                            }
                        }
                        else
                        {
                            returnValue = false;
                        }
                    }
                }

                return returnValue;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Removes the given mail user at the server.
        /// </summary>
        /// <param name="mailUserToDelete"> IDs </param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool SynBackMailUserRemove(List<int> mailUserToDelete)
        {
            if (!ConfigClass.IsPowerwall && mailUserToDelete != null)
            {
                bool returnValue = true;
                bool exceptionRaised = true;

                while (exceptionRaised)
                {
                    returnValue = true;
                    exceptionRaised = false;

                    foreach (int mailUser in mailUserToDelete)
                    {
                        try
                        {
                            if (mailUser != -1)
                            {
                                if (!this.ClientWebService.RemoveEMail(mailUser))
                                {
                                    returnValue = false;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            exceptionRaised = true;

                            ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
                            Thread.Sleep(3000);

                            break;
                        }
                    }
                }
                return returnValue;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Removes the given monitored systems from the mail user at the server.
        /// </summary>
        /// <param name="mailUser"> IDs </param>
        /// <param name="msToRemove"> MACs </param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool SyncBackMailUserRemoveMonitoredSystem(int mailUser, List<string> msToRemove)
        {
            if (!ConfigClass.IsPowerwall && msToRemove != null && mailUser != -1)
            {
                while (true)
                {
                    try
                    {
                        return this.ClientWebService.RemoveMailObserver(mailUser, msToRemove);
                    }
                    catch (Exception e)
                    {
                        ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
                        Thread.Sleep(3000);
                    }
                }
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Cluster

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clustersToAdd"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool SyncBackClusterAdd(List<Cluster> clustersToAdd)
        {
            if (!ConfigClass.IsPowerwall && clustersToAdd != null)
            {
                bool returnValue = true;
                bool exceptionRaised = true;

                while (exceptionRaised)
                {
                    returnValue = true;
                    exceptionRaised = false;

                    foreach (Cluster cluster in clustersToAdd)
                    {
                        if (cluster != null)
                        {
                            try
                            {
                                int newID = this.ClientWebService.AddCluster(cluster.HeadnodeAddress, cluster.UserName, cluster.Password, cluster.CurrentPlatform);
                                if (newID != -1)
                                {
                                    cluster.ID = (int)newID;
                                }
                                else
                                {
                                    returnValue = false;
                                }
                            }
                            catch (Exception e)
                            {
                                exceptionRaised = true;

                                ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
                                Thread.Sleep(3000);

                                break;
                            }
                        }
                        else
                        {
                            returnValue = false;
                        }
                    }
                }

                return returnValue;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Remove the given clusters from the server.
        /// </summary>
        /// <param name="clustersToRemove"> IDs </param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool SyncBackClusterRemove(List<int> clustersToRemove)
        {
            if (!ConfigClass.IsPowerwall && clustersToRemove != null)
            {
                bool returnValue = true;
                bool exceptionRaised = true;

                while (exceptionRaised)
                {
                    returnValue = true;
                    exceptionRaised = false;

                    foreach (int cluster in clustersToRemove)
                    {
                        try
                        {
                            if (cluster != -1)
                            {
                                if (!this.ClientWebService.DeleteCluster(cluster))
                                {
                                    returnValue = false;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            exceptionRaised = true;

                            ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
                            Thread.Sleep(3000);

                            break;
                        }
                    }
                }

                return returnValue;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Changesthe given clusters at the server.
        /// </summary>
        /// <param name="changedClusters">ID | Tuple <headnode url | username | password | platform></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool SyncBackClusterChanged(List<Tuple<int, Tuple<string, string, string, string>>> changedClusters)
        {
            if (!ConfigClass.IsPowerwall && changedClusters != null)
            {
                bool returnValue = true;
                bool exceptionRaised = true;

                while (exceptionRaised)
                {
                    returnValue = true;
                    exceptionRaised = false;

                    foreach (Tuple<int, Tuple<string, string, string, string>> cluster in changedClusters)
                    {
                        if (cluster != null && cluster.Item1 != -1)
                        {
                            try
                            {
                                if (!this.ClientWebService.ChangeCluster(cluster.Item1, cluster.Item2))
                                {
                                    returnValue = false;
                                }
                            }
                            catch (Exception e)
                            {
                                exceptionRaised = true;

                                ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
                                Thread.Sleep(3000);

                                break;
                            }
                        }
                        else
                        {
                            returnValue = false;
                        }
                    }
                }
                return returnValue;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #endregion

        #region Methods

        private void Initialize()
        {
            this.IgnoredMonitoredSystems.CollectionChanged += IgnoredMonitoredSystems_CollectionChanged;
            this.Elements.CollectionChanged += Elements_CollectionChanged;
            this.Clusters.CollectionChanged += Clusters_CollectionChanged;
            this.Layouts.CollectionChanged += Layouts_CollectionChanged;
            this.MailUsers.CollectionChanged += MailUsers_CollectionChanged;
        }

        private void Elements_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move))
            {
                if (!DataModel.Instance.IsPowerwall)
                {
                    StackTrace stackTrace = new StackTrace();
                    if (stackTrace.ToString().Contains("System.Windows.Controls"))
                    {
                        if (e.OldItems != null)
                        {
                            ThreadManager.CreateWorkerThread("SyncBack_OURemoved", () =>
                            {
                                List<int> ouToRemove = new List<int>();

                                foreach (var item in e.OldItems)
                                {
                                    OrganizationalUnit ou = item as OrganizationalUnit;
                                    if (ou != null)
                                    {
                                        ouToRemove.Add(ou.ID);
                                    }
                                }

                                if (ouToRemove.Count > 0)
                                {
                                    bool result = DataModel.Instance.SyncBackOURemoved(ouToRemove);
                                }
                            }, false);
                        }

                        if (e.NewItems != null)
                        {
                            ThreadManager.CreateWorkerThread("SyncBack_OUAdd", () =>
                            {
                                List<OrganizationalUnit> ouToAdd = new List<OrganizationalUnit>();

                                foreach (var item in e.NewItems)
                                {
                                    OrganizationalUnit ou = item as OrganizationalUnit;
                                    if (ou != null)
                                    {
                                        ouToAdd.Add(ou);
                                    }
                                }

                                if (ouToAdd.Count > 0)
                                {
                                    bool result = DataModel.Instance.SyncBackOUAdd(ouToAdd);
                                }
                            }, false);
                        }
                    }
                    stackTrace = null;
                }
            }
        }

        private void IgnoredMonitoredSystems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!DataModel.Instance.IsPowerwall)
            {
                StackTrace stackTrace = new StackTrace();

                if (stackTrace.ToString().Contains("System.Windows.Controls"))
                {
                    if (e.OldItems != null)
                    {
                        ThreadManager.CreateWorkerThread("SyncBack_IgnoreListRemove", () =>
                        {
                            List<Tuple<string, DateTime>> msToRemove = new List<Tuple<string, DateTime>>();

                            foreach (var item in e.OldItems)
                            {
                                Tuple<string, string> tuple = item as Tuple<string, string>;
                                if (tuple != null)
                                {
                                    msToRemove.Add(new Tuple<string, DateTime>(tuple.Item1, DateTime.Now));
                                }
                            }

                            if (msToRemove.Count > 0)
                            {
                                bool result = DataModel.Instance.SyncBackIgnoreListRemove(msToRemove);
                            }
                        }, false);
                    }

                    if (e.NewItems != null)
                    {
                        ThreadManager.CreateWorkerThread("SyncBack_IgnoreListAdd", () =>
                        {
                            List<Tuple<string, DateTime>> msToAdd = new List<Tuple<string, DateTime>>();

                            foreach (var item in e.NewItems)
                            {
                                Tuple<string, string> tuple = item as Tuple<string, string>;
                                if (tuple != null)
                                {
                                    msToAdd.Add(new Tuple<string, DateTime>(tuple.Item1, DateTime.Now));
                                }
                            }

                            if (msToAdd.Count > 0)
                            {
                                bool result = DataModel.Instance.SyncBackIgnoreListAdd(msToAdd);
                            }
                        }, false);
                    }
                }
                stackTrace = null;
            }
        }

        private void MailUsers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!DataModel.Instance.IsPowerwall)
            {
                StackTrace stackTrace = new StackTrace();
                if (stackTrace.ToString().Contains("System.Windows.Controls"))
                {
                    if (e.OldItems != null)
                    {
                        ThreadManager.CreateWorkerThread("SyncBack_MailUSerRemove", () =>
                         {
                             List<int> mailUsesrToRemove = new List<int>();

                             foreach (var item in e.OldItems)
                             {
                                 MailUser mailUser = item as MailUser;
                                 if (mailUser != null)
                                 {
                                     mailUsesrToRemove.Add(mailUser.ID);
                                 }
                             }

                             if (mailUsesrToRemove.Count > 0)
                             {
                                 bool result = DataModel.Instance.SynBackMailUserRemove(mailUsesrToRemove);
                             }
                         }, false);
                    }

                    if (e.NewItems != null)
                    {
                        ThreadManager.CreateWorkerThread("SyncBack_MailUserNew", () =>
                         {
                             List<MailUser> mailUsersToAdd = new List<MailUser>();

                             foreach (var item in e.NewItems)
                             {
                                 MailUser mailUser = item as MailUser;
                                 if (mailUser != null)
                                 {
                                     mailUsersToAdd.Add(mailUser);
                                 }
                             }

                             if (mailUsersToAdd.Count > 0)
                             {
                                 bool result = DataModel.Instance.SyncBackMailUserNew(mailUsersToAdd);
                             }
                         }, false);
                    }
                }
                stackTrace = null;
            }
        }

        private void Layouts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!DataModel.Instance.IsPowerwall)
            {
                StackTrace stackTrace = new StackTrace();
                if (stackTrace.ToString().Contains("System.Windows.Controls"))
                {
                    if (e.OldItems != null)
                    {
                        ThreadManager.CreateWorkerThread("SyncBack_LayoutRemove", () =>
                         {
                             List<int> layoutsToRemove = new List<int>();

                             foreach (var item in e.OldItems)
                             {
                                 //TODO NICHTS LÖSCHEN!!!

                                 Layout layout = item as Layout;
                                 if (layout != null)
                                 {
                                     layoutsToRemove.Add(layout.ID);
                                 }
                             }

                             if (layoutsToRemove.Count > 0)
                             {
                                 bool result = DataModel.Instance.SyncBackLayoutRemove(layoutsToRemove);
                             }
                         }, false);
                    }

                    if (e.NewItems != null)
                    {
                        ThreadManager.CreateWorkerThread("SyncBack_IgnoreListAdd", () =>
                        {
                            List<Layout> layoutsToAdd = new List<Layout>();

                            foreach (var item in e.NewItems)
                            {
                                Layout layout = item as Layout;
                                if (layout != null)
                                {
                                    layoutsToAdd.Add(layout);
                                }
                            }


                            if (layoutsToAdd.Count > 0)
                            {
                                bool result = DataModel.Instance.SyncBackLayoutAdd(layoutsToAdd);
                            }
                        }, false);
                    }
                }
                stackTrace = null;
            }
        }

        private void Clusters_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!DataModel.Instance.IsPowerwall)
            {
                StackTrace stackTrace = new StackTrace();
                if (stackTrace.ToString().Contains("System.Windows.Controls"))
                {
                    if (e.OldItems != null)
                    {
                        ThreadManager.CreateWorkerThread("SyncBack_CLusterRemove", () =>
                         {
                             List<int> clusterToRemove = new List<int>();

                             foreach (var item in e.OldItems)
                             {
                                 Cluster oldCluster = item as Cluster;
                                 if (oldCluster != null)
                                 {
                                     clusterToRemove.Add(oldCluster.ID);
                                 }
                             }

                             if (clusterToRemove.Count > 0)
                             {
                                 bool result = DataModel.Instance.SyncBackClusterRemove(clusterToRemove);
                             }
                         }, false);
                    }

                    if (e.NewItems != null)
                    {
                        ThreadManager.CreateWorkerThread("SyncBack_ClusterAdd", () =>
                         {
                             List<Cluster> clusterToAdd = new List<Cluster>();

                             foreach (var item in e.NewItems)
                             {
                                 Cluster newCluster = item as Cluster;
                                 if (newCluster != null)
                                 {
                                     clusterToAdd.Add(newCluster);
                                 }
                             }

                             if (clusterToAdd.Count > 0)
                             {
                                 bool result = DataModel.Instance.SyncBackClusterAdd(clusterToAdd);
                             }
                         }, false);
                    }
                }
                stackTrace = null;
            }
        }

        public void LayoutManager_Instance_LayoutChanged(Layout l)
        {
            if (!DataModel.Instance.IsPowerwall)
            {
                StackTrace stackTrace = new StackTrace();
                if (stackTrace.ToString().Contains("System.Windows.Controls"))
                {
                    ThreadManager.CreateWorkerThread("SyncBack_LayoutChange", () =>
                     {
                         var paramList = new List<Tuple<int, string, string, byte[], string, DateTime>>();
                         paramList.Add(new Tuple<int, string, string, byte[], string, DateTime>(l.ID, l.Name, l.UserName, l.PreviewImage, l.Data, l.Date));

                         bool result = DataModel.Instance.SyncBackLayoutChange(paramList);
                     }, false);
                }
                stackTrace = null;
            }
        }


        private void UpdateStatusBarInfo()
        {
            IEnumerable<MonitoredSystem> currentMonitoredSystems = this.Elements.GetMonitoredSystems();

            int numberOfCritical = 0;
            int numberOfWarning = 0;
            int numberOfOK = 0;
            int numberOfMaintenance = 0;

            foreach (MonitoredSystem monitoredSystem in currentMonitoredSystems)
            {
                switch (monitoredSystem.State)
                {
                    case MappingState.Critical:
                        numberOfCritical++;
                        break;
                    case MappingState.Warning:
                        numberOfWarning++;
                        break;
                    case MappingState.OK:
                        numberOfOK++;
                        break;
                    case MappingState.Maintenance:
                        numberOfMaintenance++;
                        break;
                }
            }

            this.StatusBarInfo = new Tuple<int, int, int, int, int>(numberOfCritical, numberOfWarning, numberOfOK, numberOfMaintenance, currentMonitoredSystems.Count());
        }

        /// <summary>
        /// Updates the IndicatorValues of the an indicator.
        /// The values are not sorted or compromised.
        /// </summary>
        /// <param name="monitoredSystem"></param>
        /// <param name="plugin"></param>
        /// <param name="indicator"></param>
        /// <param name="fromDate"></param>
        /// <returns></returns>
        public void UpdateIndicatorValues(Object o)
        {
            try
            {
                Tuple<MonitoredSystem, Plugin, Indicator, DateTime, int> param = (Tuple<MonitoredSystem, Plugin, Indicator, DateTime, int>)o;
                MonitoredSystem monitoredSystem = param.Item1;
                Plugin plugin = param.Item2;
                Indicator indicator = param.Item3;
                DateTime fromDate = param.Item4;
                int maxNumOfValues = param.Item5;
                // Download the values.
                List<Tuple<string, string, string, DateTime?, DateTime?, int?>> parameters = new List<Tuple<string, string, string, DateTime?, DateTime?, int?>>();
                parameters.Add(new Tuple<string, string, string, DateTime?, DateTime?, int?>(
                    monitoredSystem.MAC,
                    plugin.Name,
                    indicator.Name,
                    fromDate,
                    DateTime.Now,
                    maxNumOfValues));
                List<Tuple<string, string, string, string, MappingState, DateTime>> downloadedData = this.ClientWebService.GetData(parameters);

                // Build new value list.
                ExtendedObservableCollection<IndicatorValue> newValues = new ExtendedObservableCollection<IndicatorValue>();
                foreach (Tuple<string, string, string, string, MappingState, DateTime> tuple in downloadedData)
                {
                    if (tuple.Item1 == monitoredSystem.MAC)
                    {
                        if (tuple.Item2 == plugin.Name)
                        {
                            if (tuple.Item3 == indicator.Name)
                            {
                                newValues.Add(new IndicatorValue(
                                    tuple.Item4,
                                    indicator.DataType,
                                    tuple.Item6,
                                    tuple.Item5));
                            }
                        }
                    }

                }
                indicator.IndicatorValues = newValues;
                if (monitoredSystem != null)
                {
                    foreach (IPluginVisualization visPlugin in PluginManager.Instance.GetLoadedPlugins())
                    {
                        if (visPlugin != null && visPlugin.Name.ToLower() == plugin.Name.ToLower())
                        {
                            try
                            {
                                string res = visPlugin.CalculateMainValue(plugin.Indicators);
                                plugin.MainValue = res;
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, LogType.Exception);
            }
        }

        /// <summary>
        /// Moves a MonitoredSystem.
        /// </summary>
        /// <param name="monitoredSystem"></param>
        /// <param name="oldOuID"></param>
        /// <param name="newOuID"></param>
        public void MoveMonitoredSystem(MonitoredSystem monitoredSystem, int oldOuID, int newOuID)
        {
            if (oldOuID != newOuID && monitoredSystem != null)
            {
                // Remove the monitoredSystem from the old OU
                foreach (OrganizationalUnit ou in this.Elements.GetOrganizationalUnits())
                {
                    if (ou.ID == oldOuID)
                    {
                        ou.Elements.RemoveOnUI(monitoredSystem);
                        break;
                    }
                }

                // Add the monitoredSystem to the new OU
                foreach (OrganizationalUnit ou in this.Elements.GetOrganizationalUnits())
                {
                    if (ou.ID == newOuID)
                    {
                        ou.Elements.AddOnUI(monitoredSystem);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Moves an OrganizationalUnit.
        /// </summary>
        /// <param name="ou"></param>
        /// <param name="oldParentID"></param>
        /// <param name="newParentID"></param>
        public void MoveOrganizationalUnit(OrganizationalUnit ou, int? oldParentID, int? newParentID)
        {
            if (ou != null)
            {
                // Remove the organizational unit from the old OU
                if (oldParentID == null)
                {
                    this.Elements.RemoveOnUI(ou);
                }
                else
                {
                    foreach (OrganizationalUnit currentOu in this.Elements.GetOrganizationalUnits())
                    {
                        if (currentOu.ID == oldParentID)
                        {
                            currentOu.Elements.RemoveOnUI(ou);
                            break;
                        }
                    }
                }

                // Add the organizational unit to the new OU
                if (newParentID == null)
                {
                    this.Elements.AddOnUI(ou);
                }
                else
                {
                    bool ouFound = false;

                    foreach (OrganizationalUnit currentOu in this.Elements.GetOrganizationalUnits())
                    {
                        if (currentOu.ID == newParentID)
                        {
                            ouFound = true;
                            currentOu.Elements.AddOnUI(ou);
                            break;
                        }
                    }

                    if (!ouFound)
                    {
                        remainingOusToAdd.Add(ou);
                    }
                    else
                    {
                        List<OrganizationalUnit> addedOusToRemove = new List<OrganizationalUnit>();

                        foreach (OrganizationalUnit remainingOU in remainingOusToAdd)
                        {
                            if (remainingOU.ParentID == ou.ID)
                            {
                                ou.Elements.AddOnUI(remainingOU);
                                addedOusToRemove.Add(remainingOU);
                            }
                        }

                        foreach (OrganizationalUnit ouToRemove in addedOusToRemove)
                        {
                            remainingOusToAdd.Remove(ouToRemove);
                        }
                    }
                }

            }
        }

        #endregion

        #region Util

        /// <summary>
        /// Returns an OU Object by a given OUId.
        /// </summary>
        /// <param name="OuID"></param>
        /// <returns></returns>
        public OrganizationalUnit GetOu(int OuID)
        {

            List<ExtendedObservableCollection<TileableElement>> remainingElementsToAnalyze = new List<ExtendedObservableCollection<TileableElement>>();
            remainingElementsToAnalyze.Add(Elements);

            while (remainingElementsToAnalyze.Count > 0)
            {
                foreach (TileableElement e in remainingElementsToAnalyze.ElementAt(0))
                {
                    if (e.GetType().Equals(typeof(OrganizationalUnit)))
                    {
                        OrganizationalUnit u = e as OrganizationalUnit;
                        // found the correct OU, returning Children
                        if (u.ID == OuID)
                        {
                            return u;
                        }
                        else
                        {
                            remainingElementsToAnalyze.Add(u.Elements);
                        }
                    }
                }
                remainingElementsToAnalyze.RemoveAt(0);
            }
            return null;
        }

        public void IgnoreOU(OrganizationalUnit ou)
        {
            foreach (var child in ou.Elements)
            {
                if (child is MonitoredSystem)
                {
                    IgnoreMS((child as MonitoredSystem));
                }
                else if (child is OrganizationalUnit)
                {
                    IgnoreOU((child as OrganizationalUnit));
                }

            }
        }

        public void IgnoreMS(MonitoredSystem ms)
        {
            IgnoredMonitoredSystems.AddOnUI(new Tuple<string, string>(ms.MAC, ms.Name));
        }

        #endregion

        #region events

        public event EventHandler MonitoredSystemsChanged;

        private void OnMonitoredSystemsChanged(EventArgs e)
        {
            if (MonitoredSystemsChanged != null)
            {
                MonitoredSystemsChanged(this.Elements, e);
            }
        }

        public event EventHandler OrganisationUnitsChanged;

        private void OnOrganisationUnitsChanged(EventArgs e)
        {
            if (OrganisationUnitsChanged != null)
            {
                OrganisationUnitsChanged(this.Elements, e);
            }
        }

        #endregion
    }
}
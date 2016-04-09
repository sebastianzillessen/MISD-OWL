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
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using MISD.Core;
using MISD.Server.Properties;
using System.Runtime.CompilerServices;

namespace MISD.Server.Manager
{
    public class PluginManager
    {
        #region Singleton

        private static volatile PluginManager instance;
        private static object syncRoot = new Object();

        /// <summary>
        /// Property to access the class.
        /// </summary>
        public static PluginManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new PluginManager();
                    }
                }

                return instance;
            }
        }

        #endregion

        #region Constructors

        private PluginManager()
        {
        }

        #endregion

        #region Events

        /// <summary>
        /// This event comes up when the MEF exports have changed.
        /// </summary>
        public event EventHandler PluginsChanged;
        protected void OnPluginsChanged()
        {
            if (this.PluginsChanged != null)
            {
                this.PluginsChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// This event comes up when the server plugins have changed.
        /// </summary>
        public event EventHandler ServerPluginsChanged;
        protected void OnServerPluginsChanged()
        {
            if (this.ServerPluginsChanged != null)
            {
                this.ServerPluginsChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// This event comes up when the Bright plugins have changed.
        /// </summary>
        public event EventHandler BrightPluginsChanged;
        protected void OnBrightPluginsChanged()
        {
            if (this.BrightPluginsChanged != null)
            {
                this.BrightPluginsChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// This event comes up when the HPC plugins have changed.
        /// </summary>
        public event EventHandler HPCPluginsChanged;
        protected void OnHPCPluginsChanged()
        {
            if (this.HPCPluginsChanged != null)
            {
                this.HPCPluginsChanged(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Paths

        protected string PluginPathWorkstationLinux
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + Settings.Default.PluginPathWorkstationLinux;
            }
        }

        protected string PluginPathWorkstationWindows
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + Settings.Default.PluginPathWorkstationWindows;
            }
        }

        protected string PluginPathServer
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + Settings.Default.PluginPathServer;
            }
        }

        protected string PluginPathClusterBright
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + Settings.Default.PluginPathClusterBright;
            }
        }

        protected string PluginPathClusterHPC
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + Settings.Default.PluginPathClusterHPC;
            }
        }

        protected string PluginPathVisualisation
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + Settings.Default.PluginPathVisualization;
            }
        }

        #endregion

        #region MEF

        [ImportMany(AllowRecomposition = true)]
        public IEnumerable<IPlugin> Plugins { get; set; }

        public IEnumerable<IPlugin> ServerPlugins
        {
            get
            {
                return from p in Plugins
                       where p.TargetPlatform == Platform.Server
                       select p;
            }
        }

        public IEnumerable<IPlugin> HPCPlugins
        {
            get
            {
                return from p in Plugins
                       where p.TargetPlatform == Platform.HPC
                       select p;
            }
        }

        public IEnumerable<IPlugin> BrightPlugins
        {
            get
            {
                return from p in Plugins
                       where p.TargetPlatform == Platform.Bright
                       select p;
            }
        }

        protected AggregateCatalog aggregateCatalog;

        protected DirectoryCatalog serverPluginsCatalog;
        protected DirectoryCatalog clusterBrightPluginsCatalog;
        protected DirectoryCatalog clusterHPCPluginsCatalog;

        protected FileSystemWatcher serverPluginsWatcher;
        protected FileSystemWatcher clusterBrightPluginsWatcher;
        protected FileSystemWatcher clusterHPCPluginsWatcher;

        public void InitializeMEF()
        {
            var aggregateCatalog = new AggregateCatalog();

            serverPluginsCatalog = new DirectoryCatalog(this.PluginPathServer, "MISD.Plugins.Server.*.dll");
            serverPluginsCatalog.Changed += new EventHandler<ComposablePartCatalogChangeEventArgs>(serverPluginsCatalog_Changed);
            aggregateCatalog.Catalogs.Add(serverPluginsCatalog);

            clusterBrightPluginsCatalog = new DirectoryCatalog(this.PluginPathClusterBright, "MISD.Plugins.Bright.*.dll");
            clusterBrightPluginsCatalog.Changed += new EventHandler<ComposablePartCatalogChangeEventArgs>(clusterBrightPluginsCatalog_Changed);
            aggregateCatalog.Catalogs.Add(clusterBrightPluginsCatalog);

            clusterHPCPluginsCatalog = new DirectoryCatalog(this.PluginPathClusterHPC, "MISD.Plugins.HPC.*.dll");
            clusterHPCPluginsCatalog.Changed += new EventHandler<ComposablePartCatalogChangeEventArgs>(clusterHPCPluginsCatalog_Changed);
            aggregateCatalog.Catalogs.Add(clusterHPCPluginsCatalog);

            var pluginContainer = new CompositionContainer(aggregateCatalog);
            pluginContainer.ExportsChanged += pluginContainer_ExportsChanged;
            pluginContainer.ComposeParts(this);
        }

        void clusterHPCPluginsCatalog_Changed(object sender, ComposablePartCatalogChangeEventArgs e)
        {
            this.OnHPCPluginsChanged();
        }

        void clusterBrightPluginsCatalog_Changed(object sender, ComposablePartCatalogChangeEventArgs e)
        {
            this.OnBrightPluginsChanged();
        }

        void serverPluginsCatalog_Changed(object sender, ComposablePartCatalogChangeEventArgs e)
        {
            this.OnServerPluginsChanged();
        }

        public void InitializeFileSystemWatchers()
        {
            this.serverPluginsWatcher = new FileSystemWatcher(this.PluginPathServer);
            this.serverPluginsWatcher.Created += serverPluginsWatcher_Changed;
            this.serverPluginsWatcher.Changed += serverPluginsWatcher_Changed;
            this.serverPluginsWatcher.Deleted += serverPluginsWatcher_Changed;

            this.clusterBrightPluginsWatcher = new FileSystemWatcher(this.PluginPathClusterBright);
            this.clusterBrightPluginsWatcher.Created += clusterBrightPluginsWatcher_Changed;
            this.clusterBrightPluginsWatcher.Changed += clusterBrightPluginsWatcher_Changed;
            this.clusterBrightPluginsWatcher.Deleted += clusterBrightPluginsWatcher_Changed;

            this.clusterHPCPluginsWatcher = new FileSystemWatcher(this.PluginPathClusterHPC);
            this.clusterHPCPluginsWatcher.Created += clusterHPCPluginsWatcher_Changed;
            this.clusterHPCPluginsWatcher.Changed += clusterHPCPluginsWatcher_Changed;
            this.clusterHPCPluginsWatcher.Deleted += clusterHPCPluginsWatcher_Changed;
        }

        private void clusterHPCPluginsWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            this.UpdateDatabase();
            this.clusterHPCPluginsCatalog.Refresh();
        }

        private void clusterBrightPluginsWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            this.UpdateDatabase();
            this.clusterBrightPluginsCatalog.Refresh();
        }

        private void serverPluginsWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            this.UpdateDatabase();
            this.serverPluginsCatalog.Refresh();
        }

        void pluginContainer_ExportsChanged(object sender, ExportsChangeEventArgs e)
        {
            // Update database
            this.UpdateDatabase();

            // Notify the others
            this.OnPluginsChanged();
        }

        #endregion

        #region Plugins

        protected static Func<Database.MISDDataContext, byte, IEnumerable<Database.PluginMetadata>> GetDatabasePluginsByPlatform = CompiledQuery.Compile<Database.MISDDataContext, byte, IEnumerable<Database.PluginMetadata>>((Database.MISDDataContext dataContext, byte platform) => dataContext.PluginMetadata.Where(p => p.Platform == platform));
        protected static Func<Database.MISDDataContext, int, int, string, int> GetIndicatorCount = CompiledQuery.Compile<Database.MISDDataContext, int, int, string, int>((context, newDataID, moniSystemID, indicatorName) => context.Indicator.Where(p => (p.PluginMetadataID == newDataID && p.Name == indicatorName && p.MonitoredSystemID == moniSystemID)).Count());

        /// <summary>
        /// This method updates the database with new plugins that are read from file.
        /// </summary>
        public void UpdateDatabase()
        {
            using (var dataContext = new Database.MISDDataContext())
            {
                foreach (Platform platform in Enum.GetValues(typeof(Platform)))
                {
                    // Exclude the visualization plugins
                    if (platform == Platform.Visualization) continue;

                    try
                    {
                        // These are the locally available plugins
                        var localPlugins = this.GetPluginList(platform);

                        // These are the available plugins in the database for the current platform
                        var serverPlugins = PluginManager.GetDatabasePluginsByPlatform(dataContext, (byte)platform).ToList();

                        foreach (var plugin in localPlugins)
                        {
                            // Does a plugin with this name already exist on the server?
                            var match = serverPlugins.Where(p => p.Name == plugin.Name).FirstOrDefault();

                            Database.PluginMetadata newData;

                            if (match == null)
                            {
                                newData = new Database.PluginMetadata();
                            }
                            else
                            {
                                newData = match;
                            }

                            // Add to server
                            newData.Company = plugin.Company;
                            newData.Copyright = plugin.Copyright;
                            newData.Product = plugin.Product;
                            newData.Description = plugin.Description;
                            newData.FileName = plugin.FileName;
                            newData.Name = plugin.Name;
                            newData.Platform = (byte)platform;
                            newData.Version = plugin.Version.ToString();

                            if (match == null)
                            {
                                dataContext.PluginMetadata.InsertOnSubmit(newData);
                            }
                            dataContext.SubmitChanges();

                            if (plugin.Indicators != null)
                            {
                                foreach (var moniSystem in dataContext.MonitoredSystem)
                                {
                                    foreach (var indicator in plugin.Indicators)
                                    {
                                        if (moniSystem.OperatingSystem == (byte)platform || platform == Platform.Server)
                                        {
                                            var indicatorMatch = GetIndicatorCount(dataContext, newData.ID, moniSystem.ID, indicator.IndicatorName);

                                            if (indicatorMatch == 0)
                                            {
                                                var indi = new Database.Indicator();
                                                indi.MonitoredSystemID = moniSystem.ID;

                                                indi.FilterStatement = indicator.FilterStatement;
                                                indi.MappingDuration = indicator.MappingDuration.Ticks;
                                                indi.Name = indicator.IndicatorName;
                                                indi.PluginMetadataID = newData.ID;
                                                indi.StatementCritical = indicator.MetricCritical;
                                                indi.StatementWarning = indicator.MetricWarning;
                                                indi.StorageDuration = indicator.StorageDuration.Ticks;
                                                indi.UpdateInterval = indicator.UpdateInterval.Ticks;
                                                indi.ValueType = (byte)indicator.DataType;

                                                dataContext.Indicator.InsertOnSubmit(indi);
                                            }
                                        }
                                    }

                                    dataContext.SubmitChanges();
                                }
                            }

                            serverPlugins.Remove(match);
                        }

                        // Remove all server plugins that have no local counterpart.
                        if (serverPlugins.Count > 0)
                        {
                            dataContext.PluginMetadata.DeleteAllOnSubmit(serverPlugins);
                            dataContext.SubmitChanges();
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.WriteEntry(string.Format("PluginManager_UpdateDatabase: Could not update the database for platform: {0}", platform), e, LogType.Exception);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a list of all available plugins by reading them from file with reflection.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public List<Core.PluginMetadata> GetPluginList(Platform platform)
        {
            // This is the assembly path, e.g. "C:\Program Files\MISD Server".
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Complete the assembly path depending on the platform.
            switch (platform)
            {
                case Platform.Windows:
                    assemblyPath += Path.DirectorySeparatorChar + Settings.Default.PluginPathWorkstationWindows;
                    break;
                case Platform.Linux:
                    assemblyPath += Path.DirectorySeparatorChar + Settings.Default.PluginPathWorkstationLinux;
                    break;
                case Platform.Bright:
                    assemblyPath += Path.DirectorySeparatorChar + Settings.Default.PluginPathClusterBright;
                    break;
                case Platform.HPC:
                    assemblyPath += Path.DirectorySeparatorChar + Settings.Default.PluginPathClusterHPC;
                    break;
                case Platform.Server:
                    assemblyPath += Path.DirectorySeparatorChar + Settings.Default.PluginPathServer;
                    break;
                case Platform.Visualization:
                    assemblyPath += Path.DirectorySeparatorChar + Settings.Default.PluginPathVisualization;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Platform", "The plugin manager cannot handle the platform \"" + platform.ToString() + "\".");
            }

            // This list will hold the loaded plugin metadata
            List<PluginMetadata> metadata = new List<PluginMetadata>();

            var assemblies = from p in Directory.GetFiles(assemblyPath, "MISD.Plugins.*.dll")
                             select Assembly.LoadFrom(p);

            //Extract the assembly info, in order to fill the metadata
            foreach (var assembly in assemblies)
            {
                PluginMetadata data = new PluginMetadata();

                // Load the assembly attributes...
                var attributesData = assembly.GetCustomAttributesData();

                foreach (var attribute in attributesData)
                {
                    if (attribute.Constructor.DeclaringType == typeof(AssemblyCompanyAttribute))
                    {
                        data.Company = attribute.ConstructorArguments.First().ToString().RemoveQuotationMarks();
                    }
                    if (attribute.Constructor.DeclaringType == typeof(AssemblyTitleAttribute))
                    {
                        data.Name = attribute.ConstructorArguments.First().ToString().RemoveQuotationMarks();
                    }
                    if (attribute.Constructor.DeclaringType == typeof(AssemblyDescriptionAttribute))
                    {
                        data.Description = attribute.ConstructorArguments.First().ToString().RemoveQuotationMarks();
                    }
                    if (attribute.Constructor.DeclaringType == typeof(AssemblyDescriptionAttribute))
                    {
                        data.Description = attribute.ConstructorArguments.First().ToString().RemoveQuotationMarks();
                    }
                    if (attribute.Constructor.DeclaringType == typeof(AssemblyProductAttribute))
                    {
                        data.Product = attribute.ConstructorArguments.First().ToString().RemoveQuotationMarks();
                    }
                    if (attribute.Constructor.DeclaringType == typeof(AssemblyCopyrightAttribute))
                    {
                        data.Copyright = attribute.ConstructorArguments.First().ToString().RemoveQuotationMarks();
                    }
                }

                data.Version = assembly.GetName().Version;
                data.FileName = new FileInfo(assembly.Location).Name;

                if (platform != Platform.Visualization)
                {
                    // Get the indicator values.
                    var pluginType = typeof(IPlugin);
                    foreach (var t in assembly.GetTypes())
                    {
                        if (pluginType.IsAssignableFrom(t) && t.IsClass)
                        {
                            // This is an IPlugin child, get the indicator settings method...
                            var getIndicatorSettings = t.GetMethod("GetIndicatorSettings");

                            // ...and invoke it!
                            data.Indicators = (List<IndicatorSettings>)getIndicatorSettings.Invoke(Activator.CreateInstance(t), null);

                            // There should be only one plugin per assembly, so ignore the rest!
                            break;
                        }
                    }
                }

                metadata.Add(data);
            }

            return metadata;
        }

        /// <summary>
        /// Returns a list of all available plugins for a specific monitored systems.
        /// </summary>
        public List<Core.PluginMetadata> GetPluginList(int monitoredSystemID)
        {
            using (var dataContext = Database.DataContextFactory.CreateReadOnlyDataContext())
            {
                try
                {
                    // Get the platform of the monitored system.
                    var platform = Database.PrecompiledQueries.GetMonitoredSystemPlatformByID(dataContext, monitoredSystemID);

                    // Returns the plugin metadata as list.
                    return Database.PrecompiledQueries.GetCorePluginMetadataByMonitoredSystemIDAndPlatform(dataContext, monitoredSystemID, platform).ToList();
                }
                catch (Exception e)
                {
                    // Log the exception
                    Logger.Instance.WriteEntry("PluginManager_GetPluginList: Can't create plugin list for " + monitoredSystemID + ". " + e.ToString(), LogType.Exception);

                    // Return an empty list
                    return new List<PluginMetadata>();
                }
            }
        }

        /// <summary>
        /// This method reads a plugin assembly from file and returns a PluginFile instance.
        /// </summary>
        private PluginFile DownloadPlugin(Platform platform, string pluginName)
        {
            string fileExtension = ".dll";
            string pluginPlatformPath = "";
            string fileName = "";

            PluginFile requestedPlugin;

            // Determine the correct filename extension.
            if (platform == Platform.Windows)
            {
                pluginPlatformPath = this.PluginPathWorkstationWindows;
                fileName = "MISD.Plugins.Windows." + pluginName;
            }
            else if (platform == Platform.Linux)
            {
                pluginPlatformPath = this.PluginPathWorkstationLinux;
                fileName = "MISD.Plugins.Linux." + pluginName;
            }
            else if (platform == Platform.Server)
            {
                pluginPlatformPath = this.PluginPathServer;
                fileName = "MISD.Plugins.Server." + pluginName;
            }
            else if (platform == Platform.HPC)
            {
                pluginPlatformPath = this.PluginPathClusterHPC;
                fileName = "MISD.Plugins.HPC." + pluginName;
            }
            else if (platform == Platform.Bright)
            {
                pluginPlatformPath = this.PluginPathClusterBright;
                fileName = "MISD.Plugins.Bright." + pluginName;
            }
            else if (platform == Platform.Visualization)
            {
                pluginPlatformPath = this.PluginPathVisualisation;
                fileName = "MISD.Plugins.Visualization." + pluginName;
            }
            else
            {
                // Create a log entry.
                string errorMessage = "PluginManager_DownloadPlugin: LoadPluginFile: Ivalid operating system of workstation: " + platform.ToString() + ".";
                MISD.Core.Logger.Instance.WriteEntry(errorMessage, LogType.Exception);
                //if the query fails, the result is a empty list.
                return null;
            }

            //load the requested plugins into the result list.
            System.IO.FileStream inFile;
            string pluginFilePath;
            string base64Plugin;

            try
            {
                //load pluginfile
                pluginFilePath = pluginPlatformPath + @"\" + fileName + fileExtension;
                inFile = new System.IO.FileStream(
                    pluginFilePath,
                    System.IO.FileMode.Open,
                    System.IO.FileAccess.Read);

                byte[] binaryData = new Byte[inFile.Length];

                long bytesRead = inFile.Read(
                    binaryData,
                    0,
                    (int)inFile.Length);
                //close file
                inFile.Close();

                //convert pluginfile into Base64 UUEncoded output.
                base64Plugin = System.Convert.ToBase64String(
                    binaryData,
                    0,
                    binaryData.Length);

                //add pluginfile to the list
                requestedPlugin = new Core.PluginFile()
                {
                    FileName = fileName + fileExtension,
                    FileAsBase64 = base64Plugin
                };
            }
            catch (Exception e)
            {
                //create log
                string errorMessage = "PluginManager_DownloadPlugin: LoadPluginFile: Can't load plugin " + pluginName + ". " + e.ToString();
                MISD.Core.Logger.Instance.WriteEntry(errorMessage, LogType.Exception);
                //if the query fails, the result is a empty list.
                return null;
            }

            return requestedPlugin;
        }

        /// <summary>
        /// Downloads the plugins for the given platform and the given plugin names.
        /// </summary>
        public List<PluginFile> DownloadPlugins(Platform platform, IEnumerable<string> pluginNames)
        {
            using (var dataContext = Database.DataContextFactory.CreateReadOnlyDataContext())
            {
                var requestedFiles = new List<PluginFile>();
                string lastPluginLoaded = "[plugin unkown]";

                foreach (string pluginName in pluginNames)
                {
                    //safe pluginname for logging
                    lastPluginLoaded = pluginName;

                    //load plugin
                    PluginFile plugin = Manager.PluginManager.Instance.DownloadPlugin(platform, pluginName);
                    if (plugin != null)
                    {
                        requestedFiles.Add(plugin);
                    }
                    else
                    {
                        // Log the exception
                        MISD.Core.Logger.Instance.WriteEntry("PluginManager_DownloadPlugins: Can't load the plugin file " + lastPluginLoaded + " for platform " + platform + ".", LogType.Exception);

                        // Return an empty list.
                        requestedFiles.Clear();
                        return requestedFiles;
                    }
                }
                return requestedFiles;
            }
        }

        #endregion
    }
}

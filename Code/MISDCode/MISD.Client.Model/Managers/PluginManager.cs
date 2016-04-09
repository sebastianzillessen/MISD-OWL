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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;
using System.Reflection;
using System.IO;

using MISD.Client.Model;
using MISD.Client.Model.Synchronization;
using MISD.Client.Model.Properties;
using System.Windows;
using System.Windows.Threading;
using MISD.Core;


namespace MISD.Client.Managers
{
    public class PluginManager
    {
        #region Singleton

        /// <summary>
        /// Instance of the singleton plugin manager
        /// </summary>
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

        #endregion

        #region Fields

        /// <summary>
        /// Plugin Containter
        /// </summary>
        private CompositionContainer pluginContainer = null;

        /// <summary>
        /// Aggreagate catalog
        /// </summary>
        private AggregateCatalog aggregateCatalog = new AggregateCatalog();

        /// <summary>
        /// this defines where the plugins are stored. As we run a service, we have to get the assembly location first.
        /// </summary>
        string PluginPath
        {
            get
            {
                return Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) + Path.DirectorySeparatorChar + "Plugins";
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// List of the plugins
        /// </summary>
        [ImportMany(AllowRecomposition = true)]
        public IEnumerable<IPluginVisualization> Plugins { get; set; }

        [ImportMany(AllowRecomposition = true)]
        public IEnumerable<ITileCustomUI> TileCustomUIs { get; set; }

        #endregion

        #region Constructors

        private PluginManager()
        {
            this.LoadAvailablePlugins();

            //create directory
            if (!Directory.Exists(PluginPath))
            {
                Directory.CreateDirectory(PluginPath);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            this.isInitialSync = false;
            await this.UpdatePlugins();
        }

        /// <summary>
        /// Gets all plugins, that are currently loaded.
        /// </summary>
        /// <returns>List of all plugins.</returns>
        public List<IPluginVisualization> GetLoadedPlugins()
        {
            if (this.Plugins != null)
            {
                return this.Plugins.ToList();
            }
            else
            {
                return new List<IPluginVisualization>();
            }
        }

        private List<ResourceDictionary> LocalResourceDictionaries = new List<ResourceDictionary>();
        private bool isInitialSync = true;

        /// <summary>
        /// Load the local available plugins.
        /// </summary>
        /// <returns>All Plugins</returns>
        public List<IPluginVisualization> LoadAvailablePlugins()
        {
            Boolean result = true;
            //Intitalizes plugin container

            var catalog = new AggregateCatalog();

            if (!Directory.Exists(PluginPath))
            {
                Directory.CreateDirectory(PluginPath);
            }
            catalog.Catalogs.Add(new DirectoryCatalog(PluginPath, "MISD.*.dll"));
            pluginContainer = new CompositionContainer(catalog);

            //Fill the imports of this object
            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    this.pluginContainer.ComposeParts(this);

                    var resourceDictionaries = (from p in this.Plugins
                                                let assemblyName = p.GetType().Namespace
                                                let path = "pack://application:,,,/" + assemblyName + ";component/Generic.xaml"
                                                select new ResourceDictionary() { Source = new Uri(path) }).ToList();

                    resourceDictionaries.ForEach(p => Application.Current.Resources.MergedDictionaries.Add(p));

                    this.OnPluginsChanged();
                }));

            }
            catch (CompositionException e)
            {
                ClientLogger.Instance.WriteEntry("Can't create the plugin container. \n" + e, LogType.Exception);
            }

            if (result)
            {
                return this.Plugins.ToList();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Check for new plugins on the MISD server, download of the new plugins and load them.
        /// </summary>
        /// <returns>Empty list if new plugins are loaded, otherwise all plugins.</returns>
        public async Task<List<IPluginVisualization>> UpdatePlugins()
        {
            ClientWebServiceClient clientWebService = DataModel.Instance.ClientWebService;

            List<string> pluginsToChange = new List<string>();

            //download plugin metadata
            var metadata = await clientWebService.GetPluginListAsync("Visualization");

            //check for changes and delete old plugins
            if (metadata == null)
            {
                ClientLogger.Instance.WriteEntry("PluginManager_UpdatePlugins: Error in GetPluginList, object was null. \n", LogType.Warning);
            }
            else
            {
                foreach (var data in metadata)
                {
                    if (!this.PluginIsUptodate(data))
                    {
                        pluginsToChange.Add(data.Name);
                    }
                }
            }

            // check if new plugins are loaded
            if (pluginsToChange.Count > 0)
            {
                this.ReleasePluginContainer();

                foreach (string pluginName in pluginsToChange)
                {
                    //delete plugin
                    this.DeletePlugin(this.Plugins.FirstOrDefault(p => p.Name == pluginName));
                }

                //download new plugins
                var pluginFiles = await clientWebService.DownloadPluginsAsync(pluginsToChange);

                //store new plugins
                Boolean result = true;
                foreach (var file in pluginFiles)
                {
                    result = result && this.StorePlugin(file);
                }

                //load all available plugins
                this.LoadAvailablePlugins();

                //send event
                this.OnPluginsChanged();

                return this.Plugins.ToList();
            }
            else
            {
                return new List<IPluginVisualization>();
            }
        }

        /// <summary>
        /// Store a plugin to the assembly folder
        /// </summary>
        /// <param name="plugin">Plugin to store</param>
        /// /// <returns>Result of this method</returns>
        private Boolean StorePlugin(MISD.Core.PluginFile plugin)
        {
            // Convert the Base64 UUEncoded input into binary output.
            byte[] binaryData;
            try
            {
                binaryData = System.Convert.FromBase64String(plugin.FileAsBase64);
            }
            catch (System.ArgumentNullException e1)
            {
                ClientLogger.Instance.WriteEntry("Can't store the plugin. \n" + e1, LogType.Exception);
                return false;
            }
            catch (System.FormatException e2)
            {
                ClientLogger.Instance.WriteEntry("Can't store the plugin. \n" + e2, LogType.Exception);
                return false;
            }

            // Write out the decoded data.
            System.IO.FileStream outFile;
            string outputFileName =
                PluginPath +
                Path.DirectorySeparatorChar +
                plugin.FileName;

            try
            {
                outFile = new System.IO.FileStream(outputFileName,
                                           System.IO.FileMode.Create,
                                           System.IO.FileAccess.Write);
                outFile.Write(binaryData, 0, binaryData.Length);
                outFile.Close();
            }
            catch (System.Exception e)
            {
                ClientLogger.Instance.WriteEntry("Can't store the plugin. \n" + e, LogType.Exception);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if the plugin is consistent to the plugin metadata
        /// </summary>
        /// <param name="metadata">Metadata of a plugin</param>
        /// <returns>True if the plugin is available and the version of the plugin and the metadata is consistent</returns>
        private Boolean PluginIsUptodate(MISD.Core.PluginMetadata metadata)
        {
            //get the plugin of the given metadata
            var myPlugin = this.Plugins.FirstOrDefault(p => p.Name == metadata.Name);

            if (myPlugin == null)
            {
                return false;
            }
            else
            {
                bool result = myPlugin.GetVersion() == metadata.Version;
                return result;
            }
        }

        /// <summary>
        /// Delete the assembly
        /// </summary>
        /// <param name="plugin">Plugin to delete</param>
        /// <returns>Result of this method</returns>
        private Boolean DeletePlugin(IPluginVisualization plugin)
        {
            if (plugin != null)
            {
                //delete the assembly file
                string filePath =
                    PluginPath +
                    Path.DirectorySeparatorChar +
                    MISD.Client.Model.Properties.Settings.Default.PluginFileBase + plugin.Name +
                    ".dll";
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                    }
                    catch (System.IO.IOException e)
                    {
                        ClientLogger.Instance.WriteEntry("Can't delete the plugin. \n" + e, LogType.Exception);
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }


            return true;
        }

        /// <summary>
        /// Stops the timer jobs and release the container
        /// </summary>
        /// <returns>Result of this method</returns>
        private Boolean ReleasePluginContainer()
        {
            Boolean result = true;

            //release all container items
            try
            {
                var toRelease = pluginContainer.GetExports<IPluginVisualization>();
                this.pluginContainer.ReleaseExports<IPluginVisualization>(toRelease);
            }
            catch (Exception e)
            {
                ClientLogger.Instance.WriteEntry("Can't release the plugin container. \n", LogType.Exception);
                result = false;
            }

            return result;
        }

        #endregion
    }
}

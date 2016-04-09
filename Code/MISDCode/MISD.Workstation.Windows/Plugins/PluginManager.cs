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
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;
using System.IO;

using MISD.Core;
using MISD.Workstation.Windows.Scheduling;
using System.Reflection;

namespace MISD.Workstation.Windows.Plugins
{
    public class PluginManager
    {
        #region Singleton

        /// <summary>
        /// determins wether it is the first update cycle
        /// </summary>
        private bool initialUpdate = true;

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

        #region Properties

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
        private string pluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Properties.Settings.Default.pluginPath;

        /// <summary>
        /// List of the plugins
        /// </summary>
        [ImportMany(AllowRecomposition=true)]
        IEnumerable<IPlugin> plugins = null;

        #endregion

        #region Constructors

        private PluginManager()
        {
            this.LoadAvailablePlugins();

            //create directory
            if (!Directory.Exists(pluginPath))
            {
                WorkstationLogger.WriteLog("Creating: " + pluginPath);
                Directory.CreateDirectory(pluginPath);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all plugins, that are currently loaded.
        /// </summary>
        /// <returns>List of all plugins.</returns>
        public List<IPlugin> GetLoadedPlugins()
        {
            return this.plugins.ToList();
        }

        /// <summary>
        /// Load the available plugins.
        /// </summary>
        /// <returns>All Plugins</returns>
        public List<IPlugin> LoadAvailablePlugins()
        {
            Boolean result = true;
            //Intitalizes plugin container

            var catalog = new AggregateCatalog();

            if (!Directory.Exists(pluginPath))
            {
                Directory.CreateDirectory(pluginPath);
            }
            catalog.Catalogs.Add(new DirectoryCatalog(pluginPath));
            pluginContainer = new CompositionContainer(catalog);

            //Fill the imports of this object
            try
            {
                this.pluginContainer.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                //logging
                var messageEx1 = new StringBuilder();
                messageEx1.Append(ServerConnection.GetWorkstationName() + "_PluginManager_LoadAvailablePlugins: ");
                messageEx1.Append("Can't create the plugin container. " + compositionException.ToString());
                ServerConnection.WriteLog(messageEx1.ToString(), LogType.Exception);
                result = false;
            }

            if (result)
            {
                return this.plugins.ToList();
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
        public List<IPlugin> UpdatePlugins()
        {
            List<string> pluginsToChange = new List<string>();

            //download plugin metadata
            var metadata = ServerConnection.GetPluginList();

            //check for changes and delete old plugins
            if (metadata == null)
            {
                ServerConnection.WriteLog(
                    ServerConnection.GetWorkstationName() +
                    "_PluginManager_UpdatePlugins: Error in GetPluginList, object was null.", LogType.Exception);
            }
            else
            {
                foreach (PluginMetadata data in metadata)
                {
                    WorkstationLogger.WriteLog("Received metadata for: " + data.Name);
                    if (!this.PluginIsUptodate(data))
                    {
                        pluginsToChange.Add(data.Name);
                    }
                } 
            }

            // check if new plugins are loaded
            WorkstationLogger.WriteLog("Updating " + pluginsToChange.Count + " plugins");
            if (pluginsToChange.Count > 0)
            {
                this.ReleasePluginContainer(); 

                foreach (string pluginName in pluginsToChange)
                {
                    //delete plugin
                    this.DeletePlugin(this.plugins.FirstOrDefault(p => p.GetName() == pluginName));
                }

                //download new plugins
                WorkstationLogger.WriteLog("Downloading " + pluginsToChange.Count + " plugins");
                var pluginFiles = ServerConnection.DownloadPlugins(pluginsToChange.ToArray());

                //store new plugins
                Boolean result = true;
                foreach (PluginFile file in pluginFiles)
                {
                    result = result && this.StorePlugin(file);
                }

                //load all available plugins
                this.LoadAvailablePlugins();

                return this.plugins.ToList();
            }
            else
            {
                return new List<IPlugin>();
            }
        }

        /// <summary>
        /// Store a plugin to the assembly folder
        /// </summary>
        /// <param name="plugin">Plugin to store</param>
        /// /// <returns>Result of this method</returns>
        private Boolean StorePlugin(PluginFile plugin)
        {
            // Convert the Base64 UUEncoded input into binary output.
            byte[] binaryData;
            try
            {
                binaryData = System.Convert.FromBase64String(plugin.FileAsBase64);
            }
            catch (System.ArgumentNullException)
            {
                //Logging "Base 64 string is null."
                var messageEx1 = new StringBuilder();
                messageEx1.Append(ServerConnection.GetWorkstationName() + "_PluginManager_StorePlugin: ");
                messageEx1.Append("Can't store the plugin. " + "Base 64 string is null.");
                ServerConnection.WriteLog(messageEx1.ToString(), LogType.Exception);
                return false;
            }
            catch (System.FormatException)
            {
                //Logging "Base 64 string length is not 4 or is not an even multiple of 4."
                var messageEx2 = new StringBuilder();
                messageEx2.Append(
                    ServerConnection.GetWorkstationName() + 
                    "Base 64 string length is not 4 or is not an even multiple of 4.");
                messageEx2.Append("Can't store the plugin. " + "Base 64 string is null.");
                ServerConnection.WriteLog(messageEx2.ToString(), LogType.Exception);
                return false;
            }

            // Write out the decoded data.
            System.IO.FileStream outFile;
            string outputFileName =
                pluginPath +
                Path.DirectorySeparatorChar +
                plugin.FileName;

            WorkstationLogger.WriteLog("Storing plugin into: " + outputFileName);
                
            try
            {
                outFile = new System.IO.FileStream(outputFileName,
                                           System.IO.FileMode.Create,
                                           System.IO.FileAccess.Write);
                outFile.Write(binaryData, 0, binaryData.Length);
                outFile.Close();
            }
            catch (System.Exception exp)
            {
                //Logging
                //Logging "Base 64 string is null."
                var messageEx3 = new StringBuilder();
                messageEx3.Append(ServerConnection.GetWorkstationName() + "_PluginManager_StorePlugin: ");
                messageEx3.Append("Can't store the plugin. " + exp.ToString());
                ServerConnection.WriteLog(messageEx3.ToString(), LogType.Exception);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if the plugin is consistent to the plugin metadata
        /// </summary>
        /// <param name="metadata">Metadata of a plugin</param>
        /// <returns>True if the plugin is available and the version of the plugin and the metadata is consistent</returns>
        private Boolean PluginIsUptodate(PluginMetadata metadata)
        {
            //get the plugin of the given metadata
            var myPlugin = this.plugins.FirstOrDefault(p => p.GetName() == metadata.Name);

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
        private Boolean DeletePlugin(Core.IPlugin plugin)
        {
            if (plugin != null)
            {
                //delete the assembly file
                string filePath =
                    pluginPath + 
                    Path.DirectorySeparatorChar +
                    Properties.Settings.Default.pluginFileBase + plugin.GetName() +
                    ".dll";
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                    }
                    catch (System.IO.IOException e)
                    {
                        //Logging
                        //Logging "Base 64 string is null."
                        var messageEx1 = new StringBuilder();
                        messageEx1.Append(ServerConnection.GetWorkstationName() + "_PluginManager_DeletePlugin: ");
                        messageEx1.Append("Can't delete the plugin. " + e.ToString());
                        ServerConnection.WriteLog(messageEx1.ToString(), LogType.Exception);
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

            // don't try to access Scheduler.instance at the first update, it is null!
            if (!initialUpdate) //TODO check if nesseccary
            {
                //stop the timer jobs, if any of them are started
                if (Scheduler.started)
                {
                    foreach (IPlugin p in plugins)
                    {
                        Scheduler.Instance.StopPluginScheduling(p);
                    }
                }
            }
            else
            {
                initialUpdate = false;
            }


            //release all container items
            try
            {
                var toRelease = pluginContainer.GetExports<IPlugin>();
                this.pluginContainer.ReleaseExports<IPlugin>(toRelease);
            }
            catch (Exception e)
            {
                //logging
                //Logging "Base 64 string is null."
                var messageEx1 = new StringBuilder();
                messageEx1.Append(ServerConnection.GetWorkstationName() + "_PluginManager_ReleasePluginContainer: ");
                messageEx1.Append("Can't release the plugin container. " + e.ToString());
                ServerConnection.WriteLog(messageEx1.ToString(), LogType.Exception);

                result = false;
            }

            return result;
        }

        #endregion
    }
}

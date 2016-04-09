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
using MISD.Workstation.Linux.Scheduling;

namespace MISD.Workstation.Linux.Plugins
{
    public class PluginManager
    {
        #region Singleton

        /// <summary>
        /// Instance of the singelton plugin manager
        /// </summary>
        private static volatile PluginManager instance;

		/// <summary>
		/// The sync root for locking.
		/// </summary>
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
        /// List of the plugins
        /// </summary>
        [ImportMany(AllowRecomposition=true)]
        private IEnumerable<IPlugin> plugins = null;

        #endregion

        #region Constructors

        private PluginManager()
        {
            //create directory
            string directoryPath = Properties.Settings.Default.pluginPath;
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
			
			this.LoadAvailablePlugins();

            
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
			var path = Properties.Settings.Default.pluginPath;
			catalog.Catalogs.Add(new AssemblyCatalog(typeof(PluginManager).Assembly));
			catalog.Catalogs.Add(new DirectoryCatalog(path));
            pluginContainer = new CompositionContainer(catalog);

            //Fill the imports of this object
            try
            {
                this.pluginContainer.ComposeParts(this);
            }
            catch (CompositionException e)
            {
				WorkstationLogger.Instance.WriteLog (ServerConnection.Instance.GetWorkstationName () + "_PluginManager_LoadAvailablePlugins: " + e.Message, MISD.Core.LogType.Exception, true);
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

            //download plugin metadatas
            var metadata = ServerConnection.Instance.GetPluginList();
			
			// Logging
			PluginMetadata[] updatePluginMetaData = metadata;
			WorkstationLogger.Instance.WriteLog ("updated PluginMetadata: "  + updatePluginMetaData.Length.ToString(), MISD.Core.LogType.Debug, false);

            //check for changes and delete old plugins
			if (metadata == null){
				WorkstationLogger.Instance.WriteLog (ServerConnection.Instance.GetWorkstationName () + "_PluginManager_UpdatePlugins: Error in GetPluginList, object was null", LogType.Exception, true);
			}
			else{
				
				foreach (PluginMetadata data in metadata)
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
                    this.DeletePlugin(this.plugins.FirstOrDefault(p => p.GetName() == pluginName));
                }

                //download new plugins
                var pluginFiles = ServerConnection.Instance.DownloadPlugins(pluginsToChange.ToArray());

				if (pluginFiles != null)
				{
					// Logging
					PluginFile[] downloadedPlugins = pluginFiles;
					WorkstationLogger.Instance.WriteLog ("downloaded plugins: " + downloadedPlugins.Length, MISD.Core.LogType.Debug, false);

	                //store new plugins
	                Boolean result = true;
	                foreach (PluginFile file in pluginFiles)
	                {				
	                    result = result && this.StorePlugin(file);
	                }
				}
				else
				{
					WorkstationLogger.Instance.WriteLog ("Downloaded PluginFiles were null", LogType.Exception, false);
				}

                //load all available plugins
                List<IPlugin> loadedPlugins = this.LoadAvailablePlugins();
				WorkstationLogger.Instance.WriteLog ("loaded Plugins: " + loadedPlugins.Count.ToString(), MISD.Core.LogType.Debug, false);

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
            catch (System.ArgumentNullException e)
            {
				WorkstationLogger.Instance.WriteLog (ServerConnection.Instance.GetWorkstationName () + "_PluginManager_StorePlugin: " + e.Message, LogType.Exception, true);
                return false;
            }
            catch (System.FormatException e)
            {
				WorkstationLogger.Instance.WriteLog (ServerConnection.Instance.GetWorkstationName () + "_PluginManager_StorePlugin: " + e.Message, LogType.Exception, true);
                return false;
            }

            // Write out the decoded data.
            System.IO.FileStream outFile;
            string outputFileName =
                Properties.Settings.Default.pluginPath + Path.DirectorySeparatorChar + 
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
				WorkstationLogger.Instance.WriteLog (ServerConnection.Instance.GetWorkstationName () + "_PluginManager_StorePlugin: " + e.Message, LogType.Exception, true);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if the plugin ist konsistent to the plugin metadata
        /// </summary>
        /// <param name="metadata">Metadata of a plugin</param>
        /// <returns>True if the plugin is available and the version of the plugin and the metadata is consistent</returns>
        private Boolean PluginIsUptodate(PluginMetadata metadata)
        {
            //get the plugin of the given metadata
            var myPlugin = this.plugins.FirstOrDefault(p => p.GetName() == metadata.Name);
			if (myPlugin == null)
				return false;
			else 
            	return (myPlugin.GetVersion() == metadata.Version);
        }

        /// <summary>
        /// Delete the assembly
        /// </summary>
        /// <param name="plugin">Plugin to delete</param>
        /// <returns>Result of this method</returns>
        private Boolean DeletePlugin(Core.IPlugin plugin)
        {
			if (plugin != null) {
	            //delete the assembly file
	            string filePath = 
	                Properties.Settings.Default.pluginPath + Path.DirectorySeparatorChar +
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
						WorkstationLogger.Instance.WriteLog (ServerConnection.Instance.GetWorkstationName () + "_PluginManager_DeletePlugin: " + e.Message, LogType.Exception, true);
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

            //stop the timer jobs only if any job was started
			if (Scheduler.started)
			{
	            foreach (IPlugin p in plugins)
	            {
	                Scheduler.Instance.StopPluginScheduling(p);
	            }
			}

            //release all container items
            try
            {
                var toRelease = pluginContainer.GetExports<IPlugin>();
                this.pluginContainer.ReleaseExports<IPlugin>(toRelease);
            }
            catch (Exception e)
			{
				WorkstationLogger.Instance.WriteLog (ServerConnection.Instance.GetWorkstationName () + "_PluginManager_ReleasePluginContainer: " + e.Message, LogType.Exception, true);
                result = false;
			}

            return result;
        }

        #endregion
    }
}

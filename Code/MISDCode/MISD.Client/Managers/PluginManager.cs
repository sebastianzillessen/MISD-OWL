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

using MISD.Client.Core;

namespace MISD.Client.Managers
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
        [ImportMany(AllowRecomposition = true)]
        IEnumerable<IPluginVisualization> plugins = null;

        #endregion

        #region Constructors

        private PluginManager()
        {
        }

        #endregion


    }
}

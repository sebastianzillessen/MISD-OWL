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
using MISD.Core.Scheduling;
using MISD.Server.Database;
using MISD.Server.Properties;
using System.Collections.Generic;
using System.Linq;
using MISD.Server.Scheduling;

namespace MISD.Server.Scheduling
{
    /// <summary>
    /// This scheduler is responsible for the management of all cleaner jobs.
    /// </summary>
    public class CleanerJobScheduler : SchedulerBase
    {
        #region Singleton

        private static volatile CleanerJobScheduler instance;
        private static object syncRoot = new Object();

        /// <summary>
        /// Property to access the class.
        /// </summary>
        public static CleanerJobScheduler Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new CleanerJobScheduler();
                    }
                }

                return instance;
            }
        }

        #endregion
        
        #region INotifyPropertyChanged

        private void ApplicationSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CleanerJobInterval")
            {
                // If the global cleaner job time interval changes, update the job's internal timers.
                this.RefreshJobs();
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the CleanerJobScheduler class.
        /// </summary>
        private CleanerJobScheduler()
        {
            // Register for the application settings's property changed event.
            Settings.Default.PropertyChanged += ApplicationSettings_PropertyChanged;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the cleaner job scheduler.
        /// </summary>
        protected override void Initialize()
        {
            using (var dataContext = DataContextFactory.CreateReadOnlyDataContext())
            {

                // Create the cleaner jobs.
                this.Jobs.Add(new CleanerJob());
                this.Start();
            }
        }

        /// <summary>
        /// Refreshes the intervals of the internal timer jobs.
        /// </summary>
        public override void RefreshJobs()
        {
            foreach (var job in this.Jobs) job.Interval = Settings.Default.CleanerJobInterval;
        }

        #endregion
    }
}

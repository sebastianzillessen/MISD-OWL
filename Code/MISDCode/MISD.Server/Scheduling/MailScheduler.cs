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
using System.Threading;
using MISD.Core.Scheduling;
using MISD.Server.Scheduling;
using MISD.Server.Manager;
using MISD.Server.Properties;

namespace MISD.Server.Scheduling
{
    public class MailScheduler : SchedulerBase
    {
        #region Singleton

        private static MailScheduler instance = new MailScheduler();

        /// <summary>
        /// Property to access the class.
        /// </summary>
        public static SchedulerBase Instance
        {
            get
            {
                return instance;
            }
        }

        #endregion

        #region Constructors

        private MailScheduler()
        {
            Settings.Default.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Default_PropertyChanged);
        }

        void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DailyMailInterval")
            {
                this.RefreshJobs();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the mail scheduler.
        /// </summary>
        protected override void Initialize()
        {          
            this.Jobs.Add(new DailyMailTimerJob(Properties.Settings.Default.DailyMailInterval));
        }

        /// <summary>
        /// Refreshes all timer jobs.
        /// </summary>
        public override void RefreshJobs()
        {
            foreach (TimerJobBase job in this.Jobs)
            {
                if (job.ID == "DailyMailTimerJob")
                {
                    job.Interval = Properties.Settings.Default.DailyMailInterval;
                }
            }
        }

        #endregion
    }
}

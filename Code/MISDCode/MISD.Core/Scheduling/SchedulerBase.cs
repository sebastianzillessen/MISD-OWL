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

namespace MISD.Core.Scheduling
{
    /// <summary>
    /// Provides scheduling functionality for a variable number of timer jobs.
    /// </summary>
    public abstract class SchedulerBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the timer jobs.
        /// </summary>
        protected List<TimerJobBase> Jobs
        {
            get;
            set;
        }
		
		public static Boolean started = false;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the Scheduler class.
        /// </summary>
        protected SchedulerBase()
        {
            this.Jobs = new List<TimerJobBase>();

            // Create the timer jobs.
            this.Initialize();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the scheduler, normally by creating the necessary timer jobs.
        /// This method will be called automatically by the constructor of the SchedulerBase class.
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        /// Starts all timer jobs.
        /// </summary>
        public virtual void Start()
        {
			started = true;
            foreach (var job in this.Jobs) job.Start();
        }

        /// <summary>
        /// Stops all timer jobs.
        /// </summary>
        public virtual void Stop()
        {
            foreach (var job in this.Jobs) job.Stop();
			started = false;
        }

        /// <summary>
        /// Refreshes the scheduler when important things change.
        /// </summary>
        public abstract void RefreshJobs();

        /// <summary>
        /// Refreshes all update intervals.
        /// </summary>
        /// <param name="intervals">A dictionary contatinig ID | update interval.</param>
        public void RefreshUpdateInterval(Dictionary<string, TimeSpan> intervals)
        {
            foreach (TimerJobBase currentJob in Jobs)
            {
                foreach (KeyValuePair<string, TimeSpan> currentInterval in intervals)
                {
                    if (currentJob.ID.Equals(currentInterval.Key))
                    {
                        currentJob.Interval = currentInterval.Value;
                    }
                }
            }
        }

        #endregion
    }
}

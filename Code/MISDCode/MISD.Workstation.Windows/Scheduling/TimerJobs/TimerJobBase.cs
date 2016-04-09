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
using System.Timers;
using System.Windows.Threading;
using System.Threading;

namespace MISD.Workstation.Windows.Scheduling.TimerJobs
{
    /// <summary>
    /// Abstract base class for all kinds of timer jobs.
    /// </summary>
    public abstract class TimerJobBase : IDisposable
    {
        #region Properties

        /// <summary>
        /// Gets the ID of this timer job - consisting of "PluginName.IndicatorName".
        /// </summary>
        public string ID
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the timer that triggers the timer job execution.
        /// </summary>
        public DispatcherTimer Timer
        {
            get;
            set;
        }

        /// <summary>
        /// List of threads to do the work.
        /// </summary>
        List<Thread> WorkerThreads
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the timer tick interval.
        /// </summary>
        public TimeSpan Interval
        {
            get
            {
                return this.Timer.Interval;
            }
            set
            {
                if (value < TimeSpan.FromSeconds(1)) value = TimeSpan.FromSeconds(1);
                if (value != this.Timer.Interval)
                {
                    this.Timer.Interval = value;
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a timerjob.
        /// </summary>
        public TimerJobBase()
        {
            this.Timer = new DispatcherTimer(DispatcherPriority.Normal);
            this.Timer.Tick += new EventHandler(this.TimerTick);
            this.WorkerThreads = new List<Thread>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The method called each interval.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void TimerTick(object sender, EventArgs e)
        {
            // Get all running threads.
            this.WorkerThreads = (List<Thread>)(from p in this.WorkerThreads
                                                where p.ThreadState == ThreadState.Running
                                                select p);

            // Check if more than 4 threads are active, if so, kill one.
            if (this.WorkerThreads.Count >= 4)
            {
                var thread = this.WorkerThreads.ElementAt(0);
                thread.Abort();

                this.WorkerThreads = (List<Thread>)(from p in this.WorkerThreads
                                                    where p.ThreadState == ThreadState.Running
                                                    select p);
            }

            // Add the new thread.
            var newThread = new Thread(new ThreadStart(() => { this.TimerTickAsync(); }));
            this.WorkerThreads.Add(newThread);
            newThread.Start();
        }

        /// <summary>
        /// The method that is called during each timer tick.
        /// </summary>
        abstract protected void TimerTickAsync();

        /// <summary>
        /// Kills all threads (required for IDisposable interface).
        /// </summary>
        public void Dispose()
        {
            this.Timer.IsEnabled = false;

            while (this.WorkerThreads.Count > 0)
            {
                this.WorkerThreads.First().Abort();

                this.WorkerThreads = (from p in this.WorkerThreads
                                      where p.ThreadState == ThreadState.Running
                                      select p).ToList();
            }
        }

        #endregion
    }
}

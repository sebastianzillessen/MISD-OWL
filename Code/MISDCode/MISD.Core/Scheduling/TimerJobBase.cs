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
using System.Threading;
using System.ComponentModel;

namespace MISD.Core.Scheduling
{
    /// <summary>
    /// Abstract base class for all kinds of timer jobs.
    /// </summary>
    public abstract class TimerJobBase : IDisposable
    {
        #region Properties

        /// <summary>
        /// Gets the ID of this timer job.
        /// </summary>
        public string ID
        {
            get;
            protected set;
        }

        private TimeSpan interval = new TimeSpan(0, 30, 0);

        /// <summary>
        /// Gets or sets the timer tick interval.
        /// </summary>
        public TimeSpan Interval
        {
            get
            {
                return this.interval;
            }
            set
            {
                if (value < TimeSpan.FromSeconds(1))
                {
                    value = TimeSpan.FromSeconds(1);
                }
                if (value > TimeSpan.FromDays(7))
                {
                    value = TimeSpan.FromDays(7);
                }
                if (value != this.Interval)
                {
                    this.interval = value;
                }
            }
        }

        /// <summary>
        /// Boolean to indicate whether the timer is running.
        /// </summary>
        //private bool isStarted = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a timerjob.
        /// </summary>
        public TimerJobBase()
        {
        }

        #endregion

        #region Methods

        private Thread workerThread;

        // Track whether Dispose has been called.
        private bool disposed = false;

        private bool isStarted;
        public bool IsStarted
        {
            get
            {
                return isStarted;
            }
            set
            {
                if (this.isStarted != value)
                {
                    if (this.isStarted == false)
                    {
                        // Start the loop
                        workerThread = new Thread(new ThreadStart(Loop));
                        workerThread.Start();
                    }
                    else
                    {
                        workerThread.Abort();
                    }
                    this.isStarted = value;
                }
            }
        }

        protected abstract void Loop();

        public void Start()
        {
            this.IsStarted = true;
        }

        //private void Starter()
        //{
        //    this.Timer.Start();
        //}

        public void Stop()
        {
            this.IsStarted = false;
        }

        /// <summary>
        /// The method that is called during each timer tick.
        /// </summary>
        abstract protected void TimerTickAsync();

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        /// <summary>
        /// Kills all threads (required for IDisposable interface).
        /// </summary>
        public void Dispose()
        {
           Dispose(true);
           // Take yourself off the Finalization queue 
           // to prevent finalization code for this object
           // from executing a second time.
           GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
           // Here we don't differ between managed and unmanaged resources.

           // Check to see if Dispose has already been called.
           if(!this.disposed)
           {
               // Note that this is not thread safe.
               // Another thread could start disposing the object
               // after the managed resources are disposed,
               // but before the disposed flag is set to true.
               // If thread safety is necessary, it must be
               // implemented by the client.
               this.Stop();
           }
           disposed = true;         
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method 
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~TimerJobBase()      
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}

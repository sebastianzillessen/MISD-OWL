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
using System.Threading;
using System.Threading.Tasks;

namespace MISD.Client.Model.Managers
{
    public class WorkerThread
    {
        #region Fields

        private volatile bool threadShouldStop;
        private MISD.Client.Model.Managers.ThreadManager.MethodDelegate method;
        private MISD.Client.Model.Managers.ThreadManager.MethodDelegateWithObject methodWithParamObject;
        private object param;
        private Thread thread = null;
        private CountdownEvent threadStopped;
        private bool isRunning;
        private string name;
        private bool continous;

        private Task task = null;

        #endregion

        #region Constructors

        public WorkerThread(string name, MISD.Client.Model.Managers.ThreadManager.MethodDelegate method, bool continousThread)
        {
            this.name = name;
            this.isRunning = false;
            this.threadShouldStop = false;
            this.continous = continousThread;
            this.method = method;
            this.methodWithParamObject = null;
        }

        public WorkerThread (string name, MISD.Client.Model.Managers.ThreadManager.MethodDelegateWithObject method, object paramData, bool continousThread)
        {
            this.name = name;
            this.isRunning = false;
            this.threadShouldStop = false;
            this.continous = continousThread;
            this.methodWithParamObject = method;
            this.param = paramData;
            this.method = null;
        }

        #endregion

        #region Public methods

        public void Start()
        {
            if (this.continous)
            {
                if (this.thread == null)
                {
                    thread = new Thread(this.DoWork);

                    thread.Name = this.name;
                    thread.IsBackground = true;
                    thread.Start();
                    threadStopped = new CountdownEvent(1);
                    this.isRunning = true;
                }
                else
                {
                    // This case never occurs.
                }
            }
            else
            {
                this.task = Task.Factory.StartNew(this.DoWork);
                threadStopped = new CountdownEvent(1);
                this.isRunning = true;
            }
            
        }

        public void Stop()
        {
            this.threadShouldStop = true;
            
            if (this.thread != null)
            {
                this.thread.Interrupt();
            }

            if (this.task != null)
            {
                this.task.Wait(new CancellationToken(true));
            }
            
            this.threadStopped.Wait();
        }

        public bool IsRunning()
        {
            return this.isRunning;
        }

        #endregion

        #region Private methods

        private void DoWork()
        {
            if (this.thread != null)
            {
                Console.WriteLine("(+) STARTED Thread: " + this.name);
            }
            else if (this.task != null)
            {
                Console.WriteLine("(+) STARTED Task: " + this.name);
            }
            else
            {
                Console.WriteLine("(?) ERROR OCCURED IN THREADMANAGER");
            }
            
            try
            {
                do
                {
                    if (this.method != null)
                    {
                        this.method();
                    }
                    else
                    {
                        this.methodWithParamObject(this.param);
                    }
                } while (continous && !threadShouldStop);
            }
            catch (ThreadInterruptedException e1)
            {
                // Thread interrupted.
            }
            catch (ThreadAbortException e2)
            {
                // Thread interrupted.
            }
            catch (Exception e3)
            {
                ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e3, Core.LogType.Exception);
                ThreadManager.unexpectedClosedThreads.Add(this);
            }

            this.threadStopped.Signal();
            this.isRunning = false;
            ThreadManager.RemoveWorkerThread(this);

            if (this.thread != null)
            {
                Console.WriteLine("(-) STOPPED Thread: " + this.name);
            }
            else if (this.task != null)
            {
                Console.WriteLine("(-) STOPPED TASK: " + this.name);
            }
            else
            {
                Console.WriteLine("(?) ERROR OCCURED IN THREADMANAGER");
            }
        }

        #endregion

    }

    public class ThreadManager
    {

        #region Delegate definitions

        public delegate void MethodDelegate();
        public delegate void MethodDelegateWithObject(object param);

        #endregion

        #region Fields

        private static List<WorkerThread> myWorkerThreads = new List<WorkerThread>();
        public static List<WorkerThread> unexpectedClosedThreads = new List<WorkerThread>();

        #endregion

        #region Constructors

        private ThreadManager() {}

        #endregion

        #region Methods

        public static WorkerThread CreateWorkerThread(string name, MethodDelegate method, bool continous)
        {
            WorkerThread newWorkerThread = new WorkerThread(name, method, continous);
            myWorkerThreads.Add(newWorkerThread);
            newWorkerThread.Start();
            return newWorkerThread;
        }

        public static WorkerThread CreateWorkerThread(string name, MethodDelegateWithObject method, object data, bool continous)
        {
            WorkerThread newWorkerThread = new WorkerThread(name, method, data, continous);
            myWorkerThreads.Add(newWorkerThread);
            newWorkerThread.Start();
            return newWorkerThread;
        }

        public static void RemoveWorkerThread(WorkerThread workerThreadToRemove)
        {
            myWorkerThreads.Remove(workerThreadToRemove);
        }

        public static void KillAllThreads()
        {
            var temp = new List<WorkerThread>(myWorkerThreads);

            foreach (WorkerThread workerThreadToKill in temp)
            {
                workerThreadToKill.Stop();
            }

            while (myWorkerThreads.Count > 0)
            {
                Thread.Sleep(1000);
            }
        }

        #endregion
    }
}

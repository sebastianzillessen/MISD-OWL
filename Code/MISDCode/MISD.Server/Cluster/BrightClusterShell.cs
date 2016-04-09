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
using Renci.SshNet;
using System.Runtime.CompilerServices;
using System.Threading;
using MISD.Core;

namespace MISD.Server.Cluster
{
    public class BrightClusterShell : IDisposable
    {
        private ShellStream shellStream;

        private Dictionary<string, Tuple<DateTime, string>> oldResponses;
        private TimeSpan validTimeSpan;

        private SshClient client;
        public string password { get; private set; }
        public string username { get; private set; }
        public string url { get; private set; }

        // Track whether Dispose has been called.
        private bool disposed = false;

        /// <summary>
        /// Constructor for a bright cluster shell.
        /// </summary>
        /// <param name="url">The URL of the head node.</param>
        /// <param name="username">The username for the SSH conenction.</param>
        /// <param name="password">The password for the SSH connection.</param>
        public BrightClusterShell(string url, string username, string password)
        {
            try
            {
                this.url = url;
                this.password = password;
                this.username = username;

                oldResponses = new Dictionary<string, Tuple<DateTime, string>>();
                validTimeSpan = new TimeSpan(0, 1, 0);

                InitConnection();
                shellStream = client.CreateShellStream("terminal", 80, 24, 800, 600, 1024);
                string[] commands = new string[] { "module add cmsh", "cmsh -i cert.pfx -p 123456" };
                foreach (string c in commands)
                {
                    shellStream.WriteLine(c);
                }
            }
            catch (Exception e)
            {
                Logger.Instance.WriteEntry("BirghtClusterShell: Problem initilizing bright cluster, " + e.ToString(), LogType.Exception);
                throw e;
            }

        }

        /// <summary>
        /// Initializes the connection to the cluster management software.
        /// </summary>
        private void InitConnection()
        {
            if (client == null)
            {
                client = new SshClient(url, username, password);
            }
            client.ConnectionInfo.Timeout = new TimeSpan(0, 10, 0);
            client.Connect();
        }

        /// <summary>
        /// Method to join string to be able to cache results.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private string Join(List<string> c)
        {
            string r = "";
            foreach (string s in c)
            {
                r += s + "#";
            }
            return r;
        }

        /// <summary>
        /// Adds a new result to the cached results.
        /// </summary>
        /// <param name="res">The result of the commands.</param>
        /// <param name="commands">The commands.</param>
        private void AddResult(string res, List<string> commands)
        {
            // remove last commands, as it is "main" for purposes of the shellstream
            List<string> cleanCommands = commands;
            cleanCommands.RemoveAt(cleanCommands.Count - 1);
            string key = Join(cleanCommands);

            if (oldResponses.Count > 5000)
            {
                oldResponses.Clear();
            }
            else
            {
                if (oldResponses.ContainsKey(key))
                {
                    oldResponses.Remove(key);
                }
            }
            Tuple<DateTime, string> element = new Tuple<DateTime, string>(DateTime.Now, res);
            oldResponses.Add(key, element);
        }

        /// <summary>
        /// Returns the cached result if it's fresh enough.
        /// </summary>
        /// <param name="commands"></param>
        /// <returns></returns>
        private string GetResultIfFreshEnough(List<string> commands)
        {
            string c = Join(commands);
            Tuple<DateTime, string> existingResult;
            if (oldResponses.TryGetValue(c, out existingResult))
            {
                // we have an element in the oldResponses
                DateTime now = DateTime.Now;
                DateTime limit = now.Subtract(validTimeSpan);
                if (existingResult.Item1.Ticks >= limit.Ticks)
                {
                    return existingResult.Item2;
                }
                else
                {
                    oldResponses.Remove(c);
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Runs a list of commands on the SSH shell to get results.
        /// </summary>
        /// <param name="commands">The commands.</param>
        /// <returns>A string containg the whole result.</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public string RunCommands(List<string> commands)
        {
            if (!client.IsConnected)
            {
                InitConnection();
            }
            string oldRes = GetResultIfFreshEnough(commands);
            if (oldRes == null)
            {
                commands.Add("main");
                foreach (string c in commands)
                {
                    shellStream.WriteLine(c);
                }

                var line = shellStream.ReadLine();
                // skip lines with severity tags, detail information, unset information and basis lines (hestia1->main)
                while (line.Contains("[info]") ||
                       line.Contains("[notice]") ||
                       line.Contains("[warning]") ||
                       line.Contains("[error]") ||
                       line.Contains("[alert]") ||
                       line.Contains("For details type:") ||
                       line.Contains("Sysinfo not set.") ||
                       line.Contains("[hestia1->main]"))
                {
                    line = shellStream.ReadLine();
                }

                var res = "";

                while (line != null)
                {
                    res += line + "\r\n";
                    line = shellStream.ReadLine();
                    if (line.StartsWith("[hestia1->main]"))
                    {
                        break;
                    }
                }

                AddResult(res, commands);

                return res;
            }
            else
            {
                return oldRes;
            }
        }

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        /// <summary>
        /// Disposes the shellstream to implement IDisposable.
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
            if (!this.disposed && this.shellStream != null)
            {
                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.
                this.shellStream.Close();
            }
            disposed = true;
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method 
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~BrightClusterShell()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }
    }
}

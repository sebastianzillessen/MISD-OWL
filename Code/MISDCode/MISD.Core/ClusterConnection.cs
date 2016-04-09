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
using MISD.Core;

namespace MISD.Core
{
    /// <summary>
    /// This abstract class is used for implementing ClusterConnections to multiple Clusters. 
    /// Known extended classes are BrighClusterConnection and HPCClusterConnection.
    /// </summary>
    public abstract class ClusterConnection : IDisposable
    {
        /// <summary>
        /// Track whether Dispose has been called.
        /// </summary>
        protected bool disposed = false;

        /// <summary>
        /// Stores the Connection object.
        /// </summary>
        protected object ConnectionObject = null;

        /// <summary>
        /// Inits a connection to a cluster using the specified credentials
        /// </summary>
        /// <param name="url">the url to the cluster</param>
        /// <param name="username">the username to access the cluster</param>
        /// <param name="password">the password to access the cluster</param>
        public abstract void Init(string url, string username, string password);

        /// <summary>
        /// Copies the Connection to a new Instance to have multiple accesses
        /// </summary>
        /// <returns>A new instance of the cluster connection</returns>
        public abstract ClusterConnection CopyConnection();

        /// <summary>
        /// returns a list of monitoredSystemNames as FQDN 
        /// </summary>
        /// <returns></returns>
        public abstract List<WorkstationInfo> GetNodes();

        /// <summary>
        /// returns the connection object
        /// </summary>
        /// <returns></returns>
        public object GetConnection()
        {
            return this.ConnectionObject;
        }

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
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
            // Check to see if Dispose has already been called.
            if(!this.disposed)
            {
               // Has to be implemented by the child classes.
            }
            disposed = true;         
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method 
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~ClusterConnection()      
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }
    }
}

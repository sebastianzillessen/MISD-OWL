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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MISD.TCPUtil
{
    /// <summary>
    /// This class provides the tcp conncetion for the powerwall communication with the controler pc.
    /// The flag "TCPConnection" in the Settings.settings determines whether this class provides the server functions or the client functions.
    /// If this application is executed on a normal desktop client, this class has no function.
    /// </summary>
    public class TCPConnection
    {
        #region Fields

        private Server serverRef;
        private ClientThread clientRef;
        public event EventHandler newDataReceived;
        private volatile List<object> dataReceived = new List<object>();
        private static object syncDataReceived = new Object();

        #endregion


        #region eventlistener
        public delegate void ShutdownHandler();
        public event ShutdownHandler ShutdownEvent;
        #endregion
        #region Properties

        private Server ServerRef
        {
            get
            {
                return this.serverRef;
            }
            set
            {
                this.serverRef = value;
            }
        }

        private ClientThread ClientRef
        {
            get
            {
                return this.clientRef;
            }
            set
            {
                this.clientRef = value;
            }
        }

        #endregion

        #region Constructos

        public TCPConnection(TCPRole role, IPAddress serverIP, int serverPort)
        {
            switch (role)
            {
                case TCPRole.Server:
                    try
                    {
                        this.ServerRef = new Server(serverPort);
                        this.ServerRef.Start();
                    }
                    catch (FormatException e)
                    {
                        Console.WriteLine("Unexceped exception occured: " + e);
                    }

                    break;
                case TCPRole.Client:
                    try
                    {
                        this.ClientRef = new ClientThread(this, serverIP, serverPort);
                        this.ClientRef.Start();
                    }
                    catch (FormatException e)
                    {
                        Console.WriteLine("Unexceped exception occured: " + e);
                    }
                    break;
                default:
                    // The current settings don't match for using a tcp connection.
                    // This application is used as desktop client of MISD.
                    break;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sends data over the tcp connection.
        /// </summary>
        /// <param name="data">Should be serializable.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Send(object data)
        {
            if (this.serverRef != null)
            {
                this.serverRef.Send(data);
            }
        }


        /// <summary>
        /// Shutdowns all client applications
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ShutdownClients()
        {
            if (this.serverRef != null)
            {
                this.serverRef.Send("TCP_CONNECTION:SHUTDOWN_CLIENTS");
            }
        }

        /// <summary>
        /// Close TCPConnection
        /// </summary>
        public void Stop()
        {
            try
            {
                //this.ShutdownClients();
                if (serverRef != null) this.serverRef.Stop();
                if (clientRef != null) this.clientRef.Stop();
            }
            catch (Exception e)
            {

                Console.WriteLine("Unexcepted exception occured: " + e);
            }
        }

        /// <summary>
        /// Stores data in the local storage. The event newDataReceived will be triggered.
        /// </summary>
        /// <param name="data"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Receive(object data)
        {
            if (data.ToString().Equals("TCP_CONNECTION:SHUTDOWN_CLIENTS"))
            {
                if (ShutdownEvent != null) 
                    ShutdownEvent();
            }
            else
            {
                lock (syncDataReceived)
                {
                    this.dataReceived.Add(data);
                }
                this.newDataReceived(this, new EventArgs());
            }
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool HasData()
        {
            lock (syncDataReceived)
            { return (this.dataReceived.Count > 0); }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public object PopData()
        {
            if (HasData())
            {
                object o;
                lock (syncDataReceived)
                {
                    o = dataReceived.ElementAt(0);
                    dataReceived.RemoveAt(0);
                }
                return o;
            }
            return null;
        }

        #endregion

    }
}

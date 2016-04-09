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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MISD.TCPUtil
{
    /// <summary>
    /// This class provides the server functionality for a tcp connection.
    /// </summary>
    public class Server
    {
        #region Fields

        private int port;
        private Thread mainThread;
        private List<ServerThread> clientThreads;
        private bool running = true;

        #endregion

        #region Properties

        /// <summary>
        /// Sets and gets the port of the server.
        /// </summary>
        public int Port
        {
            get
            {
                return this.port;
            }
            set
            {
                this.port = value;
            }
        }

        /// <summary>
        /// Sets and gets the thread of the server.
        /// </summary>
        private Thread MainThread
        {
            get
            {
                return this.mainThread;
            }
            set
            {
                this.mainThread = value;
            }
        }

        /// <summary>
        /// Sets and gets the threads of the clients.
        /// </summary>
        private List<ServerThread> ClientThreads
        {
            get
            {
                if (this.clientThreads == null)
                {
                    this.clientThreads = new List<ServerThread>();
                }
                return this.clientThreads;
            }
            set
            {
                this.clientThreads = value;
            }
        }

        #endregion

        #region Constructors

        public Server(int port)
        {
            this.Port = port;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the server.
        /// </summary>
        public void Start()
        {
            if (this.MainThread == null)
            {
                running = true; 
                this.MainThread = new Thread(Run);
                this.MainThread.IsBackground = true;
                this.MainThread.Start();
                
            }
        }

        /// <summary>
        /// This method is called after the start. Here the server does his work.
        /// </summary>
        private void Run()
        {
            Console.WriteLine(">>> Server started");
            TcpListener serverSocket = null;
            TcpClient clientSocket = null;

            try
            {

                serverSocket = new TcpListener(IPAddress.Any, this.Port);
                clientSocket = default(TcpClient);

                serverSocket.Start();

                while (this.running)
                {
                    try
                    {
                        clientSocket = serverSocket.AcceptTcpClient();
                        clientSocket.NoDelay = true;

                        ServerThread serverThread = new ServerThread(clientSocket);
                        this.ClientThreads.Add(serverThread);
                        serverThread.Start();
                    }
                    catch (ThreadAbortException)
                    {
                        running = false;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception occured: " + e);
            }
            finally
            {
                if (clientSocket != null)
                {
                    clientSocket.Close();
                }
                if (serverSocket != null)
                {
                    serverSocket.Stop();
                }
            }

        }

        /// <summary>
        /// Sends data to all clients which a connected with the server.
        /// </summary>
        /// <param name="data"></param>
        public void Send(object data)
        {
            foreach (ServerThread clientThread in this.ClientThreads)
            {
                clientThread.AddData(data);
            }
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        public void Stop()
        {
            if (this.MainThread != null)
            {

                this.running = false;
                foreach (ServerThread serverThread in this.ClientThreads)
                {
                    serverThread.Stop();
                }

            }
        }

        #endregion
    }
}

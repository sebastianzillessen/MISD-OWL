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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MISD.TCPUtil
{
    public class ClientThread
    {
        #region Fields

        private TcpClient clientSocket;
        private NetworkStream serverStream;
        private Thread receiveThread;
        private IPAddress serverIP;
        private int serverPort;
        private TCPConnection home;

        #endregion

        #region Properties

        /// <summary>
        /// Sets and gets the connection to the server.
        /// </summary>
        private TcpClient ClientSocket
        {
            get
            {
                return this.clientSocket;
            }
            set
            {
                this.clientSocket = value;
            }
        }

        /// <summary>
        /// Sets and gets the stream of the connection.
        /// </summary>
        private NetworkStream ServerStream
        {
            get
            {
                return this.serverStream;
            }
            set
            {
                this.serverStream = value;
            }
        }

        /// <summary>
        /// Sets and gets the thread for the connection.
        /// </summary>
        private Thread ReceiveThread
        {
            get
            {
                return this.receiveThread;
            }
            set
            {
                this.receiveThread = value;
            }
        }

        /// <summary>
        /// Sets and gets the ip address of the server.
        /// </summary>
        private IPAddress ServerIP
        {
            get
            {
                return this.serverIP;
            }
            set
            {
                this.serverIP = value;
            }
        }

        /// <summary>
        /// Sets and gehts the port of the server.
        /// </summary>
        private int ServerPort
        {
            get
            {
                return this.serverPort;
            }
            set
            {
                this.serverPort = value;
            }
        }

        /// <summary>
        /// Gets and sets the home tcp connection class where the client has to store the received data.
        /// </summary>
        private TCPConnection Home
        {
            get
            {
                return this.home;
            }
            set
            {
                this.home = value;
            }
        }

        #endregion


        #region Constructors

        public ClientThread(TCPConnection home, IPAddress serverIP, int serverPort)
        {
            this.ServerIP = serverIP;
            this.ServerPort = serverPort;
            this.Home = home;
        }

        #endregion

        #region Methods

        public void Start()
        {
            if (this.ReceiveThread == null)
            {
                this.ReceiveThread = new Thread(Run);
                this.ReceiveThread.IsBackground = true;
                this.ReceiveThread.Start();
            }
        }

        private void Run()
        {
            NetworkStream stream = null;
            MemoryStream ms = null;

            bool wantReconnect = true;


            while (wantReconnect)
            {
                try
                {
                    IPEndPoint serverEndPoint = new IPEndPoint(this.ServerIP, this.ServerPort);
                    this.ClientSocket = new TcpClient();
                    this.ClientSocket.Connect(serverEndPoint);
                    this.ClientSocket.NoDelay = true;

                    stream = this.ClientSocket.GetStream();
                    IFormatter formatter = new BinaryFormatter();

                    Console.WriteLine(">>> Client started");
                    while (true)
                    {
                        BinaryReader br = new BinaryReader(stream);
                        long length = br.ReadInt64();
                        byte[] bytes = br.ReadBytes((int)length);
                        ms = new MemoryStream(bytes);
                        ms.Position = 0;
                        this.Home.Receive(formatter.Deserialize(ms));

                        ms.Flush();
                        ms.Close();
                    }
                }
                catch (ThreadAbortException)
                {
                    // Thread stopped.
                    wantReconnect = false;

                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception occured: " + e);
                    wantReconnect = true;
                    Thread.Sleep(3000);
                }
                finally
                {
                    if (ms != null)
                    {
                        ms.Flush();
                        ms.Close();
                        ms.Dispose();
                    }
                    if (stream != null)
                    {
                        stream.Flush();
                        stream.Close();
                        stream.Dispose();
                    }
                }
            }
        }

        public void Stop()
        {
            if (this.ReceiveThread != null)
            {
                this.ReceiveThread.Abort();
            }
        }

        #endregion

    }
}

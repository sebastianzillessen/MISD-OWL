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
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MISD.TCPUtil
{
    public class ServerThread
    {
        #region Fields

        private TcpClient clientSocket;
        private BlockingCollection<object> dataToSend;
        private Thread sendThread;
        private bool running = true;

        #endregion

        #region Properties

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

        private BlockingCollection<object> DataToSend
        {
            get
            {
                if (this.dataToSend == null)
                {
                    this.dataToSend = new BlockingCollection<object>();
                }
                return this.dataToSend;
            }
            set
            {
                this.dataToSend = value;
            }
        }

        private Thread SendThread
        {
            get
            {
                return this.sendThread;
            }
            set
            {
                this.sendThread = value;
            }
        }

        #endregion

        #region Constructors

        public ServerThread(TcpClient clientSocket)
        {
            this.ClientSocket = clientSocket;

        }

        #endregion

        #region Methods

        public bool AddData(object data)
        {
            if (this.running)
            {
                this.DataToSend.Add(data);
                return true;
            }
            else
            {
                Console.WriteLine("Rejected to send dataobject {0}", data);
                return false;
            }
        }

        public void Start()
        {
            if (this.SendThread == null)
            {
                this.running = true;
                this.SendThread = new Thread(Run);
                this.SendThread.IsBackground = true;
                this.SendThread.Start();

            }
        }

        private void Run()
        {
            NetworkStream stream = null;
            MemoryStream serializedObjectStream = null;

            try
            {
                stream = this.ClientSocket.GetStream();
                IFormatter formatter = new BinaryFormatter();

                while (running || DataToSend.Count > 0)
                {
                    try
                    {
                        object data = this.DataToSend.Take();
                        if (data != null)
                        {
                            serializedObjectStream = new MemoryStream();
                            formatter.Serialize(serializedObjectStream, data);
                            BinaryWriter bw = new BinaryWriter(stream);
                            bw.Write(serializedObjectStream.Length);
                            serializedObjectStream.WriteTo(stream);

                            serializedObjectStream.Flush();
                            serializedObjectStream.Close();
                        }
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
                if (serializedObjectStream != null)
                {
                    serializedObjectStream.Flush();
                    serializedObjectStream.Close();
                    serializedObjectStream.Dispose();
                }
                if (stream != null)
                {
                    stream.Flush();
                    stream.Close();
                    stream.Dispose();
                }
            }
        }

        public void Stop()
        {
            this.running = false;
            if (this.SendThread != null)
            {
                this.SendThread.Abort();
            }
        }

        #endregion
    }
}

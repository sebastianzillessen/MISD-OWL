using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using MISD.Client.Model;
using System.Threading.Tasks;

using MISD.TCPUtil;

namespace MISD.TCPUtilTestApplaction
{
    class Program
    {
        public static TCPConnection tcpConnection;

        static void Main(string[] args)
        {
            Console.WriteLine("Test TCPConnetion");
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Who are you?");
            string tempRole = Console.ReadLine();
            
            TCPRole role;
            if (tempRole.ToLower().Contains("server"))
            {
                role = TCPRole.Server;
            }
            else
            {
                role = TCPRole.Client;
            }


            tcpConnection = new TCPConnection(role, IPAddress.Parse("129.69.220.10"), 3001);

            if (role == TCPRole.Server)
            {
                Random r = new Random(2000);
                while (true)
                {
                    Console.WriteLine("Send data: ");
                    string data = Console.ReadLine();

                    if (data == "go")
                    {
                        MISD.Client.Model.LayoutChangeCommand l = new MISD.Client.Model.LayoutChangeCommand(r.Next(), new MonitoredSystemState(r.Next()));

                        tcpConnection.Send(l);
                    }
                    else
                    {
                        tcpConnection.Send(data);
                    }

                }
            }
            else
            {
                tcpConnection.newDataReceived += tcpConnection_newDataReceived;
            }
        }

        private static void tcpConnection_newDataReceived(object sender, EventArgs e)
        {

            //while (tcpConnection.DataReceived.Count > 0)
            while(tcpConnection.HasData())
            {
                //object data = tcpConnection.DataReceived.ElementAt(0);
                object data = tcpConnection.PopData();
                if (data.GetType() == typeof(string))
                {
                    Console.WriteLine(data.GetType() + " | " + data);
                }
                else if (data.GetType() == typeof(MISD.Client.Model.LayoutChangeCommand))
                {
                    Console.WriteLine("LayoutChangeCommand " + ((MISD.Client.Model.LayoutChangeCommand)data).ToString());
                }
                else if (data.GetType() == typeof(Daten))
                {
                    //TypeConverter converter = TypeDescriptor.GetConverter(data);
                    //object convertedData = converter.ConvertFrom(data);

                    Console.WriteLine(data.GetType() + " | " + ((Daten)data).value);
                }
                else
                {
                    Console.WriteLine(">>> Unknown delivered type");
                }

                //tcpConnection.DataReceived.RemoveAt(0);
            }
        }
    }

    [Serializable()]
    public class Daten
    {
        public int value = 0;

        public Daten(int i)
        {
            this.value = i;
        }
    }
}

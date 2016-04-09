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
using System.Runtime.InteropServices;
using System.Collections;
using System.Net;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;
using System.Management.Automation;


namespace MISD.Server.Cluster
{
    public class HpcUtility
    {
        /// <summary>
        /// Queries the given DHCP-Server for all Clients in the given subnet.
        /// </summary>
        /// <param name="server">Name or ip-address of the dhcp-server</param>
        /// <param name="subnet">ip-address of the subnet</param>
        /// <returns>Arraylist containing the MAC-addresses of the clients.</returns>
        private static ArrayList findDhcpClients(string server, string subnet)
        {

            // set up container for processed clients
            ArrayList foundClients = new ArrayList();

            // make call to unmanaged code
            uint parsedMask = StringIPAddressToUInt32(subnet);
            uint resumeHandle = 0;
            uint numClientsRead = 0;
            uint totalClients = 0;

            IntPtr info_array_ptr;

            uint response = DhcpEnumSubnetClients(
                server,
                parsedMask,
                ref resumeHandle,
                65536,
                out info_array_ptr,
                ref numClientsRead,
                ref totalClients
                );

            // set up client array casted to a DHCP_CLIENT_INFO_ARRAY
            // using the pointer from the response object above
            DHCP_CLIENT_INFO_ARRAY rawClients =
                (DHCP_CLIENT_INFO_ARRAY)Marshal.PtrToStructure(info_array_ptr, typeof(DHCP_CLIENT_INFO_ARRAY));


            // loop through the clients structure inside rawClients
            // adding to the dchpClient collection
            IntPtr current = rawClients.Clients;

            for (int i = 0; i < (int)rawClients.NumElements; i++)
            {
                // 1. Create machine object using the struct
                DHCP_CLIENT_INFO rawMachine =
                    (DHCP_CLIENT_INFO)Marshal.PtrToStructure(Marshal.ReadIntPtr(current), typeof(DHCP_CLIENT_INFO));

                // 2. create new C# dhcpClient object and add to the

                // collection (for hassle-free use elsewhere!!)

                dhcpClient thisClient = new dhcpClient();

                thisClient.ip = UInt32IPAddressToString(rawMachine.ip);

                thisClient.hostname = rawMachine.ClientName;

                thisClient.mac = String.Format("{0:x2}{1:x2}.{2:x2}{3:x2}.{4:x2}{5:x2}",

                    Marshal.ReadByte(rawMachine.mac.Data),

                    Marshal.ReadByte(rawMachine.mac.Data, 1),

                    Marshal.ReadByte(rawMachine.mac.Data, 2),

                    Marshal.ReadByte(rawMachine.mac.Data, 3),

                    Marshal.ReadByte(rawMachine.mac.Data, 4),

                    Marshal.ReadByte(rawMachine.mac.Data, 5));


                foundClients.Add(thisClient);

                // 3. move pointer to next machine
                current = (IntPtr)((int)current + (int)Marshal.SizeOf(typeof(IntPtr)));
            }
            return foundClients;
        }

        /// <summary>
        /// Converts the given ip-address to the UInt32 representation.
        /// </summary>
        /// <param name="ip">IP to convert.</param>
        /// <returns>UInt32 representation.</returns>
        private static uint StringIPAddressToUInt32(string ip)
        {
            // convert string IP to uint IP e.g. "1.2.3.4" -> 16909060
            IPAddress i = System.Net.IPAddress.Parse(ip);

            byte[] ipByteArray = i.GetAddressBytes();

            uint ipUint = (uint)ipByteArray[0] << 24;

            ipUint += (uint)ipByteArray[1] << 16;

            ipUint += (uint)ipByteArray[2] << 8;

            ipUint += (uint)ipByteArray[3];

            return ipUint;
        }


        /// <summary>
        /// Converts the given UInt32IP-address to normal IP-format.
        /// e.g. 16909060 -> "1.2.3.4"
        /// </summary>
        /// <param name="ip"></param>
        /// <returns>Converted ip as a string.</returns>
        private static string UInt32IPAddressToString(uint ip)
        {
            IPAddress i = new IPAddress(ip);
            string[] ipArray = i.ToString().Split('.');

            return ipArray[3] + "." + ipArray[2] + "." + ipArray[1] + "." + ipArray[0];
        }

        /// <summary>
        /// Converts a MAC-address, e.g. 0123.4567.89ab ->01.23.45.67.89.AB
        /// </summary>
        /// <param name="mac">MAC-addrees to convert.</param>
        /// <returns>Converted mac-address</returns>
        private static string convertMac(string mac)
        {
            string result = "";
            mac = mac.Replace(".", "").ToUpper();

            for (int i = 0; i < mac.Length; i++)
            {
                result += mac[i];

                if (i % 2 == 1 && i != mac.Length - 1)
                {
                    result += ":";
                }
            }
            return result;
        }

        /// <summary>
        /// This method queries the given DHCP-Servers for IPs an MAC-Addresses
        /// of all Clients in the same subnet as this DHCP-Server
        /// </summary>
        /// <param name="dhcpServerAddress">the DHCP-Server-Address</param>
        /// <returns>dictionarty with the nodename and the mac-address</returns>
        public static Dictionary<string, string> getSubnetInformation(string dhcpServerAddress, HpcClusterConnection hpcClusterConnection)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            // gather clients
            ArrayList clients = findDhcpClients(dhcpServerAddress, calculatePrivateSubNet(hpcClusterConnection));


            foreach (dhcpClient d in clients)
            {
                result.Add(d.hostname, convertMac(d.mac));
            }
            return result;
        }

        /// <summary>
        /// Retrieves the subnet of the client, that the pipeline is connected to.
        /// </summary>
        /// <param name="pipeline">Pipeline that is connected to the server</param>
        /// <returns>the subnet-address</returns>
        private static string calculatePrivateSubNet(HpcClusterConnection hpcClusterConnection)
        {
            string ipAddress = "";
            string subnetMask = "";
            string result = "";

            //acquires ip and subnetmask
            HpcPipelineObject pipelineObject = hpcClusterConnection.getPipeline();

            pipelineObject.pipeline.Commands.AddScript("Add-PSSnapin Microsoft.Hpc");
            pipelineObject.pipeline.Commands.AddScript("Get-HpcNetworkInterface -Type Private");
            Collection<PSObject> results1 = pipelineObject.pipeline.Invoke();

            hpcClusterConnection.freePipeline(pipelineObject);

            PSObject obj = results1[0];

            subnetMask = obj.Properties["Subnetmask"].Value.ToString();
            ipAddress = obj.Properties["IpAddress"].Value.ToString();

            string[] ipArray = ipAddress.Split('.');
            string[] networkMaskArray = subnetMask.Split('.');

            string[] subnetArray = new String[4];

            for (int i = 0; i < 4; i++)
            {
                subnetArray[i] = (Convert.ToInt32(ipArray[i]) & Convert.ToInt32(networkMaskArray[i])).ToString();
            }

            for (int i = 0; i < subnetArray.Length; i++)
            {
                result += subnetArray[i];

                if (i != subnetArray.Length - 1)
                {
                    result += ".";
                }
            }
            return result;
        }

        [DllImport("dhcpsapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint DhcpEnumSubnetClients(
                string ServerIpAddress,
                uint SubnetAddress,
            ref uint ResumeHandle,
                uint PreferredMaximum,
            out IntPtr ClientInfo,
            ref uint ElementsRead,
            ref uint ElementsTotal
        );
    }

    // c# class for processed clients
    public class dhcpClient
    {
        public string hostname { get; set; }
        public string ip { get; set; }
        public string mac { get; set; }
    }


    // structs for use with call to unmanaged code
    [StructLayout(LayoutKind.Sequential)]
    public struct DHCP_CLIENT_INFO_ARRAY
    {
        public uint NumElements;
        public IntPtr Clients;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct DHCP_CLIENT_UID
    {
        public uint DataLength;
        public IntPtr Data;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct DHCP_CLIENT_INFO
    {
        public uint ip;
        public uint subnet;
        public DHCP_CLIENT_UID mac;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string ClientName;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string ClientComment;
    }
}
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
using System.Runtime.Serialization;

namespace MISD.Core
{
    /// <summary>
    /// Contains general information about a workstation.
    /// </summary>
    [DataContract]
    public class WorkstationInfo
    {
        public WorkstationInfo () {}

        public WorkstationInfo(int ID, string name, MappingState state, string fqdn, bool isAvailable, string currentOS, string macAddress, int ouID, DateTime? lastUpdate)
		{
            this.ID = ID;
			this.Name = name;
			this.State = state;
            this.FQDN = fqdn;
            this.IsAvailable = isAvailable;
            this.CurrentOS = currentOS;
            this.MacAddress = macAddress;
            this.OuID = ouID;
            this.LastUpdate = lastUpdate;
		}

        public WorkstationInfo(int ID, string name, MappingState state, string fqdn, bool isAvailable, Platform currentOS, string macAddress, int ouID, DateTime? lastUpdate)
        {
            this.ID = ID;
            this.Name = name;
            this.State = state;
            this.FQDN = fqdn;
            this.IsAvailable = isAvailable;
            this.CurrentOS = currentOS.ToString();
            this.MacAddress = macAddress;
            this.OuID = ouID;
            this.LastUpdate = lastUpdate;
        }

        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public MappingState State { get; set; }

        [DataMember]
        public string FQDN { get; set; }

        [DataMember]
        public bool IsAvailable { get; set; }

        [DataMember]
        public string CurrentOS { get; set; }

        [DataMember]
        public string MacAddress { get; set; }

        [DataMember]
        public int OuID { get; set; }
       
        [DataMember]
        public DateTime? LastUpdate { get; set; }
    }
}

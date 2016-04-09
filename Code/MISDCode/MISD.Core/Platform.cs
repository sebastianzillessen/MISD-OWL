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

using System.Runtime.Serialization;

namespace MISD.Core
{
    /// <summary>
    /// Defines the two different supported operating systems as well as the two supported cluster manager.
    /// </summary>
    [DataContract]
    public enum Platform
    {
        [EnumMember]
        Windows = 0,
        [EnumMember]
        Linux = 1,
        [EnumMember]
        Bright = 2,
        [EnumMember]
        HPC = 3,
        [EnumMember]
        Server = 4,
        [EnumMember]
        Visualization = 5
    }
}

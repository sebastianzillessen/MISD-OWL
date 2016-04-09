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
    /// Contains information about a plugin without containing the plugin file itself.
    /// </summary>
    [DataContract]
    public class PluginMetadata
    {
        [DataMember]
        public Version Version { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public string Company { get; set; }

        [DataMember]
        public string Copyright { get; set; }

        [DataMember]
        public string Product { get; set; }

        [DataMember]
        public List<IndicatorSettings> Indicators { get; set; }
    }
}

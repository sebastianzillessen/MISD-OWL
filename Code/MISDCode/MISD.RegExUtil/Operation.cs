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
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace MISD.RegExUtil
{
    /// <summary>
    /// This enumeration provides the operations for which the RegExGen can create regular expressions.
    /// </summary>
    [DataContract]
    public enum Operation
    {
        /// <summary>
        /// '>'
        /// </summary>
        [EnumMember]
        Major = 0,

        /// <summary>
        /// '&lt;'
        /// </summary>
        [EnumMember]
        Less = 1,

        /// <summary>
        /// '='
        /// </summary>
        [EnumMember]
        Equal= 2,

        /// <summary>
        /// Text contains the given value.
        /// </summary>
        [EnumMember]
        Contain = 3,

        /// <summary>
        /// Text doesn't contain the given value.
        /// </summary>
        [EnumMember]
        NotContain = 4
    }
}

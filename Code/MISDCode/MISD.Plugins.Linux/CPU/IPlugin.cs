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

namespace MISD.Core
{
    public interface IPlugin
    {
        /// <summary>
        /// Returns the name of the plugin .
        /// </ summary>
        /// <returns> name of the plugin </returns>
        string GetName();

        /// <summary>
        /// Returns the name of the author .
        /// </summary>
        /// <returns> name of the author </returns>
        string GetAuthor();

        /// <summary >
        /// Returns the creation date .
        /// </summary>
        /// <returns> creation date </returns>
        DateTime GetCreationDate();

        /// <summary>
        /// Returns the version of the plugin
        /// </summary>
        /// <returns>version of the plugin</returns>
        int GetVersion();

        /// <summary>
        /// Returns the description of the Plugin .
        /// </summary>
        /// <returns> description of the plugin </returns>
        string GetDescription();

        /// <summary>
        /// Acquires all data that can be retrieved from this plugin .
        /// </summary>
        /// <returns>A list containing tuples of: IndicatorName IndicatorValue .</returns>
        List<Tuple<string, object, MISD.Core.DataType>> AcquireData();

        /// <summary>
        /// Acquires the data of the specified plugin values .
        /// </summary>
        /// <param name =" indicatorName "> The names of the indicators that shall b retrieved .</param>
        /// <returns>A list containing tuples of: Indicatorname IndicatorValue .</returns>
        List<Tuple<string, object, MISD.Core.DataType>> AcquireData(List<string> indicatorName);
    }
}

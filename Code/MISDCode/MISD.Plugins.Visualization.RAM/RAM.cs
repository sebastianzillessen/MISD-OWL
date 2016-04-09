﻿/*
* Copyright 2012 
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
using System.ComponentModel.Composition;
using MISD.Client.Model;

namespace MISD.Plugins.Visualization.RAM
{
    [Export(typeof(IPluginVisualization))]
    public class RAM : IPluginVisualization
    {

        public string Name
        {
            get { return "RAM"; }
        }

        public int Rows
        {
            get { return 1; }
        }

        public int Columns
        {
            get { return 2; }
        }

        /// <summary>
        /// Gets the plugin's main value as a string. Normally this is the newest value of the most interesting indicator.
        /// </summary>
        /// <param name="indicators">Indicator values which come from the server database</param>
        /// <returns>The plugin's main value as a string. Don't forget its unit or percentage.</returns>
        public string CalculateMainValue(IEnumerable<Indicator> indicatorValues)
        {
            if (indicatorValues == null || indicatorValues.Count() == 0)
            {
                return "-";
            }
            try
            {
                var sizeIndicators = from p in indicatorValues
                                     where p.Name == "Size"
                                     select p.IndicatorValues;
                if (sizeIndicators == null || sizeIndicators.Count() == 0)
                {
                    return "-";
                }
                var sizes = sizeIndicators.First();

                var newestSize = from p in sizes
                                 orderby p.Timestamp descending
                                 select p.Value;
                if (newestSize == null || newestSize.Count() == 0)
                {
                    return "-";
                }
                return newestSize.First() + " MB";
            }
            catch (Exception)
            {
                return "-";
            }
        }
    }
}

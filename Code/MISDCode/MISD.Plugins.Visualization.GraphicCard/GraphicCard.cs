/*
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

namespace MISD.Plugins.Visualization.GraphicCard
{

    [Export(typeof(IPluginVisualization))]
    public class GraphicCard : IPluginVisualization
    {
        public string Name
        {
            get { return "GraphicCard"; }
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
                var numberIndicators = (from p in indicatorValues
                                        where p.Name == "NumberOfDevices"
                                        select p.IndicatorValues).FirstOrDefault();
                if (numberIndicators == null || numberIndicators.Count() == 0)
                {
                    return "-";
                }
                var number = numberIndicators.First();

                if (number == null || number.Value.ToString().Equals(""))
                {
                    return "-";
                }
                else
                {
                    return number.Value.ToString();
                }
            }
            catch (Exception)
            {
                return "-";
            }
        }
    }
}

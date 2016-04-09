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
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MISD.Client.Model;

namespace MISD.Plugins.Visualization.ICMPEchoRequest
{
    [Export(typeof(IPluginVisualization))]
    public class ICMPEchoRequest : IPluginVisualization
    {
        public string Name
        {
            get { return "ICMPEchoRequest"; }
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
                var durationIndicators = from p in indicatorValues
                                     where p.Name == "Duration"
                                     select p.IndicatorValues;
                if (durationIndicators == null || durationIndicators.Count() == 0)
                {
                    return "-";
                }
                var names = durationIndicators.First();

                var orderedDurations = from p in names
                                 orderby p.Timestamp descending
                                 select p.Value;
                if (orderedDurations == null || orderedDurations.Count() == 0)
                {
                    return "-";
                }
                else
                {
                    try{
                        if (Convert.ToInt32(orderedDurations.First().ToString()) == -1
                            || Convert.ToInt32(orderedDurations.First().ToString()) == Int32.MaxValue)
                        {
                            return "unavail.";
                        }else{
                            return orderedDurations.First().ToString() + " ms";
                        }
                        }catch(Exception){
                            return orderedDurations.First().ToString() + " ms";
                        }
                }
            }
            catch (Exception)
            {
                return "-";
            }
        }
    }
}

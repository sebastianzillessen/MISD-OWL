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
using System.Text.RegularExpressions;
using System.Threading;

using MISD.Core;
using MISD.Core.Scheduling;
using MISD.Workstation.Windows.Plugins;
using MISD.RegExUtil;

namespace MISD.Workstation.Windows.Scheduling.TimerJobs
{
    /// <summary>
    /// The timerjob for an indicator.
    /// </summary>
    public class IndicatorTimerJob : TimerJobBase
    {
        #region Properties

        /// <summary>
        /// The plugins itself used in the indicator.
        /// </summary>
        private IPlugin Plugin
        {
            get;
            set;
        }

        /// <summary>
        /// The indicator used in this timerjob.
        /// </summary>
        public string Indicator
        {
            get;
            set;
        }

        /// <summary>
        /// The filter expression for the indicator.
        /// </summary>
        private string Filter
        {
            get;
            set;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a timerjob for an indicator.
        /// </summary>
        /// <param name="plugin">The name of the plugin for the timerjob.</param>
        /// <param name="indicator">The name of the indicator for the timerjob.</param>
        /// <param name="interval">The update interval for the timerjob.</param>
        public IndicatorTimerJob(IPlugin plugin, string indicator, TimeSpan interval)
        {
            this.ID = plugin.GetName() + "." + indicator;
            this.Indicator = indicator;
            this.Interval = interval;
            this.Plugin = plugin;
        }

        #endregion

        #region Methods
        /// <summary>
        /// overrides the TimerJobBase Loop, since indicator timers should tick first,
        /// so every indicator value will be sent instantly.
        /// </summary>
        protected override void Loop()
        {
            Random random = new Random();
            try
            {
                Thread.Sleep(random.Next(1, 20) * 1000);

                var start = DateTime.Now;

                while (this.IsStarted)
                {
                    // tick first, sleep after
                    this.TimerTickAsync();
                    var sleepTime = this.Interval - (DateTime.Now - start);
                    if (sleepTime < TimeSpan.FromSeconds(1))
                    {
                        sleepTime = TimeSpan.FromSeconds(1);
                    }

                    Thread.Sleep((int)sleepTime.TotalMilliseconds);

                    start = DateTime.Now;
                }
            }
            catch (ThreadAbortException)
            {
                // Return
            }
        }

        /// <summary>
        /// The method that aquires, filters and sends the indicator values.
        /// </summary>
        protected override void TimerTickAsync()
        {
            // Aquire data
            List<Tuple<string, object, MISD.Core.DataType>> values = new List<Tuple<string, object, MISD.Core.DataType>>();
            values = GetValues();

            // Filter values
            List<Tuple<string, object, MISD.Core.DataType>> filteredValues = new List<Tuple<string, object, MISD.Core.DataType>>();
            filteredValues = FilterValues(values);
            // Send values
            if (filteredValues.Count > 0)
            {
                List<Tuple<string, Object, MISD.Core.DataType, DateTime>> valuesToSend = new List<Tuple<string, object, MISD.Core.DataType, DateTime>>();
                foreach (Tuple<string, object, MISD.Core.DataType> current in filteredValues)
                {
                    Tuple<string, Object, MISD.Core.DataType, DateTime> currentValue = new Tuple<string, Object, MISD.Core.DataType, DateTime>
                            (this.Indicator, current.Item2, current.Item3, DateTime.Now);
                    valuesToSend.Add(currentValue);
                }
                ServerConnection.UploadIndicatorValues(this.Plugin.GetName(), valuesToSend.ToArray());
            }
        }

        /// <summary>
        /// Method to aquire data with the plugin.
        /// </summary>
        /// <returns>A list containing IndicatorName | IndicatorValue | IndicatorValueDataType.</returns>
        private List<Tuple<string, object, MISD.Core.DataType>> GetValues()
        {
            List<string> indicatorName = new List<String>();
            indicatorName.Add(this.Indicator);
            try
            {
                List<Tuple<string, object, MISD.Core.DataType>> l = this.Plugin.AcquireData(indicatorName);
                WorkstationLogger.WriteLog("Indicator: " + this.Indicator + " Value: " + l.First().Item2.ToString());
                return l;
            }
            catch (Exception e)
            {
                WorkstationLogger.WriteLog("Couldn't acquire value for: " + this.Indicator + ". Error: " + e.Message);
                ServerConnection.WriteLog(ServerConnection.GetWorkstationName() + 
                    " GetValues(): Couldn't acquire data for " + this.Indicator + " Error Message: " + e.Message, LogType.Exception);
            }
            return new List<Tuple<string, object, MISD.Core.DataType>>();
        }

        /// <summary>
        /// Method to filter the values.
        /// </summary>
        /// <param name="unfilteredValues">The list of all values.</param>
        /// <returns>A list containing IndicatorName | IndicatorValue | IndicatorValueDataType.</returns>
        private List<Tuple<string, object, MISD.Core.DataType>> FilterValues(List<Tuple<string, object, MISD.Core.DataType>> unfilteredValues)
        {
            // Collect old filters
            List<Tuple<string, string>> oldFilters = new List<Tuple<string, string>>();
            foreach (IndicatorSettings current in this.Plugin.GetIndicatorSettings())
            {
                oldFilters.Add(new Tuple<string, string>(current.IndicatorName, current.FilterStatement));
            }
            // Get filter expression
            List<Tuple<string, string>> pluginFilters = ServerConnection.GetFilters(this.Plugin.GetName(), oldFilters.ToArray()).ToList();
            foreach (Tuple<string, string> current in pluginFilters)
            {
                if (current.Item1 == this.Indicator)
                {
                    this.Filter = current.Item2;
                }
            }

            // filter only if there's a valid statement, else return all values
            if (this.Filter == null || this.Filter.Trim().Equals("") || this.Filter.Trim().Equals("."))
            {
                return unfilteredValues;
            }
            
            // Filter values
            List<Tuple<string, object, MISD.Core.DataType>> filteredValues = new List<Tuple<string, object, MISD.Core.DataType>>();
            foreach (Tuple<string, object, MISD.Core.DataType> current in unfilteredValues)
            {
                if (RegExUtility.Match(current.Item1, this.Filter))
                {
                    filteredValues.Add(current);
                }
            }
            return filteredValues;
        }

        #endregion
    }
}

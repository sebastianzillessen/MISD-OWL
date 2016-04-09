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
using MISD.Core;
using MISD.Core.Scheduling;
using MISD.Server.Manager;
using MISD.Server.Services;
using System.Threading;

namespace MISD.Server.Scheduling
{
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!WARNING!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    // This is a old approach using one thread for each combination node/plugin/indicator
    // See "GlobalScheduler" for current version
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!WARNING!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

    [Obsolete("Not used anymore", false)]
    public class GlobalTimerJob : TimerJobBase
    {
        #region Properties

        /// <summary>
        /// The monitored system.
        /// </summary>
        protected WorkstationInfo system;

        /// <summary>
        /// The plugins itself used in the indicator.
        /// </summary>
        protected IPlugin plugin;

        /// <summary>
        /// The indicator used in this timerjob.
        /// </summary>
        public string indicator;

        /// <summary>
        /// The service to be able to upload values.
        /// </summary>
        protected WorkstationWebService webService;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a timerjob for an indicator.
        /// </summary>
        /// <param name="monitoredSystem">The monitored system.</param>
        /// <param name="plugin">The plugin.</param>
        /// <param name="indicator">The indicator.</param>
        /// <param name="interval">The update interval.</param>
        public GlobalTimerJob(WorkstationInfo monitoredSystem, IPlugin plugin, string indicator, TimeSpan interval)
        {
            this.ID = monitoredSystem.ID + "." + plugin.GetName() + "." + indicator;
            this.system = monitoredSystem;
            this.plugin = plugin;
            this.indicator = indicator;
            this.Interval = interval;
        }

        #endregion

        #region Methods
        protected override void Loop()
        {
            Random random = new Random();
            try
            {
                Thread.Sleep(random.Next(1, 20) * 1000);

                var start = DateTime.Now;

                while (this.IsStarted)
                {
                    var sleepTime = this.Interval - (DateTime.Now - start);
                    if (sleepTime < TimeSpan.FromSeconds(1))
                    {
                        sleepTime = TimeSpan.FromSeconds(1);
                    }

                    Thread.Sleep((int)sleepTime.TotalMilliseconds);

                    start = DateTime.Now;

                    this.TimerTickAsync();
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
            try
            {
                // Aquire data
                var values = GetValues();

                if (values != null && values.Count > 0)
                {
                    // Filter values
                    var filteredValues = FilterValues(values);

                    // Send values
                    if (filteredValues != null && filteredValues.Count > 0)
                    {
                        var valuesToSend = (from p in filteredValues
                                            select new Tuple<string, Object, MISD.Core.DataType, DateTime>(indicator, p.Item2, p.Item3, DateTime.Now)).ToList();

                        WorkstationManager.Instance.UploadIndicatorValues(system.ID, plugin.GetName(), valuesToSend);
                    }
                }
            }
            catch (Exception e)
            {
                MISD.Core.Logger.Instance.WriteEntry("GlobalTimerJob_TimerTickAsync: Problem with ID " + this.ID + ". " + e.ToString(), LogType.Exception);
            }
        }

        /// <summary>
        /// Method to aquire data with the plugin.
        /// </summary>
        /// <returns>A list containing IndicatorName | IndicatorValue | IndicatorValueDataType.</returns>
        private List<Tuple<string, object, MISD.Core.DataType>> GetValues()
        {
            var temp = plugin.AcquireData(new List<String> { indicator }, system.FQDN);
            if (temp == null)
            {
                temp = new List<Tuple<string, object, DataType>>();
            }
            // value itself is null
            if (temp != null && temp.Count > 0 && temp.First() != null && temp.First().Item2 == null)
            {
                temp = new List<Tuple<string, object, DataType>>();
            }

            return temp;
        }

        /// <summary>
        /// Method to filter the values.
        /// </summary>
        /// <param name="unfilteredValues">The list of all values.</param>
        /// <returns>A list containing IndicatorName | IndicatorValue | IndicatorValueDataType.</returns>
        private List<Tuple<string, object, MISD.Core.DataType>> FilterValues(List<Tuple<string, object, MISD.Core.DataType>> unfilteredValues)
        {
            List<Tuple<string, object, MISD.Core.DataType>> result;

            var filteredValues = (from p in unfilteredValues
                                  where FilterManager.Instance.GetFilterValue(system.ID, plugin.GetName(), indicator, p.Item1)
                                  select p);

            if (filteredValues == null)
            {
                result = new List<Tuple<string, object, MISD.Core.DataType>>();
            }
            else
            {
                result = filteredValues.ToList();
            }

            return result;
        }

        #endregion
    }
}

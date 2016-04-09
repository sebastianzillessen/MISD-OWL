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
using MISD.Workstation.Linux.Plugins;
using MISD.RegExUtil;

namespace MISD.Workstation.Linux.Scheduling.TimerJobs
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
                bool result = true;
				foreach (Tuple<string, object, MISD.Core.DataType> current in filteredValues)
                {
                    try{
						WorkstationLogger.Instance.WriteLog ("[SENDING VALUE][Plugin="+this.Plugin.GetName()+"] " 
						                  + current.Item1 + "(" +this.Indicator+"):" + current.Item2.ToString(), LogType.Debug, false);
						
						bool r = ServerConnection.Instance.UploadIndicatorValue(this.Plugin.GetName(), 
							this.Indicator, current.Item2, current.Item3, DateTime.Now
						);
						result = result && r;
					}
					catch(Exception e){
						WorkstationLogger.Instance.WriteLog (this.Indicator + ": Error during sending!" + e.Message, LogType.Exception, false);
					}
                }
				if (!result){
					WorkstationLogger.Instance.WriteLog ("ERROR DURING DATA SENDING PROCESS RETURNED FALSE", LogType.Warning, false);
				}
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
				return this.Plugin.AcquireData(indicatorName);
			}
			catch (Exception e)
			{
				WorkstationLogger.Instance.WriteLog (ServerConnection.Instance.GetWorkstationName () + "_IndicatorTimerJob_GetValues: " + e.Message, LogType.Exception, true);
				
				// Return empty list
				return new List<Tuple<string, object, MISD.Core.DataType>> ();
			}

        }

        /// <summary>
        /// Method to filter the values.
        /// </summary>
        /// <param name="unfilteredValues">The list of all values.</param>
        /// <returns>A list containing IndicatorName | IndicatorValue | IndicatorValueDataType.</returns>
        private List<Tuple<string, object, MISD.Core.DataType>> FilterValues (List<Tuple<string, object, MISD.Core.DataType>> unfilteredValues)
		{
			// Get old or default filter from plugin assembly
			if (this.Filter == null)
			{
				foreach (IndicatorSettings indicatorSetting in this.Plugin.GetIndicatorSettings())
				{
					if (indicatorSetting.IndicatorName == this.Indicator)
					{
						this.Filter = indicatorSetting.FilterStatement;
					}
				}
			}

            // Get filter expression
			this.Filter = ServerConnection.Instance.GetFilter(this.Plugin.GetName(),this.Indicator, this.Filter);

            // Filter values
			if (this.Filter == null || this.Filter.Trim() == "" || this.Filter.Trim() == ".")
			{
				// Here is no filtering required.
				return unfilteredValues;
			}
			else
			{
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
        }

        #endregion
    }
}

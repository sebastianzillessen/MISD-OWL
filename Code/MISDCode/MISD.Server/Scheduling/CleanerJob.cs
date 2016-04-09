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

using MISD.Core.Scheduling;
using MISD.Server.Database;
using MISD.Server.Properties;
using System;
using System.Linq;
using System.Text;
using MISD.Server.Manager;
using MISD.Core;
using System.Collections.Generic;
using System.Threading;

namespace MISD.Server.Scheduling
{
    /// <summary>
    /// This timer job is responsible for removing all indicator values, 
    /// that have exceeded the indicator's storage duration.
    /// </summary>
    public class CleanerJob : TimerJobBase
    {

        #region Constructors
        /// <summary>
        /// Constructor to create a cleaner for a specific indicator.
        /// </summary>
        public CleanerJob()
        {
            this.ID = "CleanerTimerJob";
            this.Interval = Settings.Default.CleanerJobInterval;
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
        /// Do the cleaning job.
        /// </summary>
        protected override void TimerTickAsync()
        {
            long valuesDeleted = 0;

            using (var dataContext = DataContextFactory.CreateDataContext())
            {
                //logging
                StringBuilder messageStart = new StringBuilder();
                messageStart.Append("CleanterJob_TimerTickAsync: ");
                messageStart.Append("Start cleaning.");
                Logger.Instance.WriteEntry(messageStart.ToString(), LogType.Info);


                long now = DateTime.Now.Ticks;

                // Find storage durations to remove.
                var indicators = (from p in dataContext.Indicator
                                  select p.Name).ToList();

                //Find indicator value to remove
                #region delete indicator values for all indicators
                foreach (var ind in indicators)
                {
                    bool newValues = true;

                    //delete the first's values.
                    while (newValues)
                    {
                        //get some indicator values to remove of this indicator.
                        // The cleaner deletes only some values to decrease the runtime of the folowing sql statement.
                        var indicatorValueToRemove = (from q in dataContext.Indicator
                                                      where q.Name == ind
                                                      from r in q.IndicatorValue
                                                      let storageDuration = q.StorageDuration == null ? 864000000000 : q.StorageDuration
                                                      let deadLineTicks = now - storageDuration
                                                      let indicatorValueTicks = r.Timestamp
                                                      where indicatorValueTicks <= deadLineTicks
                                                      select r.ID).Take(Properties.Settings.Default.CleanerTakeCount).ToList();

                        if (indicatorValueToRemove.Count > 0)
                        {
                            newValues = true;
                        }
                        else
                        {
                            newValues = false;
                        }
                        
                        valuesDeleted = valuesDeleted + indicatorValueToRemove.Count;

                        // Remove indicator value if necessary
                        #region delete indicator value
                        try
                        {
                            //build delete indicatorvalute query
                            var deleteQuery = new StringBuilder();
                            deleteQuery.Append("DELETE FROM [MISD].[dbo].[IndicatorValue] ");
                            deleteQuery.Append("WHERE [ID] IN (");
                            foreach (var current in indicatorValueToRemove)
                            {
                                deleteQuery.Append(current + ",");
                            }
                            deleteQuery.Length--;
                            deleteQuery.Append(")");

                            //delete indicatorvalue
                            if (indicatorValueToRemove.Count > 0)
                            {
                                dataContext.ExecuteQuery<bool>(deleteQuery.ToString());
                            }
                        }
                        catch (Exception e)
                        {
                            MISD.Core.Logger.Instance.WriteEntry("CleanerJob_TimerTickAsync: Can't delete IndicatorValue. " + e.ToString(), LogType.Exception);
                        }
                        #endregion
                    }
                }
                #endregion
            }

            // Clean valueX tables
            CleanValueX(DataType.String);
            CleanValueX(DataType.Int);
            CleanValueX(DataType.Float);

            #region end logging
            StringBuilder messageEnd = new StringBuilder();
            messageEnd.Append("CleanterJob_TimerTickAsync: ");
            messageEnd.Append("End cleaning. " + valuesDeleted + " values deleted.");
            Logger.Instance.WriteEntry(messageEnd.ToString(), LogType.Info);
            #endregion
        }

        private void CleanValueX(DataType dataType)
        {
            using (var dataContext = DataContextFactory.CreateDataContext())
            {
                try
                {
                    string queryIndicator = "SELECT [ID] FROM [MISD].[dbo].[Indicator] WHERE [ValueType] =" + (byte)dataType;

                    var query = new StringBuilder();
                    query.Append("DELETE FROM [MISD].[dbo].[Value" + dataType.ToString() + "]");
                    query.Append("WHERE [ID] NOT IN ");
                    query.Append("(SELECT [ValueID] FROM [MISD].[dbo].[IndicatorValue] AS iv JOIN (" + queryIndicator + ") AS i ON iv.IndicatorID = i.ID)");

                    dataContext.ExecuteQuery<bool>(query.ToString());
                }
                catch (Exception e)
                {
                    Logger.Instance.WriteEntry("CleanerJob_CleanValueX: Problem cleaning values: " + e.ToString(), LogType.Exception);                    
                }
            }
        }

        #endregion
    }
}

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
using MISD.Core.Scheduling;
using MISD.Server.Email;
using System.Threading;

namespace MISD.Server.Scheduling
{
    class DailyMailTimerJob : TimerJobBase
    {
        public DailyMailTimerJob(TimeSpan interval)
        {
            this.ID = "DailyMailTimerJob";
            this.Interval = interval;
        }

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
        /// Send the daily mail to alle rescievers
        /// </summary>
        protected override void TimerTickAsync()
        {
            Mailer.Instance.SendAllDailyMails();
        }
    }
}

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
using MISD.Core;

namespace MISD.Server.Email.Templates
{
    /// <summary>
    /// This class extends the WarningTemplate class in file WarningTemplate1.cs. 
    /// In the auto-generated file WarningTemplate1.cs you can't make changes.
    /// </summary>
    partial class DailyMailTemplate
    {
        private DailyMailTemplateData myData;

        /// <summary>
        /// Data constructor for the warning template
        /// </summary>
        /// <param name="data">List of WarningTemplateData</param>
        public DailyMailTemplate(DailyMailTemplateData data)
        {
            this.myData = data;
        }
    }
}

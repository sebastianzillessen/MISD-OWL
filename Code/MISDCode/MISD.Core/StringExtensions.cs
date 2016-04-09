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
    public static class StringExtensions
    {
        public static string RemoveQuotationMarks(this string s)
        {
            if (!string.IsNullOrWhiteSpace(s) && s.Length >= 2)
            {
                if (s.First() == '\"')
                {
                    s = s.Substring(1);
                }
                if (s.Last() == '\"')
                {
                    s = s.Substring(0, s.Length - 1);
                }
            }
            return s;
        }
    }
}

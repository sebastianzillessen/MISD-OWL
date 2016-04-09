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
using System.Reflection;
using System.Text;

namespace MISD.Core
{
    public static class IPluginExtensions
    {
		public static string GetName(this IPlugin plugin)
        {
			return ((AssemblyTitleAttribute) Assembly.GetAssembly(plugin.GetType()).GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title;
        }

        public static string GetDescription(this IPlugin plugin)
        {
			return ((AssemblyDescriptionAttribute) Assembly.GetAssembly(plugin.GetType()).GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)[0]).Description;
        }

        public static string GetCompany(this IPlugin plugin)
        {
			return ((AssemblyCompanyAttribute) Assembly.GetAssembly(plugin.GetType()).GetCustomAttributes(typeof(AssemblyCompanyAttribute), false)[0]).Company;
        }

		public static string GetProduct (this IPlugin plugin)
		{
			return ((AssemblyProductAttribute) Assembly.GetAssembly(plugin.GetType()).GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0]).Product;
		}

        public static string GetCopyright(this IPlugin plugin)
        {
			return ((AssemblyCopyrightAttribute) Assembly.GetAssembly(plugin.GetType()).GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0]).Copyright;
        }

        public static Version GetVersion(this IPlugin plugin)
        {
			return System.Reflection.Assembly.GetAssembly(plugin.GetType()).GetName().Version;
		}
    }
}
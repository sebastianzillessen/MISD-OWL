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
 * MISD-OWL is distributed without any warranty, witmlhout even the
 * implied warranty of merchantability or fitness for a particular purpose.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MISD.Core
{
    public class MonitoredSystemState
    {
        public int Level
        {
            get;
            set;
        }

        private List<int> shownPlugins;

        public List<int> ShownPlugins
        {
            get
            {
                if (shownPlugins == null){
                    shownPlugins = new List<int>();
                }
                return shownPlugins;
            }
            private set
            {
                shownPlugins = value;
            }
        }

        public void ShowPlugin(int id)
        {
            ShownPlugins.Remove(id);
            ShownPlugins.Add(id);
        }

        public void HidePlugin(int id)
        {
            ShownPlugins.Remove(id);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            MonitoredSystemState s = obj as MonitoredSystemState;
            if ((System.Object)s == null)
                return false;

            return Equals(s);
        }

        public bool Equals(MonitoredSystemState s)
        {
            if ((object)s == null)
                return false;
            if (this.Level != s.Level)
                return false;
            List<int> otherPlugins = s.ShownPlugins;
            foreach (int x in ShownPlugins)
            {
                if (!otherPlugins.Remove(x))
                    return false;
            }
            return otherPlugins.Count == 0;

        }

    }
}

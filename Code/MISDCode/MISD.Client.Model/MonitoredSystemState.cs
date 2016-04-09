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

namespace MISD.Client.Model
{
    [Serializable]
    public class MonitoredSystemState
    {
        public MonitoredSystemState()
        {
        }

        public MonitoredSystemState(string StringRepresentation)
            : this()
        {
            Level = Convert.ToInt32(StringRepresentation.Split(':')[0]);
            foreach (String s in StringRepresentation.Split(':')[1].Split(','))
            {
                if (!s.Trim().Equals(""))
                    ShowPlugin(s);
            }
        }

        public MonitoredSystemState(int level)
            : this()
        {
            this.Level = level;
        }

        public MonitoredSystemState(int level, string[] PluginList)
            : this(level)
        {
            shownPlugins = PluginList.ToList<string>();
        }

        private int level = (from p in DataModel.Instance.LevelDefinitions
                             orderby p.Level, p.Rows, p.ID ascending
                             select p).First().Level;

        public int Level
        {
            get
            {
                return level;
            }
            set
            {
                if (value < 0 && value != -1)
                {
                    value = Math.Abs(value);
                }

                if ((from p in DataModel.Instance.LevelDefinitions
                     where p.LevelID == value
                     select p).Count() == 1)
                {
                    if (value != this.level)
                    {
                        this.level = value;
                        this.ShownPlugins.Clear();
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Level is no valid Level definition");
                }
            }
        }


        private List<string> shownPlugins;
        private int id;

        public List<string> ShownPlugins
        {
            get
            {
                if (shownPlugins == null)
                {
                    shownPlugins = new List<string>();
                }
                return shownPlugins;
            }
            private set
            {
                shownPlugins = value;
            }
        }

        public void ShowPlugin(string name)
        {
            ShownPlugins.Add(name);
        }

        public void HidePlugin(string name)
        {
            shownPlugins.Remove(name);
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
            List<string> otherPlugins = s.ShownPlugins;
            foreach (var x in ShownPlugins)
            {
                if (!otherPlugins.Remove(x))
                    return false;
            }
            return otherPlugins.Count == 0;

        }


        public override string ToString()
        {
            string p = "";
            return Level + ":" + String.Join(",", ShownPlugins);
        }
    }
}

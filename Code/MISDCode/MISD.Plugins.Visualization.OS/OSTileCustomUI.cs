/*
* Copyright 2012 
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
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MISD.Client.Model;

namespace MISD.Plugins.Visualization.OS
{
    [Export(typeof(ITileCustomUI))]
    public class OSTileCustomUI : TileCustomUI
    {
        public OSTileCustomUI()
        {
            this.Name = " - ";
            this.Version = " - ";
            this.UpTime = " - ";
        }

        #region Properties
        public String Name
        {
            get
            {
                return (String)this.GetValue(NameProperty);
            }
            set
            {
                this.SetValue(NameProperty, value);
            }
        }
        public String Version
        {
            get
            {
                return (String)this.GetValue(VersionProperty);
            }
            set
            {
                this.SetValue(VersionProperty, value);
            }
        }
        public String UpTime
        {
            get
            {
                return (String)this.GetValue(UpTimeProperty);
            }
            set
            {
                this.SetValue(UpTimeProperty, value);
            }
        }
        #endregion

        #region Dependeny Properties
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(String), typeof(OSTileCustomUI));

        public static readonly DependencyProperty VersionProperty =
            DependencyProperty.Register("Version", typeof(String), typeof(OSTileCustomUI));

        public static readonly DependencyProperty UpTimeProperty =
            DependencyProperty.Register("UpTime", typeof(String), typeof(OSTileCustomUI));
        #endregion

        public override void SelectIndicatorValues()
        {
            try
            {
                // Name
                var name = (from p in this.Indicators
                                        where p.Name == "Name"
                                        from q in p.IndicatorValues
                                        orderby q.Timestamp descending
                                        select q).FirstOrDefault();
                if (name != null && !name.Value.ToString().Equals(""))
                {
                    this.Name = name.Value.ToString();
                }
                else
                {
                    this.Name = " - ";
                }
            }
            catch (Exception)
            {
                Console.WriteLine("OS VIS PLUGIN: Problem at getting Name");
                this.Name = " - ";
            }

            try
            {
                // Version
                var version = (from p in this.Indicators
                            where p.Name == "Version"
                            from q in p.IndicatorValues
                            orderby q.Timestamp descending
                            select q).FirstOrDefault();
                if (version != null && !version.Value.ToString().Equals(""))
                {
                    this.Version = version.Value.ToString();
                }
                else
                {
                    this.Version = " - ";
                }
            }
            catch (Exception)
            {
                Console.WriteLine("OS VIS PLUGIN: Problem at getting Version");
                this.Version = " - ";
            }



            try
            {
                // Uptime
                var upTime = (from p in this.Indicators
                               where p.Name == "Uptime"
                               from q in p.IndicatorValues
                               orderby q.Timestamp descending
                               select q).FirstOrDefault();
                if (upTime != null && !upTime.Value.ToString().Equals(""))
                {
                    this.UpTime = upTime.Value.ToString();
                }
                else
                {
                    this.UpTime = " - ";
                }
            }
            catch (Exception)
            {
                Console.WriteLine("OS VIS PLUGIN: Problem at getting Uptime");
                this.UpTime = " - ";
            }
        }
    }
}

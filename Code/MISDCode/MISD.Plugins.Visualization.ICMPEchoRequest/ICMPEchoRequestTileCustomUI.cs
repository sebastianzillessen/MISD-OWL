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

using MISD.Client.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MISD.Plugins.Visualization.ICMPEchoRequest
{
    [Export(typeof(ITileCustomUI))]
    class ICMPEchoRequestTileCustomUI : TileCustomUI
    {
          public ICMPEchoRequestTileCustomUI()
        {
            this.Duration = new ExtendedObservableCollection<IndicatorValue>();
        }

        #region Properties

        public ExtendedObservableCollection<IndicatorValue> Duration
        {
            get
            {
                return (ExtendedObservableCollection<IndicatorValue>)this.GetValue(DurationProperty);
            }
            set
            {
                this.SetValue(DurationProperty, value);
            }
        }

        #endregion

        #region Dependency Property



        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(ExtendedObservableCollection<IndicatorValue>), typeof(ICMPEchoRequestTileCustomUI));

        #endregion

        public override void SelectIndicatorValues()
        {
            this.IndicatorValues.ClearOnUI();

            // Duration
            var durations = (from p in this.Indicators
                                    where p.Name == "Duration"
                                    from q in p.IndicatorValues
                                    select q);
            this.Duration.BeginAddRange(DiagramHelper.filterDiagramValues(durations));

        }
    }
}

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

namespace MISD.Plugins.Visualization.GraphicCard
{
    [Export(typeof(ITileCustomUI))]
    public class GraphicCardTileCustomUI : TileCustomUI
    {
        public GraphicCardTileCustomUI()
        {
            this.GraphicCardNames = new ExtendedObservableCollection<IndicatorValue>();
            this.Number = " - ";
        }

        #region Properties
        public ExtendedObservableCollection<IndicatorValue> GraphicCardNames
        {
            get
            {
                return (ExtendedObservableCollection<IndicatorValue>)this.GetValue(GraphicCardNamesProperty);
            }
            set
            {
                this.SetValue(GraphicCardNamesProperty, value);
            }
        }

        public String Number
        {
            get
            {
                return (String)this.GetValue(NumberProperty);
            }
            set
            {
                this.SetValue(NumberProperty, value);
            }
        }
        #endregion

        #region Dependency Property

        public static readonly DependencyProperty GraphicCardNamesProperty =
        DependencyProperty.Register("GraphicCardNames", typeof(ExtendedObservableCollection<IndicatorValue>), typeof(GraphicCardTileCustomUI));

        public static readonly DependencyProperty NumberProperty =
       DependencyProperty.Register("Number", typeof(String), typeof(GraphicCardTileCustomUI));
        
        #endregion

        public override void SelectIndicatorValues()
        {
            this.IndicatorValues.ClearOnUI();

            try
            {
                // NamePerDevice
                var newestNames = (from p in this.Indicators
                                   where p.Name == "NamePerDevice"
                                   from q in p.IndicatorValues
                                   select q).FirstOrDefault();
                if (newestNames != null)
                {
                    var splittedStrings = newestNames.Value.ToString().Split(';');
                    foreach (string name in splittedStrings)
                    {
                        this.GraphicCardNames.BeginAddOnUI(new IndicatorValue(name, Core.DataType.String, newestNames.Timestamp, newestNames.MappingState));
                    }

                }
            }
            catch (Exception)
            {
                Console.WriteLine("GRAPHIC CARD VIS PLUGIN: Problem at getting NamePerDevice");
            }


            try
            {
                // Number of devices
                var numberOfDevices = (from p in this.Indicators
                                       where p.Name == "NumberOfDevices"
                                       select p.IndicatorValues).FirstOrDefault();
                if (numberOfDevices != null && numberOfDevices.Count > 0)
                {
                    Number = numberOfDevices.First().Value.ToString();
                }
                else
                {
                    Number = " - ";
                }
            }
            catch (Exception)
            {
                Console.WriteLine("GRAPHIC CARD VIS PLUGIN: Problem at getting NumberOfDevices");
            }
        }
    }
}

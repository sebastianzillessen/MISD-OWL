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

namespace MISD.Plugins.Visualization.RAM
{
    [Export(typeof(ITileCustomUI))]
    public class RAMTileCustomUI : TileCustomUI
    {

        public RAMTileCustomUI()
        {
            this.SwapLoad = new ExtendedObservableCollection<IndicatorValue>();
            this.SwapSize = " - ";
            this.Load = new ExtendedObservableCollection<IndicatorValue>();
            this.Size = " - ";
        }

        #region Properties
        public String Size
        {
            get
            {
                return (String)this.GetValue(SizeProperty);
            }
            set
            {
                this.SetValue(SizeProperty, value);
            }
        }
        public ExtendedObservableCollection<IndicatorValue> Load
        {
            get
            {
                return (ExtendedObservableCollection<IndicatorValue>)this.GetValue(LoadProperty);
            }
            set
            {
                this.SetValue(LoadProperty, value);
            }
        }
        public String SwapSize
        {
            get
            {
                return (String)this.GetValue(SwapSizeProperty);
            }
            set
            {
                this.SetValue(SwapSizeProperty, value);
            }
        }

        public ExtendedObservableCollection<IndicatorValue> SwapLoad
        {
            get
            {
                return (ExtendedObservableCollection<IndicatorValue>)this.GetValue(SwapLoadProperty);
            }
            set
            {
                this.SetValue(SwapLoadProperty, value);
            }
        }
        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register("Size", typeof(String), typeof(RAMTileCustomUI));

        public static readonly DependencyProperty LoadProperty =
            DependencyProperty.Register("Load", typeof(ExtendedObservableCollection<IndicatorValue>), typeof(RAMTileCustomUI));

        public static readonly DependencyProperty SwapLoadProperty =
            DependencyProperty.Register("SwapLoad", typeof(ExtendedObservableCollection<IndicatorValue>), typeof(RAMTileCustomUI));

        public static readonly DependencyProperty SwapSizeProperty =
            DependencyProperty.Register("SwapSize", typeof(String), typeof(RAMTileCustomUI));
        #endregion


        public override void SelectIndicatorValues()
        {

            this.Load.ClearOnUI();
            this.SwapLoad.ClearOnUI();
            try
            {
                // Size
                var size = (from p in this.Indicators
                            where p.Name == "Size"
                            from q in p.IndicatorValues
                            orderby q.Timestamp descending
                            select q).FirstOrDefault();
                if (size != null && !size.Value.ToString().Equals(""))
                {
                    this.Size = size.Value.ToString() + " MB";
                }
                else 
                {
                    this.Size = " - ";
                }
            }
            catch (Exception)
            {
                Console.WriteLine("RAM VIS PLUGIN: Problem at getting Size");
                this.Size = " - ";
            }

            try
            {
                // Load
                var loads = (from p in this.Indicators
                                               where p.Name == "Load"
                                               from q in p.IndicatorValues
                                               select q);
                this.Load.BeginAddRange(DiagramHelper.filterDiagramValues(loads));

            }
            catch (Exception)
            {
                Console.WriteLine("RAM VIS PLUGIN: Problem at getting Load");
            }

            try
            {
                // SwapSize
                var swapSize = (from p in this.Indicators
                            where p.Name == "SwapSize"
                            from q in p.IndicatorValues
                            orderby q.Timestamp descending
                            select q).FirstOrDefault();
                if (swapSize != null && !swapSize.Value.ToString().Equals(""))
                {
                    this.SwapSize = swapSize.Value.ToString() + " MB";
                }
                else
                {
                    this.SwapSize = " - ";
                }
            }
            catch (Exception)
            {
                Console.WriteLine("RAM VIS PLUGIN: Problem at getting SwapSize");
                this.SwapSize = " - ";
            }

            try
            {
                // SwapLoad
                var swapLoads = (from p in this.Indicators
                                        where p.Name == "SwapLoad"
                                        from q in p.IndicatorValues
                                        select q);
                this.SwapLoad.BeginAddRange(DiagramHelper.filterDiagramValues(swapLoads));

            }
            catch (Exception)
            {
                Console.WriteLine("RAM VIS PLUGIN: Problem at getting SwapLoad");
            }
        }
    }
}

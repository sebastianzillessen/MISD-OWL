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
using System.Windows.Controls.DataVisualization.Charting;
using MISD.Client.Model;

namespace MISD.Plugins.Visualization.CPU
{
    [Export(typeof(ITileCustomUI))]
    public class CPUTileCustomUI : TileCustomUI
    {
        public CPUTileCustomUI()
        {
            this.Temperature = new ExtendedObservableCollection<IndicatorValue>();
            this.TemperaturePerCore = new ExtendedObservableCollection<ExtendedObservableCollection<IndicatorValue>>();
            this.Load = new ExtendedObservableCollection<IndicatorValue>();
            this.LoadPerCore = new ExtendedObservableCollection<ExtendedObservableCollection<IndicatorValue>>();
            this.Name = " - ";
        }

        #region Properties

        public string Name
        {
            get
            {
                return (string)this.GetValue(NameProperty);
            }
            set
            {
                this.SetValue(NameProperty, value);
            }
        }

        public ExtendedObservableCollection<IndicatorValue> Temperature
        {
            get
            {
                return (ExtendedObservableCollection<IndicatorValue>)this.GetValue(TemperatureProperty);
            }
            set
            {
                this.SetValue(TemperatureProperty, value);
            }
        }

        public ExtendedObservableCollection<ExtendedObservableCollection<IndicatorValue>> TemperaturePerCore
        {
            get
            {
                return (ExtendedObservableCollection<ExtendedObservableCollection<IndicatorValue>>)this.GetValue(TemperaturePerCoreProperty);
            }
            set
            {
                this.SetValue(TemperaturePerCoreProperty, value);
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

        public ExtendedObservableCollection<ExtendedObservableCollection<IndicatorValue>> LoadPerCore
        {
            get
            {
                return (ExtendedObservableCollection<ExtendedObservableCollection<IndicatorValue>>)this.GetValue(LoadPerCoreProperty);
            }
            set
            {
                this.SetValue(LoadPerCoreProperty, value);
            }
        }

        #endregion

        #region Dependency Property

        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(CPUTileCustomUI));

        public static readonly DependencyProperty TemperatureProperty =
            DependencyProperty.Register("Temperature", typeof(ExtendedObservableCollection<IndicatorValue>), typeof(CPUTileCustomUI));

        public static readonly DependencyProperty TemperaturePerCoreProperty =
            DependencyProperty.Register("TemperaturePerCore", typeof(ExtendedObservableCollection<ExtendedObservableCollection<IndicatorValue>>), typeof(CPUTileCustomUI));

        public static readonly DependencyProperty LoadProperty =
            DependencyProperty.Register("Load", typeof(ExtendedObservableCollection<IndicatorValue>), typeof(CPUTileCustomUI));

        public static readonly DependencyProperty LoadPerCoreProperty =
            DependencyProperty.Register("LoadPerCore", typeof(ExtendedObservableCollection<ExtendedObservableCollection<IndicatorValue>>), typeof(CPUTileCustomUI));

        #endregion

        public override void SelectIndicatorValues()
        {
            this.IndicatorValues.ClearOnUI();

            this.Temperature.ClearOnUI();
            this.TemperaturePerCore.ClearOnUI();

            this.Load.ClearOnUI();
            this.LoadPerCore.ClearOnUI();

            try
            {
                // Name
                var name = (from p in this.Indicators
                            where p.Name == "ProcessorName"
                            from q in p.IndicatorValues
                            orderby q.Timestamp descending
                            select q).FirstOrDefault();
                if (name != null) this.Name = name.Value as string;
                else this.Name = " - ";
            }
            catch (Exception)
            {
                Console.WriteLine("CPU VIS PLUGIN: Problem at getting ProcessorName");
                this.Name = " - ";
            }

            try
            {
                var temperatures = (from p in this.Indicators
                                               where p.Name == "Temperature"
                                               from q in p.IndicatorValues
                                               orderby q.Timestamp descending
                                               select new IndicatorValue(Convert.ToByte(q.Value), q.DataType, q.Timestamp, q.MappingState));
                ExtendedObservableCollection<IndicatorValue> filteredTemps = DiagramHelper.filterDiagramValues(temperatures);
                Temperature.BeginAddRange(filteredTemps);


                // this adds all values to the diagram, way too much for wpf toolkit diagrams
                // Temperature
                //this.Temperature.BeginAddRange(from p in this.Indicators
                //                               where p.Name == "Temperature"
                //                               from q in p.IndicatorValues
                //                               select new IndicatorValue(Convert.ToByte(q.Value), q.DataType, q.Timestamp, q.MappingState));

            }
            catch (Exception)
            {
                Console.WriteLine("CPU VIS PLUGIN: Problem at getting Temperature");
            }

            try
            {
                // Temperature Per Core
                var temperatures = from p in this.Indicators
                                   where p.Name == "TemperaturePerCore"
                                   from q in p.IndicatorValues
                                   select q;
                if (temperatures.Count() > 0)
                {
                    IndicatorValue first = null;
                    int z = 0;
                    while (first == null)
                    {
                        if(!temperatures.ElementAt(z).Value.ToString().Equals("")){
                            first = temperatures.ElementAt(z);
                        }
                        z++;
                    }

                    for (int i = 0; i < first.Value.ToString().Split(';').Length; i++)
                    {
                        ExtendedObservableCollection<IndicatorValue> rawValues = new ExtendedObservableCollection<IndicatorValue>();
                        ExtendedObservableCollection<IndicatorValue> filteredValues;

                        foreach (IndicatorValue tempValue in temperatures)
                        {
                            string[] temps = tempValue.Value.ToString().Split(';');
                            if (temps.Length > 0 && temps.Length == first.Value.ToString().Split(';').Length && !temps[i].Equals(""))
                            {
                                rawValues.Add(new IndicatorValue(Convert.ToByte(temps[i]), tempValue.DataType, tempValue.Timestamp, tempValue.MappingState));
                            }

                        }
                        filteredValues = DiagramHelper.filterDiagramValues(rawValues);
                        this.TemperaturePerCore.BeginAddOnUI(filteredValues);
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("CPU VIS PLUGIN: Problem at getting TemperaturePerCore");
            }


            try
            {
                // Load
                var load = (from p in this.Indicators
                                        where p.Name == "Load"
                                        from q in p.IndicatorValues
                                        select new IndicatorValue(Convert.ToByte(q.Value), q.DataType, q.Timestamp, q.MappingState));
                this.Load.BeginAddRange(DiagramHelper.filterDiagramValues(load));

            }
            catch (Exception)
            {
                Console.WriteLine("CPU VIS PLUGIN: Problem at getting Load");
            }

            try
            {
                // Load Per Core
                var loadValues = from p in this.Indicators
                                      where p.Name == "LoadPerCore"
                                      from q in p.IndicatorValues
                                      select q;

                if (loadValues.Count() > 0)
                {

                    IndicatorValue first = null;
                    int z = 0;
                    while (first == null)
                    {
                        if (!loadValues.ElementAt(z).Value.ToString().Equals(""))
                        {
                            first = loadValues.ElementAt(z);
                        }
                        z++;
                    }

                    for (int i = 0; i < first.Value.ToString().Split(';').Length; i++)
                    {
                        ExtendedObservableCollection<IndicatorValue> rawValues = new ExtendedObservableCollection<IndicatorValue>();
                        ExtendedObservableCollection<IndicatorValue> filteredValues;

                        foreach (IndicatorValue value in loadValues)
                        {
                            string[] loads = value.Value.ToString().Split(';');
                            if (loads.Length > 0 && loads.Length == (first.Value as string).Split(';').Length && !loads[i].Equals(""))
                            {
                                rawValues.Add(new IndicatorValue(Convert.ToByte(loads[i]), value.DataType, value.Timestamp, value.MappingState));
                            }
                        }
                        filteredValues = DiagramHelper.filterDiagramValues(rawValues);
                        this.LoadPerCore.BeginAddOnUI(filteredValues);
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("CPU VIS PLUGIN: Problem at getting LoadPerCore");
            }
        }
    }
}

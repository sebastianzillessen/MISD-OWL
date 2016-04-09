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

namespace MISD.Plugins.Visualization.Storage
{
    [Export(typeof(ITileCustomUI))]
    public class StorageTileCustomUI : TileCustomUI
    {
        public StorageTileCustomUI()
        {
            this.Capacity = " - ";
            this.Load = new ExtendedObservableCollection<IndicatorValue>();
            this.NumberOfDrives = " - ";
            this.CapacityPerDrive = new ExtendedObservableCollection<IndicatorValue>();
            this.LoadPerDrive = new ExtendedObservableCollection<IndicatorValue>();
        }

        #region Properties
        public String Capacity
        {
            get
            {
                return (String)this.GetValue(CapacityProperty);
            }
            set
            {
                this.SetValue(CapacityProperty, value);
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
        public String NumberOfDrives
        {
            get
            {
                return (String)this.GetValue(NumberOfDrivesProperty);
            }
            set
            {
                this.SetValue(NumberOfDrivesProperty, value);
            }
        }
        public ExtendedObservableCollection<IndicatorValue> CapacityPerDrive
        {
            get
            {
                return (ExtendedObservableCollection<IndicatorValue>)this.GetValue(CapacityPerDriveProperty);
            }
            set
            {
                this.SetValue(CapacityPerDriveProperty, value);
            }
        }
        public ExtendedObservableCollection<IndicatorValue> LoadPerDrive
        {
            get
            {
                return (ExtendedObservableCollection<IndicatorValue>)this.GetValue(LoadPerDriveProperty);
            }
            set
            {
                this.SetValue(LoadPerDriveProperty, value);
            }
        }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty CapacityProperty =
            DependencyProperty.Register("Capacity", typeof(String), typeof(StorageTileCustomUI));

        public static readonly DependencyProperty LoadProperty =
            DependencyProperty.Register("Load", typeof(ExtendedObservableCollection<IndicatorValue>), typeof(StorageTileCustomUI));

        public static readonly DependencyProperty NumberOfDrivesProperty =
            DependencyProperty.Register("NumberOfDrives", typeof(String), typeof(StorageTileCustomUI));

        public static readonly DependencyProperty CapacityPerDriveProperty =
            DependencyProperty.Register("CapacityPerDrive", typeof(ExtendedObservableCollection<IndicatorValue>), typeof(StorageTileCustomUI));

        public static readonly DependencyProperty LoadPerDriveProperty =
            DependencyProperty.Register("LoadPerDrive", typeof(ExtendedObservableCollection<IndicatorValue>), typeof(StorageTileCustomUI));
        #endregion
        public override void SelectIndicatorValues()
        {
            this.Load.ClearOnUI();
            this.CapacityPerDrive.ClearOnUI();
            this.LoadPerDrive.ClearOnUI();

            try
            {
                // Capacity
                var capacity = (from p in this.Indicators
                            where p.Name == "Capacity"
                            from q in p.IndicatorValues
                            orderby q.Timestamp descending
                            select q).FirstOrDefault();
                if (capacity != null && !capacity.Value.ToString().Equals(""))
                {
                    this.Capacity = capacity.Value.ToString() + " MB";
                }
                else
                {
                    this.Capacity = " - ";
                }
            }
            catch (Exception)
            {
                Console.WriteLine("STORAGE VIS PLUGIN: Problem at getting Capacity");
                this.Capacity = " - ";
            }


            try
            {
                // Load
                var loads = (from p in this.Indicators
                                               where p.Name == "Load"
                                               from q in p.IndicatorValues
                                               select q);

                ExtendedObservableCollection<IndicatorValue> filteredLoad = DiagramHelper.filterDiagramValues(loads);
                
                this.Load.BeginAddRange(filteredLoad);

            }
            catch (Exception)
            {
                Console.WriteLine("STORAGE VIS PLUGIN: Problem at getting Load");
            }


            try
            {
                // NumberOfDrives
                var numberOfDrives = (from p in this.Indicators
                                where p.Name == "NumberOfDrives"
                                from q in p.IndicatorValues
                                orderby q.Timestamp descending
                                select q).FirstOrDefault();
                if (numberOfDrives != null && !numberOfDrives.Value.ToString().Equals(""))
                {
                    this.NumberOfDrives = numberOfDrives.Value.ToString();
                }
                else
                {
                    this.NumberOfDrives = " - ";
                }
            }
            catch (Exception)
            {
                Console.WriteLine("STORAGE VIS PLUGIN: Problem at getting NumberOfDrives");
                this.NumberOfDrives = " - ";
            }

            try
            {
                // CapacityPerDrive
                var capacityPerDrive = (from p in this.Indicators
                                      where p.Name == "CapacityPerDrive"
                                      from q in p.IndicatorValues
                                      orderby q.Timestamp descending
                                      select q).FirstOrDefault();
                if (capacityPerDrive != null && !capacityPerDrive.Value.ToString().Equals(""))
                {
                    for (int i = 0; i < capacityPerDrive.Value.ToString().Split(';').Length; i++)
                    {
                        CapacityPerDrive.BeginAddOnUI(new IndicatorValue(Convert.ToInt32(capacityPerDrive.Value.ToString().Split(';')[i]), capacityPerDrive.DataType,
                            capacityPerDrive.Timestamp, capacityPerDrive.MappingState));
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("STORAGE VIS PLUGIN: Problem at getting CapacityPerDrive");
            }

            try
            {
                // LoadPerDrive
                var loadPerDrive = (from p in this.Indicators
                                        where p.Name == "LoadPerDrive"
                                        from q in p.IndicatorValues
                                        orderby q.Timestamp descending
                                        select q).FirstOrDefault();
                if (loadPerDrive != null && !loadPerDrive.Value.ToString().Equals(""))
                {
                    for (int i = 0; i < loadPerDrive.Value.ToString().Split(';').Length; i++)
                    {
                        LoadPerDrive.BeginAddOnUI(new IndicatorValue(Convert.ToByte(loadPerDrive.Value.ToString().Split(';')[i]), loadPerDrive.DataType,
                            DateTime.Now.AddDays(i), loadPerDrive.MappingState));
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("STORAGE VIS PLUGIN: Problem at getting LoadPerDrive");
            }
        }
    }
}

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

namespace MISD.Plugins.Visualization.NetworkAdapter
{
    [Export(typeof(ITileCustomUI))]
    public class NetworkAdapterTileCustomUI : TileCustomUI
    {

        public NetworkAdapterTileCustomUI()
        {
            NumberOfAdapters = " - ";
            NamePerAdapter = new ExtendedObservableCollection<IndicatorValue>();
            IPPerAdapter = new ExtendedObservableCollection<IndicatorValue>();
            MACPerAdapter = new ExtendedObservableCollection<IndicatorValue>();
            UpPerAdapter = new List<KeyValuePair<string, int>>();
            DownPerAdapter = new List<KeyValuePair<string, int>>();
        }

        #region Properties
        public String NumberOfAdapters
        {
            get
            {
                return (String)this.GetValue(NumberOfAdaptersProperty);
            }
            set
            {
                this.SetValue(NumberOfAdaptersProperty, value);
            }
        }
        public ExtendedObservableCollection<IndicatorValue> NamePerAdapter
        {
            get
            {
                return (ExtendedObservableCollection<IndicatorValue>)this.GetValue(NamePerAdapterProperty);
            }
            set
            {
                this.SetValue(NamePerAdapterProperty, value);
            }
        }
        public ExtendedObservableCollection<IndicatorValue> IPPerAdapter
        {
            get
            {
                return (ExtendedObservableCollection<IndicatorValue>)this.GetValue(IPPerAdapterProperty);
            }
            set
            {
                this.SetValue(IPPerAdapterProperty, value);
            }
        }
        public ExtendedObservableCollection<IndicatorValue> MACPerAdapter
        {
            get
            {
                return (ExtendedObservableCollection<IndicatorValue>)this.GetValue(MACPerAdapterProperty);
            }
            set
            {
                this.SetValue(MACPerAdapterProperty, value);
            }
        }
        public List<KeyValuePair<string, int>> UpPerAdapter
        {
            get
            {
                return (List<KeyValuePair<string, int>>)this.GetValue(UpPerAdapterProperty);
            }
            set
            {
                this.SetValue(UpPerAdapterProperty, value);
            }
        }
        public List<KeyValuePair<string, int>> DownPerAdapter
        {
            get
            {
                return (List<KeyValuePair<string, int>>)this.GetValue(DownPerAdapterProperty);
            }
            set
            {
                this.SetValue(DownPerAdapterProperty, value);
            }
        }
        #endregion

        #region Dependency Property

        public static readonly DependencyProperty NumberOfAdaptersProperty =
            DependencyProperty.Register("NumberOfAdapters", typeof(String), typeof(NetworkAdapterTileCustomUI));

        public static readonly DependencyProperty NamePerAdapterProperty =
           DependencyProperty.Register("NamePerAdapter", typeof(ExtendedObservableCollection<IndicatorValue>), typeof(NetworkAdapterTileCustomUI));

        public static readonly DependencyProperty IPPerAdapterProperty =
            DependencyProperty.Register("IPPerAdapter", typeof(ExtendedObservableCollection<IndicatorValue>), typeof(NetworkAdapterTileCustomUI));

        public static readonly DependencyProperty MACPerAdapterProperty =
            DependencyProperty.Register("MACPerAdapter", typeof(ExtendedObservableCollection<IndicatorValue>), typeof(NetworkAdapterTileCustomUI));

        public static readonly DependencyProperty UpPerAdapterProperty =
          DependencyProperty.Register("UpPerAdapter", typeof(List<KeyValuePair<string, int>>), typeof(NetworkAdapterTileCustomUI));

        public static readonly DependencyProperty DownPerAdapterProperty =
          DependencyProperty.Register("DownPerAdapter", typeof(List<KeyValuePair<string, int>>), typeof(NetworkAdapterTileCustomUI));

        #endregion
        public override void SelectIndicatorValues()
            {

                this.NamePerAdapter.ClearOnUI();
                this.IPPerAdapter.ClearOnUI();
                this.MACPerAdapter.ClearOnUI();
                this.UpPerAdapter.ClearOnUI();
                this.DownPerAdapter.ClearOnUI();

                try
                {
                    // NumberOfAdapters
                    var numberOfAdapters = (from p in this.Indicators
                                where p.Name == "NumberOfAdapters"
                                from q in p.IndicatorValues
                                orderby q.Timestamp descending
                                select q).FirstOrDefault();
                    if (numberOfAdapters != null && !numberOfAdapters.Value.ToString().Equals(""))
                    {
                        this.NumberOfAdapters = numberOfAdapters.Value.ToString();
                    }
                    else
                    {
                        this.NumberOfAdapters = " - "; 
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("NETWORK ADAPTER VIS PLUGIN: Problem at getting NumberOfAdapters");
                    this.NumberOfAdapters = " - ";
                }

                try
                {
                    // NamePerAdapter
                    var namePerAdapter = (from p in this.Indicators
                                            where p.Name == "NamePerAdapter"
                                            from q in p.IndicatorValues
                                            orderby q.Timestamp descending
                                            select q).FirstOrDefault();
                    if (namePerAdapter != null && !namePerAdapter.Value.ToString().Equals(""))
                    {
                        for (int i = 0; i < namePerAdapter.Value.ToString().Split(';').Length; i++)
                        {
                            NamePerAdapter.BeginAddOnUI(new IndicatorValue(namePerAdapter.Value.ToString().Split(';')[i],
                                Core.DataType.String, namePerAdapter.Timestamp, namePerAdapter.MappingState));
                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("NETWORK ADAPTER VIS PLUGIN: Problem at getting NamePerAdapter");
                }

                try
                {
                    // IPPerAdapter
                    var iPPerAdapter = (from p in this.Indicators
                                          where p.Name == "IPPerAdapter"
                                          from q in p.IndicatorValues
                                          orderby q.Timestamp descending
                                          select q).FirstOrDefault();
                    if (iPPerAdapter != null && !iPPerAdapter.Value.ToString().Equals(""))
                    {
                        for (int i = 0; i < iPPerAdapter.Value.ToString().Split(';').Length; i++)
                        {
                            IPPerAdapter.BeginAddOnUI(new IndicatorValue(iPPerAdapter.Value.ToString().Split(';')[i],
                                Core.DataType.String, iPPerAdapter.Timestamp, iPPerAdapter.MappingState));
                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("NETWORK ADAPTER VIS PLUGIN: Problem at getting IPPerAdapter");
                }

                try
                {
                    // MACPerAdapter
                    var mACPerAdapter = (from p in this.Indicators
                                        where p.Name == "MACPerAdapter"
                                        from q in p.IndicatorValues
                                        orderby q.Timestamp descending
                                        select q).FirstOrDefault();
                    if (mACPerAdapter != null && !mACPerAdapter.Value.ToString().Equals(""))
                    {
                        for (int i = 0; i < mACPerAdapter.Value.ToString().Split(';').Length; i++)
                        {
                            MACPerAdapter.BeginAddOnUI(new IndicatorValue(mACPerAdapter.Value.ToString().Split(';')[i],
                                Core.DataType.String, mACPerAdapter.Timestamp, mACPerAdapter.MappingState));
                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("NETWORK ADAPTER VIS PLUGIN: Problem at getting MACPerAdapter");
                }

                try
                {
                    // UpPerAdapter
                    var upPerAdapter = (from p in this.Indicators
                                        where p.Name == "UpPerAdapter"
                                         from q in p.IndicatorValues
                                         orderby q.Timestamp descending
                                         select q).FirstOrDefault();
                    if (upPerAdapter != null && !upPerAdapter.Value.ToString().Equals(""))
                    {
                        for (int i = 0; i < upPerAdapter.Value.ToString().Split(';').Length; i++)
                        {
                            if (!upPerAdapter.Value.ToString().Split(';')[i].Equals("0"))
                            {
                                UpPerAdapter.Add(new KeyValuePair<string, int>("Adapter" + i, Convert.ToInt32(upPerAdapter.Value.ToString().Split(';')[i].Replace(",", ""))));
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("NETWORK ADAPTER VIS PLUGIN: Problem at getting UpPerAdapter");
                }

                try
                {
                    // DownPerAdapter
                    var downPerAdapter = (from p in this.Indicators
                                          where p.Name == "DownPerAdapter"
                                        from q in p.IndicatorValues
                                        orderby q.Timestamp descending
                                        select q).FirstOrDefault();
                    if (downPerAdapter != null && !downPerAdapter.Value.ToString().Equals(""))
                    {
                        for (int i = 0; i < downPerAdapter.Value.ToString().Split(';').Length; i++)
                        {
                            if (!downPerAdapter.Value.ToString().Split(';')[i].Equals("0"))
                            {
                                DownPerAdapter.Add(new KeyValuePair<string, int>("Adapter" + i, Convert.ToInt32(downPerAdapter.Value.ToString().Split(';')[i].ToString().Replace(",", ""))));
                            }
                                                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("NETWORK ADAPTER VIS PLUGIN: Problem at getting DownPerAdapter");
                }
            }
        }
    
}

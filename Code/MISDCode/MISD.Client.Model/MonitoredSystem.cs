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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MISD.Core;
using System.Collections;
using MISD.Client.Managers;
using MISD.Client.Model.Managers;

namespace MISD.Client.Model
{
    /// <summary>
    /// This class represents a monitored system.
    /// </summary>
    [Serializable]
    public class MonitoredSystem : TileableElement
    {
        #region Fields

        private int ouID;
        private string mac;
        private MappingState state;
        private DateTime? resetDate;
        private ExtendedObservableCollection<Plugin> plugins;
        private string currentPlatform;
        private bool isAvailable;
        private DateTime? lastUpdate;
        private int sequence = 0;
        private bool customUIValuesLoaded = false;

        #endregion

        #region Properties


        /// <summary>
        /// Gets or sets the MAC of this monitored system.
        /// </summary>
        public string MAC
        {
            get
            {
                return this.mac;
            }
            set
            {
                if (this.mac != value)
                {
                    this.mac = value;
                    this.OnPropertyChanged();
                }

            }
        }

        /// <summary>
        /// Gets or sets the state of this monitored system.
        /// </summary>
        public MappingState State
        {
            get
            {
                return this.state;
            }
            set
            {
                if (this.state != value)
                {
                    this.state = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public DateTime? ResetDate
        {
            get
            {
                return resetDate;
            }
            set
            {
                if (this.resetDate != value)
                {
                    resetDate = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets and sets the plugins of this monitored System.
        /// </summary>
        public ExtendedObservableCollection<Plugin> Plugins
        {
            get
            {
                if (this.plugins == null) this.plugins = new ExtendedObservableCollection<Plugin>();
                return this.plugins;
            }
            set
            {
                if (this.plugins != value)
                {
                    if (this.plugins != null)
                    {
                        value.CollectionChanged -= value_CollectionChanged;
                    }
                    if (value != null)
                    {
                        value.CollectionChanged += value_CollectionChanged;
                    }
                    this.plugins = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged("CriticalPlugins");
                }
            }
        }

        void value_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.OnPropertyChanged("CriticalPlugins");
        }

        public IEnumerable<Plugin> CriticalPlugins
        {
            get
            {
                if (this.Plugins.Count == 0)
                {
                    return null;
                }
                else
                {
                    return from p in this.Plugins
                           where p.PluginMapping == MappingState.Warning || p.PluginMapping == MappingState.Critical
                           select p;
                }
            }
        }

        /// <summary>
        /// Needed to update ui components
        /// </summary>
        public int Sequence
        {
            get
            {
                return sequence;
            }
            set
            {
                if (this.sequence != value)
                {
                    sequence = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets and sets the platform of this monitored System.
        /// </summary>
        public string CurrentPlatform
        {
            get
            {
                return this.currentPlatform;
            }
            set
            {
                if (this.currentPlatform != value)
                {
                    this.currentPlatform = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets and sets the IsAvailable flag of this monitored System.
        /// </summary>
        public bool IsAvailable
        {
            get
            {
                return this.isAvailable;
            }
            set
            {
                if (this.isAvailable != value)
                {
                    this.isAvailable = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets and sets the ou ID of this monitored System.
        /// </summary>
        public int OuID
        {
            get
            {
                return this.ouID;
            }
            set
            {
                if (this.ouID != value)
                {
                    DataModel.Instance.MoveMonitoredSystem(this, this.ouID, value);

                    this.ouID = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the last update time of this monitored system.
        /// </summary>
        public DateTime? LastUpdate
        {
            get
            {
                return this.lastUpdate;
            }
            set
            {
                if (this.lastUpdate != value)
                {
                    this.lastUpdate = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public bool CustomUIValuesLoaded
        {
            get { return this.customUIValuesLoaded; }
            set
            {
                if (this.customUIValuesLoaded != value)
                {
                    this.customUIValuesLoaded = value;
                    this.OnPropertyChanged();
                }
            }
        }

        #endregion

        #region constructors

        public MonitoredSystem(string name, int id, string mac, string fqdn, MappingState state, DateTime? resetDate, ExtendedObservableCollection<Plugin> plugins,
            string platform, bool isAvailable, int ouID, DateTime? lastUpdate)
            : base(id, name, fqdn)
        {
            Intialize();
            this.MAC = mac;
            this.State = state;
            this.ResetDate = resetDate;
            this.Plugins = plugins;
            this.CurrentPlatform = platform;
            this.IsAvailable = isAvailable;
            this.OuID = ouID;
            this.LastUpdate = lastUpdate;
            MonitoredSystemState ms = LayoutManager.Instance.GetMSState(id);
            if (ms == null)
            {
                var l = (from p in DataModel.Instance.LevelDefinitions
                         orderby p.Level ascending
                         select p.LevelID).First();
                LayoutManager.Instance.SetMSState(ID, l);
            }
            else
            {
                foreach (var p in ms.ShownPlugins)
                {
                    LayoutManager.Instance.SetMSState(ID, ms.Level, p, true);
                }
            }
        }

        #endregion

        #region Methods

        protected override void MoveElement(int ID, float Value)
        {
            if (this.ID == ID)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OrganizationalUnit ou = DataModel.Instance.GetOu(OuID);
                    if (ou != null)
                    {
                        ExtendedObservableCollection<TileableElement> children = ou.Elements;
                        children.Sort();
                    }
                });
            }

        }

        private void Intialize()
        {
            this.PropertyChanged += MonitoredSystem_PropertyChanged;
            LayoutManager.Instance.MonitoredSystemStateChanged += Instance_MonitoredSystemStateChanged;
        }

        // fire a update to get the converteers run again...
        void Instance_MonitoredSystemStateChanged(int ID, int level, string[] flippedPlugins)
        {
            if (ID == this.ID)
            {
                this.Sequence = Sequence++;
            }
        }

        void MonitoredSystem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!DataModel.Instance.IsPowerwall)
            {
                StackTrace stackTrace = new StackTrace();
                if (!stackTrace.ToString().Contains("DataModel"))
                {
                    DateTime updateTime = DateTime.Now;

                    switch (e.PropertyName)
                    {
                        case "State":
                            this.LastUpdate = updateTime;
                            WorkerThread workerThreadState = ThreadManager.CreateWorkerThread("BackSync_MonitoredSystem.State", () =>
                            {
                                List<Tuple<string, DateTime>> paramList = new List<Tuple<string, DateTime>>();
                                paramList.Add(new Tuple<string, DateTime>(this.MAC, updateTime));

                                bool result;
                                if (this.State == MappingState.Maintenance)
                                {
                                    result = DataModel.Instance.SyncBackMonitoredSystemMaintenanceActivated(paramList);
                                }
                                else
                                {
                                    result = DataModel.Instance.SyncBackMonitoredSystemMaintenanceDeactivated(paramList);
                                }
                            }, false);
                            break;
                        case "ResetDate":
                            this.LastUpdate = updateTime;
                            WorkerThread workerThreadResetDate = ThreadManager.CreateWorkerThread("BackSync_MonitoredSystem.ResetDate", () =>
                            {
                                List<Tuple<string, DateTime>> paramList = new List<Tuple<string, DateTime>>();
                                paramList.Add(new Tuple<string, DateTime>(this.MAC, updateTime));

                                bool result = DataModel.Instance.SyncBackMonitoredSystemResetDateUpdated(paramList);
                            }, false);
                            break;
                        case "OuID":
                            this.LastUpdate = updateTime;
                            WorkerThread workerThreadOuID = ThreadManager.CreateWorkerThread("BackSync_MonitoredSystem.OuID", () =>
                            {
                                List<Tuple<string, int, DateTime>> paramList = new List<Tuple<string, int, DateTime>>();
                                paramList.Add(new Tuple<string, int, DateTime>(this.MAC, this.OuID, updateTime));

                                bool result = DataModel.Instance.SyncBackMonitoredSystemMoved(paramList);
                            }, false);
                            break;
                        case "LastUpdate":
                            /* Do nothing */
                            break;
                        case "Name":
                            this.LastUpdate = updateTime;

                            Console.WriteLine(">>> NAME CHANGED");

                            WorkerThread workerThreadName = ThreadManager.CreateWorkerThread("BackSync_MonitoredSystem.Name", () =>
                            {
                                List<Tuple<string, string, DateTime>> paramList = new List<Tuple<string, string, DateTime>>();
                                paramList.Add(new Tuple<string, string, DateTime>(this.MAC, this.Name, updateTime));

                                bool result = DataModel.Instance.SyncBackMonitoredSystemName(paramList);
                            }, false);
                            break;
                        default:
                            break;
                    }
                }
                stackTrace = null;
            }
        }

        #endregion

        public override bool Equals(Object obj)
        {
            if (obj != null)
            {
                if (obj is MonitoredSystem)
                {
                    return this.MAC == (obj as MonitoredSystem).MAC;
                }
                else if (obj is Tuple<int, string, string, string>)
                {
                    //for tuple using se Model.MailUser
                    //Tuple: ID | MAC | Name | FQDN
                    return this.MAC == (obj as Tuple<int, string, string, string>).Item2;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }
    }
}
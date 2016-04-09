using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MISD.Client.Model.Managers;

namespace MISD.Client.Model
{
    public class MailUser : BindableBase
    {
        #region Fields

        private int id;
        private string name;
        private string email;
        private bool dailyMail;
        private ExtendedObservableCollection<Tuple<int, string, string, string>> registeredMonitoredSystems;

        #endregion

        #region Properties

        public int ID
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
                this.OnPropertyChanged();
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
                this.OnPropertyChanged();
            }
        }

        public string Email
        {
            get
            {
                return this.email;
            }
            set
            {
                this.email = value;
                this.OnPropertyChanged();
            }
        }

        public bool DailyMail
        {
            get
            {
                return this.dailyMail;
            }
            set
            {
                this.dailyMail = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the list of registered monitoroed systems of this mail user.
        /// Tuple: ID | MAC | Name | FQDN
        /// </summary>
        public ExtendedObservableCollection<Tuple<int, string, string, string>> RegisteredMonitoredSystems
        {
            get
            {
                if (this.registeredMonitoredSystems == null)
                {
                    this.registeredMonitoredSystems = new ExtendedObservableCollection<Tuple<int, string, string, string>>();
                }
                return this.registeredMonitoredSystems;
            }
            set
            {
                this.registeredMonitoredSystems = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region Constructors

        public MailUser()
        {
            Initialize();
        }

        public MailUser(int id, string name, string email, bool dailyMAil, ExtendedObservableCollection<Tuple<int, string, string, string>> registeredMonitoredSystems)
        {
            this.ID = id;
            this.Name = name;
            this.Email = email;
            this.DailyMail = dailyMAil;
            this.RegisteredMonitoredSystems = registeredMonitoredSystems;

            Initialize();
        }

        #endregion

        #region Methods

        private void Initialize()
        {
            this.PropertyChanged += MailUserProperties_PropertyChanged;
            this.RegisteredMonitoredSystems.CollectionChanged += RegisteredMonitoredSystems_PropertyChanged;
        }

        private void MailUserProperties_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!DataModel.Instance.IsPowerwall)
            {
                StackTrace stackTrace = new StackTrace();
                if (!stackTrace.ToString().Contains("DataModel"))
                {
                    switch (e.PropertyName)
                    {
                        case "Name":
                            WorkerThread workerThreadName = ThreadManager.CreateWorkerThread("BackSync_MailUser.Name", () =>
                            {
                                List<Tuple<int, string, string>> paramList = new List<Tuple<int, string, string>>();
                                paramList.Add(new Tuple<int, string, string>(this.ID, this.Name, this.Email));

                                bool result = DataModel.Instance.SyncBackMailUserChanged(paramList);
                            }, false);
                            break;
                        case "Email":
                            WorkerThread workerThreadEmail = ThreadManager.CreateWorkerThread("BackSync_MailUser.Email", () =>
                            {
                                List<Tuple<int, string, string>> paramList = new List<Tuple<int, string, string>>();
                                paramList.Add(new Tuple<int, string, string>(this.ID, this.Name, this.Email));

                                bool result = DataModel.Instance.SyncBackMailUserChanged(paramList);
                            }, false);
                            break;
                        case "DailyMail":
                            WorkerThread workerThreadDailMail = ThreadManager.CreateWorkerThread("BackSync_MailUser.DailyMail", () =>
                            {
                                List<Tuple<int, bool>> paramList = new List<Tuple<int, bool>>();
                                paramList.Add(new Tuple<int, bool>(this.ID, this.DailyMail));

                                bool result = DataModel.Instance.SyncBackMailUserChangedDailyMail(paramList);
                            }, false);
                            break;
                        default:
                            break;
                    }
                }
                stackTrace = null;
            }
        }

        private void RegisteredMonitoredSystems_PropertyChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!DataModel.Instance.IsPowerwall)
            {
                StackTrace stackTrace = new StackTrace();
                if (!stackTrace.ToString().Contains("DataModel"))
                {
                    if (e.NewItems != null)
                    {
                        List<string> newMonitoredSystem = new List<string>();

                        foreach (var item in e.NewItems)
                        {
                            Tuple<int, string, string, string> newItem = item as Tuple<int, string, string, string>;
                            if (newItem != null)
                            {
                                newMonitoredSystem.Add(newItem.Item2);
                            }
                        }

                        WorkerThread workerThreadNewMonitoredSystems = ThreadManager.CreateWorkerThread("BackSync_MailUser.RegisteredMonitoredSystems", () =>
                        {
                            bool result = DataModel.Instance.SyncBackMailUserAddMonitoredSystem(this.ID, newMonitoredSystem);
                        }, false);
                    }

                    if (e.OldItems != null)
                    {
                        List<string> oldMonitoredSystems = new List<string>();

                        foreach (var item in e.OldItems)
                        {
                            Tuple<int, string, string, string> oldItem = item as Tuple<int, string, string, string>;
                            if (oldItem != null)
                            {
                                oldMonitoredSystems.Add(oldItem.Item2);
                            }
                        }

                        WorkerThread workerThreadOldMSs = ThreadManager.CreateWorkerThread("BackSync_MailUser.RegisteredMonitoredSystems", () =>
                        {
                            bool result = DataModel.Instance.SyncBackMailUserRemoveMonitoredSystem(this.ID, oldMonitoredSystems);
                        }, false);
                    }
                }
                stackTrace = null;
            }
        }

        #endregion
    }
}

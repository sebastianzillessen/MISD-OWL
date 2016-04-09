using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MISD.Client.Managers;
using MISD.Client.Model.Managers;

namespace MISD.Client.Model
{
    /// <summary>
    /// This class represents an organizational unit.
    /// </summary>
    [Serializable]
    public class OrganizationalUnit : TileableElement
    {
        #region Fields
        private int? parentID;
        private ExtendedObservableCollection<TileableElement> elements;
        private DateTime? lastUpdate;

        private bool firstChange = true;


        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the id of the parent ou of this organizatioal unit.
        /// </summary>
        public int? ParentID
        {
            get
            {
                return this.parentID;
            }
            set
            {
                if (this.parentID != value || firstChange)
                {

                    DataModel.Instance.MoveOrganizationalUnit(this, this.parentID, value);
                    firstChange = false;

                    this.parentID = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the elements of this organizational unit.
        /// </summary>
        public ExtendedObservableCollection<TileableElement> Elements
        {
            get
            {
                if (this.elements == null) this.elements = new ExtendedObservableCollection<TileableElement>();
                return this.elements;
            }
            set
            {
                if (this.elements != value)
                {
                    if (this.elements != null)
                    {
                        this.elements.CollectionChanged -= Elements_CollectionChanged;
                    }
                    this.elements = value;
                    if (this.elements != null)
                    {
                        this.elements.CollectionChanged += Elements_CollectionChanged;
                    }
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the last update time of this organizational unit.
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

        #endregion

        #region Constructors

        public OrganizationalUnit(int id, string name, string fqdn, int? parentID, ExtendedObservableCollection<TileableElement> elements, DateTime? lastUpdate) : base(id, name, fqdn)
        {
            Initialize();
            this.ParentID = parentID;
            this.Elements = elements;
            this.LastUpdate = lastUpdate;
            LayoutManager.Instance.SetOUState(ID,false);
        }

        #endregion

        #region Methods

        protected override void MoveElement(int ID, float Value)
        {
            if (this.ID == ID)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (parentID.HasValue)
                    {
                        OrganizationalUnit ou = DataModel.Instance.GetOu(this.parentID.Value);
                        if (ou != null)
                        {
                            ExtendedObservableCollection<TileableElement> children = ou.Elements;
                            children.Sort();
                        }
                    }
                    else
                    {
                        DataModel.Instance.Elements.Sort();
                    }
                });
            }
        }

        private void Initialize()
        {
            this.PropertyChanged += OrganizationalUnit_PropertyChanged; 
        }

        private void Elements_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move))
            {
                if (!DataModel.Instance.IsPowerwall)
                {
                    StackTrace stackTrace = new StackTrace();
                    if (stackTrace.ToString().Contains("System.Windows.Controls"))
                    {
                        if (e.OldItems != null)
                        {
                            List<int> ouToRemove = new List<int>();

                            foreach (var item in e.OldItems)
                            {
                                OrganizationalUnit ou = item as OrganizationalUnit;
                                if (ou != null)
                                {
                                    ouToRemove.Add(ou.ID);
                                }
                            }

                            if (ouToRemove.Count > 0)
                            {
                                WorkerThread workerThread = ThreadManager.CreateWorkerThread("BackSync_OrganizationalUnit.Elements", () =>
                                {
                                    bool result = DataModel.Instance.SyncBackOURemoved(ouToRemove);
                                }, false);
                            }
                        }

                        if (e.NewItems != null)
                        {
                            List<OrganizationalUnit> ouToAdd = new List<OrganizationalUnit>();

                            foreach (var item in e.NewItems)
                            {
                                OrganizationalUnit ou = item as OrganizationalUnit;
                                if (ou != null)
                                {
                                    ouToAdd.Add(ou);
                                }
                            }

                            if (ouToAdd.Count > 0)
                            {
                                WorkerThread workerThread = ThreadManager.CreateWorkerThread("BackSync_OrganizationalUnit.Elements", () =>
                                {
                                    bool result = DataModel.Instance.SyncBackOUAdd(ouToAdd);
                                }, false);
                            }
                        }
                    }
                    stackTrace = null;
                }
            }
        }

        void OrganizationalUnit_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!DataModel.Instance.IsPowerwall)
            {
                StackTrace stackTrace = new StackTrace();
                if (!stackTrace.ToString().Contains("DataModel"))
                {
                    DateTime updateTime = DateTime.Now;

                    switch (e.PropertyName)
                    {
                        case "Name":
                            this.LastUpdate = updateTime;
                            WorkerThread workerThread = ThreadManager.CreateWorkerThread("BackSync_OrganizationalUnit.Name", () =>
                            {
                                List<Tuple<int, string, DateTime>> paramList = new List<Tuple<int, string, DateTime>>();
                                paramList.Add(new Tuple<int, string, DateTime>(this.ID, this.Name, updateTime));

                                bool result = DataModel.Instance.SyncBackOUChangedName(paramList);
                            }, false);
                            break;
                        case "ParentID":
                            this.LastUpdate = updateTime;
                            WorkerThread workerThreadParentID = ThreadManager.CreateWorkerThread("BackSync_OrganizationalUnit.ParentID", () =>
                            {
                                List<Tuple<int, int?, DateTime>> paramList = new List<Tuple<int, int?, DateTime>>();
                                paramList.Add(new Tuple<int, int?, DateTime>(this.ID, this.ParentID, updateTime));

                                bool result = DataModel.Instance.SyncBackOUMoved(paramList);
                            }, false);
                            break;
                        case "LastUpdate":
                            /* Do nothing */
                            break;
                        default:
                            break;
                    }
                }
                stackTrace = null;
            }
        }

        #endregion
    }
}

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
 * MISD-OWL is distributed without any warranty, witmlhout even the
 * implied warranty of merchantability or fitness for a particular purpose.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MISD.Core;
using MISD.Client.Model;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using MISD.Client.Managers;
using MISD.Client.Model.Resources;
using System.Globalization;
using System.Threading;
using MISD.Client.Properties;
using System.ComponentModel;
using System.Windows.Data;

namespace MISD.Client.ViewModel
{
    /// <summary>
    /// This is the view model for the main window.
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {

        #region Singleton

        private static volatile MainWindowViewModel instance;
        private static object syncRoot = new Object();

        /// <summary>
        /// Property to access the class.
        /// </summary>
        public static MainWindowViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new MainWindowViewModel();
                    }
                }

                return instance;
            }
        }
        private MainWindowViewModel()
        {
            // FontFamily und FontSize
            this.ApplicationFont = new FontFamily(Settings.Default["FontFamily"] as string);
            this.FontSize = Convert.ToDouble(Settings.Default["FontSize"]);

            LayoutManager.Instance.PropertyChanged += Instance_PropertyChanged;
            DataModel.MonitoredSystemsChanged += new EventHandler(MainWindowViewModel_UpdateElementView);
            DataModel.OrganisationUnitsChanged += new EventHandler(MainWindowViewModel_UpdateOUView);
        }

        void Instance_PropertyChanged(KeyValuePair<string, string> property)
        {
            if (property.Key.Equals(LayoutManager.PropertyValues.FONT_NAME.ToString()))
            {
                this.ApplicationFont = new FontFamily(property.Value);
            }
            else if (property.Key.Equals(LayoutManager.PropertyValues.FONT_SIZE.ToString()))
            {
                this.FontSize = Convert.ToInt32(property.Value);
            }
            else if (property.Key.Equals(LayoutManager.PropertyValues.NUMBER_OF_CHAR.ToString()))
            {
                this.NumberOfCharactersInTileName = Convert.ToInt32(property.Value);
            }
        }
        #endregion

        #region Fields

        private bool isMenuOpen = false;
        private MenuStates isMenuItemOpen = MenuStates.Closed;

        private ColorScheme colorscheme = ColorScheme.Light;

        private MISD.Client.Model.Layout currentLayout = null;

        private int selectedMSFilterdCount = 0;
        private int selectedUnignoredMSFilterdCount = 0;
        private int selectedOUFilterdCount = 0;

        // a set of all selected tileableelemts
        private ExtendedObservableCollection<object> selectedElements = new ExtendedObservableCollection<object>();
        public ExtendedObservableCollection<object> emailUnregisterdMSListSelection = new ExtendedObservableCollection<object>();

        //collection views
        private ICollectionView elemetsView;
        private ICollectionView ouView;
        private ICollectionView mailUserView;
        private ICollectionView clusterView;
        private ICollectionView ignoredMonitoredSystemView;
        private ICollectionView observedMonitoredSystemsView;
        private ICollectionView unObservedMonitoredSystemsView;

        //filter strings
        private string monitoredSystemFilterString = String.Empty;
        private string organisationunitFilterString = String.Empty;
        private string mailUserFilterString = String.Empty;
        private string clusterFilterString = String.Empty;
        private string ignoredMonitoredSystemFilterString = String.Empty;
        private string observedMonitoredSystemFilterString = String.Empty;
        private string unObservedMonitoredSystemFilterString = String.Empty;


        #endregion

        #region Properties

        public MISD.Client.Model.Layout selectedLayout = new Model.Layout();



        #region multiselect

        public String PathOfOU
        {
            get
            {
                if (SelectedOUs.Count == 1)
                {
                    String path = "";
                    var ou = SelectedOUs.FirstOrDefault();
                    path += ou.Name;
                    while (ou.ParentID != null)
                    {
                        ou = DataModel.GetOu((int)ou.ParentID);
                        path = ou.Name + "." + path;
                    }
                    return path;
                }
                else
                {
                    return Strings.NoSingleSelection;
                }
            }
            set
            {
                this.OnPropertyChanged();
            }
        }

        public int SystemsinOUs
        {
            get
            {
                int count = 0;
                ExtendedObservableCollection<TileableElement> list = new ExtendedObservableCollection<TileableElement>();
                foreach (OrganizationalUnit ou in SelectedOUs)
                {
                    list.Add(ou);
                }
                count += list.GetMonitoredSystems().Count();
                return count;
            }
            set
            {
                this.OnPropertyChanged();
            }
        }

        public Visibility ousSelected
        {
            get
            {
                if (SelectedOUs.Count.Equals(0))
                {
                    return Visibility.Hidden;
                }
                else { return Visibility.Visible; }
            }
            set
            {
                if (SelectedOUs.Count.Equals(0))
                {
                    value = Visibility.Hidden;
                }
                else { value = Visibility.Visible; }
                this.OnPropertyChanged();
            }
        }

        public Visibility msSelected
        {
            get
            {
                if (SelectedSystems.Count.Equals(0))
                {
                    return Visibility.Hidden;
                }
                else { return Visibility.Visible; }
            }
            set
            {
                if (SelectedSystems.Count.Equals(0))
                {
                    value = Visibility.Hidden;
                }
                else { value = Visibility.Visible; }
                this.OnPropertyChanged();
            }
        }

        public ExtendedObservableCollection<object> SelectedElements
        {
            get
            {
                return this.selectedElements;
            }
            set
            {
                this.selectedElements = value;
                this.OnPropertyChanged();
            }
        }

        public ExtendedObservableCollection<OrganizationalUnit> SelectedOUs
        {
            get
            {
                var result = new ExtendedObservableCollection<OrganizationalUnit>();
                foreach (var selected in selectedElements)
                {
                    if (selected is OrganizationalUnit)
                    {
                        result.Add((selected as OrganizationalUnit));
                    }
                }
                return result;
            }

        }

        public ExtendedObservableCollection<MonitoredSystem> SelectedSystems
        {
            get
            {
                var result = new ExtendedObservableCollection<MonitoredSystem>();
                foreach (var selected in selectedElements)
                {
                    if (selected is MonitoredSystem)
                    {
                        result.Add((selected as MonitoredSystem));
                    }
                }
                return result;
            }
        }

        public int SelectedItemsCount
        {
            get
            {
                return SelectedOUs.Count + SelectedElements.Count;
            }
        }

        public int SelectedMSFilterdCount
        {
            get
            {
                return selectedMSFilterdCount;
            }
            set
            {
                selectedMSFilterdCount = value;
                OnPropertyChanged();
            }
        }

        public int SelectedUnignoredMSFilterdCount
        {
            get
            {
                return selectedUnignoredMSFilterdCount;
            }
            set
            {
                selectedUnignoredMSFilterdCount = value;
                OnPropertyChanged();
            }
        }

        public int SelectedOUFilterdCount
        {
            get
            {
                return selectedOUFilterdCount;
            }
            set
            {
                selectedOUFilterdCount = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region application menu openings

        public bool IsMenuOpen
        {
            get
            {
                return isMenuOpen;

            }
            set
            {
                if (this.isMenuOpen != value)
                {
                    this.isMenuOpen = value;
                }
                this.OnPropertyChanged();

            }
        }

        public MenuStates IsMenuItemOpen
        {
            get
            {
                return isMenuItemOpen;
            }
            set
            {
                if (this.isMenuItemOpen != value)
                {
                    this.isMenuItemOpen = value;
                    this.OnPropertyChanged();
                }
            }
        }

        #endregion

        public IEnumerable<double> FontSizes
        {
            get
            {
                var fontSizes = new List<double>();

                fontSizes.Add(8);
                fontSizes.Add(9);
                fontSizes.Add(10);
                fontSizes.Add(11);
                fontSizes.Add(12);

                fontSizes.Add(14);
                fontSizes.Add(16);
                fontSizes.Add(18);
                fontSizes.Add(20);
                fontSizes.Add(22);
                fontSizes.Add(24);
                fontSizes.Add(26);
                fontSizes.Add(28);

                fontSizes.Add(36);
                fontSizes.Add(48);
                fontSizes.Add(72);

                return fontSizes;
            }
        }

        #region layout properties

        public MISD.Client.Model.Layout CurrentLayout
        {
            get
            {
                if (this.currentLayout == null)
                {
                    this.currentLayout = DataModel.Layouts.First();
                }
                return this.currentLayout;
            }
            set
            {
                this.currentLayout = value;
                this.OnPropertyChanged();
            }
        }

        public ExtendedObservableCollection<string> PluginPriority
        {
            get
            {
                return LayoutManager.Instance.PluginPriority;
            }
            set
            {
                LayoutManager.Instance.PluginPriority = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region mail properties

        #endregion

        private ExtendedObservableCollection<string> regexList;
        public ExtendedObservableCollection<string> RegexList
        {
            get
            {
                if (regexList == null)
                {
                    regexList = new ExtendedObservableCollection<string>();
                    regexList.Add("initialItem");
                }
                return regexList;
            }
            set
            {
                regexList = value;
            }
        }

        #endregion

        #region Variable Tile Sizes & Font Sizes

        /// <summary>
        /// Determines if the ui is running as a Powerwall or not.
        /// </summary>
        public bool IsPowerwall
        {
            get
            {
                return ConfigClass.IsPowerwall;
            }
            set
            {
                ConfigClass.IsPowerwall = value;
            }
        }


        /// <summary>
        /// Gets or sets the application's UI font.
        /// </summary>
        public FontFamily ApplicationFont
        {
            get
            {
                return new FontFamily(Settings.Default["FontFamily"] as string);
            }
            set
            {
                // Prüfung muss weggelassen werden, da sonst initialer aufrug schief geht
                // if ((new FontFamily(Settings.Default["FontFamily"] as string)) != value)
                {
                    Settings.Default["FontFamily"] = value.ToString();
                    Settings.Default.Save();
                    this.OnPropertyChanged();

                    Application.Current.Resources["ApplicationGlobalFont"] = value;
                    LayoutManager.Instance.SetProperty(LayoutManager.PropertyValues.FONT_NAME.ToString(), value.Source);
                    this.OnPropertyChanged("CharacterWidth");
                    this.OnPropertyChanged("TileWidth");
                    this.OnPropertyChanged("CharacterHeight");
                    this.OnPropertyChanged("TileRowHeight");
                    this.OnPropertyChanged("TileSpecialRowHeight");
                }
            }
        }

        /// <summary>
        /// Gets or sets the application's UI font size.
        /// </summary>
        public double FontSize
        {
            get
            {
                return Convert.ToDouble(Settings.Default["FontSize"]);
            }
            set
            {
                // Prüfung muss weggelassen werden, da sonst initialer aufrug schief geht
                //  if (Convert.ToDouble(Settings.Default["FontSize"]) != value)
                {
                    Settings.Default["FontSize"] = (int)value;
                    Settings.Default.Save();

                    Application.Current.Resources["ApplicationGlobalFontSize"] = value;
                    LayoutManager.Instance.SetProperty(LayoutManager.PropertyValues.FONT_SIZE.ToString(), value.ToString());

                    this.OnPropertyChanged();
                    this.OnPropertyChanged("CharacterWidth");
                    this.OnPropertyChanged("TileWidth");
                    this.OnPropertyChanged("CharacterHeight");
                    this.OnPropertyChanged("TileRowHeight");
                    this.OnPropertyChanged("TileSpecialRowHeight");
                }
            }
        }

        /// <summary>
        /// Gets or sets the number of characters that will be displayed in the tile's header.
        /// </summary>
        public int NumberOfCharactersInTileName
        {
            get
            {
                return (int)Settings.Default["NumOfChars"];
            }
            set
            {

                Settings.Default["NumOfChars"] = value;
                Settings.Default.Save();
                LayoutManager.Instance.SetProperty(LayoutManager.PropertyValues.NUMBER_OF_CHAR.ToString(), value.ToString());
                this.OnPropertyChanged();
                this.OnPropertyChanged("CharacterWidth");
                this.OnPropertyChanged("TileWidth");
                this.OnPropertyChanged("CharacterHeight");
                this.OnPropertyChanged("TileRowHeight");
                this.OnPropertyChanged("TileSpecialRowHeight");
            }
        }

        /// <summary>
        /// Gets the default width of a tile.
        /// </summary>
        public double TileWidth
        {
            get
            {
                if (this.NumberOfCharactersInTileName < 8)
                {
                    return 11 * CharacterWidth + 2 * 20;
                }
                else
                {
                    return (this.NumberOfCharactersInTileName + 3) * CharacterWidth + 2 * 20;
                }
            }
        }

        /// <summary>
        /// Gets the width of one character in the current FontFamily.
        /// </summary>
        public double CharacterWidth
        {
            get
            {
                FormattedText formattedText = new FormattedText("M", CultureInfo.GetCultureInfo(Thread.CurrentThread.CurrentUICulture.Name),
                                                  FlowDirection.LeftToRight, new Typeface(ApplicationFont.Source), this.FontSize, Brushes.Black);
                return formattedText.Width;
            }
        }

        /// <summary>
        /// Gets the height of one character in the current FontFamily.
        /// </summary>
        public double CharacterHeight
        {
            get
            {
                FormattedText formattedText = new FormattedText("jpqMIT", CultureInfo.GetCultureInfo(Thread.CurrentThread.CurrentUICulture.Name),
                                                            FlowDirection.LeftToRight, new Typeface(ApplicationFont.Source),
                                                            this.FontSize, Brushes.Black);
                return formattedText.Height;
            }
        }

        /// <summary>
        /// Gets the height of one tile row.
        /// </summary>
        public double TileRowHeight
        {
            get
            {
                return CharacterHeight + 12;
            }
        }

        /// <summary>
        /// Gets the height of one special tile row (e.g. header, status bar).
        /// </summary>
        public double TileSpecialRowHeight
        {
            get
            {
                return CharacterHeight + 12;
            }
        }

        #endregion

        #region collection views
        public ICollectionView ElementsView
        {
            get
            {
                if (elemetsView == null)
                {
                    elemetsView = CollectionViewSource.GetDefaultView(new ExtendedObservableCollection<object>());
                }
                return elemetsView;
            }
            set
            {
                elemetsView = value;
                //set filter
                elemetsView.Filter = MonitoredSystemFilter;
                OnPropertyChanged();
            }
        }

        public ICollectionView OUView
        {
            get
            {
                if (ouView == null)
                {
                    ouView = CollectionViewSource.GetDefaultView(new ExtendedObservableCollection<object>());
                }
                return ouView;
            }
            set
            {
                ouView = value;
                //set filter;
                ouView.Filter = OUFilter;
                OnPropertyChanged();
            }
        }

        public ICollectionView MailUserView
        {
            get
            {
                if (mailUserView == null)
                {
                    mailUserView = CollectionViewSource.GetDefaultView(DataModel.Instance.MailUsers);
                    //set filter
                    mailUserView.Filter = MailUserFilter;
                }
                return mailUserView;
            }
        }

        public ICollectionView ClusterView
        {
            get
            {
                if (clusterView == null)
                {
                    clusterView = CollectionViewSource.GetDefaultView(DataModel.Instance.Clusters);
                    //set filter
                    clusterView.Filter = ClusterFilter;
                }
                return clusterView;
            }
        }

        public ICollectionView IgnoredMonitoredSystemView
        {
            get
            {
                if (ignoredMonitoredSystemView == null)
                {
                    ignoredMonitoredSystemView = CollectionViewSource.GetDefaultView(DataModel.Instance.IgnoredMonitoredSystems);
                    //set filter
                    ignoredMonitoredSystemView.Filter = IgnoredMonitoredSystemFilter;
                }
                return ignoredMonitoredSystemView;
            }
        }

        public ICollectionView ObservedMonitoredSystemView
        {
            get
            {
                if (observedMonitoredSystemsView == null)
                {
                    observedMonitoredSystemsView = CollectionViewSource.GetDefaultView(new ExtendedObservableCollection<object>());
                }
                return observedMonitoredSystemsView;
            }
            set
            {
                observedMonitoredSystemsView = value;
                observedMonitoredSystemsView.Filter = ObservedMonitoredSystemFilter;
                OnPropertyChanged();
            }
        }

        public ICollectionView UnObservedMonitoredSystemView
        {
            get
            {
                if (unObservedMonitoredSystemsView == null)
                {
                    unObservedMonitoredSystemsView = CollectionViewSource.GetDefaultView(new ExtendedObservableCollection<object>());
                }
                return unObservedMonitoredSystemsView;
            }
            set
            {
                unObservedMonitoredSystemsView = value;
                unObservedMonitoredSystemsView.Filter = UnObservedMonitoredSystemFilter;
                OnPropertyChanged();
            }
        }

        #endregion

        #region update views

        private void MainWindowViewModel_UpdateElementView(object sender, EventArgs e)
        {
            ElementsView = CollectionViewSource.GetDefaultView(DataModel.Elements.GetMonitoredSystems());
        }

        private void MainWindowViewModel_UpdateOUView(object sender, EventArgs e)
        {
            OUView = CollectionViewSource.GetDefaultView(DataModel.Elements.GetOrganizationalUnits());
        }

        #endregion

        #region filter strings for views
        public string MonitoredSystemFilterString
        {
            get
            {
                return monitoredSystemFilterString;
            }
            set
            {
                monitoredSystemFilterString = value;
                ElementsView.Refresh();
                OnPropertyChanged();
            }
        }

        public string OrganisationUnitFilterString
        {
            get
            {
                return organisationunitFilterString;
            }
            set
            {
                organisationunitFilterString = value;
                OUView.Refresh();
                OnPropertyChanged();
            }
        }

        public string MailUserFilterString
        {
            get
            {
                return mailUserFilterString;
            }
            set
            {
                mailUserFilterString = value;
                MailUserView.Refresh();
            }
        }

        public string ClusterFilterString
        {
            get
            {
                return clusterFilterString;
            }
            set
            {
                clusterFilterString = value;
                ClusterView.Refresh();
            }
        }

        public string IgnoredMonitoredSystemFilterString
        {
            get
            {
                return ignoredMonitoredSystemFilterString;
            }
            set
            {
                ignoredMonitoredSystemFilterString = value;
                IgnoredMonitoredSystemView.Refresh();
            }
        }

        public string ObservedMonitoredSystemFilterString
        {
            get
            {
                return observedMonitoredSystemFilterString;
            }
            set
            {
                observedMonitoredSystemFilterString = value;
                ObservedMonitoredSystemView.Refresh();
            }
        }

        public string UnObservedMonitoredSystemFilterString
        {
            get
            {
                return unObservedMonitoredSystemFilterString;
            }
            set
            {
                unObservedMonitoredSystemFilterString = value;
                UnObservedMonitoredSystemView.Refresh();
            }
        }

        #endregion

        #region filter
        private bool MonitoredSystemFilter(object item)
        {
            if (MonitoredSystemFilterString.Length == 0)
            {
                return true;
            }

            if (item is MonitoredSystem)
            {
                var ms = item as MonitoredSystem;

                // filter this element
                return (ms.Name.ToLower().Contains(MonitoredSystemFilterString.ToLower()));
            }
            else
            {
                return true;
            }
        }

        private bool OUFilter(object item)
        {
            if (OrganisationUnitFilterString.Length == 0)
            {
                return true;
            }

            if (item is OrganizationalUnit)
            {
                var ou = item as OrganizationalUnit;

                //filter element
                return (ou.Name.ToLower().Contains(OrganisationUnitFilterString.ToLower()));
            }
            else
            {
                return true;
            }

        }

        private bool MailUserFilter(object item)
        {
            if (MailUserFilterString.Length == 0)
            {
                return true;
            }

            if (item is MailUser)
            {
                var mailUser = item as MailUser;
                return (mailUser.Name.ToLower().Contains(MailUserFilterString.ToLower()));
            }
            else
            {
                return true;
            }
        }

        private bool ClusterFilter(object item)
        {
            if (ClusterFilterString.Length == 0)
            {
                return true;
            }

            if (item is Cluster)
            {
                var cluster = item as Cluster;
                return (cluster.HeadnodeAddress.ToLower().Contains(ClusterFilterString.ToLower()));
            }
            else
            {
                return true;
            }
        }

        private bool IgnoredMonitoredSystemFilter(object item)
        {
            if (IgnoredMonitoredSystemFilterString.Length == 0)
            {
                return true;
            }

            if (item is Tuple<string, string>)
            {
                var ms = item as Tuple<string, string>;
                return (ms.Item2.ToLower().Contains(IgnoredMonitoredSystemFilterString.ToLower()));
            }
            else
            {
                return true;
            }
        }

        public bool ObservedMonitoredSystemFilter(object item)
        {
            if (ObservedMonitoredSystemFilterString.Length == 0)
            {
                return true;
            }

            if (item is Tuple<int, string, string, string>)
            {
                var ms = item as Tuple<int, string, string, string>;
                return (ms.Item3.ToLower().Contains(ObservedMonitoredSystemFilterString.ToLower()));
            }
            else
            {
                return true;
            }
        }

        public bool UnObservedMonitoredSystemFilter(object item)
        {
            if (UnObservedMonitoredSystemFilterString.Length == 0)
            {
                return true;
            }

            if (item is Tuple<int, string, string, string>)
            {
                var ms = item as Tuple<int, string, string, string>;
                return (ms.Item3.ToLower().Contains(UnObservedMonitoredSystemFilterString.ToLower()));
            }
            else
            {
                return true;
            }
        }
        #endregion

    }
}

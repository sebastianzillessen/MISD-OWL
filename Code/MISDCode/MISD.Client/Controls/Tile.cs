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

using MISD.Client.Managers;
using MISD.Client.Model;
using MISD.Client.ViewModel;
using MISD.Client.ViewModel.Converters;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace MISD.Client.Controls
{
    /// <summary>
    /// Implements the behaviour of a tile in the user interface.
    /// </summary>
    [TemplatePart(Name = "PART_BackButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_MinusButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_PlusButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_IndicatorButton", Type = typeof(ListBox))]
    public class Tile : Control
    {
        #region Constructors

        static Tile()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Tile), new FrameworkPropertyMetadata(typeof(Tile)));
        }

        public Tile()
        {
            // Starting with smallest level.
            this.CurrentLevel = (from p in DataModel.Instance.LevelDefinitions
                                 orderby p.Level, p.Rows, p.ID ascending
                                 select p).First();

            //Task.Factory.StartNew(new Action(() =>
            //{
            //    LayoutManager.Instance.MonitoredSystemStateChanged += Instance_MonitoredSystemStateChanged;
            //    LayoutManager.Instance.PluginPriority.CollectionChanged += PluginPriority_CollectionChanged;
            //}));

            LayoutManager.Instance.MonitoredSystemStateChanged += Instance_MonitoredSystemStateChanged;
            LayoutManager.Instance.PluginPriority.CollectionChanged += PluginPriority_CollectionChanged;

            // DRAG for drag-and-drop
            this.PreviewMouseLeftButtonDown += this.PreviewMouseLeftButtonDownHandler;
            this.MouseMove += this.MouseMoveHandler;

            // Update MSState
            Task.Factory.StartNew(new Action(() =>
            {
                LayoutManager.Instance.CurrentLayout.SetState(this.MonitoredSystemID, LayoutManager.Instance.CurrentLayout.GetMSState(this.MonitoredSystemID));
            }));
            // Update MSState
            Task.Factory.StartNew(new Action(() =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    MainWindow.Current.MainTreeView.GetContainerFromItem(this.MonitoredSystem).IsSelected = MainWindow.Current.MainTreeView.SelectedItems.Contains(this.MonitoredSystem);
                });

            }));

        }

        #endregion

        #region Fields

        // Fields to enable drag-and-drop
        private Point startPosition;
        private bool startIsOU = false;
        private int startID = -1;
        private int startOuID = -1;
        private string startName = "";

        #endregion

        #region Properties

        // TODO: ViewModel
        public DataModel DataModel
        {
            get
            {
                return DataModel.Instance;
            }
        }

        public Button BackButton { get; set; }
        public Button PlusButton { get; set; }
        public Button MinusButton { get; set; }
        public ListBox PluginList { get; set; }
        public ListBox CustomUiList { get; set; }

        public LevelDefinition CurrentLevel
        {
            get
            {
                return (LevelDefinition)this.GetValue(Tile.CurrentLevelProperty);
            }
            set
            {
                // Inform Layout Manager about changes.
                if (MonitoredSystem != null && CurrentLevel != null)
                    LayoutManager.Instance.SetMSState(MonitoredSystem.ID, value.LevelID);
                this.SetValue(Tile.CurrentLevelProperty, value);
            }
        }

        public MainWindowViewModel ViewModel
        {
            get { return MainWindowViewModel.Instance; }
        }

        /// <summary>
        /// Gets or sets the current monitored system.
        /// </summary>
        public MonitoredSystem MonitoredSystem
        {
            get
            {
                try
                {
                    return this.Dispatcher.Invoke(() => { return (MonitoredSystem)this.GetValue(Tile.MonitoredSystemProperty); });
                }
                catch (Exception)
                {
                    return null;
                }
            }
            set
            {
                this.SetValue(Tile.MonitoredSystemProperty, value);
                // Inform Layout Manager about changes.
                if (MonitoredSystem != null && CurrentLevel != null)
                    LayoutManager.Instance.SetMSState(MonitoredSystem.ID, CurrentLevel.LevelID);
            }
        }

        private int MonitoredSystemID
        {
            get
            {
                return this.Dispatcher.Invoke(() => { return ((MonitoredSystem)this.GetValue(Tile.MonitoredSystemProperty)).ID; });
            }
        }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty MonitoredSystemProperty =
            DependencyProperty.Register("MonitoredSystem", typeof(MonitoredSystem), typeof(Tile),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(OnMonitoredSystemChanged)));

        public static readonly DependencyProperty CurrentLevelProperty =
            DependencyProperty.Register("CurrentLevel", typeof(LevelDefinition), typeof(Tile),
               new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None,
                     new PropertyChangedCallback(OnCurrentLevelChanged)));

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var LayoutRoot = GetTemplateChild("LayoutRoot") as Grid;
            LayoutRoot.DataContext = this;

            this.BackButton = GetTemplateChild("PART_BackButton") as Button;
            if (this.BackButton != null)
            {
                this.BackButton.Click += BackButton_Click;
            }

            this.PlusButton = GetTemplateChild("PART_PlusButton") as Button;
            if (this.PlusButton != null)
            {
                this.PlusButton.Click += PlusButton_Click;
            }

            this.MinusButton = GetTemplateChild("PART_MinusButton") as Button;
            if (this.MinusButton != null)
            {
                this.MinusButton.Click += MinusButton_Click;
            }

            var converter = new RowToMaxHeightConverter();

            this.PluginList = GetTemplateChild("PART_PluginList") as ListBox;
            if (this.PluginList != null)
            {
                addPluginSorting();
                this.PluginList.SelectionChanged += PluginList_SelectionChanged;

                MultiBinding multiBinding = new MultiBinding();
                multiBinding.Converter = converter;

                multiBinding.Bindings.Add(new Binding("CurrentLevel.Rows") { Source = this });
                multiBinding.Bindings.Add(new Binding("TileRowHeight") { Source = MainWindowViewModel.Instance });

                this.PluginList.SetBinding(MaxHeightProperty, multiBinding);
            }

            var indicatorsList = GetTemplateChild("PART_IndicatorsList") as ListBox;
            if (indicatorsList != null)
            {
                MultiBinding multiBinding = new MultiBinding();
                multiBinding.Converter = converter;

                multiBinding.Bindings.Add(new Binding("CurrentLevel.Rows") { Source = this });
                multiBinding.Bindings.Add(new Binding("TileRowHeight") { Source = MainWindowViewModel.Instance });

                indicatorsList.SetBinding(MaxHeightProperty, multiBinding);
            }

            this.CustomUiList = GetTemplateChild("PART_CustomUIList") as ListBox;
            if (this.CustomUiList != null)
            {
                MultiBinding multiBinding = new MultiBinding();
                multiBinding.Converter = converter;

                multiBinding.Bindings.Add(new Binding("CurrentLevel.Rows") { Source = this });
                multiBinding.Bindings.Add(new Binding("TileRowHeight") { Source = MainWindowViewModel.Instance });

                this.CustomUiList.SetBinding(MaxHeightProperty, multiBinding);
            }

            var header = GetTemplateChild("PART_Header") as Grid;
            if (header != null)
            {
                Binding binding = new Binding("TileSpecialRowHeight");
                binding.Source = MainWindowViewModel.Instance;
                binding.Mode = BindingMode.OneWay;
                header.SetBinding(HeightProperty, binding);
            }

            var statusBar = GetTemplateChild("PART_StatusBar") as Grid;
            if (statusBar != null)
            {
                Binding binding = new Binding("TileSpecialRowHeight");
                binding.Source = MainWindowViewModel.Instance;
                binding.Mode = BindingMode.OneWay;
                statusBar.SetBinding(HeightProperty, binding);
            }

            setWidthBinding();
            setMinWidthBinding();
        }

        public void setMinWidthBinding()
        {
            this.ClearValue(MinWidthProperty);
            Binding binding = new Binding("TileWidth");
            binding.Source = MainWindowViewModel.Instance;
            binding.Mode = BindingMode.OneWay;
            this.SetBinding(MinWidthProperty, binding);
        }

        public void setWidthBinding()
        {
            this.ClearValue(WidthProperty);
            Binding binding = new Binding("TileWidth");
            binding.Source = MainWindowViewModel.Instance;
            binding.Mode = BindingMode.OneWay;
            this.SetBinding(WidthProperty, binding);
        }

        void addPluginSorting()
        {
            if (this.PluginList != null)
            {
                while (this.PluginList.Items.SortDescriptions.Count > 0) { this.PluginList.Items.SortDescriptions.RemoveAt(0); }
                this.PluginList.Items.SortDescriptions.Add(new SortDescription("SortingProperty",
                                  ListSortDirection.Ascending));
            }
        }

        void PluginPriority_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            addPluginSorting();
        }

        private void PluginList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // The selection in the plugin list changed, so go to visual state "PluginDetails"
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                VisualStateManager.GoToState(this, "PluginDetails", true);

                // Inform Layout Manager about changes.
                var plugin = this.PluginList.SelectedItem as Plugin;
                if (plugin != null && MonitoredSystem != null && CurrentLevel != null)
                    LayoutManager.Instance.SetMSState(this.MonitoredSystem.ID, CurrentLevel.LevelID, plugin.Name);
            }
        }

        public void MinusButton_Click(object sender, RoutedEventArgs e)
        {
            // Reduce the level until the smallest level is reached
            var tempList = (from p in DataModel.Instance.LevelDefinitions
                            where p.Level <= CurrentLevel.Level
                            orderby p.Level, p.Rows, p.ID ascending
                            select p).ToList();

            for (int i = 0; i < tempList.Count; i++)
            {
                if (tempList.ElementAt(i).ID == CurrentLevel.ID && i > 0)
                {
                    this.CurrentLevel = tempList.ElementAt(i - 1);
                    break;
                }
            }
        }

        public void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            // Increment the level until the maximum level is reached
            var tempList = (from p in DataModel.Instance.LevelDefinitions
                            where p.Level >= CurrentLevel.Level
                            orderby p.Level, p.Rows, p.ID ascending
                            select p).ToList();

            for (int i = 0; i < tempList.Count; i++)
            {
                if (tempList.ElementAt(i).ID == CurrentLevel.ID && i < tempList.Count - 1)
                {
                    this.CurrentLevel = tempList.ElementAt(i + 1);
                    break;
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var plugin = PluginList.SelectedItem as Plugin;
            if (plugin != null && MonitoredSystem != null && CurrentLevel != null)
                LayoutManager.Instance.SetMSState(this.MonitoredSystem.ID, CurrentLevel.LevelID, plugin.Name, false);

            // Clear the selection...
            this.PluginList.SelectedIndex = -1;

            // ...and return to normal state
            VisualStateManager.GoToState(this, "Default", true);
        }

        /// <summary>
        /// Saves the start-values for drag-and-drop.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviewMouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // get the position and values of the tile to prepare dragging
                this.startPosition = e.GetPosition(this);
                this.startID = this.MonitoredSystem.ID;
                this.startOuID = this.MonitoredSystem.OuID;
                this.startName = this.MonitoredSystem.Name;
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION: Tile_PreviewMouseLeftButtonDownHandler: " + ex.ToString());
            }
        }

        double TileWidth
        {
            get
            {
                return MainWindowViewModel.Instance.TileWidth;
            }
        }
        double TileMargin = 8;

        protected override Size MeasureOverride(Size constraint)
        {
            var rootGrid = VisualTreeHelper.GetChild(this, 0) as Grid;

            // Recalculate DesiredSize
            rootGrid.Measure(constraint);

            var width = rootGrid.DesiredSize.Width;

            var overflow = width % TileWidth;
            bool doIt = overflow > 1;
            var factor = Math.Round((width - overflow) / TileWidth) + (doIt ? 1 : 0);

            factor = Math.Max(1, factor);

            var desiredSize = new Size(factor * TileWidth + (factor - 1) * 2 * TileMargin, rootGrid.DesiredSize.Height);

            rootGrid.Measure(desiredSize);

            return desiredSize;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            return base.ArrangeOverride(this.DesiredSize);
        }

        /// <summary>
        /// Proofes and initiates a drag-and-drop activity.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            try
            {
                // Get the current mouse position
                Point mousePosition = e.GetPosition(null);
                Vector diff = startPosition - mousePosition;

                if (e.LeftButton == MouseButtonState.Pressed &&
                   (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    // initiate the drag-and-drop if the OU and the monitored system are valid
                    if (startOuID != -1 && startID != -1)
                    {
                        DataObject dataObject = new DataObject(new Tuple<bool, int, int, string>(startIsOU, startID, startOuID, startName));
                        DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Move);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION: Tile_MouseMoveHandler: " + ex.ToString());
            }

        }

        private static void OnMonitoredSystemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tile = d as Tile;
            tile.MonitoredSystem.PropertyChanged += tile.MonitoredSystem_PropertyChanged;
        }

        void MonitoredSystem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CustomUIValuesLoaded")
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (this.CurrentLevel.UseCustomUI == true)
                    {
                        VisualStateManager.GoToState(this, "CustomUI", true);
                    }
                }));
            }
        }

        private void LoadIndicatorValues()
        {
            // Get all indicator values for visualization
            Task.WaitAll((from p in this.MonitoredSystem.Plugins
                          from q in p.Indicators
                          let task = new Tuple<MonitoredSystem, Plugin, Indicator, DateTime, int>(this.MonitoredSystem, p, q, DateTime.Now - TimeSpan.FromDays(365), 500)
                          select Task.Factory.StartNew(() => { DataModel.Instance.UpdateIndicatorValues(task); })).ToArray());

            // Values should be loaded now
            this.Dispatcher.Invoke(() => { this.MonitoredSystem.CustomUIValuesLoaded = true; });
        }

        private static void OnCurrentLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newLevel = e.NewValue as LevelDefinition;
            if (newLevel != null)
            {
                var tile = d as Tile;
                if (newLevel.UseCustomUI)
                {
                    tile.ClearValue(WidthProperty);

                    if (!tile.MonitoredSystem.CustomUIValuesLoaded)
                    {
                        VisualStateManager.GoToState(d as FrameworkElement, "LoadingCustomUI", true);

                        if (!DataModel.Instance.IsPowerwall)
                        {
                            Task.Factory.StartNew(() => { tile.LoadIndicatorValues(); });
                        }
                    }
                    else
                    {
                        VisualStateManager.GoToState(d as FrameworkElement, "CustomUI", true);
                    }
                }
                else
                {
                    var oldLevel = e.OldValue as LevelDefinition;
                    if (oldLevel != null)
                    {
                        if (oldLevel.UseCustomUI)
                        {
                            VisualStateManager.GoToState(d as FrameworkElement, "Default", true);
                        }
                    }
                    tile.setWidthBinding();
                }
            }
        }

        #region powerwallsupport
        // TODO Level Setzen
        private void Instance_MonitoredSystemStateChanged(int ID, int level, string[] flippedPlugins)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                //TODO hier fliegt ne excepton levels leer
                if (this.MonitoredSystem != null && this.MonitoredSystem.ID == ID)
                {
                    LevelDefinition newLevel;
                    try
                    {
                        newLevel = (from p in DataModel.Instance.LevelDefinitions
                                    where p.LevelID == level
                                    select p).First();
                    }
                    catch
                    {
                        newLevel = (from p in DataModel.Instance.LevelDefinitions
                                    orderby p.Level
                                    select p).First();
                    }

                    this.SetValue(Tile.CurrentLevelProperty, newLevel);
                    if (newLevel.UseCustomUI == true)
                    {
                        foreach (var item in PluginList.Items)
                        {
                            var p = item as Plugin;
                            if (p != null)
                            {
                                if (p.CustomUI != null)
                                {
                                    if (flippedPlugins.Contains(p.Name))
                                        p.CustomUI.Expand();
                                    else
                                        p.CustomUI.Collapse();
                                }
                            }
                        }
                    }
                    else
                    {
                        if (flippedPlugins.Length == 0)
                        {
                            this.PluginList.SelectedIndex = -1;
                            // TODO bei level ausführen
                            VisualStateManager.GoToState(this, "Default", true);
                        }
                        else
                        {
                            foreach (var p in PluginList.Items)
                            {
                                if (p as Plugin != null && flippedPlugins.Contains((p as Plugin).Name))
                                {
                                    this.PluginList.SelectedItem = p;
                                }
                            }
                        }
                    }
                }
            }));

        }
        #endregion
    }
}
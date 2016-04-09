using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MISD.Core;
using System.Windows.Media;
using System.Windows.Data;
using MISD.Client.Managers;
using System.Windows.Controls.Primitives;

namespace MISD.Client.Model
{
    public class TileCustomUI : Control, ITileCustomUI
    {
        #region Constructors

        static TileCustomUI()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TileCustomUI), new FrameworkPropertyMetadata(typeof(TileCustomUI)));
        }

        public TileCustomUI()
        {
            this.IndicatorValues = new ExtendedObservableCollection<IndicatorValue>();
            this.Indicators = new ExtendedObservableCollection<Indicator>();
        }

        #endregion

        #region Properties

        public ExtendedObservableCollection<IndicatorValue> IndicatorValues
        {
            get
            {
                return (ExtendedObservableCollection<IndicatorValue>)this.GetValue(TileCustomUI.IndicatorValuesProperty);
            }
            set
            {
                this.SetValue(TileCustomUI.IndicatorValuesProperty, value);
            }
        }

        public ExtendedObservableCollection<Indicator> Indicators
        {
            get
            {
                return (ExtendedObservableCollection<Indicator>)this.GetValue(TileCustomUI.IndicatorsProperty);
            }
            set
            {
                this.SetValue(TileCustomUI.IndicatorsProperty, value);
            }
        }

        public Brush Image
        {
            get
            {
                return (Brush)this.GetValue(ImageProperty);
            }
            set
            {
                this.SetValue(ImageProperty, value);
            }
        }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty IndicatorsProperty =
            DependencyProperty.Register("Indicators", typeof(ExtendedObservableCollection<Indicator>), typeof(TileCustomUI),
            new PropertyMetadata(OnIndicatorsChanged));

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(Brush), typeof(TileCustomUI));

        public static readonly DependencyProperty IndicatorValuesProperty =
            DependencyProperty.Register("IndicatorValues", typeof(ExtendedObservableCollection<IndicatorValue>), typeof(TileCustomUI));
        private ToggleButton expander;

        #endregion

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var layoutRoot = this.GetTemplateChild("PART_LayoutRoot") as Grid;
            if (layoutRoot != null) layoutRoot.DataContext = this;

            expander = searchExpanderAndRegisterEvent();
            var parent = VisualTreeHelper.GetParent(this) as ContentPresenter;

            Binding binding = new Binding("Tag");
            binding.Source = parent;
            binding.Mode = BindingMode.OneWay;
            this.SetBinding(IndicatorsProperty, binding);
        }

        /// <summary>
        /// searches the next highest expander and registers state changed events on it
        /// </summary>
        private ToggleButton searchExpanderAndRegisterEvent()
        {
            try
            {
                DependencyObject element = this;
                ToggleButton expander = null;
                for (int i = 0; i < 10; i++)
                {
                    element = VisualTreeHelper.GetParent(element);
                    if (element == null)
                        return null;
                    if ((element as Grid) != null)
                    {
                        var grid = element as Grid;
                        foreach (var x in grid.Children)
                        {
                            if ((x as ToggleButton) != null)
                            {
                                expander = x as ToggleButton;
                                break;
                            }
                        }
                        if (expander != null)
                            break;
                    }
                    if ((element as ToggleButton) != null)
                    {
                        expander = element as ToggleButton;
                        break;
                    }
                }
                if (expander != null)
                {
                    expander.Unchecked += expander_Collapsed;
                    expander.Checked += expander_Expanded;
                    expander_Expanded(expander, null);
                    return expander;
                }
            }
            catch (Exception e)
            {
                ClientLogger.Instance.WriteEntry("Problem in SearchExpander and register: " + e, LogType.Exception);
            }
            return null;

        }


        void expander_Expanded(object sender, RoutedEventArgs e)
        {
            ToggleButton ex = sender as ToggleButton;
            expander = ex;
            if (ex != null)
            {
                MonitoredSystem ms = (MonitoredSystem)ex.Tag;
                MonitoredSystemState state = LayoutManager.Instance.GetMSState(ms.ID);
                var ms_id = ms.ID;
                var pluginName = (ex.FindName("PluginName") as TextBlock).Text;
                int level = state.Level;
                if (ms_id >= 0 && level >= 0)
                {
                    if (!state.ShownPlugins.Contains(pluginName))
                        LayoutManager.Instance.SetMSState(ms_id, level, pluginName, true, true);
                }
            }
        }

        void expander_Collapsed(object sender, RoutedEventArgs e)
        {
            ToggleButton ex = sender as ToggleButton;
            expander = ex;
            if (ex != null)
            {
                MonitoredSystem ms = (MonitoredSystem)ex.Tag;
                MonitoredSystemState state = LayoutManager.Instance.GetMSState(ms.ID);
                var ms_id = ms.ID;
                var pluginName = (ex.FindName("PluginName") as TextBlock).Text;
                int level = state.Level;
                if (ms_id >= 0 && level >= 0)
                {
                    if (state.ShownPlugins.Contains(pluginName))
                        LayoutManager.Instance.SetMSState(ms_id, level, pluginName, false, true);
                }
            }
        }


        private static void OnIndicatorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                (e.OldValue as ExtendedObservableCollection<Indicator>).CollectionChanged -= (d as TileCustomUI).Indicators_Changed;
            }
            if (e.NewValue != null)
            {
                (e.NewValue as ExtendedObservableCollection<Indicator>).CollectionChanged += (d as TileCustomUI).Indicators_Changed;
                try
                {
                    (d as TileCustomUI).SelectIndicatorValues();
                }
                catch (Exception x)
                {
                    ClientLogger.Instance.WriteEntry("Error in TileCustomUi : On indicatorsChanged" + e, LogType.Exception);
                }
            }
        }

        private void Indicators_Changed(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.SelectIndicatorValues();
        }

        public virtual void SelectIndicatorValues()
        {
            this.IndicatorValues.Clear();
            foreach (var indicator in Indicators)
            {
                this.IndicatorValues.BeginAddRange(indicator.IndicatorValues);
            }
        }

        #endregion


        public void Expand()
        {
            StateExpander(true);
        }

        public void Collapse()
        {
            StateExpander(false);
        }

        private void StateExpander(bool open)
        {
            if (expander == null)
            {
                OnApplyTemplate();
                expander = searchExpanderAndRegisterEvent();
            }
            if (expander != null)
            {
                expander.IsChecked = open;
            }
            else
            {
                ClientLogger.Instance.WriteEntry("WtateExpanderFor TileCustomUI, Could not find the expander", LogType.Warning);
            }
        }
    }
}
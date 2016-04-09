using MISD.Client.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;


namespace MISD.Client.Controls
{
    public class ExtendedTreeView : TreeView
    {
        public ExtendedTreeView()
        {
            this.DefaultStyleKey = typeof(ExtendedTreeView);
            this.SelectedItems = new ExtendedObservableCollection<object>();
            this.WhiteList = new ExtendedObservableCollection<object>();

            (this.SelectedItems as ExtendedObservableCollection<object>).CollectionChanged += ExtendedTreeView_CollectionChanged;
            this.GetItemContainerGenerators().ForEach(p => p.StatusChanged += ItemContainerGenerator_StatusChanged);
        }

        #region Whitelist / Blacklist

        public ExtendedObservableCollection<object> WhiteList
        {
            get
            {
                if (this.GetValue(WhiteListProperty) == null)
                {
                    WhiteList = new ExtendedObservableCollection<object>();
                }

                this.RaiseEvent(new RoutedPropertyChangedEventArgs<object>(null, null, WhiteListChangedEvent));

                return (ExtendedObservableCollection<object>)this.GetValue(WhiteListProperty);
            }
            set
            {
                this.SetValue(ExtendedTreeView.WhiteListProperty, value);
            }
        }

        public static readonly DependencyProperty WhiteListProperty =
            DependencyProperty.Register("WhiteList", typeof(ExtendedObservableCollection<object>), typeof(ExtendedTreeView),
                new PropertyMetadata(new PropertyChangedCallback(OnWhiteListChanged)));

        private static void OnWhiteListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                (d as ExtendedTreeView).WhiteList.CollectionChanged -= (d as ExtendedTreeView).ExtendedTreeView_WhiteList_CollectionChanged;
            }
            if (e.NewValue != null)
            {
                (d as ExtendedTreeView).WhiteList.CollectionChanged += (d as ExtendedTreeView).ExtendedTreeView_WhiteList_CollectionChanged;
            }

            //raise event
            (d as ExtendedTreeView).RaiseEvent(new RoutedPropertyChangedEventArgs<object>(e.OldValue, e.NewValue, WhiteListChangedEvent));
        }


        private void ExtendedTreeView_WhiteList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.RaiseEvent(new RoutedPropertyChangedEventArgs<object>(null, null, WhiteListChangedEvent));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the selection mode of this tree view.
        /// </summary>
        public SelectionMode SelectionMode
        {
            get
            {
                return (SelectionMode)this.GetValue(ExtendedTreeView.SelectionModeProperty);
            }
            set
            {
                this.SetValue(ExtendedTreeView.SelectionModeProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the currently selected item of this tree view.
        /// </summary>
        public new object SelectedItem
        {
            get
            {
                return this.GetValue(ExtendedTreeView.SelectedItemProperty);
            }
            set
            {
                bool raise = false;
                object oldValue = this.SelectedItem;
                if (value != this.SelectedItem)
                {
                    raise = true;
                }
                this.SetValue(ExtendedTreeView.SelectedItemProperty, value);
                if (raise) this.RaiseEvent(new RoutedPropertyChangedEventArgs<object>(oldValue, this.SelectedItem, SelectedItemChangedEvent));
            }
        }

        /// <summary>
        /// Gets or sets the currently selected items of this tree view.
        /// </summary>
        public IList<object> SelectedItems
        {
            get
            {
                var selectedItems = this.GetValue(ExtendedTreeView.SelectedItemsProperty);
                if (selectedItems == null)
                {
                    this.SelectedItems = new ExtendedObservableCollection<object>();
                }
                return (IList<object>)this.GetValue(ExtendedTreeView.SelectedItemsProperty);
            }
            set
            {
                this.SetValue(ExtendedTreeView.SelectedItemsProperty, value);
            }
        }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty SelectionModeProperty =
            DependencyProperty.Register("SelectionMode", typeof(SelectionMode), typeof(ExtendedTreeView));

        public static readonly new DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(ExtendedTreeView));

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(IList<object>), typeof(ExtendedTreeView),
            new PropertyMetadata(new PropertyChangedCallback(OnSelectedItemsChanged)));

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                (e.OldValue as ExtendedObservableCollection<object>).CollectionChanged -= (d as ExtendedTreeView).ExtendedTreeView_CollectionChanged;
            }
            if (e.NewValue != null)
            {
                (e.NewValue as ExtendedObservableCollection<object>).CollectionChanged += (d as ExtendedTreeView).ExtendedTreeView_CollectionChanged;
            }
        }


        #endregion

        #region Events

        public new event RoutedPropertyChangedEventHandler<object> SelectedItemChanged
        {
            add
            {
                AddHandler(SelectedItemChangedEvent, value);
            }
            remove
            {
                RemoveHandler(SelectedItemChangedEvent, value);
            }
        }

        public event SelectionChangedEventHandler SelectionChanged
        {
            add
            {
                AddHandler(SelectionChangedEvent, value);
            }
            remove
            {
                RemoveHandler(SelectionChangedEvent, value);
            }
        }

        public event RoutedEventHandler WhiteListChanged
        {
            add
            {
                AddHandler(WhiteListChangedEvent, value);
            }
            remove
            {
                RemoveHandler(WhiteListChangedEvent, value);
            }
        }

        #endregion

        #region Routed Events

        public static readonly new RoutedEvent SelectedItemChangedEvent =
            EventManager.RegisterRoutedEvent("SelectedItemChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<object>), typeof(ExtendedTreeView));

        public static readonly RoutedEvent SelectionChangedEvent =
            EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(ExtendedTreeView));

        public static readonly RoutedEvent WhiteListChangedEvent =
            EventManager.RegisterRoutedEvent("WhiteListChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<object>), typeof(ExtendedTreeView));
        #endregion

        #region Selection Logic

        public Boolean Expand(OrganizationalUnit ou)
        {
            var firstType = typeof(OrganizationalUnit);
            var itemToExpand = (from p in this.GetItemContainers()
                                where p.DataContext == ou
                                select p).ToList();
            if (itemToExpand.Count > 0)
            {
                itemToExpand.ForEach(p => p.IsExpanded = true);
                return true;
            }
            else
            {
                return false;
            }
        }
        public Boolean Collapse(OrganizationalUnit ou)
        {
            var firstType = typeof(OrganizationalUnit);
            var itemToExpand = (from p in this.GetItemContainers()
                                where p.DataContext == ou
                                select p).ToList();
            if (itemToExpand.Count > 0)
            {
                itemToExpand.ForEach(p => p.IsExpanded = false);
                return true;
            }
            else { return false; }
        }

        // Markiert alle sichtbaren Monitored Systems. Falls keine sichtbar sind, werden alle ous markiert.
        public void SelectAll()
        {
            this.UnselectAll();
            IList<object> list = (IList<object>)new ExtendedObservableCollection<object>();
            foreach (var e in MISD.Client.Model.DataModel.Instance.Elements.GetMonitoredSystems())
            {
                list.Add(e);
            }
            this.SelectedItems = list;
            this.ExtendedTreeView_CollectionChanged(this, new System.Collections.Specialized.NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

        }

        public void UnselectAll()
        {
            this.SelectedItems = (IList<object>)new ExtendedObservableCollection<object>();
            this.ExtendedTreeView_CollectionChanged(this, new System.Collections.Specialized.NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void UnselectAllExceptOf(ExtendedTreeViewItem treeViewItem)
        {
            var containers = this.GetItemContainers();
            containers.Remove(treeViewItem);
            containers.ForEach(p => p.IsSelected = false);
            // Notwendig für Drag and Drop, da Element nicht existiert, während es wechselt
            this.SelectedItems = new ExtendedObservableCollection<object>();
            var containersToUpdate2 = (from p in this.GetItemContainers()
                                       where p == treeViewItem
                                       select p).ToList();
            containersToUpdate2.ForEach(p => p.Focus()); 
        }


        protected override System.Windows.DependencyObject GetContainerForItemOverride()
        {
            return new ExtendedTreeViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is ExtendedTreeView;
        }

        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            this.GetItemContainerGenerators().ForEach(p => p.StatusChanged -= ItemContainerGenerator_StatusChanged);
            this.GetItemContainerGenerators().ForEach(p => p.StatusChanged += ItemContainerGenerator_StatusChanged);

            this.GetItemContainers().ForEach(p =>
            {
                if (p != null)
                {
                    p.TreeViewBacklink = this;
                }
            });
        }

        protected void ExtendedTreeView_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // First expands ous
            foreach (var tile in this.SelectedItems)
            {
                if ((tile.GetType() == typeof(MonitoredSystem)))
                {
                    if ((((MonitoredSystem)tile)).OuID != null)
                    {
                        if (MainWindow.Current.MainTreeView.Expand(DataModel.Instance.GetOu(((MonitoredSystem)tile).OuID)) == false)
                        {
                            HierachicalExpand((TileableElement)tile);
                        }

                    }
                }
                if ((tile.GetType() == typeof(OrganizationalUnit)))
                {
                    if ((((OrganizationalUnit)tile).ParentID) != null)
                    {
                        if (MainWindow.Current.MainTreeView.Expand(DataModel.Instance.GetOu((int)((OrganizationalUnit)tile).ParentID)) == false)
                        {
                            HierachicalExpand((TileableElement)tile);
                        }

                    }
                }
            }


            // Now set the selected item
            var selectedItems = this.SelectedItems as ExtendedObservableCollection<object>;
            if (selectedItems.Count > 0)
            {
                this.SelectedItem = selectedItems[0];

                foreach (var obj in selectedItems)
                {
                    var container = this.GetContainerFromItem(obj);
                    if (container != null)
                    {
                        container.IsSelected = true;
                    }
                }

                // Dont use this Code or Drag and Drop wont work


                // //Secure that Selection only contains one type of item
                //if (this.SelectedItem.GetType() == typeof(OrganizationalUnit))
                //{
                //    var containersToDeselect = (from p in this.GetItemContainers()
                //                                where p.DataContext.GetType() == typeof(MonitoredSystem)
                //                                select p).ToList();
                //    containersToDeselect.ForEach(p => p.IsSelected = false);

                //}
                //if (this.SelectedItem.GetType() == typeof(MonitoredSystem))
                //{
                //    var containersToDeselect = (from p in this.GetItemContainers()
                //                                where p.DataContext.GetType() == typeof(OrganizationalUnit)
                //                                select p).ToList();
                //    containersToDeselect.ForEach(p => p.IsSelected = false);
                //}
            }
            else
            {
                this.SelectedItem = null;
            }


            // Find all treeview items that are selected but not in SelectedItems
            var containersToUpdate = (from p in this.GetItemContainers()
                                      where p.IsSelected == true
                                      where !this.SelectedItems.Contains(p.DataContext)
                                      select p).ToList();

            containersToUpdate.ForEach(p => p.IsSelected = false);

            // Find all treeview items that are  in SelectedItems but not selected
            var containersToUpdate2 = (from p in this.GetItemContainers()
                                       where p.IsSelected == false
                                       where this.SelectedItems.Contains(p.DataContext)
                                       select p).ToList();

            containersToUpdate2.ForEach(p => p.IsSelected = true);

            this.RaiseEvent(new SelectionChangedEventArgs(SelectionChangedEvent, new List<object>() as IList, new List<object>() as IList));

            MainWindow.Current.SelectionChanged();

        }

        private void HierachicalExpand(TileableElement tileableElement)
        {
            Task.Factory.StartNew(new Action(() =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    List<int> ous = new List<int>();
                    foreach (var o in DataModel.Instance.Elements.GetOrganizationalUnits())
                    {
                        if (o.GetMonitoredSystems().Contains(tileableElement))
                        {
                            ous.Add(o.ID);
                        }
                    }
                    foreach (var u in ous)
                    {
                        MISD.Client.Managers.LayoutManager.Instance.CurrentLayout.SetState(u, true);
                    }
                });

            }));

        }

        #endregion

    }

    /// <summary>
    /// Provides some convenient extension methods for the extended tree view.
    /// </summary>
    public static class TreeViewExtensions
    {
        public static void ForEachTreeViewItem(this ExtendedTreeView treeView, Action<ExtendedTreeViewItem> action)
        {
            treeView.GetItemContainers().ForEach(action);
        }

        public static void ForEachItemContainerGenerator(this ExtendedTreeView treeView, Action<ItemContainerGenerator> action)
        {
            treeView.GetItemContainerGenerators().ForEach(action);
        }

        public static ExtendedTreeViewItem GetContainerFromItem(this ExtendedTreeView treeView, object item)
        {
            return (from p in treeView.GetItemContainers()
                    where p.DataContext == item
                    select p).FirstOrDefault();
        }

        public static object GetItemFromContainer(this ExtendedTreeView treeView, ExtendedTreeViewItem item)
        {
            return item.DataContext;
        }

        public static List<ItemContainerGenerator> GetItemContainerGenerators(this ExtendedTreeView treeView)
        {
            var result = new List<ItemContainerGenerator>();
            result.AddRange(treeView.ItemContainerGenerator.GetItemContainerGenerators());
            return result;
        }

        public static List<ItemContainerGenerator> GetItemContainerGenerators(this ItemContainerGenerator itemContainerGenerator)
        {
            var result = new List<ItemContainerGenerator>();
            result.Add(itemContainerGenerator);

            if (itemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                var containers = (from p in itemContainerGenerator.Items
                                  select itemContainerGenerator.ContainerFromItem(p) as ExtendedTreeViewItem).ToList();

                foreach (var container in containers)
                {
                    if (container != null)
                    {
                        result.AddRange(container.ItemContainerGenerator.GetItemContainerGenerators());
                    }
                }
            }

            return result;
        }

        public static List<ExtendedTreeViewItem> GetItemContainers(this ExtendedTreeView treeView)
        {
            return (from p in treeView.GetItemContainerGenerators()
                    where p.Status == GeneratorStatus.ContainersGenerated
                    from q in p.Items
                    select p.ContainerFromItem(q) as ExtendedTreeViewItem).ToList();
        }
    }
}
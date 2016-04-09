using MISD.Client.Managers;
using MISD.Client.Model;
using MISD.Client.ViewModel;
using System;
using System.Collections.Generic;
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
    public class ExtendedTreeViewItem : TreeViewItem
    {
        private ExtendedTreeView treeViewBacklink;

        public ExtendedTreeView TreeViewBacklink
        {
            set
            {
                treeViewBacklink = value;
            }

            get
            {
                return treeViewBacklink;
            }
        }


        public ExtendedTreeViewItem()
        {
            base.Selected += ExtendedTreeViewItem_Selected;
            base.Unselected += ExtendedTreeViewItem_Unselected;


            // Update OUExpansion

            Task.Factory.StartNew(new Action(() =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (this.DataContext.GetType() == typeof(OrganizationalUnit) && MISD.Client.Managers.LayoutManager.Instance.CurrentLayout.GetOUState(((OrganizationalUnit)this.DataContext).ID)==true)
                    {
                        this.IsExpanded = true;
                    }
                    if (this.DataContext.GetType() == typeof(OrganizationalUnit) && MISD.Client.Managers.LayoutManager.Instance.CurrentLayout.GetOUState(((OrganizationalUnit)this.DataContext).ID) == false)
                    {
                        this.IsExpanded = false;
                    }
                });

            }));

        }

        #region Properties

       

        /// <summary>
        /// Gets or sets a value that indicates whether this tree view item is selected.
        /// </summary>
        public new bool IsSelected
        {
            get
            {
                return (bool)this.GetValue(IsSelectedProperty);
            }
            set
            {
                this.SetValue(IsSelectedProperty, value);
                if (value) this.Select(); else this.Unselect();
            }
        }

        #endregion

        #region Dependency Properties

        public static readonly new DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(ExtendedTreeViewItem));

        #endregion

        #region Events

        public new event RoutedEventHandler Selected
        {
            add
            {
                AddHandler(SelectedEvent, value);
            }
            remove
            {
                RemoveHandler(SelectedEvent, value);
            }
        }

        public new event RoutedEventHandler Unselected
        {
            add
            {
                AddHandler(UnselectedEvent, value);
            }
            remove
            {
                RemoveHandler(UnselectedEvent, value);
            }
        }

        #endregion

        #region Routed Events

        public static readonly new RoutedEvent SelectedEvent =
            EventManager.RegisterRoutedEvent("Selected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ExtendedTreeViewItem));

        public static readonly new RoutedEvent UnselectedEvent =
            EventManager.RegisterRoutedEvent("Unselected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ExtendedTreeViewItem));

        #endregion

        #region Selection Logic

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ExtendedTreeViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is ExtendedTreeView;
        }

        protected void ExtendedTreeViewItem_Unselected(object sender, RoutedEventArgs e)
        {
            // This is the default tree view item unselected event handler
            e.Handled = true;
            if (Keyboard.Modifiers != ModifierKeys.Control)
            {
                this.TreeViewBacklink.UnselectAllExceptOf(this);
            }
        }

        protected void ExtendedTreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            // This is the default tree view item selected event handler
            e.Handled = true;
            if (this.IsSelected == true && Keyboard.Modifiers == ModifierKeys.Control)
            {
                this.IsSelected = false;
            }
            else
            {
                this.IsSelected = true;
            }
        }

        protected void Select()
        {
            if (this.TreeViewBacklink == null || this.TreeViewBacklink.SelectedItems.Contains(this.DataContext))
            {
                return;
            }

            if (this.TreeViewBacklink.SelectionMode == SelectionMode.Single)
            {
                // If we are in single selection mode, deselect all other elements
                // prior to selecting the current element
                this.TreeViewBacklink.UnselectAllExceptOf(this);
            }
            else
            {
                // If we are in multiple selection mode, we must check whether the 
                // other selected elements are of the same type as this element
                // If yes, they stay selected, if not, they will get unselected
                // Since we can only select one type of elements, we will look at 
                // the first element only
                Type selectionType = null;

                if (TreeViewBacklink.SelectedItems.Count > 0) { selectionType = TreeViewBacklink.SelectedItems.First().GetType(); }

                if (selectionType != this.DataContext.GetType())
                {
                    // Remove the existing selection
                    this.TreeViewBacklink.UnselectAllExceptOf(this);
                }
                else
                {
                    // If control is not pressed, remove the selection
                    if (Keyboard.Modifiers != ModifierKeys.Control)
                    {
                        this.TreeViewBacklink.UnselectAllExceptOf(this);
                    }
                    else
                    {
                        if (this.IsSelected == true)
                        {
                            Unselect();
                        }
                        else
                        {
                            this.Focus();
                        }
                    }
                }

            }

            this.TreeViewBacklink.SelectedItems.Add(this.DataContext);

            this.RaiseEvent(new RoutedEventArgs(ExtendedTreeViewItem.SelectedEvent));

        }

        protected void Unselect()
        {
            this.TreeViewBacklink.SelectedItems.Remove(this.DataContext);

            this.RaiseEvent(new RoutedEventArgs(ExtendedTreeViewItem.UnselectedEvent));
        }

        #endregion
    }
}
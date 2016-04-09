using MISD.Client.Managers;
using MISD.Client.Model;
using MISD.Client.ViewModel;
using MISD.Client.ViewModel.Converters;
using MISD.RegExUtil;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MISD.Client.Controls.Menu
{
    /// <summary>
    /// Interaktionslogik für ApplicationMenu.xaml
    /// </summary>
    public partial class ApplicationMenu : UserControl
    {
        public ApplicationMenu()
        {
            InitializeComponent();

            MainWindowViewModel.Instance.SelectedElements.CollectionChanged += 
                new NotifyCollectionChangedEventHandler(ApplicationMenu_MonitoringMSFilterdList_SelectionSync);

            MainWindowViewModel.Instance.SelectedElements.CollectionChanged +=
                new NotifyCollectionChangedEventHandler(ApplicationMenu_UnignoredMSFilterdList_SelectionSync);

            MainWindowViewModel.Instance.SelectedElements.CollectionChanged +=
                new NotifyCollectionChangedEventHandler(ApplicationMenu_AdministrationOUFilterdList_SelectionSync);
        }

        #region click handlers

        private void ExitApplicationMenuItem_Click(object sender, MouseButtonEventArgs e)
        {
            MainWindow.Current.Close();
        }

        private void EmailDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = EmailAdressList.SelectedItem as MailUser;
            DataModel.Instance.MailUsers.RemoveOnUI(selected);
        }

        private void EmailAdd_Click(object sender, RoutedEventArgs e)
        {
            var newMailUser = new MailUser(-1, "NewUser", "user@host.de", false, new ExtendedObservableCollection<Tuple<int, string, string, string>>());
            DataModel.Instance.MailUsers.AddOnUI(newMailUser);
            EmailAdressList.SelectedItem = newMailUser;
        }

        private void IgnoredOn_Click(object sender, RoutedEventArgs e)
        {
            IList<object> selected;
            if (MainWindowViewModel.Instance.MonitoredSystemFilterString.Length == 0)
            {
                selected = UnignoredMSList.SelectedItems;
            }
            else
            {
                selected = (IList<object>)UnignoredMSFilterdList.SelectedItems;
            }
            foreach (var item in selected)
            {
                if (item is MonitoredSystem)
                {
                    DataModel.Instance.IgnoreMS((item as MonitoredSystem));
                }
                if (item is OrganizationalUnit)
                {
                    DataModel.Instance.IgnoreOU((item as OrganizationalUnit));
                }
            }
        }

        private void IgnoredOff_Click(object sender, RoutedEventArgs e)
        {
            var selected = CloneList(IgnoredMSList.SelectedItems);
            foreach (var item in selected)
            {
                if (item is Tuple<string, string>)
                {
                    DataModel.Instance.IgnoredMonitoredSystems.RemoveOnUI(item as Tuple<string, string>);
                }
            }
        }


        private void OUDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = CloneList(AdministrationOUList.SelectedItems);

            foreach (var item in selected)
            {
                if (item is OrganizationalUnit)
                {
                    var ou = item as OrganizationalUnit;
                    if (ou.ParentID == null)
                    {
                        DataModel.Instance.Elements.RemoveOnUI(ou);
                    }
                    else
                    {
                        DataModel.Instance.GetOu((int)ou.ParentID).Elements.RemoveOnUI(ou);
                    }
                }
            }
        }

        private void OUAdd_Click(object sender, RoutedEventArgs e)
        {
            OrganizationalUnit newOU = null;

            var selected = AdministrationOUList.SelectedItems;
            foreach (var item in selected)
            {
                newOU = new OrganizationalUnit(
                    -1,
                    "newOU",
                    "",
                    null,
                    new ExtendedObservableCollection<TileableElement>(),
                    null);

                if (item is OrganizationalUnit)
                {
                    (item as OrganizationalUnit).Elements.AddOnUI(newOU);
                }
            }

            if (selected.Count > 0)
            {
                AdministrationOUList.SelectedItems.Clear();
                AdministrationOUList.SelectedItems.Add(newOU);
            }
        }

        private void ClusterAdd_Click(object sender, RoutedEventArgs e)
        {
            var newCluster = new Cluster(
                -1,
                "headnodeadress.newcluster.de",
                "HPC",
                "user",
                "password");

            DataModel.Instance.Clusters.AddOnUI(newCluster);
            AdministrationClusterList.SelectedItem = newCluster;
        }

        private void ClusterDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = AdministrationClusterList.SelectedItem;
            if (selected is Cluster)
            {
                DataModel.Instance.Clusters.RemoveOnUI((selected as Cluster));
            }
            this.AdministrationClusterList.SelectedIndex = 0;

        }

        private void PluginDown_Click(object sender, RoutedEventArgs e)
        {
            var selected = PriorityPlugins.SelectedItem;
            if (selected != null && selected is string)
            {
                int oldIndex = LayoutManager.Instance.PluginPriority.IndexOf(selected as string);
                if (oldIndex != LayoutManager.Instance.PluginPriority.Count - 1)
                {
                    LayoutManager.Instance.PluginPriority.Move(oldIndex, oldIndex + 1);
                }
            }
            MainWindowViewModel.Instance.PluginPriority = MainWindowViewModel.Instance.PluginPriority;

        }

        private void PluginUp_Click(object sender, RoutedEventArgs e)
        {
            var selected = PriorityPlugins.SelectedItem;
            if (selected != null && selected is string)
            {
                int oldIndex = LayoutManager.Instance.PluginPriority.IndexOf(selected as string);
                if (oldIndex > 0)
                {
                    LayoutManager.Instance.PluginPriority.Move(oldIndex, oldIndex - 1);
                }
            }
            MainWindowViewModel.Instance.PluginPriority = MainWindowViewModel.Instance.PluginPriority;
        }

        private void RegExFilterGenAdd_Click(object sender, RoutedEventArgs e)
        {
            if (MonitoringIndicatorList.SelectedItem != null)
            {
                RegExUtil.Operation op = RegExOperationConvertBack(this.RegExGenFilterOp.SelectedItem as string);
                string part = RegExUtility.GenerateRegEx(RegExGeneratorFilter.Text, op);
                string oldRegEx = (MonitoringIndicatorList.SelectedItem as Indicator).FilterStatement;

                var mergeList = new List<string>();
                mergeList.Add(oldRegEx);
                mergeList.Add(part);

                (MonitoringIndicatorList.SelectedItem as Indicator).FilterStatement = RegExUtility.MergeRegEx(mergeList);
                RegExGeneratorFilter.Text = "";
            }
        }

        private void RegExMetricWarningGenAdd_Click(object sender, RoutedEventArgs e)
        {
            if (MonitoringIndicatorList.SelectedItem != null)
            {
                RegExUtil.Operation op = RegExOperationConvertBack(this.RegExGenMetricWarningOp.SelectedItem as string);
                string part = RegExUtility.GenerateRegEx(RegExGeneratorMetricWarning.Text, op);
                string oldRegEx = (MonitoringIndicatorList.SelectedItem as Indicator).StatementWarning;

                var mergeList = new List<string>();
                mergeList.Add(oldRegEx);
                mergeList.Add(part);

                (MonitoringIndicatorList.SelectedItem as Indicator).StatementWarning = RegExUtility.MergeRegEx(mergeList);
                RegExGeneratorMetricWarning.Text = "";
            }
        }

        private void RegExMetricCriticalGenAdd_Click(object sender, RoutedEventArgs e)
        {
            if (MonitoringIndicatorList.SelectedItem != null)
            {
                RegExUtil.Operation op = RegExOperationConvertBack(this.RegExGenMetricCriticalOp.SelectedItem as string);
                string part = RegExUtility.GenerateRegEx(RegExGeneratorMetricCritical.Text, op);
                string oldRegEx = (MonitoringIndicatorList.SelectedItem as Indicator).StatementCritical;

                var mergeList = new List<string>();
                mergeList.Add(oldRegEx);
                mergeList.Add(part);

                (MonitoringIndicatorList.SelectedItem as Indicator).StatementCritical = RegExUtility.MergeRegEx(mergeList);
                RegExGeneratorMetricCritical.Text = "";
            }
        }

        private void RegExFilterGenDelete_Click(object sender, RoutedEventArgs e)
        {
            (MonitoringIndicatorList.SelectedItem as Indicator).FilterStatement = "";
        }

        private void RegExMetricWarningGenDelete_Click(object sender, RoutedEventArgs e)
        {
            (MonitoringIndicatorList.SelectedItem as Indicator).StatementWarning = "";
        }

        private void RegExMetricCriticalGenDelete_Click(object sender, RoutedEventArgs e)
        {
            (MonitoringIndicatorList.SelectedItem as Indicator).StatementCritical = "";
        }

        private void FontSizeUp_Click(object sender, RoutedEventArgs e)
        {
            var sizeInList = (from q in MainWindowViewModel.Instance.FontSizes
                              where q >= MainWindowViewModel.Instance.FontSize
                              select q).Min();
            var sizeIdx = MainWindowViewModel.Instance.FontSizes.ToList().IndexOf(sizeInList);
            var maxIdx = MainWindowViewModel.Instance.FontSizes.ToList().IndexOf(MainWindowViewModel.Instance.FontSizes.Max());

            if (sizeInList == MainWindowViewModel.Instance.FontSize && sizeIdx < maxIdx)
            {
                MainWindowViewModel.Instance.FontSize = MainWindowViewModel.Instance.FontSizes.ToList().ElementAt(sizeIdx + 1);

            }
            else
            {
                MainWindowViewModel.Instance.FontSize = MainWindowViewModel.Instance.FontSizes.ToList().ElementAt(sizeIdx);
            }
        }

        private void FonSizeDown_Click(object sender, RoutedEventArgs e)
        {
            var sizeInList = (from q in MainWindowViewModel.Instance.FontSizes
                              where q <= MainWindowViewModel.Instance.FontSize
                              select q).Max();
            var sizeIdx = MainWindowViewModel.Instance.FontSizes.ToList().IndexOf(sizeInList);

            if (sizeInList == MainWindowViewModel.Instance.FontSize && sizeIdx > 0)
            {
                MainWindowViewModel.Instance.FontSize = MainWindowViewModel.Instance.FontSizes.ToList().ElementAt(sizeIdx - 1);

            }
            else
            {
                MainWindowViewModel.Instance.FontSize = MainWindowViewModel.Instance.FontSizes.ToList().ElementAt(sizeIdx);
            }

        }

        private void EmailObserverOn_Click(object sender, RoutedEventArgs e)
        {
            var emailUser = this.EmailAdressList.SelectedItem as MailUser;
            var selected = this.EmailUnregisterdMSList.SelectedItems;

            foreach (var i in selected)
            {
                if (i is Tuple<int, string, string, string> && emailUser != null)
                {
                    var item = i as Tuple<int, string, string, string>;
                    emailUser.RegisteredMonitoredSystems.AddOnUI(item);
                }
            }
        }

        private void EmailObserverOff_Click(object sender, RoutedEventArgs e)
        {
            var emailUser = this.EmailAdressList.SelectedItem as MailUser;
            var selected = CloneList(this.EmailRegisterdMSList.SelectedItems);

            foreach (var i in selected)
            {
                if (i is Tuple<int, string, string, string> && emailUser != null)
                {
                    var item = i as Tuple<int, string, string, string>;
                    emailUser.RegisteredMonitoredSystems.RemoveOnUI(item);
                }
            }
        }

        private void LevelAdd_Click(object sender, RoutedEventArgs e)
        {
            var newLevel = new LevelDefinition()
            {
                UseCustomUI = false,
                HasStatusBar = false,
                Rows = 1
            };

            DataModel.Instance.LevelDefinitions.AddOnUI(newLevel);

            LevelDefinitionsListBox.SelectedItem = newLevel;
        }

        private void LevelDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = LevelDefinitionsListBox.SelectedItem as LevelDefinition;

            if (selected != null && DataModel.Instance.LevelDefinitions.Count > 1)
            {
                DataModel.Instance.LevelDefinitions.RemoveOnUI(selected);
                LevelDefinitionsListBox.SelectedIndex = 0;
            }
        }

        #endregion


        #region selection handler

        private void EmailAdressList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var converter = new MonitoredSystemWhiteListToBlackListConverter();
            var selected = this.EmailAdressList.SelectedItem;

            if (selected != null && selected is MailUser)
            {
                var mailUser = selected as MailUser;
                //set observed monitored systems list
                MainWindowViewModel.Instance.ObservedMonitoredSystemView
                    = CollectionViewSource.GetDefaultView(mailUser.RegisteredMonitoredSystems);

                //set un observred monitored systems list
                MainWindowViewModel.Instance.UnObservedMonitoredSystemView
                    = CollectionViewSource.GetDefaultView(converter.Convert(mailUser.RegisteredMonitoredSystems, null, null, null));
            }
            else
            {
                MainWindowViewModel.Instance.ObservedMonitoredSystemView = CollectionViewSource.GetDefaultView(new ExtendedObservableCollection<object>());
                MainWindowViewModel.Instance.UnObservedMonitoredSystemView = CollectionViewSource.GetDefaultView(new ExtendedObservableCollection<object>());
            }
        }

        private void MonitoringMSFilterdList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainWindowViewModel.Instance.SelectedMSFilterdCount = this.MonitoringMSFilterdList.SelectedItems.Count;

            var stackTrace = new StackTrace();
            if (!stackTrace.ToString().Contains("ApplicationMenu_MonitoringMSFilterdList_SelectionSync"))
            {
                var selected = CloneList(MonitoringMSFilterdList.SelectedItems);
                MainWindowViewModel.Instance.SelectedElements.Clear();
                foreach (var item in selected)
                {
                    MainWindowViewModel.Instance.SelectedElements.Add(item);
                } 
            }
            stackTrace = null;
        }

        public void ApplicationMenu_MonitoringMSFilterdList_SelectionSync(Object sender, NotifyCollectionChangedEventArgs  e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    
                    MonitoringMSFilterdList.SelectedItems.Remove(item);
                }
            }

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (!MonitoringMSFilterdList.SelectedItems.Contains(item))
                    {
                        MonitoringMSFilterdList.SelectedItems.Add(item);
                    }
                }
            }
        }

        private void UnignoredMSFilterdList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainWindowViewModel.Instance.SelectedUnignoredMSFilterdCount = this.UnignoredMSFilterdList.SelectedItems.Count;

            var stackTrace = new StackTrace();
            if (!stackTrace.ToString().Contains("ApplicationMenu_UnignoredMSFilterdList_SelectionSync"))
            {
                var selected = CloneList(UnignoredMSFilterdList.SelectedItems);
                MainWindowViewModel.Instance.SelectedElements.Clear();
                foreach (var item in selected)
                {
                    MainWindowViewModel.Instance.SelectedElements.Add(item);
                }
            }
            stackTrace = null;
        }

        public void ApplicationMenu_UnignoredMSFilterdList_SelectionSync(Object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    UnignoredMSFilterdList.SelectedItems.Remove(item);
                }
            }

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (!UnignoredMSFilterdList.SelectedItems.Contains(item))
                    {
                        UnignoredMSFilterdList.SelectedItems.Add(item);
                    }
                }
            }
        }

        private void AdministrationOUFilterdList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainWindowViewModel.Instance.SelectedOUFilterdCount = this.AdministrationOUFilterdList.SelectedItems.Count;

            var stackTrace = new StackTrace();
            var x = stackTrace.ToString();
            if (!stackTrace.ToString().Contains("ApplicationMenu_AdministrationOUFilterdList_SelectionSync"))
            {
                var selected = CloneList(AdministrationOUFilterdList.SelectedItems);
                MainWindowViewModel.Instance.SelectedElements.Clear();
                foreach (var item in selected)
                {
                    MainWindowViewModel.Instance.SelectedElements.Add(item);
                }
            }
            stackTrace = null;
        }

        public void ApplicationMenu_AdministrationOUFilterdList_SelectionSync(Object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    AdministrationOUFilterdList.SelectedItems.Remove(item);
                }
            }
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is OrganizationalUnit && !AdministrationOUFilterdList.SelectedItems.Contains(e.NewItems))
                    {
                        AdministrationOUFilterdList.SelectedItems.Add(item);
                    }
                }
            }
        }

        #endregion

        #region util

        private List<object> CloneList(IList<object> list)
        {
            var workList = new List<object>();
            foreach (var item in list)
            {
                workList.Add(item);
            }
            return workList;
        }

        private List<object> CloneList(System.Collections.IList list)
        {
            var workList = new List<object>();
            foreach (var item in list)
            {
                workList.Add(item);
            }
            return workList;
        }

        private string RegExOperationConvert(RegExUtil.Operation value)
        {
            switch (value)
            {
                case RegExUtil.Operation.Contain:
                    return "&#8712;";
                case RegExUtil.Operation.Equal:
                    return "=";
                case RegExUtil.Operation.Less:
                    return "&lt;";
                case RegExUtil.Operation.Major:
                    return "&gt;";
                case RegExUtil.Operation.NotContain:
                    return "&#8713;";
                default:
                    return "&#8712;";
            }

        }

        private RegExUtil.Operation RegExOperationConvertBack(string value)
        {

            switch (value)
            {
                case "&#8712;":
                    return RegExUtil.Operation.Contain;
                case "=":
                    return RegExUtil.Operation.Equal;
                case "&lt;":
                    return RegExUtil.Operation.Less;
                case "&gt;":
                    return RegExUtil.Operation.Major;
                case "&#8713;":
                    return RegExUtil.Operation.NotContain;
                default:
                    return RegExUtil.Operation.Contain;
            }
        }

        #endregion

    }
}

using MISD.Client.Controls;
using MISD.Client.Controls.Panels;
using MISD.Client.Model.Resources;
using MISD.Client.Managers;
using MISD.Client.Model;
using MISD.Client.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;
using MISD.Client.Properties;
using System.Windows.Media.Imaging;
using System.IO;
using System.Security.Principal;
using System.Runtime.Serialization.Formatters.Binary;
using MISD.Client.Model.Managers;

namespace MISD.Client
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Properties
        public Boolean hastried = false;

        public static MainWindow Current { get; set; }

        private Point startPosition;
        private bool startIsOU = true;
        private int startOuID = -1;
        private int startPartentID = -1;
        private string startName = "";

        private bool firstDrop = true;

        #endregion

        #region MainWindow

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;
            this.SizeChanged += MainWindow_SizeChanged;

            // Assign the datacontext to enable databinding
            this.DataContext = MainWindowViewModel.Instance;

            // Multiselection initialisieren
            //MainTreeView.SelectedItemChanged +=
            //    new RoutedPropertyChangedEventHandler<object>(MainTreeView_SelectedItemChanged);

            MainTreeView.Focusable = true;

            Current = this;
            if (ConfigClass.IsPowerwall)
                LayoutManager.Instance.TcpConnection.ShutdownEvent += TcpConnection_ShutdownEvent;
        }

        private void TcpConnection_ShutdownEvent()
        {
            Console.WriteLine("Got Shutdown-Event. Go down right now");
            Application.Current.Dispatcher.Invoke(new Action(() =>
                       {
                           this.forcedClose();
                       }));
        }

        private void forcedClose()
        {
            ThreadManager.KillAllThreads();
            DataModel.Instance.StopSynchronization();
            if (DataModel.Instance.IsOperator || DataModel.Instance.IsPowerwall)
            {
                LayoutManager.Instance.TcpConnection.Stop();
            }

            Application.Current.Shutdown();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            if (ConfigClass.IsPowerwall || ConfigClass.IsOperator)
            {
                double scale = ContentRoot.RenderSize.Height / ConfigClass.PowerwallHeight;
                ScaleTransform transform = new ScaleTransform(scale, scale);
                PowerwallCanvas.LayoutTransform = transform;
            }

        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            this.forcedClose();
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!ConfigClass.IsPowerwall)
            {
                MessageBoxResult result = MessageBox.Show(Strings.CloseText, Strings.CloseTitel, MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    List<Layout> list = new List<Layout>();
                    var newLayout = new Layout();
                    newLayout.Name = WindowsIdentity.GetCurrent().Name + ": " + System.DateTime.Now.ToString();
                    newLayout.UserName = WindowsIdentity.GetCurrent().Name;
                    newLayout.PreviewImage = MakeAndStoreScreenshot();
                    newLayout.Date = System.DateTime.Now;
                    newLayout.Data = SerializeBase64(LayoutManager.Instance.CurrentLayout);
                    list.Add(newLayout);
                    if (MISD.Client.Model.DataModel.Instance.IsSyncing == true)
                    {
                        WorkerThread workerThreadName = ThreadManager.CreateWorkerThread("BackSync_LayoutAdd", () =>
                        {
                            var sync = MISD.Client.Model.DataModel.Instance.SyncBackLayoutAdd(list);

                        }, false);
                    }
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
                else if (result == MessageBoxResult.No)
                {

                }

                // close powerwall apps.
                /*if ((result == MessageBoxResult.No || result == MessageBoxResult.Yes) && ConfigClass.IsOperator)
                {
                    MessageBoxResult closePowerwall = MessageBox.Show(Strings.ClosePowerwallText, Strings.ClosePowerwallTitle, MessageBoxButton.YesNo);
                    if (closePowerwall == MessageBoxResult.Yes)
                        LayoutManager.Instance.ShutdownClients();
                }*/


            }
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (ConfigClass.IsPowerwall)
            {
                this.WindowStyle = WindowStyle.None;
                //this.WindowState = WindowState.Maximized;
                TranslateTransform transform = new TranslateTransform(ConfigClass.PowerwallOffset, 0);
                MainTreeView.RenderTransform = transform;

                Canvas.SetLeft(MainTreeView, ConfigClass.PowerwallOffset);
                // This code is needed to show the offset bar
                //for (int i = 0; i < ConfigClass.PowerwallWidth; i += 250)
                //{
                //    var textBox = new TextBox();
                //    textBox.Text = "" + i;
                //    textBox.FontSize = 50;
                //    textBox.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                //    PowerwallCanvas.Children.Add(textBox);

                //    TranslateTransform transform2 = new TranslateTransform(ConfigClass.PowerwallOffset + i, 0);
                //    textBox.RenderTransform = transform2;
                //}
                this.Height = SystemParameters.VirtualScreenHeight;
                this.Width = SystemParameters.VirtualScreenWidth;
                this.Top = 0;
                this.Left = 0;
            }

            // Start the datamodel synchronization
            Task.Factory.StartNew(() => { DataModel.Instance.StartSynchronization(); });
            //if (ConfigClass.IsPowerwall)
            {
                this.RegisterForOUChanges();
            }
        }

        public MainWindowViewModel ViewModel
        {
            get { return MainWindowViewModel.Instance; }
        }

        #endregion

        #region RibbonButtons

        private void Ribbon_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            MainWindowViewModel.Instance.IsMenuOpen = false;
        }

        private void ApplicationMenuToggleButton_Checked_1(object sender, RoutedEventArgs e)
        {
            // TODO dieser Code wird ausgeführt, aber der Tab ist immer noch sichtbar
            // Möglicher Weise über delay im Binding änderbar
            // funktioniert exact jedes zweite mal
            this.Ribbons.IsMinimized = true;
            // Aktualisiert selection binding

        }

        # region LayoutButtons

        #region Converters

        private string SerializeBase64(Layout o)
        {
            MemoryStream ws = new MemoryStream();
            try
            {
                // Serialize to a base 64 string
                byte[] bytes;
                long length = 0;
                BinaryFormatter sf = new BinaryFormatter();
                sf.Serialize(ws, o);
                length = ws.Length;
                bytes = ws.GetBuffer();
                string encodedData = bytes.Length + ":" + Convert.ToBase64String(bytes, 0, bytes.Length, Base64FormattingOptions.None);
                ws.Flush();
                ws.Close();
                return encodedData;
            }
            catch (Exception e)
            {
                ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, Core.LogType.Exception);
                return null;
            }
            finally
            {
                ws.Flush();
                ws.Close();
                ws.Dispose();
            }
        }

        public Layout DeserializeBase64(string s)
        {
            if (s != null)
            {
                // We need to know the exact length of the string - Base64 can sometimes pad us by a byte or two
                int p = s.IndexOf(':');
                int length = Convert.ToInt32(s.Substring(0, p));

                // Extract data from the base 64 string!
                byte[] memorydata = Convert.FromBase64String(s.Substring(p + 1));
                MemoryStream rs = new MemoryStream(memorydata, 0, length);
                try
                {
                    BinaryFormatter sf = new BinaryFormatter();
                    object o = sf.Deserialize(rs);
                    rs.Flush();
                    rs.Close();
                    return o as Layout;
                }
                catch (Exception e)
                {
                    ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, Core.LogType.Exception);
                    return null;
                }
                finally
                {
                    rs.Flush();
                    rs.Close();
                    rs.Dispose();
                }
            }
            else
            {
                return null;
            }
        }

        public byte[] ImageToByteStream(System.Drawing.Bitmap im)
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            try
            {
                im.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                stream.Position = 0;
                byte[] data = new byte[stream.Length];
                stream.Read(data, 0, (int)stream.Length);
                return data;
            }
            catch (Exception e)
            {
                ClientLogger.Instance.WriteEntry("Unexpected exception occured: " + e, Core.LogType.Exception);
                return null;
            }
            finally
            {
                stream.Flush();
                stream.Close();
                stream.Dispose();
            }
        }

        #endregion

        private void Ribbon_Layouts_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            RibbonGallery gallery = e.OriginalSource as RibbonGallery;
            if (gallery == null)
            {
                return;
            }
            MainWindowViewModel.Instance.selectedLayout = gallery.SelectedItem as Layout;

            if (MainWindowViewModel.Instance.selectedLayout.GetType() != typeof(Layout))
            {
                return;
            }
            MISD.Client.Managers.LayoutManager.Instance.CurrentLayout = MainWindowViewModel.Instance.selectedLayout;
            if (DeserializeBase64(MainWindowViewModel.Instance.selectedLayout.Data) != null)
            {
                MISD.Client.Managers.LayoutManager.Instance.CurrentLayout.Data = ((Layout)DeserializeBase64(MainWindowViewModel.Instance.selectedLayout.Data)).Data;
            }
            else
            {
                MISD.Client.Managers.LayoutManager.Instance.CurrentLayout.Data = MainWindowViewModel.Instance.selectedLayout.Data;
            }

        }

        private void DeleteLayout(object sender, RoutedEventArgs e)
        {
            if (DataModel.Instance.Layouts.Count > 0)
            {
                DataModel.Instance.Layouts.Remove(MainWindowViewModel.Instance.selectedLayout);
                DataModel.Instance.LastChangedLayout = "neu";

            }
        }

        private void NewLayout(object sender, RoutedEventArgs e)
        {
            var newLayout = new Layout();
            if (!Settings.Default.IsPowerwall & !Settings.Default.IsOperator)
            {
                var dialog = new Controls.DialogTextbox.DialogTextBox();
                dialog.ShowDialog();
                if (dialog.LayoutName != null)
                {
                    if (dialog.LayoutName != Strings.NewName && dialog.LayoutName != "")
                    {
                        newLayout.Name = dialog.LayoutName;
                    }
                    else
                    {
                        newLayout.Name = WindowsIdentity.GetCurrent().Name + ": " + System.DateTime.Now.ToString();
                    }
                }
                else { return; }
            }
            else { newLayout.Name = WindowsIdentity.GetCurrent().Name + ": " + System.DateTime.Now.ToString(); }
            newLayout.UserName = WindowsIdentity.GetCurrent().Name;
            newLayout.PreviewImage = MakeAndStoreScreenshot();
            newLayout.Date = System.DateTime.Now;
            newLayout.Data = SerializeBase64(LayoutManager.Instance.CurrentLayout);
            DataModel.Instance.Layouts.Add(newLayout);
            DataModel.Instance.LastChangedLayout = "neu";
            LayoutGalery.SelectedItem = newLayout;
            while (LayoutGalery.SelectedItem.GetType() != typeof(Layout))
            {
                LayoutGalery.SelectedItem = newLayout;
            }
        }

        private void SaveLayout(object sender, RoutedEventArgs e)
        {
            var newLayout = new Layout();
            newLayout.PreviewImage = MakeAndStoreScreenshot();
            newLayout.UserName = WindowsIdentity.GetCurrent().Name;
            newLayout.Date = System.DateTime.Now;
            newLayout.Data = SerializeBase64(LayoutManager.Instance.CurrentLayout);
            WorkerThread workerThreadName = ThreadManager.CreateWorkerThread("BackSync_LayoutChanged", () =>
            {
                DataModel.Instance.LayoutManager_Instance_LayoutChanged(newLayout);

            }, false);
            DataModel.Instance.LastChangedLayout = "neu";
        }

        private byte[] MakeAndStoreScreenshot()
        {
            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap((int)this.Width, (int)this.Height))
            {
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(new System.Drawing.Point((int)this.Left, (int)this.Top), System.Drawing.Point.Empty, new System.Drawing.Size((int)this.Width, (int)this.Height));
                    float scale = (float)(50 / this.Width);
                    return ImageToByteStream(ResizeBitmap(bitmap, (int)(this.Width * scale), (int)(this.Height * scale)));
                }
            }
        }
        private System.Drawing.Bitmap ResizeBitmap(System.Drawing.Bitmap sourceBMP, int width, int height)
        {
            System.Drawing.Bitmap result = new System.Drawing.Bitmap(width, height);
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(result))
                g.DrawImage(sourceBMP, 0, 0, width, height);
            return result;
        }

        #endregion

        #region TileButtons


        private void Ribbon_Levels_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            RibbonGallery gallery = e.OriginalSource as RibbonGallery;
            if (gallery == null)
            {
                return;
            }
            LevelDefinition selectedLevelDefinition = gallery.SelectedItem as LevelDefinition;

            if (selectedLevelDefinition == null)
            {
                return;
            }


            foreach (MonitoredSystem monitoredSystem in (DataContext as MainWindowViewModel).SelectedSystems)
            {
                MonitoredSystemState state = LayoutManager.Instance.GetMSState(monitoredSystem.ID);
                if (state == null)
                {
                    state = new MonitoredSystemState();
                }

                LayoutManager.Instance.SetMSState(monitoredSystem.ID, selectedLevelDefinition.LevelID);
            }
        }


        private void DeleteTile(object sender, RoutedEventArgs e)
        {
            foreach (MonitoredSystem ms in (DataContext as MainWindowViewModel).SelectedSystems)
            {
                foreach (OrganizationalUnit ou in DataModel.Instance.Elements.GetOrganizationalUnits())
                {
                    if (ou.ID == ms.OuID)
                    {
                        ou.Elements.RemoveOnUI(ms);
                        DataModel.Instance.IgnoredMonitoredSystems.Add(new Tuple<string, string>(ms.MAC, ms.Name));
                    }
                }
            }
            MainTreeView.UnselectAll();
        }

        private void OpenEmailSettings(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel.Instance.IsMenuOpen = true;
            MainWindowViewModel.Instance.IsMenuItemOpen = MenuStates.EmailSettings;
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel.Instance.IsMenuOpen = true;
            MainWindowViewModel.Instance.IsMenuItemOpen = MenuStates.Administration;
        }

        #endregion

        #region OUButtons

        private void DeleteOUs(object sender, RoutedEventArgs e)
        {
            List<OrganizationalUnit> listToDelete = new List<OrganizationalUnit>((DataContext as MainWindowViewModel).SelectedOUs);
            foreach (OrganizationalUnit outoDelete2 in listToDelete)
            {
                if (outoDelete2.ParentID == null)
                {
                    DataModel.Instance.Elements.RemoveOnUI(outoDelete2);
                }
                else
                {
                    foreach (OrganizationalUnit ou in DataModel.Instance.Elements.GetOrganizationalUnits())
                    {
                        if (ou.ID == outoDelete2.ParentID)
                        {
                            ou.Elements.RemoveOnUI(outoDelete2);
                        }
                    }
                }
            }

            MainTreeView.UnselectAll();
        }

        #endregion

        #endregion

        #region context menu


        private void ContextMenu_ContextMenuOpening_1(object sender, System.Windows.Controls.ContextMenuEventArgs e)
        {
            //imenu.CustomPopupPlacementCallback = new System.Windows.Controls.Primitives.CustomPopupPlacementCallback(this.PositionContextMenu);
        }

        //public CustomPopupPlacement[] PositionContextMenu(Size size1, Size size2, Point p)
        //{
        //    var c = new CustomPopupPlacement(new Point(10, 10), PopupPrimaryAxis.None);
        //    return new CustomPopupPlacement[] { c };
        //}

        #region context menu click methods
        /// <summary>
        /// Adds a new ou (name: "newOU") into the given ous
        /// </summary>
        private void KontextMenuItem_Click_AddOU(object sender, RoutedEventArgs e)
        {
            if ((DataContext as MainWindowViewModel).SelectedOUs.Count == 0)
            {
                var newOU = new OrganizationalUnit(
                    -1,
                    "newOU",
                    "",
                    null,
                    new ExtendedObservableCollection<TileableElement>(),
                    null);
            }
            else
            {
                foreach (var item in (DataContext as MainWindowViewModel).SelectedOUs)
                {
                    var parent = DataModel.Instance.GetOu(item.ID);
                    int? parentID = null;
                    if (parent != null)
                    {
                        parentID = parent.ID;
                    }

                    var newOU = new OrganizationalUnit(
                        -1,
                        "newOU",
                        "",
                        parentID,
                        new ExtendedObservableCollection<TileableElement>(), null);
                }
            }
        }


        private void KontextMenuItem_Click_IncreaseLevel(object sender, RoutedEventArgs e)
        {
            if ((DataContext as MainWindowViewModel).SelectedSystems.Count == 0 && (DataContext as MainWindowViewModel).SelectedOUs.Count != 0)
            {
                ExtendedObservableCollection<TileableElement> list = new ExtendedObservableCollection<TileableElement>();
                foreach (OrganizationalUnit ou in (DataContext as MainWindowViewModel).SelectedOUs)
                {
                    MainTreeView.Expand(ou);
                    list.Add(ou);
                }
                foreach (MonitoredSystem monitoredSystem in list.GetMonitoredSystems())
                {
                    MonitoredSystemState state = LayoutManager.Instance.GetMSState(monitoredSystem.ID);
                    if (state == null)
                    {
                        state = new MonitoredSystemState();
                    }

                    // Increases the level until the highest level is reached
                    var tempList = (from p in DataModel.Instance.LevelDefinitions
                                    where p.Level > (from q in DataModel.Instance.LevelDefinitions
                                                     where q.LevelID == state.Level
                                                     select q).First().Level
                                    orderby p.Level, p.Rows, p.ID ascending
                                    select p).ToList();


                    if (tempList.Count > 0)
                    {
                        LayoutManager.Instance.SetMSState(monitoredSystem.ID, tempList.ElementAt(0).LevelID);
                    }
                }
            }
            else
            {
                foreach (MonitoredSystem monitoredSystem in (DataContext as MainWindowViewModel).SelectedSystems)
                {
                    MonitoredSystemState state = LayoutManager.Instance.GetMSState(monitoredSystem.ID);
                    if (state == null)
                    {
                        state = new MonitoredSystemState();
                    }

                    // Increases the level until the highest level is reached
                    var tempList = (from p in DataModel.Instance.LevelDefinitions
                                    where p.Level > (from q in DataModel.Instance.LevelDefinitions
                                                     where q.LevelID == state.Level
                                                     select q).First().Level
                                    orderby p.Level, p.Rows, p.ID ascending
                                    select p).ToList();


                    if (tempList.Count > 0)
                    {
                        LayoutManager.Instance.SetMSState(monitoredSystem.ID, tempList.ElementAt(0).LevelID);
                    }
                }
            }
        }

        /// <summary>
        /// Toggle maintance state of the given monitored systems
        /// </summary>
        private void KontextMenuItem_Click_DecreaseLevel(object sender, RoutedEventArgs e)
        {
            if ((DataContext as MainWindowViewModel).SelectedSystems.Count == 0 && (DataContext as MainWindowViewModel).SelectedOUs.Count != 0)
            {
                ExtendedObservableCollection<TileableElement> list = new ExtendedObservableCollection<TileableElement>();
                foreach (OrganizationalUnit ou in (DataContext as MainWindowViewModel).SelectedOUs)
                {
                    MainTreeView.Expand(ou);
                    list.Add(ou);
                }
                foreach (MonitoredSystem monitoredSystem in list.GetMonitoredSystems())
                {
                    MonitoredSystemState state = LayoutManager.Instance.GetMSState(monitoredSystem.ID);
                    if (state == null)
                    {
                        state = new MonitoredSystemState();
                    }

                    // Reduce the level until the smallest level is reached
                    var tempList = (from p in DataModel.Instance.LevelDefinitions
                                    where p.Level < (from q in DataModel.Instance.LevelDefinitions
                                                     where q.LevelID == state.Level
                                                     select q).First().Level
                                    orderby p.Level descending
                                    select p).ToList();

                    if (tempList.Count > 0)
                    {
                        LayoutManager.Instance.SetMSState(monitoredSystem.ID, tempList.ElementAt(0).LevelID);
                    }
                }
            }
            else
            {
                foreach (MonitoredSystem monitoredSystem in (DataContext as MainWindowViewModel).SelectedSystems)
                {
                    MonitoredSystemState state = LayoutManager.Instance.GetMSState(monitoredSystem.ID);
                    if (state == null)
                    {
                        state = new MonitoredSystemState();
                    }

                    // Reduce the level until the smallest level is reached
                    var tempList = (from p in DataModel.Instance.LevelDefinitions
                                    where p.Level < (from q in DataModel.Instance.LevelDefinitions
                                                     where q.LevelID == state.Level
                                                     select q).First().Level
                                    orderby p.Level descending
                                    select p).ToList();

                    if (tempList.Count > 0)
                    {
                        LayoutManager.Instance.SetMSState(monitoredSystem.ID, tempList.ElementAt(0).LevelID);
                    }
                }
            }
        }


        private void KontextMenuItem_Click_Maintance(object sender, RoutedEventArgs e)
        {
            var hasNoMaintenace = false;
            if ((DataContext as MainWindowViewModel).SelectedSystems.Count == 0 && (DataContext as MainWindowViewModel).SelectedOUs.Count != 0)
            {
                ExtendedObservableCollection<TileableElement> list = new ExtendedObservableCollection<TileableElement>();
                foreach (OrganizationalUnit ou in (DataContext as MainWindowViewModel).SelectedOUs)
                {
                    MainTreeView.Expand(ou);
                    list.Add(ou);
                }
                foreach (MonitoredSystem monitoredSystem in list.GetMonitoredSystems())
                {
                    if (monitoredSystem.State != MISD.Core.MappingState.Maintenance)
                    {
                        hasNoMaintenace = true;
                    }
                }
                if (hasNoMaintenace)
                {
                    foreach (MonitoredSystem monitoredSystem in list.GetMonitoredSystems())
                    {

                        monitoredSystem.State = MISD.Core.MappingState.Maintenance;
                    }
                }
                else
                {
                    foreach (MonitoredSystem monitoredSystem in list.GetMonitoredSystems())
                    {

                        monitoredSystem.State = MISD.Core.MappingState.OK;
                    }
                }

            }
            else
            {
                foreach (var item in (DataContext as MainWindowViewModel).SelectedSystems)
                {
                    if (item.State != MISD.Core.MappingState.Maintenance)
                    {
                        hasNoMaintenace = true;
                    }
                }
                if (hasNoMaintenace)
                {
                    foreach (MonitoredSystem monitoredSystem in (DataContext as MainWindowViewModel).SelectedSystems)
                    {

                        monitoredSystem.State = MISD.Core.MappingState.Maintenance;
                    }
                }
                else
                {
                    foreach (MonitoredSystem monitoredSystem in (DataContext as MainWindowViewModel).SelectedSystems)
                    {

                        monitoredSystem.State = MISD.Core.MappingState.OK;
                    }
                }
            }
        }

        /// <summary>
        /// Resets the mapping of the given monitored systems
        /// </summary>
        private void KontextMenuItem_Click_ResetMapping(object sender, RoutedEventArgs e)
        {
            if ((DataContext as MainWindowViewModel).SelectedSystems.Count == 0 && (DataContext as MainWindowViewModel).SelectedOUs.Count != 0)
            {
                ExtendedObservableCollection<TileableElement> list = new ExtendedObservableCollection<TileableElement>();
                foreach (OrganizationalUnit ou in (DataContext as MainWindowViewModel).SelectedOUs)
                {
                    MainTreeView.Expand(ou);
                    list.Add(ou);
                }
                foreach (MonitoredSystem monitoredSystem in list.GetMonitoredSystems())
                {
                    monitoredSystem.ResetDate = DateTime.Now;
                    monitoredSystem.State = MISD.Core.MappingState.OK;
                }
            }
            else
            {
                foreach (var item in (DataContext as MainWindowViewModel).SelectedSystems)
                {
                    item.ResetDate = DateTime.Now;
                    item.State = MISD.Core.MappingState.OK;
                }
            }
        }

        /// <summary>
        /// Show the email settingss menu
        /// </summary>
        private void KontextMenuItem_Click_EmailSettings(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel.Instance.IsMenuOpen = true;
            MainWindowViewModel.Instance.IsMenuItemOpen = MenuStates.EmailSettings;
        }

        /// <summary>
        /// Show the monitoring settings menu
        /// </summary>
        private void KontextMenuItem_Click_Monitoring(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel.Instance.IsMenuOpen = true;
            MainWindowViewModel.Instance.IsMenuItemOpen = MenuStates.Monitoring;

        }
        #endregion


        #region context menu label methods
        private void MenuItem_AddOU_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            MainContextMenu.Tag = Strings.AddOU;
        }

        private void MenuItem_IncreaseLevel_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            MainContextMenu.Tag = Strings.Levels + " +";
        }

        private void MenuItem_DecreaseLevel_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            MainContextMenu.Tag = Strings.Levels + " -";
        }

        private void MenuItem_Maintance_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            MainContextMenu.Tag = Strings.Maintenance + " ON/OFF";
        }

        private void MenuItem_ResetMapping_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            MainContextMenu.Tag = Strings.Refresh;
        }

        private void MenuItem_EmailSettings_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            MainContextMenu.Tag = Strings.MailSettings;
        }

        private void MenuItem_Monitoring_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            MainContextMenu.Tag = Strings.Monitoring;
        }

        private void MenuItem_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            MainContextMenu.Tag = "";
        }

        #endregion

        #endregion

        #region Powerwall
        private void OuHasExpanded(object sender, RoutedEventArgs e)
        {
            // This event should not forwarded to the superior ou.
            e.Handled = true;

            //if (ConfigClass.IsOperator)
            {
                LayoutManager.Instance.SetOUState(Convert.ToInt32((sender as Expander).Tag.ToString()), true);
            }
        }

        /// <summary>
        /// Triggeerd when a OU changes its state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OuHasCollapsed(object sender, RoutedEventArgs e)
        {
            // This event should not forwarded to the superior ou.
            e.Handled = true;

            //if (ConfigClass.IsOperator)
            {
                LayoutManager.Instance.SetOUState(Convert.ToInt32((sender as Expander).Tag.ToString()), false);
            }
        }

        /// <summary>
        /// triggered when a ou collapses
        /// </summary>
        private void RegisterForOUChanges()
        {
            //if (ConfigClass.IsPowerwall)
            {
                LayoutManager.Instance.OUStateChanged += Instance_OUStateChanged;
            }

        }

        /// <summary>
        /// Method called when a ou changed its state and you should react on it
        /// </summary>
        /// <param name="ID">OU ID</param>
        /// <param name="open">Wether to open it or not</param>
        private void Instance_OUStateChanged(int ID, bool open)
        {
            foreach (OrganizationalUnit ou in DataModel.Instance.Elements.GetOrganizationalUnits())
            {
                if (ou.ID == ID)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                       {
                           if (open)
                           {
                               MainTreeView.Expand(ou);
                               /*foreach (var v in ou.Elements)
                               {
                                   Console.WriteLine("Opening subelements ou "+v.ID);
                                   OrganizationalUnit u = v as OrganizationalUnit;
                                   //MonitoredSystem m = v as MonitoredSystem;
                                   if (u != null)
                                   {
                                       bool? b = LayoutManager.Instance.GetOuState(u.ID);
                                       if (!b.HasValue)
                                           b = false;
                                       Instance_OUStateChanged(u.ID, b.Value);
                                   }
                               }*/

                           }
                           else
                           {
                               MainTreeView.Collapse(ou);
                           }
                       }));

                }
            }
        }

        #endregion

        #region Multiselect

        public void SelectionChanged()
        {
            MainWindowViewModel.Instance.ousSelected = Visibility.Collapsed;
            MainWindowViewModel.Instance.msSelected = Visibility.Collapsed;
            MainWindowViewModel.Instance.SystemsinOUs = 0;
            MainWindowViewModel.Instance.PathOfOU = "";
            return;
        }

        void MainTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (MainTreeView.SelectedItem == null)
            {
                MainWindowViewModel.Instance.ousSelected = Visibility.Collapsed;
                MainWindowViewModel.Instance.msSelected = Visibility.Collapsed;
                return;
            }

            if (MainTreeView.SelectedItem.GetType().Equals(typeof(MonitoredSystem)))
            {
                MainWindowViewModel.Instance.ousSelected = Visibility.Collapsed;
                MainWindowViewModel.Instance.msSelected = Visibility.Collapsed;
            }
            else
            {
                if (MainTreeView.SelectedItem.GetType().Equals(typeof(OrganizationalUnit)))
                {
                    MainWindowViewModel.Instance.ousSelected = Visibility.Collapsed;
                    MainWindowViewModel.Instance.msSelected = Visibility.Collapsed;
                }
            }
        }

        #endregion

        #region Drag and Drop

        /// <summary>
        /// Enables the dropping of OUs or MonitoredSystems inside the hierarchy.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DropHandler(object sender, DragEventArgs e)
        {
            if (firstDrop)
            {
                foreach (var item in DataModel.Instance.Elements.GetMonitoredSystems())
                {
                    if (LayoutManager.Instance.GetValue(item.ID, false) == item.ID)
                    {
                        LayoutManager.Instance.SetValue(item.ID, false, item.ID);
                    }
                }
                foreach (var item in DataModel.Instance.Elements.GetOrganizationalUnits())
                {
                    if (LayoutManager.Instance.GetValue(item.ID, true) == item.ID)
                    {
                        LayoutManager.Instance.SetValue(item.ID, true, item.ID);
                    }
                }
                firstDrop = false;
            }


            try
            {
                // tuple: bool isOU, int ID/ouID, int ouID/parentID
                Tuple<bool, int, int, string> oldValues = (Tuple<bool, int, int, string>)e.Data.GetData(typeof(Tuple<bool, int, int, string>));

                if (oldValues.Item2 == -1)
                {
                    return;
                }

                // get the treeviewitem where the drop took place
                DependencyObject element = sender as Grid;
                while (element != null && !element.GetType().Equals(typeof(ExtendedTreeViewItem)))
                {
                    element = VisualTreeHelper.GetParent(element);
                }

                // get the panel where the drop took place
                DependencyObject panel = sender as Grid;
                while (panel != null && !panel.GetType().Equals(typeof(SpaceFillingPanel)))
                {
                    panel = VisualTreeHelper.GetParent(panel);
                }

                // check if drop is on a monitored system
                if (panel != null)
                {
                    //find out which element was hit
                    DependencyObject parent = VisualTreeHelper.HitTest((panel as SpaceFillingPanel), e.GetPosition(panel as SpaceFillingPanel)).VisualHit;
                    while (parent != null && !(parent.GetType().Equals(typeof(Tile))))
                    {
                        parent = VisualTreeHelper.GetParent(parent);
                    }

                    // find the corresponding monitored system
                    if (parent != null)
                    {
                        //Debug
                        //Console.WriteLine("Hittest true");

                        var monitoredSystem = (parent as Tile).DataContext as MonitoredSystem;

                        // check if the OU has changed as well
                        if (monitoredSystem != null)
                        {
                            // dropping a ou
                            if (oldValues.Item1 == true)
                            {
                                //Debug
                                //Console.WriteLine("Dropping OU hittest");

                                foreach (OrganizationalUnit current in DataModel.Instance.Elements.GetOrganizationalUnits())
                                {
                                    if (current.ID.Equals(oldValues.Item2) && oldValues.Item3 != monitoredSystem.OuID && current.ID != monitoredSystem.OuID)
                                    {
                                        bool dropDeeperInside = false;

                                        foreach (var innerElement in current.GetOrganizationalUnits())
                                        {
                                            if (innerElement.ID == (monitoredSystem.OuID))
                                            {
                                                dropDeeperInside = true;
                                            }
                                        }

                                        if (!dropDeeperInside)
                                        {
                                            var isOpen = LayoutManager.Instance.GetOuState(current.ID);
                                            if (isOpen == null)
                                            {
                                                isOpen = false;
                                            }
                                            current.ParentID = monitoredSystem.OuID;
                                            LayoutManager.Instance.CurrentLayout.SetState(current.ID, (bool)isOpen);
                                        }
                                    }
                                }

                                //Debug
                                //Console.WriteLine("Dropping OU with ID " + oldValues.Item2 + " and name " + oldValues.Item4 + " before monitoredSystem with ID " + monitoredSystem.ID + " and name " + monitoredSystem.Name);

                                LayoutManager.Instance.MoveBefore(oldValues.Item2, monitoredSystem.ID, true, false);
                                e.Handled = true;
                                return;
                            }
                            // dropping a ms
                            if (oldValues.Item1 == false && oldValues.Item2 != monitoredSystem.ID)
                            {
                                //Debug
                                //Console.WriteLine("Dropping MS hittest");

                                foreach (MonitoredSystem current in DataModel.Instance.Elements.GetMonitoredSystems())
                                {
                                    if (current.ID.Equals(oldValues.Item2) && oldValues.Item3 != monitoredSystem.OuID)
                                    {
                                        var oldState = LayoutManager.Instance.GetMSState(current.ID);
                                        current.OuID = monitoredSystem.OuID;
                                        LayoutManager.Instance.CurrentLayout.SetState(current.ID, oldState);
                                    }
                                }

                                //Debug
                                //Console.WriteLine("Dropping monitoredSystem with ID " + oldValues.Item2 + " and name " + oldValues.Item4 + " before monitoredSystem with ID " + monitoredSystem.ID + " and name " + monitoredSystem.Name);

                                LayoutManager.Instance.MoveBefore(oldValues.Item2, monitoredSystem.ID, false, false);
                                e.Handled = true;
                                return;
                            }
                        }
                    }
                }

                // dropping a OU
                if (oldValues.Item1 == true && element != null)
                {
                    //Debug
                    //Console.WriteLine("Dropping OU");

                    var ou = (element as ExtendedTreeViewItem).DataContext as TileableElement;

                    // check if the old and new parent OU are different
                    if (ou != null && ou.GetType().Equals(typeof(OrganizationalUnit)) && !oldValues.Item3.Equals(ou.ID))
                    {
                        foreach (OrganizationalUnit current in DataModel.Instance.Elements.GetOrganizationalUnits())
                        {
                            if (current.ID.Equals(oldValues.Item2) && current.ID != ou.ID)
                            {
                                bool dropDeeperInside = false;

                                foreach (var innerElement in current.GetOrganizationalUnits())
                                {
                                    if (innerElement.ID == (ou.ID))
                                    {
                                        dropDeeperInside = true;
                                    }
                                }

                                if (!dropDeeperInside)
                                {
                                    var isOpen = LayoutManager.Instance.GetOuState(current.ID);
                                    if (isOpen == null)
                                    {
                                        isOpen = false;
                                    }
                                    current.ParentID = ou.ID;
                                    LayoutManager.Instance.CurrentLayout.SetState(current.ID, (bool)isOpen);
                                }
                            }
                        }
                    }

                    // find nearest element
                    if ((element as ExtendedTreeViewItem).ItemsSource != null)
                    {
                        var casted = (element as ExtendedTreeViewItem).ItemsSource.Cast<TileableElement>();
                        List<Tuple<double, TileableElement>> differences = new List<Tuple<double, TileableElement>>();

                        foreach (var castedItem in casted)
                        {
                            var container = MainTreeView.GetContainerFromItem(castedItem);
                            if (container != null)
                            {
                                GeneralTransform trans = container.TransformToAncestor(MainTreeView);
                                Point offset = trans.Transform(new Point(0, 0));
                                var difference = Math.Sqrt(Math.Pow((offset.X - e.GetPosition(MainTreeView).X), 2) + Math.Pow((offset.Y - e.GetPosition(MainTreeView).Y), 2));
                                differences.Add(new Tuple<double, TileableElement>(difference, (castedItem as TileableElement)));
                            }
                        }

                        if (differences.Count > 0)
                        {
                            differences.Sort((x, y) => (x.Item1.CompareTo(y.Item1)));
                            if (differences.ElementAt(0).Item2.ID != oldValues.Item2)
                            {
                                if (differences.ElementAt(0).Item2.GetType().Equals(typeof(MonitoredSystem)))
                                {
                                    //Debug
                                    //Console.WriteLine("Dropping OU with ID " + oldValues.Item2 + " and name " + oldValues.Item4 + " before MS with ID " + differences.ElementAt(0).Item2.ID + " and name " + differences.ElementAt(0).Item2.Name);

                                    LayoutManager.Instance.MoveBefore(oldValues.Item2, differences.ElementAt(0).Item2.ID, true, false);
                                }
                                else
                                {
                                    //Debug
                                    //Console.WriteLine("Dropping OU with ID " + oldValues.Item2 + " and name " + oldValues.Item4 + " before OU with ID " + differences.ElementAt(0).Item2.ID + " and name " + differences.ElementAt(0).Item2.Name);

                                    LayoutManager.Instance.MoveBefore(oldValues.Item2, differences.ElementAt(0).Item2.ID, true, true);
                                }
                            }
                        }
                    }
                }
                // dropping an MonitoredSystem
                else if (oldValues.Item1 == false && element != null)
                {
                    //Debug
                    //Console.WriteLine("Dropping MS");

                    // find the OU of this panel
                    var ou = (element as ExtendedTreeViewItem).DataContext as TileableElement;

                    // check if the old and new OU are different
                    if (ou != null && ou.GetType().Equals(typeof(OrganizationalUnit)) && !oldValues.Item3.Equals(ou.ID))
                    {
                        foreach (MonitoredSystem current in DataModel.Instance.Elements.GetMonitoredSystems())
                        {
                            if (current.ID.Equals(oldValues.Item2) && !oldValues.Item3.Equals(ou.ID))
                            {
                                var oldState = LayoutManager.Instance.GetMSState(current.ID);
                                current.OuID = ou.ID;
                                LayoutManager.Instance.CurrentLayout.SetState(current.ID, oldState);
                            }
                        }
                    }

                    // find nearest element
                    if ((element as ExtendedTreeViewItem).ItemsSource != null)
                    {
                        var casted = (element as ExtendedTreeViewItem).ItemsSource.Cast<TileableElement>();
                        List<Tuple<double, TileableElement>> differences = new List<Tuple<double, TileableElement>>();

                        foreach (var castedItem in casted)
                        {
                            var container = MainTreeView.GetContainerFromItem(castedItem);
                            if (container != null)
                            {
                                GeneralTransform trans = container.TransformToAncestor(MainTreeView);
                                Point offset = trans.Transform(new Point(0, 0));
                                var difference = Math.Sqrt(Math.Pow((offset.X - e.GetPosition(MainTreeView).X), 2) + Math.Pow((offset.Y - e.GetPosition(MainTreeView).Y), 2));
                                differences.Add(new Tuple<double, TileableElement>(difference, (castedItem as TileableElement)));
                            }
                        }

                        if (differences.Count > 0)
                        {
                            differences.Sort((x, y) => (x.Item1.CompareTo(y.Item1)));
                            if (differences.ElementAt(0).Item2.ID != oldValues.Item2)
                            {
                                if (differences.ElementAt(0).Item2.GetType().Equals(typeof(MonitoredSystem)))
                                {
                                    //Debug
                                    //Console.WriteLine("Dropping MS with ID " + oldValues.Item2 + " and name " + oldValues.Item4 + " before MS with ID " + differences.ElementAt(0).Item2.ID + " and name " + differences.ElementAt(0).Item2.Name);

                                    LayoutManager.Instance.MoveBefore(oldValues.Item2, differences.ElementAt(0).Item2.ID, false, false);
                                }
                                else
                                {
                                    //Debug
                                    //Console.WriteLine("Dropping MS with ID " + oldValues.Item2 + " and name " + oldValues.Item4 + " before OU with ID " + differences.ElementAt(0).Item2.ID + " and name " + differences.ElementAt(0).Item2.Name);

                                    LayoutManager.Instance.MoveBefore(oldValues.Item2, differences.ElementAt(0).Item2.ID, false, true);
                                }
                            }
                        }
                    }
                }
                e.Handled = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION: MainWindow_DropHandler: " + ex.ToString());
            }
        }

        /// <summary>
        /// Enables the dropping of OUs in the root to be able to flatten the hierarchy.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MetaDropHandler(object sender, DragEventArgs e)
        {
            if (firstDrop)
            {
                foreach (var item in DataModel.Instance.Elements.GetMonitoredSystems())
                {
                    if (LayoutManager.Instance.GetValue(item.ID, false) == item.ID)
                    {
                        LayoutManager.Instance.SetValue(item.ID, false, item.ID);
                    }
                }
                foreach (var item in DataModel.Instance.Elements.GetOrganizationalUnits())
                {
                    if (LayoutManager.Instance.GetValue(item.ID, true) == item.ID)
                    {
                        LayoutManager.Instance.SetValue(item.ID, true, item.ID);
                    }
                }
                firstDrop = false;
            }

            try
            {
                // Tuple: bool isOU, int ID, int ouID
                Tuple<bool, int, int, string> oldValues = (Tuple<bool, int, int, string>)e.Data.GetData(typeof(Tuple<bool, int, int, string>));

                if (oldValues.Item2 == -1)
                {
                    return;
                }

                // dropping a OU
                if (oldValues.Item1 == true)
                {
                    foreach (OrganizationalUnit current in DataModel.Instance.Elements.GetOrganizationalUnits())
                    {
                        if (current.ID.Equals(oldValues.Item2))
                        {
                            List<Tuple<double, int, string>> differences = new List<Tuple<double, int, string>>();
                            foreach (var item in this.MainTreeView.Items)
                            {
                                if (item.GetType().Equals(typeof(OrganizationalUnit)))
                                {
                                    var container = MainTreeView.GetContainerFromItem(item);
                                    GeneralTransform trans = container.TransformToAncestor(MainTreeView);
                                    Point offset = trans.Transform(new Point(0, 0));
                                    var difference = Math.Sqrt(Math.Pow((offset.X - e.GetPosition(MainTreeView).X), 2) + Math.Pow((offset.Y - e.GetPosition(MainTreeView).Y), 2));
                                    differences.Add(new Tuple<double, int, string>(difference, (item as OrganizationalUnit).ID, (item as OrganizationalUnit).Name));
                                }
                            }

                            if (differences.Count > 0)
                            {
                                differences.Sort((x, y) => (x.Item1.CompareTo(y.Item1)));
                                if (differences.ElementAt(0).Item2 != oldValues.Item3)
                                {
                                    LayoutManager.Instance.MoveBefore(oldValues.Item2, differences.ElementAt(0).Item2, true, true);
                                    //Console.WriteLine("DEBUG MetaDropHandler: moved ou with id " + oldValues.Item2 + " and name " + oldValues.Item4 + " before ou with id " + differences.ElementAt(0).Item2 + " and name " + differences.ElementAt(0).Item3);
                                }
                            }

                            current.ParentID = null;
                            e.Handled = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION: MainWindow_DropHandler: " + ex.ToString());
            }
        }

        /// <summary>
        /// Saves the start-values for drag-and-drop.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // get the position and prepare for dragging
                startPosition = e.GetPosition(this);

                // find the OU that is being dragged
                var depObj = sender as DependencyObject;
                while (depObj != null && !depObj.GetType().Equals(typeof(ExtendedTreeViewItem)))
                {
                    depObj = VisualTreeHelper.GetParent(depObj);
                }

                if (depObj != null)
                {
                    var ou = (depObj as ExtendedTreeViewItem).DataContext as OrganizationalUnit;

                    if (ou != null)
                    {
                        startIsOU = true;
                        startOuID = ou.ID;
                        startPartentID = ou.ParentID == null ? -1 : (int)ou.ParentID;
                        startName = ou.Name;
                    }

                    e.Handled = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION: MainWindow_MouseLeftButtonDownHandler: " + ex.ToString());
            }
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
                // get the current mouse position
                Point mousePosition = e.GetPosition(null);
                Vector diff = startPosition - mousePosition;

                if (e.LeftButton == MouseButtonState.Pressed &&
                    (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    // initiate the drag-and-drop if the OU is valid
                    if (startOuID != -1)
                    {
                        DataObject dataObject = new DataObject(new Tuple<bool, int, int, string>(startIsOU, startOuID, startPartentID, startName));

                        DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Move);
                    }
                }

                if (e.LeftButton == MouseButtonState.Released)
                {
                    startOuID = -1;
                    startPartentID = -1;
                    startName = "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION: MainWindow_MouseMoveHandler: " + ex.ToString());
            }
        }

        #endregion

        #region Mouse Clicks


        private void MainTreeView_MouseRightButtonDown_outter(object sender, MouseButtonEventArgs e)
        {
            startOuID = -1;
            startPartentID = -1;
            startName = "";

            MainTreeView.UnselectAll();
        }

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseEventArgs e)
        {
            startOuID = -1;
            startPartentID = -1;
            startName = "";

            TreeViewItem item = sender as TreeViewItem;
            if (item != null)
            {
                if (item.IsSelected == false)
                {
                    item.Focus();
                }
                e.Handled = true;
            }
        }

        private void MainTreeView_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            MainTreeView.UnselectAll();
        }

        #endregion

    }
}
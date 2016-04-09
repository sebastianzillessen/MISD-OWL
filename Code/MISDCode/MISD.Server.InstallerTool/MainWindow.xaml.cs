using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Threading;
using System.Threading;

namespace MISD.Server.InstallerTool
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 50), DispatcherPriority.ApplicationIdle, (o, ex) =>
            {
                if (ServiceInstaller.ServiceIsInstalled("ServerService"))
                {
                    switch (ServiceInstaller.GetServiceStatus("ServerService"))
                    {
                        case ServiceState.Unknown:
                            PluginServiceState.Text = "Unbekannt";
                            break;
                        case ServiceState.NotFound:
                            PluginServiceState.Text = "Nicht gefunden";
                            break;
                        case ServiceState.Stopped:
                            PluginServiceState.Text = "Gestoppt";
                            break;
                        case ServiceState.StartPending:
                            PluginServiceState.Text = "Wird gestartet...";
                            break;
                        case ServiceState.StopPending:
                            PluginServiceState.Text = "Wird gestoppt...";
                            break;
                        case ServiceState.Running:
                            PluginServiceState.Text = "Gestartet";
                            break;
                        case ServiceState.ContinuePending:
                            PluginServiceState.Text = "Wird fortgesetzt...";
                            break;
                        case ServiceState.PausePending:
                            PluginServiceState.Text = "Wird pausiert...";
                            break;
                        case ServiceState.Paused:
                            PluginServiceState.Text = "Pausiert";
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    PluginServiceState.Text = "Nicht installiert";
                }
               
            }, Dispatcher.CurrentDispatcher).Start();
        }

        #region Service

        private void InstallPluginService(object sender, MouseButtonEventArgs e)
        {
            new Thread(new ThreadStart(() =>
            {
                try { ServiceInstaller.InstallAndStart("ServerService", "Server Service", Directory.GetCurrentDirectory() + "\\MISD.Server.exe"); }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            })).Start();
        }

        private void StartPluginService(object sender, MouseButtonEventArgs e)
        {
            new Thread(new ThreadStart(() =>
            {
                try { ServiceInstaller.StartService("ServerService"); }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            })).Start();
        }

        private void StopPluginService(object sender, MouseButtonEventArgs e)
        {
            new Thread(new ThreadStart(() =>
            {
                try { ServiceInstaller.StopService("ServerService"); }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            })).Start();
        }

        private void UninstallPluginService(object sender, MouseButtonEventArgs e)
        {
            new Thread(new ThreadStart(() =>
            {
                try { ServiceInstaller.Uninstall("ServerService"); }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            })).Start();
        }
        #endregion
    }
}

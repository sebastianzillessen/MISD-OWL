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
using System.Threading;
using System.Windows.Threading;
using System.IO;

namespace MISD.Workstation.InstallerTool
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
                if (ServiceInstaller.ServiceIsInstalled("WorkstationService"))
                {
                    switch (ServiceInstaller.GetServiceStatus("WorkstationService"))
                    {
                        case ServiceState.Unknown:
                            WorkstationServiceState.Text = "Unbekannt";
                            break;
                        case ServiceState.NotFound:
                            WorkstationServiceState.Text = "Nicht gefunden";
                            break;
                        case ServiceState.Stopped:
                            WorkstationServiceState.Text = "Gestoppt";
                            break;
                        case ServiceState.StartPending:
                            WorkstationServiceState.Text = "Wird gestartet...";
                            break;
                        case ServiceState.StopPending:
                            WorkstationServiceState.Text = "Wird gestoppt...";
                            break;
                        case ServiceState.Running:
                            WorkstationServiceState.Text = "Gestartet";
                            break;
                        case ServiceState.ContinuePending:
                            WorkstationServiceState.Text = "Wird fortgesetzt...";
                            break;
                        case ServiceState.PausePending:
                            WorkstationServiceState.Text = "Wird pausiert...";
                            break;
                        case ServiceState.Paused:
                            WorkstationServiceState.Text = "Pausiert";
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    WorkstationServiceState.Text = "Nicht installiert";
                }
            }, Dispatcher.CurrentDispatcher).Start();
        }

        #region Service

        private void InstallWorkstationService(object sender, MouseButtonEventArgs e)
        {
            new Thread(new ThreadStart(() =>
            {
                try { ServiceInstaller.InstallAndStart("WorkstationService", "Workstation Service", Directory.GetCurrentDirectory() + "\\MISD.Workstation.Windows.exe"); }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + ex.StackTrace);
                }
            })).Start();
        }

        private void StartWorkstationService(object sender, MouseButtonEventArgs e)
        {
            new Thread(new ThreadStart(() =>
            {
                try { ServiceInstaller.StartService("WorkstationService"); }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            })).Start();
        }

       
        private void StopWorkstationService(object sender, MouseButtonEventArgs e)
        {
            new Thread(new ThreadStart(() =>
            {
                try { ServiceInstaller.StopService("WorkstationService"); }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            })).Start();
        }

        
        private void UninstallWorkstationService(object sender, MouseButtonEventArgs e)
        {
            new Thread(new ThreadStart(() =>
            {
                try { ServiceInstaller.Uninstall("WorkstationService"); }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            })).Start();
        }

        #endregion
    }
}

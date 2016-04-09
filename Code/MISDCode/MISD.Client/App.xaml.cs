using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using MISD.Client.Managers;
using MISD.Client.Model;
using System.Threading.Tasks;
using MISD.Client;
using System.Windows.Input;

namespace MISD.Client
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.Exit += App_Exit;
            AppDomain.CurrentDomain.SetShadowCopyFiles();
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            //DataModel.Instance.StopSynchronization();
        }

        protected async override void OnExit(ExitEventArgs e)
        {
            await DataModel.Instance.Save();
            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Task.Factory.StartNew(new Action(() =>
            {
                var x = LayoutManager.Instance;
            }));
        }
    }
}

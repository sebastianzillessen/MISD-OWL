using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Threading;
using MISD.Workstation.Windows.WSWebService;
using System.Reflection;

namespace MISD.Workstation.Windows
{
    public class WorkstationService : ServiceBase
    {

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            Thread starterThread = new Thread(this.startingActions);
            starterThread.Start();

        }

        /// <summary>
        /// Starting actions are executed in a separate thread to keep the OnStart Method clean and fast.
        /// </summary>
        private void startingActions()
        {
            AppDomain.CurrentDomain.SetShadowCopyFiles();
            WorkstationLogger.WriteLog("Initializing webservice.");
            WorkstationWebServiceClient ws = ServerConnection.GetWorkstationWebService();
            WorkstationLogger.WriteLog("Logging " + ServerConnection.GetWorkstationName() + " in. "
                + " MAC: " + ServerConnection.GetMacAdress());
            try
            {
                if (ServerConnection.SignIn((byte)MISD.Core.Platform.Windows))
                {
                    WorkstationLogger.WriteLog("Successfully signed in.");
                }
                else
                {
                    WorkstationLogger.WriteLog("Couldn't sign in.");
                }
                WorkstationLogger.WriteLog("Starting schedulers.");
                Scheduling.Scheduler.Instance.Start();
            }
            catch (Exception e)
            {
                WorkstationLogger.WriteLog("Error: " + e.Message);
            }
        }

        protected override void OnStop()
        {
            WorkstationLogger.WriteLog("Stoping schedulers.");
            Scheduling.Scheduler.Instance.Stop();
            WorkstationLogger.WriteLog("Signing " + ServerConnection.GetWorkstationName() + " out.");
            ServerConnection.SignOut();
            WorkstationLogger.Close();
        }

        private void InitializeComponent()
        {
            // 
            // WorkstationService
            // 
            this.ServiceName = "MISDWorkstationService";

        }
    }
}

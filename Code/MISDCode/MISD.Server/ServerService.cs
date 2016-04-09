using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using System.Threading;

namespace MISD.Server
{
    public class ServerService : ServiceBase
    {
        protected override void OnStart(string[] args)
        {
            new Thread(new ThreadStart(InternalServer.Instance.Start)).Start();
        }

        protected override void OnStop()
        {
            InternalServer.Instance.Stop();
        }

        private void InitializeComponent()
        {
            // 
            // ServerService
            // 
            this.ServiceName = "ServerService";
        }
    }
}

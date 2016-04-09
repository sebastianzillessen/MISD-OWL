using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;

namespace MISD.Workstation.Windows
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        protected override void OnCommitted(System.Collections.IDictionary savedState)
        {
            base.OnCommitted(savedState);

            // Launch our service directly
            ServiceController sc = new ServiceController("MISDWorkstationService");
            sc.Start();
        }
    }
}

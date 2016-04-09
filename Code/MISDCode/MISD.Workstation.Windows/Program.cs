using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using MISD.Workstation.Windows.Properties;
using System.ComponentModel;

namespace MISD.Workstation.Windows
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new WorkstationService() 
			};
            ServiceBase.Run(ServicesToRun);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISD.Client.Core
{
    public static class Extensions
    {
        public static Version GetVersion(this IPluginVisualization plugin)
        {
            return System.Reflection.Assembly.GetAssembly(plugin.GetType()).GetName().Version;
        }
    }
}

using MISD.Client.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISD.Plugins.Windows.Storage.Visualization
{
    public class PluginVisualization : IPluginVisualization
    {
        public string Name
        {
            get
            {
                return "Storage";
            }
        }

        public int Rows
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int Columns
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}

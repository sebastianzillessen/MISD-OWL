using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISD.Client.Model.Managers
{
    public interface ILayouter
    {
        void MSValueChanged(int ID, float value);
        void OUValueChanged(int ID, float value);
        void PropertyChanged(KeyValuePair<string, string> property);
        void OUStateChanged(int ID, bool open);
        void MSStateChanged(int ID, MonitoredSystemState state);
        void LayoutChanged(Layout layout);
    }
}

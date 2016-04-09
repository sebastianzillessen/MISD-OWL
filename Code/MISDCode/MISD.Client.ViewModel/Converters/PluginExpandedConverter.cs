using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using MISD.Client.Managers;
using MISD.Client.Model;

namespace MISD.Client.ViewModel.Converters
{
    public class PluginExpandedConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            MonitoredSystem ms = null;
            string plugin_name = null;
            foreach (var o in values)
            {
                if ((o as MonitoredSystem) != null)
                {
                    ms = o as MonitoredSystem;
                }
                if ((o as string) != null){
                    plugin_name = o as string;}
                //if ((o as int?) != null)
                //{
                //    Console.WriteLine("Sequence was " + o);
                //}
            }
            if (plugin_name == null || ms == null)
            {
                return false;
            }
            MonitoredSystemState state = LayoutManager.Instance.GetMSState(ms.ID);
            return state.ShownPlugins.Contains(plugin_name);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

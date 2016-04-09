using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using MISD.Client.Model;

namespace MISD.Client.ViewModel.Converters
{
    public class MenuOpenToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is MenuStates && ((MenuStates)value) != MenuStates.Closed)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                if (((bool)value) == true)
                {
                    return MenuStates.Monitoring;
                }
                else
                {
                    return MenuStates.Closed;
                }
            }
            else
            {
                return MenuStates.Closed;
            }
        }
    }
}

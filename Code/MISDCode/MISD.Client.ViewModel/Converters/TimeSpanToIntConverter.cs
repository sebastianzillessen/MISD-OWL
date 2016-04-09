using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MISD.Client.ViewModel.Converters
{
    public class TimeSpanToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is TimeSpan)
            {
                return ((TimeSpan)value).Days;
            }
            else
            {
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int)
            {
                return new TimeSpan((int)value, 0, 0, 0);
            }
            else
            {
                return new TimeSpan(30, 0, 0, 0); ;
            }
        }
    }
}

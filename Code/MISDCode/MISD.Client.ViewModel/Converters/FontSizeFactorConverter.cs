using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MISD.Client.ViewModel.Converters
{
    public class FontSizeFactorConverter : IValueConverter
    {
        private double factor = 1.3;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double)
            {
                return ((double)value) * factor;
            }
            else
            {
                return Application.Current.Resources["ApplicationGlobalFontSize"];
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double)
            {
                return ((double)value) / factor;
            }
            else
            {
                return Application.Current.Resources["ApplicationGlobalFontSize"];
            }
        }
    }
}

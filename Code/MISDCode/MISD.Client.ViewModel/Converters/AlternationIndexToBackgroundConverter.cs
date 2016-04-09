using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace MISD.Client.ViewModel.Converters
{
    public class AlternationIndexToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int)
            {
                int val = (int)value;
                if (val == 0)
                {
                    return Brushes.Transparent;
                }
                else
                {
                    return new SolidColorBrush(new Color() { A = 70, R = 255, G = 255, B = 255 });
                }
            }
            return Brushes.Purple;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

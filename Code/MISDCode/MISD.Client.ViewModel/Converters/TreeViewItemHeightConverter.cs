using MISD.Client.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MISD.Client.ViewModel.Converters
{
    public class TreeViewItemHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double d = (double)value
                   - 29 /* Top until text begins */
                   - MainWindowViewModel.Instance.CharacterHeight /* Text */
                   - 29 /* Until Panel begins */
                   - 28 /* Bottom margin inside expander */
                   - 24 /* Lower outer margin */;

            // fix unstable (flipping in inner hierarchies) layout
            if (d < ((double)value - 92))
            {
                d = (double)value - 92;
            }
            return d;// (int) d;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

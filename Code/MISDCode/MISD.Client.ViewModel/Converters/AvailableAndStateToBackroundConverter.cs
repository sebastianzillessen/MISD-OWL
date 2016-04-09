using MISD.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace MISD.Client.ViewModel.Converters
{
    public class AvailableAndStateToBackroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var state = values[0];
            var avilable = values[1];

            if (avilable is bool)
            {
                if (!((bool)avilable))
                {
                    return Brushes.Navy;
                }
            }
            if (state is MappingState)
            {
                // get colours
                var stateConverter = new StateToBackgroundConverter();
                return stateConverter.Convert(state, targetType, parameter, culture);
            }
            else
            {
                return Brushes.Violet;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

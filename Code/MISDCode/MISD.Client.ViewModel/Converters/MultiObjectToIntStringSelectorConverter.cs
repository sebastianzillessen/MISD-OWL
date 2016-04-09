using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MISD.Client.ViewModel.Converters
{
    public class MultiObjectToIntStringSelectorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var unFilterdSource = values[0] as int?;
            var filterdSource = values[1] as int?;
            var filterString = values[2] as string;

            if (filterString != null)
            {
                if (filterString.Length == 0)
                {
                    return unFilterdSource.ToString();
                }
                else
                {
                    return filterdSource.ToString();
                }
            }
            else
            {
                return "0";
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

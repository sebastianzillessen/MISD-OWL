using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MISD.Client.ViewModel.Converters
{
    public class MultiObjectStringSelectorConverter : IMultiValueConverter
    {
        private object unFilterdSource;
        private object filterdSource;
        private object filterString;

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            unFilterdSource = values[0];
            filterdSource = values[1];
            filterString = values[2];

            if (filterString is string)
            {
                if ((filterString as string).Length == 0)
                {
                    return unFilterdSource;
                }
                else
                {
                    return filterdSource;
                }
            }
            else
            {
                return "";
            }         
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            object[] result = new object[3];

            if (filterString is string)
            {
                if ((filterString as string).Length == 0)
                {
                    result[0] = value;
                    result[1] = filterdSource;
                }
                else
                {
                    result[0] = unFilterdSource;
                    result[1] = value;
                }
            }
            else
            {
                result[0] = null;
                result[1] = null;
            }
            
            result[2] = filterString;

            return result;

        }
    }
}

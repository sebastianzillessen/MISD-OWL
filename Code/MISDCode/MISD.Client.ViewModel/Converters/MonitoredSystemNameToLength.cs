using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MISD.Client.ViewModel.Converters
{
    //public class MonitoredSystemNameToLength : IValueConverter
    //{
    //    public object Convert(object value, Type targetType,
    //        object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        if (value == null)
    //            return string.Empty;
    //        if (parameter == null)
    //            return value;
    //        int _MaxLength=MainWindowViewModel.Instance.NumberOfCharactersInTileName;
    //        //if (!int.TryParse(parameter.ToString(), out _MaxLength))
    //        //    return value;
    //        var _String = value.ToString();
    //        if (_String.Length > _MaxLength)
    //            _String = _String.Substring(0, _MaxLength) + "...";
    //        return _String;
    //    }

    //    public object ConvertBack(object value, Type targetType,
    //        object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class MonitoredSystemNameToLength : IMultiValueConverter
    {
       public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        
        {
            if (values[0] == null)
                return string.Empty;
            if (values[1] == null)
                return values[0];
            int _MaxLength;
            if (!int.TryParse(values[1].ToString(), out _MaxLength))
                return values[0];
            var _String = values[0].ToString();
            if (_String.Length > _MaxLength)
                _String = _String.Substring(0, _MaxLength) + "...";
            return _String;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

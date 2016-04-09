using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace MISD.Client.ViewModel.Converters
{
    public class CurrentValueToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str = "";
            var content = value as string;
            if (string.IsNullOrWhiteSpace(content)) return "";
            if (content.IndexOf("\n") != -1)
            {
                str = content.Substring(0, content.IndexOf("\n") - 1);
            }
            else
            {
                str = content;
            }
            //int length;
            //try
            //{
            //    if (parameter != null)
            //    {
            //        length = Int32.Parse(parameter.ToString());
            //    }
            //    else
            //    {
            //        // Arno von "length = 0;" geändert
            //        length = MainWindowViewModel.Instance.NumberOfCharactersInTileName / 2;
            //    }
            //}
            //catch
            //{
            //    length = MainWindowViewModel.Instance.NumberOfCharactersInTileName / 2;
            //}
            //if (str != null && str.Length > length)
            //{
            //    string l = str.Substring(0, length / 2);
            //    string r = str.Substring(str.Length - length / 2, length / 2);
            //    str = l + "..." + r;
            //}
            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

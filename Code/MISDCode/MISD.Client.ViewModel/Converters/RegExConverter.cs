using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using MISD.RegExUtil;

namespace MISD.Client.ViewModel.Converters
{
    public class RegExConverter : IValueConverter
    {
        string oldValue = "";

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string)
            {
                //safe value
                oldValue = value as string;
            }
            var startList = new List<Tuple<Operation, string>>();
            startList.Add(new Tuple<Operation,string>(Operation.Contain, ""));
            return startList;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string result = "";
            var resultRegExList = new List<string>();

            if (value is List<Tuple<Operation, string>>)
            {
                var regExList = value as List<Tuple<Operation, string>>;

                // if the list contains only ´the initial item
                if (regExList.Count == 0 || (regExList.Count == 1 && regExList.ElementAt(0).Item2 == ""))
                {
                    return oldValue;
                }

                foreach (var r in regExList)
                {
                    if (r is Tuple<Operation, string>)
                    {
                        var opAndString = r as Tuple<Operation, string>;
                        resultRegExList.Add(RegExUtility.GenerateRegEx(opAndString.Item2, opAndString.Item1));
                    }
                }
                result = RegExUtility.MergeRegEx(resultRegExList);
            }

            return result;
        }
    }
}

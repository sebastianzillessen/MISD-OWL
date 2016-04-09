using MISD.RegExUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MISD.Client.ViewModel.Converters
{
    public class RegExOperationToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Operation)
            {
                var op = (Operation) value;

                switch (op)
                {
                    case Operation.Contain:
                        return "&#8712;";
                    case Operation.Equal:
                        return "=";
                    case Operation.Less:
                        return "&lt;";
                    case Operation.Major:
                        return "&gt;";
                    case Operation.NotContain:
                        return "&#8713;";
                    default:
                        return "&#8712;";
                }

            }
            else
            {
                return "&#8712;";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string)
            {
                var op = value as string;

                switch (op)
                {
                    case "&#8712;":
                        return Operation.Contain;
                    case "=":
                        return Operation.Equal;
                    case "&lt;":
                        return Operation.Less;
                    case "&gt;":
                        return Operation.Major;
                    case "&#8713;":
                        return Operation.NotContain;
                    default:
                        return Operation.Contain;
                }
            }
            else
            {
                return Operation.Contain;
            }
        }
    }
}

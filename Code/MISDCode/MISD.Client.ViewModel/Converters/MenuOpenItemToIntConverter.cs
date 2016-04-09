using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using MISD.Client.Model;

namespace MISD.Client.ViewModel.Converters
{
    public class MenuOpenItemToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is MenuStates)
            {
                switch ((MenuStates)value)
                {
                    case MenuStates.Monitoring:
                        return 0;
                    case MenuStates.Administration:
                        return 1;
                    case MenuStates.EmailSettings:
                        return 2;
                    case MenuStates.View:
                        return 3;
                    case MenuStates.Settings:
                        return 4;
                    case MenuStates.Info:
                        return 5;
                    default:
                        return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int)
            {
                switch ((int)value)
                {
                    case 0:
                        return MenuStates.Monitoring;
                    case 1:
                        return MenuStates.Administration;
                    case 2:
                        return MenuStates.EmailSettings;
                    case 3:
                        return MenuStates.View;
                    case 4:
                        return MenuStates.Settings;
                    case 5:
                        return MenuStates.Info;
                    default:
                        return MenuStates.Closed;
                }
            }
            else
            {
                return MenuStates.Closed;
            }
        }
    }
}

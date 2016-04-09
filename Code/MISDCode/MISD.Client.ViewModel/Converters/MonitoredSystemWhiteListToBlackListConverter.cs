using MISD.Client.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MISD.Client.ViewModel.Converters
{
    public class MonitoredSystemWhiteListToBlackListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var blackList = new List<Tuple<int, string, string, string>>();
            if (value is ExtendedObservableCollection<Tuple<int, string, string, string>>)
            {
                var whiteList = value as ExtendedObservableCollection<Tuple<int, string, string, string>>;

                if (whiteList.Count > 0)
                {
                    
                    foreach (var element in DataModel.Instance.Elements.GetMonitoredSystems())
                    {
                        bool found = false;

                        foreach (var white in whiteList)
                        {
                            if (element.Equals(white))
                            {
                                found = true;
                            }
                        }
                        if (!found)
                        {
                            //for tuple using see Model.MailUser
                            // Tuple: ID | MAC | Name | FQDN
                            blackList.Add(new Tuple<int, string, string, string>(
                                element.ID,
                                element.MAC,
                                element.Name,
                                element.FQDN));
                        }
                    }
                }
                else
                {
                    foreach (var element in DataModel.Instance.Elements.GetMonitoredSystems())
                    {
                        //for tuple using see Model.MailUser
                        // Tuple: ID | MAC | Name | FQDN
                        blackList.Add(new Tuple<int, string, string, string>(
                            element.ID,
                            element.MAC,
                            element.Name,
                            element.FQDN));
                    }
                }
            }
            return blackList;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

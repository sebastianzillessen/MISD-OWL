using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using MISD.Client.Model;

namespace MISD.Client.ViewModel.Converters
{
    public class LevelDefinitionsToPlusButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is LevelDefinition)
            {
                var tempList = (from p in DataModel.Instance.LevelDefinitions
                                where p.Level >= (value as LevelDefinition).Level
                                orderby p.Level, p.Rows, p.ID ascending
                                select p).ToList();

                for (int i = 0; i < tempList.Count; i++)
                {
                    if (tempList.ElementAt(i).ID == (value as LevelDefinition).ID && i < tempList.Count - 1)
                    {
                        return Visibility.Visible;
                    }
                }
            }
            return Visibility.Collapsed;
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

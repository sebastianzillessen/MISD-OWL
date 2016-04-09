/*
* Copyright 2012 Paul Brombosch, Ehssan Doust, David Krauss,
* Fabian Müller, Yannic Noller, Hanna Schäfer, Jonas Scheurich,
* Arno Schneider, Sebastian Zillessen
*
* This file is part of MISD-OWL, a project of the
* University of Stuttgart (Institution VISUS, Studienprojekt Spring 2012).
*
* MISD-OWL is published under GNU Lesser General Public License Version 3.
* MISD-OWL is free software, you are allowed to redistribute and/or
* modify it under the terms of the GNU Lesser General Public License
* Version 3 or any later version. For details see here:
* http://www.gnu.org/licenses/lgpl.html
*
* MISD-OWL is distributed without any warranty, without even the
* implied warranty of merchantability or fitness for a particular purpose.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using MISD.Client.Model;
using MISD.Client.Model.Resources;
using MISD.Core;
namespace MISD.Client.ViewModel.Converters
{
    public class MonitoredSystemStateToMaintenanceTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            String text;
            if (value is MappingState)
            {
                switch ((MappingState)value)
                {
                    case MappingState.OK:
                        text= Strings.SetToMaintenance;
                        break;
                    case MappingState.Warning:
                        text = Strings.SetToMaintenance;
                        break;
                    case MappingState.Critical:
                        text = Strings.SetToMaintenance;
                        break;
                    case MappingState.Maintenance:
                       text= Strings.StopMaintenance;
                       break;
                    default:
                       return text = "";
                }
          
            }
            else
            {
                text = "";
            }
            return text;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            throw new NotImplementedException();
        }
    }
}

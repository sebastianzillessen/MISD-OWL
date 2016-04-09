using MISD.Client.Model;
using System.Windows;
using System.Windows.Controls;

namespace MISD.Client.ViewModel.Selectors
{
    public class TreeViewTemplateSelector : DataTemplateSelector
    {
        public DataTemplate OrganizationalUnitTemplate { get; set; }
        public DataTemplate MonitoredSystemTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is OrganizationalUnit)
            {
                return this.OrganizationalUnitTemplate;
            }
            else
            {
                return this.MonitoredSystemTemplate;
            }
        }
    }
}

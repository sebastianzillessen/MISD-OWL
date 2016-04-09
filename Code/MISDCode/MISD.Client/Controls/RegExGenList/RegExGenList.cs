using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MISD.Client.Controls.RegExGenList
{
    public class RegExGenList : System.Windows.Controls.ListBox
    {
        public RegExGenList()
        {
            this.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new RegExGenListItem();
        }

        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            this.ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;
            this.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;

            this.GetREgExGenListItem().ForEach(p => p.RegExGenListBackLink = this);

        }

        
    } 

    public static class RegExGenListExtensions
    {

        public static List<RegExGenListItem> GetREgExGenListItem(this RegExGenList listBox)
        {
            List<RegExGenListItem> result = new List<RegExGenListItem>();
            if (listBox.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                foreach (var item in listBox.ItemContainerGenerator.Items)
                {
                    var listboxItem = listBox.ItemContainerGenerator.ContainerFromItem(item);
                    result.Add(listboxItem as RegExGenListItem);
                }
                
            }
            return result;
        }
    }
}
